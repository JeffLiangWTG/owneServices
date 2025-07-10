using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ResString = CargoWise.Main.ResString;
using Timer = System.Windows.Forms.Timer;

namespace Enterprise.Startup.Tools
{
	internal partial class ThreadMonitorForm : ZChildForm
	{
		public ThreadMonitorForm(ThreadMonitor threadMonitor)
			: base(threadMonitor)
		{
			InitializeComponent();

			this.threadMonitor = threadMonitor;
			callStackDict = new Dictionary<string, string>();

			currentTimer = new Timer();
		}

		readonly Dictionary<string, string> callStackDict;
		readonly ThreadMonitor threadMonitor;
		readonly Timer currentTimer;

		const int MaxLinesCount = 10000;

		void BtnCollect_Click(object sender, EventArgs e)
		{
			LogCallStack();
		}

		void LSTTimes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (LSTTimes.SelectedItem != null)
			{
				TxtStackTrace.Text = callStackDict[LSTTimes.SelectedItem.ToString()];
			}
		}

		void BtnCopy_Click(object sender, EventArgs e)
		{
			if (!Globals.IsTest && callStackDict.Any())
			{
				var dataList = callStackDict.Select(c => string.Format(CultureInfo.InvariantCulture, @"[{0}]{1}{2}", c.Key, System.Environment.NewLine, c.Value));
				var text = string.Join(System.Environment.NewLine, dataList);

				if (!SafeClipboard.SetText(text))
				{
					Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
				}
			}
		}

		void BtnClear_Click(object sender, EventArgs e)
		{
			LSTTimes.DataSource = null;
			TxtStackTrace.Text = string.Empty;
			callStackDict.Clear();
		}

		void BtnStart_Click(object sender, EventArgs e)
		{
			if (currentTimer.Enabled)
			{
				StopTimer();
				UpdateControlEnableAndCaption();
			}
			else
			{
				StartTimer();
			}
		}

		void StartTimer()
		{
			PerformValidation();

			if (threadMonitor.HasErrors)
			{
				ShowErrorMessageBox();
			}
			else
			{
				StopTimer();

				currentTimer.Tick += CurrentTimer_Tick;
				currentTimer.Interval = threadMonitor.IntervalMillisecondSafe;

				currentTimer.Start();

				UpdateControlEnableAndCaption();
			}
		}

		void StopTimer()
		{
			currentTimer.Tick -= CurrentTimer_Tick;
			currentTimer.Stop();
		}

		void LogCallStack()
		{
			var dateTime = ZDateTime.Now.ToString("O", CultureInfo.InvariantCulture);
			threadMonitor.LogCallStack();

			callStackDict.Add(dateTime, threadMonitor.CallStack);

			var arrary = callStackDict.Keys.ToArray();
			Array.Reverse(arrary);

			if (arrary.Length > MaxLinesCount)
			{
				var lastKey = arrary.Last();
				callStackDict.Remove(lastKey);
			}

			LSTTimes.DataSource = arrary;
			LSTTimes.SelectedIndex = 0;
		}

		void ShowErrorMessageBox()
		{
			var message = ResString.GetMultilingualString("5834d086-da61-4c49-8037-d376a00e0bee", "There are errors that need to be corrected before this automatic tracking can be start.");
			var caption = ResString.GetMultilingualString("6d113e18-9108-4c4f-8295-1855bc4d6af8", "Unable to start this automatic tracking...");

			using (var messageBox = new ZErrorMessageBox(threadMonitor, message, caption))
			{
				ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
			}
		}

		void UpdateControlEnableAndCaption()
		{
			var isTimerRunning = currentTimer.Enabled;

			BtnStart.Text = isTimerRunning
				? ResString.GetMultilingualString("b67aaf98-6f6a-4c64-9901-f40409bdce7b", "Stop")
				: ResString.GetMultilingualString("f308bd76-7ff1-4fc1-a705-30c9dbeb6ad7", "Start");

			BtnStart.BackColor = isTimerRunning
				? Color.OrangeRed
				: Color.DarkSeaGreen;

			TxtIntervalNumber.Enabled = !isTimerRunning;
			CmbIntervalType.Enabled = !isTimerRunning;
		}

		void CurrentTimer_Tick(object sender, EventArgs e)
		{
			LogCallStack();
		}

		protected override void Dispose(bool disposing)
		{
			StopTimer();
			currentTimer.Dispose();

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
