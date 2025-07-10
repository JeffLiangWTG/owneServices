using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.RemotePrinting.Client
{
	public partial class ThreadMonitorForm : Form
	{
		public ThreadMonitorForm(Thread mainThread)
		{
			InitializeComponent();
			InitializeThreadGroupBox();
			InitializeIntervalTypeComboBox();

			MainThread = mainThread;
			CallStackDictionary = new Dictionary<string, string>();
		}

		readonly Thread MainThread;
		internal Thread PrintClientThread { get; set; }
		readonly Dictionary<string, string> CallStackDictionary;
		const int MaxLinesCount = 10000;

		void StartButton_Click(object sender, EventArgs e)
		{
			if (CurrentTimer.Enabled)
			{
				StopTimer();
				UpdateComponentEnableAndCaption();
			}
			else
			{
				StartTimer();
			}
		}

		void TimesListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (TimesListBox.SelectedItem != null)
			{
				StackTraceTextBox.Text = CallStackDictionary[TimesListBox.SelectedItem.ToString()];
			}
		}

		void CollectThreadSnapshotButton_Click(object sender, EventArgs e)
		{
			LogCallStack();
		}

		void CopyAllButton_Click(object sender, EventArgs e)
		{
			if (CallStackDictionary.Any())
			{
				var dataList = CallStackDictionary.Select(c => string.Format(CultureInfo.InvariantCulture, @"[{0}]{1}{2}", c.Key, System.Environment.NewLine, c.Value));
				var text = string.Join(System.Environment.NewLine, dataList);
				SafeClipboard.SetDataObject(text);
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			TimesListBox.DataSource = null;
			StackTraceTextBox.Text = string.Empty;
			CallStackDictionary.Clear();
		}

		#region Timer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "errorMessage")]
		bool ValidateBefreRunning()
		{
			var errorMessage = string.Empty;
			if (ThreadComboBox.SelectedIndex == -1)
			{
				errorMessage += "Please select a valid Thread.\r\n";
			}

			if (IntervalTypeComboBox.SelectedIndex == -1)
			{
				errorMessage += "Please select a valid Interval Type.\r\n";
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				StackTraceTextBox.Text = errorMessage;
				return false;
			}

			return true;
		}

		void StartTimer()
		{
			if (!ValidateBefreRunning())
			{
				return;
			}

			StopTimer();

			CurrentTimer.Tick += CurrentTimer_Tick;
			CurrentTimer.Interval = IntervalMillisecondSafe;

			CurrentTimer.Start();

			UpdateComponentEnableAndCaption();
		}

		void StopTimer()
		{
			CurrentTimer.Tick -= CurrentTimer_Tick;
			CurrentTimer.Stop();
		}

		void CurrentTimer_Tick(object sender, EventArgs e)
		{
			LogCallStack();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogCallStack")]
		void LogCallStack()
		{
			if (ThreadComboBox.SelectedIndex == -1)
			{
				LogCallStack("Please select a valid Thread.");
				return;
			}

			var thread = ThreadComboBox.SelectedItem.ToString() == TrackThreadTypeConstant.PrintClient ? PrintClientThread : MainThread;
			if (thread == null || !thread.IsAlive)
			{
				LogCallStack("The selected thread is no longer active.");
				return;
			}

			string callStack;
			try
			{
#pragma warning disable 0618
				try
				{
					if (thread != Thread.CurrentThread)
					{
						thread.Suspend();
					}

					callStack = new StackTrace(thread, true).ToString();
				}
				finally
				{
					if (thread != Thread.CurrentThread)
					{
						thread.Resume();
					}
				}
#pragma warning restore 0618
			}
			catch (ThreadStateException ex)
			{
				callStack = ex.ToString();
			}
			catch (SecurityException ex)
			{
				callStack = ex.ToString();
			}

			LogCallStack(callStack);
		}

		void LogCallStack(string callStack)
		{
			var dateTime = DateTime.Now.ToString("O", CultureInfo.InvariantCulture);
			CallStackDictionary.Add(dateTime, callStack);

			var arrary = CallStackDictionary.Keys.ToArray();
			Array.Reverse(arrary);

			if (arrary.Length > MaxLinesCount)
			{
				var lastKey = arrary.Last();
				CallStackDictionary.Remove(lastKey);
			}

			TimesListBox.DataSource = arrary;
			TimesListBox.SelectedIndex = 0;
			StackTraceTextBox.Text = callStack;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "StartButton")]
		void UpdateComponentEnableAndCaption()
		{
			var isTimerRunning = CurrentTimer.Enabled;

			StartButton.Text = isTimerRunning ? "Stop" : "Start";
			StartButton.BackColor = isTimerRunning ? Color.OrangeRed : Color.DarkSeaGreen;

			IntervalNumber.Enabled = !isTimerRunning;
			IntervalTypeComboBox.Enabled = !isTimerRunning;
		}

		#endregion

		#region ThreadGroup

		void InitializeThreadGroupBox()
		{
			ThreadComboBox.Items.Clear();
			ThreadComboBox.Items.AddRange(ThreadTypes.ToArray());
			ThreadComboBox.SelectedIndex = 0;
		}

		List<string> threadTypes;
		List<string> ThreadTypes
		{
			get
			{
				if (threadTypes == null)
				{
					threadTypes = new List<string>();
					threadTypes.Add(TrackThreadTypeConstant.Main);
					threadTypes.Add(TrackThreadTypeConstant.PrintClient);
				}
				return threadTypes;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Main Thread")]
		static class TrackThreadTypeConstant
		{
			public const string Main = "Main Thread";
			public const string PrintClient = "Print Client Thread";
		}

		#endregion

		#region Interval Type & Number

		void InitializeIntervalTypeComboBox()
		{
			IntervalTypeComboBox.Items.Clear();
			IntervalTypeComboBox.Items.AddRange(IntervalTypes.ToArray());
			IntervalTypeComboBox.SelectedIndex = 1;
		}

		List<string> intervalTypes;
		List<string> IntervalTypes
		{
			get
			{
				if (intervalTypes == null)
				{
					intervalTypes = new List<string>();
					intervalTypes.Add(IntervalTypeConstant.Millisecond);
					intervalTypes.Add(IntervalTypeConstant.Second);
					intervalTypes.Add(IntervalTypeConstant.Minute);
				}
				return intervalTypes;
			}
		}

		public int IntervalMillisecondSafe
		{
			get
			{
				var intervalType = IntervalTypeComboBox.SelectedItem;
				var intervalNumber = (int)IntervalNumber.Value;
				switch (intervalType)
				{
					case IntervalTypeConstant.Millisecond:
						return intervalNumber > MaxMilliseconds
								? MaxMilliseconds
								: (int)TimeSpan.FromMilliseconds(intervalNumber).TotalMilliseconds;

					case IntervalTypeConstant.Second:
						return intervalNumber > MaxSeconds
								? MaxMilliseconds
								: (int)TimeSpan.FromSeconds(intervalNumber).TotalMilliseconds;

					case IntervalTypeConstant.Minute:
						return intervalNumber > MaxMinutes
								? MaxMilliseconds
								: (int)TimeSpan.FromMinutes(intervalNumber).TotalMilliseconds;

					default:
						return MaxMilliseconds;
				}
			}
		}

		int MaxSeconds => (int)maxTimeSpan.TotalSeconds;
		int MaxMinutes => (int)maxTimeSpan.TotalMinutes;
		int MaxMilliseconds => (int)maxTimeSpan.TotalMilliseconds;
		TimeSpan maxTimeSpan = TimeSpan.FromHours(1);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "IntervalTypeConstant")]
		static class IntervalTypeConstant
		{
			public const string Millisecond = "Millisecond";
			public const string Second = "Second";
			public const string Minute = "Minute";
		}

		#endregion
	}
}
