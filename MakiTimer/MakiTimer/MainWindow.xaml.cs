using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;//DispatcherTimer

namespace MakiTimer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        int MLimit=10,SLimit=0;//設定時間(秒または分)
        string LeftTimeChar;//残り時間の文字列
        private DispatcherTimer Now = null;//現在時刻用
        private DispatcherTimer Timer = null;//タイマー用
        DateTime StartUp,StartTime, EndTime;//起動時刻,カウントダウン開始時刻,カウントダウン終了時刻
        TimeSpan LimitSpan, LeftTimeSpan, NowTimeSpan, OldTimeSpan;//設定時間,経過時間,Pauseした時の経過時間
        Boolean Paused = false;//ポーズ状態判断
        private System.Media.SoundPlayer player = null;//サウンドプレイヤー

        public MainWindow()
        {
            InitializeComponent();
            //現在時刻用用DispatcherTimer
            Now = new DispatcherTimer(DispatcherPriority.Normal);
            Now.Interval = new TimeSpan(0, 0, 0, 1, 0);//1秒ごと更新
            Now.Tick += new EventHandler(Now_Tick);
            Now.Start();//現在時刻用は起動後スタート
            StartUp = DateTime.Now;//デバッグ

            //ストップウォッチ用DispatcherTimer
            Timer = new DispatcherTimer(DispatcherPriority.Normal);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);//100msごと更新
            Timer.Tick += new EventHandler(Timer_Tick);

        }

        void Now_Tick(object sender, EventArgs e)
        {
            NowTime.Content = DateTime.Now.ToString("現在時刻はM月d日ddddH時mm分ss秒");

        }

        private void MLimitChanged(object sender, TextChangedEventArgs e)//分設定テキストボックスの値が変更されたとき
        {
            MLimit = int.Parse(MLimitTB.Text);
            //初期化のときに'オブジェクト参照がオブジェクト インスタンスに設定されていません。'のエラーが解決できなかったので
            if (DateTime.Now < StartUp + new TimeSpan(99,0,0,0,0))//起動後99日以内のみ動作するようにする
            {
                Message.Content = MLimitTB.Text + "分" + SLimitTB.Text + "秒に\nセットします。";//設定時間表示
            }

        }
        private void MLimitBtnClick(object sender, RoutedEventArgs e)//設定時間変更ボタンが押されたとき
        {
            Control ctrl = (Control)sender;
            switch (ctrl.Name)//どちらのボタンが押されたか
            {
                case "MUpBtn"://▲のとき
                    MLimit += 1;
                    MLimitTB.Text = string.Concat(MLimit);
                    break;

                case "MDownBtn"://▼のとき
                    if (MLimit != 0) MLimit -= 1;//>1のときのみ-1
                    MLimitTB.Text = string.Concat(MLimit);
                    break;
            }
        }
        private void SLimitChanged(object sender, TextChangedEventArgs e)//秒設定テキストボックスの値が変更されたとき
        {
            SLimit = int.Parse(SLimitTB.Text);//設定時間(秒)更新
            Message.Content = MLimitTB.Text + "分" + SLimitTB.Text + "秒に\nセットします。";//設定時間表示
        }
        private void SLimitBtnClick(object sender, RoutedEventArgs e)//設定時間変更ボタンが押されたとき
        {
            Control ctrl = (Control)sender;
            switch (ctrl.Name)//どちらのボタンが押されたか
            {
                case "SUpBtn"://▲のとき
                    SLimit += 1;
                    SLimitTB.Text = string.Concat(SLimit);
                    break;

                case "SDownBtn"://▼のとき
                    if (SLimit != 0) SLimit -= 1;//>1のときのみ-1
                    SLimitTB.Text = string.Concat(SLimit);
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Control ctrl = (Control)sender;
            switch (ctrl.Name)//どのボタンが押されたか
            {
                case "StartBtn"://スタートボタン
                    Start();
                    break;

                case "ResetBtn"://リセットボタン
                    Reset();
                    break;

                case "PauseBtn"://ポーズボタン
                    Pause();
                    break;
            }
        }

        private void Start()//スタートボタンが押されたとき
        {
            StartBtn.IsEnabled = false;//スタートボタン無効化
            ResetBtn.IsEnabled = true;//リセットボタン有効化
            PauseBtn.IsEnabled = true;//ポーズボタン有効化
            MUpBtn.IsEnabled = false;//時間増加ボタン有効化
            MDownBtn.IsEnabled = false;//時間減少ボタン有効化
            SUpBtn.IsEnabled = false;//時間増加ボタン有効化
            SDownBtn.IsEnabled = false;//時間減少ボタン有効化

            StartTime = DateTime.Now;//カウントダウン開始時刻を記録
            if (Paused==true)//ポーズ有効のとき
            {
                //LimitSpanはポーズボタンが↓↓↓押された時点の残り時間を記録
                EndTime = StartTime.Add(LimitSpan);//再開時刻に残り時間を足してカウントダウン終了時間を計算
                Paused = false;//ポーズ解除
            }
            else//ポーズ無効のとき
            {
                //設定された値MLimitとSLimitを終了時間に足す
                LimitSpan = new TimeSpan(0, MLimit, SLimit);//TimeSpan型に変換
                EndTime = StartTime.Add(LimitSpan);//カウントダウン終了時刻を記録
            }
            Timer.Start();

            player = new System.Media.SoundPlayer(@"C:\Users\Owner\source\repos\MakiTimer\MakiTimer\start.wav");//再生するファイルのパス
            player.Play();//非同期再生する
        }

        private void Reset()//リセットボタンが押されたとき
        {
            StartBtn.IsEnabled = true;//スタートボタン有効化
            ResetBtn.IsEnabled = true;//リセットボタン無効化
            PauseBtn.IsEnabled = false;//ポーズボタン無効
            MUpBtn.IsEnabled = true;//時間増加ボタン有効化
            MDownBtn.IsEnabled = true;//時間減少ボタン有効化
            SUpBtn.IsEnabled = true;//時間増加ボタン有効化
            SDownBtn.IsEnabled = true;//時間減少ボタン有効化

            Paused = false;//ポーズ解除

            Message.Content = MLimitTB.Text + "分" + SLimitTB.Text + "秒に\nセットします。";//設定時間表示

            Timer.Stop();//タイマー停止

            player = new System.Media.SoundPlayer(@"C:\Users\Owner\source\repos\MakiTimer\MakiTimer\Reset.wav");//再生するファイルのパス
            player.Play();//非同期再生する
        }
        private void Pause()//ポーズボタンが押されたとき
        {
            StartBtn.IsEnabled = true;//スタートボタン有効化
            ResetBtn.IsEnabled = true;//リセットボタン有効化
            PauseBtn.IsEnabled = false;//ポーズボタン無効化


            LimitSpan = EndTime - DateTime.Now; //ポーズボタンが押された時点の残り時間を記録

            Timer.Stop();//タイマー停止

            Paused = true;//ポーズ有効

            Message.Content = "残り時間は\n" + LeftTimeChar + "\n現在ポーズ中です";//ポーズ中のメッセージ

            player = new System.Media.SoundPlayer(@"C:\Users\Owner\source\repos\MakiTimer\MakiTimer\Pause.wav");//再生するファイルのパス
            player.Play();//非同期再生する
        }

        void Timer_Tick(object sender, EventArgs e)//タイマー更新
        {
            LeftTimeSpan = EndTime - DateTime.Now;//残り時間を計算
            LeftTimeChar = LeftTimeSpan.ToString(@"m'分's'秒'");//残り時間を文字列に変換し保管
            Message.Content = "残り時間は\n" + LeftTimeChar + "\nです。";//残り時間表示

            if (LeftTimeSpan <= new TimeSpan(0, 0, 0, 0, 0))
            {
                Timer.Stop();
                this.Activate();//このウィンドウにフォーカスさせる
                this.Topmost = true;//最前面に移動
                this.Topmost = false;//最前面を解除
                player = new System.Media.SoundPlayer(@"C:\Users\Owner\source\repos\MakiTimer\MakiTimer\TimeUp.wav");//再生するファイルのパス
                player.Play();//非同期再生する
                MessageBox.Show("経過しました。", " Infomation", MessageBoxButton.OK, MessageBoxImage.Information);
                Reset();
            }
        }
    }
}