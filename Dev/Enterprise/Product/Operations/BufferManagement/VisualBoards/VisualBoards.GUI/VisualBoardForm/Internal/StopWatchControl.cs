using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public sealed partial class StopWatchControl : ZUserControl, IDisposable
	{
		static MultilingualString PauseMeetingToolTipText
		{
			get { return ResString.GetMultilingualString("5c111460-a3cf-43a7-b96b-5794c3357a08", "Pause board meeting"); }
		}

		static MultilingualString ShowNextChannelToolTipText
		{
			get { return ResString.GetMultilingualString("dfb7f89c-4b97-49e4-a846-364c79a8b9ca", "Expand the channel after the currently selected channel"); }
		}

		static MultilingualString ShowPreviousChannelToolTipText
		{
			get { return ResString.GetMultilingualString("31974c1f-e498-42f0-9341-87f909569b49", "Expand the channel before the currently selected channel"); }
		}

		static MultilingualString ResumeBoardMeetingToolTipText
		{
			get { return ResString.GetMultilingualString("c2435c5a-d9e8-4c48-83b4-e8922a831543", "Resume board meeting"); }
		}

		static MultilingualString RefreshBoardMeetingToolTipText
		{
			get { return ResString.GetMultilingualString("51a32591-8266-45af-99c8-68c59a2d5245", "Reset board meeting timer"); }
		}

		public StopWatchControl()
		{
			InitializeComponent();
			InitTimerAndParameters();

			TotalElapsedTimeDisplayLabel.Font = new Font("Arial", 20.25f, FontStyle.Bold);
			StartButton.ToolTipCaption = PauseMeetingToolTipText;
			ResetButton.ToolTipCaption = RefreshBoardMeetingToolTipText;
			PreviousChannelButton.ToolTipCaption = ShowPreviousChannelToolTipText;
			NextChannelButton.ToolTipCaption = ShowNextChannelToolTipText;
		}

		IWindowsTimer timer;
		ZDateTime startTime;
		TimeSpan totalElapsedTime;
		TimeSpan elapsedTimeBeforePause;
		bool timerRunning;

		void InitTimerAndParameters()
		{
			InitTimer();
			InitParameter();
		}

		void InitTimer()
		{
			timer = ObjectFactory.Get<IWindowsTimer>();
			timer.Interval = 1000;
			timer.Tick += new EventHandler(Timer_Tick);
		}

		void InitParameter()
		{
			startTime = ZDateTime.Empty;
			totalElapsedTime = TimeSpan.Zero;
			elapsedTimeBeforePause = TimeSpan.Zero;
			timerRunning = false;
			TotalElapsedTimeDisplayLabel.Text = totalElapsedTime.ToString();
		}

#if DEBUG
		public
#endif
 void Timer_Tick(object sender, EventArgs e)
		{
			if (timerRunning)
			{
				totalElapsedTime = ZDateTime.Now - startTime + elapsedTimeBeforePause;
				totalElapsedTime = new TimeSpan(totalElapsedTime.Hours, totalElapsedTime.Minutes, totalElapsedTime.Seconds);
				TotalElapsedTimeDisplayLabel.Text = totalElapsedTime.ToString();
			}
		}

		public void StartOrResume()
		{
			startTime = ZDateTime.Now;

			timer.Start();
			timerRunning = true;
			StartButton.ToolTipCaption = PauseMeetingToolTipText;
			StartButton.BackgroundImage = Properties.Resources.pause;
		}

		public void Pause()
		{
			elapsedTimeBeforePause += ZDateTime.Now - startTime;

			timer.Stop();
			timerRunning = false;
			StartButton.ToolTipCaption = ResumeBoardMeetingToolTipText;
			StartButton.BackgroundImage = Properties.Resources.play;
		}

		public void Reset()
		{
			InitParameter();
			StartButton.ToolTipCaption = ResumeBoardMeetingToolTipText;
			StartButton.BackgroundImage = Properties.Resources.play;
		}

		void ShowPreviousChannel()
		{
			((BoardMeetingModeForm)FindForm()).ShowPreviousChannel();
		}

		void ShowNextChannel()
		{
			((BoardMeetingModeForm)FindForm()).ShowNextChannel();
		}

		public TimeSpan GetElapsedTime()
		{
			return totalElapsedTime;
		}

		public bool IsRunning
		{
			get { return timerRunning; }
		}

		void StartButton_Click(object sender, EventArgs e)
		{
			if (!timerRunning)
			{
				StartOrResume();
			}
			else
			{
				Pause();
			}
		}

		void ResetButton_Click(object sender, EventArgs e)
		{
			Reset();
		}

		void PreviousChannelButton_Click(object sender, EventArgs e)
		{
			ShowPreviousChannel();
		}

		void NextChannelButton_Click(object sender, EventArgs e)
		{
			ShowNextChannel();
		}

		public new void Dispose()
		{
			timer.Tick -= new EventHandler(Timer_Tick);
			timer.Stop();
		}

#if DEBUG
		public Image ButtonBackgroundImage_ForTest
		{
			get { return StartButton.BackgroundImage; }
		}
#endif

	}
}
