using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools
{
	public partial class HotKeyMonitorForm : ZChildForm
	{
		public HotKeyMonitorForm()
		{
			InitializeComponent();
			currentTimer = new Timer();
		}

		SortedDictionary<string, string> keyPressSpans;
		SortedDictionary<string, string> dataSource;
		readonly Timer currentTimer;
		string latestRecordTime = string.Empty;

		void RefreshSpanTimes(bool forced = false)
		{
			var monitorLatestRecordTime = HotKeyMonitorProvider.GetHotKeyMonitor().GetLatestRecordTime();
			if (!forced && !NeedRefresh(monitorLatestRecordTime))
			{
				return;
			}

			if (OnlyShowProcessedKeysCheckBox.Checked)
			{
				dataSource = HotKeyMonitorProvider.GetHotKeyMonitor().GetFilteredSpans();
			}
			else
			{
				dataSource = HotKeyMonitorProvider.GetHotKeyMonitor().GetAllSpans();
				keyPressSpans = dataSource;
			}

			if (dataSource.IsNullOrEmpty())
			{
				ClearText();
				return;
			}

			var lastIndex = SpanTimes.SelectedIndex;

			var array = dataSource.Keys.ToArray();
			SpanTimes.DataSource = null;
			SpanTimes.DataSource = array;
			var newIndex = lastIndex >= array.Length ? array.Length - 1 : lastIndex;
			SpanTimes.SelectedIndex = newIndex;
			TxtKeyPressSpans.SelectionStart = TxtKeyPressSpans.Text.Length - 1;
			TxtKeyPressSpans.ScrollToCaret();
		}

		bool NeedRefresh(string monitorLatestRecordTime)
		{
			if (string.IsNullOrEmpty(monitorLatestRecordTime))
			{
				return false;
			}

			var needRefresh = !latestRecordTime.Equals(monitorLatestRecordTime);
			latestRecordTime = monitorLatestRecordTime;
			return needRefresh;
		}

		void SpanTimes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (SpanTimes.SelectedItem != null)
			{
				TxtKeyPressSpans.Text = dataSource[SpanTimes.SelectedItem.ToString()];
			}
		}

		void BtnStart_Click(object sender, EventArgs e)
		{
			var hotKeyMonitor = HotKeyMonitorProvider.GetHotKeyMonitor();
			hotKeyMonitor.Enabled = !hotKeyMonitor.Enabled;

			if (hotKeyMonitor.Enabled)
			{
				BtnStart.Text = Res.GetString("4705087d-b63f-41aa-875a-0e6f35850deb", "Stop");
				BtnStart.BackColor = System.Drawing.Color.LightCoral;
				StartTimer();
			}
			else
			{
				BtnStart.Text = Res.GetString("1585a857-c36c-413b-8d2f-ad65405229bd", "Start");
				BtnStart.BackColor = System.Drawing.Color.DarkSeaGreen;
				StopTimer();
			}
		}

		void StartTimer()
		{
			StopTimer();
			currentTimer.Tick += CurrentTimer_Tick;
			currentTimer.Interval = 1000;
			currentTimer.Start();
		}

		void StopTimer()
		{
			currentTimer.Tick -= CurrentTimer_Tick;
			currentTimer.Stop();
		}

		void CurrentTimer_Tick(object sender, EventArgs e)
		{
			RefreshSpanTimes();
		}

		void BtnCopy_Click(object sender, EventArgs e)
		{
			if (keyPressSpans != null && keyPressSpans.Any())
			{
				var text = CombineAllInfo();
				if (!SafeClipboard.SetText(text))
				{
					Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
				}
			}
		}

		void BtnClear_Click(object sender, EventArgs e)
		{
			var hotKeyMonitor = HotKeyMonitorProvider.GetHotKeyMonitor();
			hotKeyMonitor.ClearAll();
			keyPressSpans = null;
			dataSource = null;
			TxtAdditionalMsg.Text = string.Empty;
			ClearText();
		}

		void ClearText()
		{
			SpanTimes.DataSource = null;
			TxtKeyPressSpans.Text = string.Empty;
		}

		void OnlyShowProcessedKeysCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			RefreshSpanTimes(true);
		}

		string CombineAllInfo()
		{
			var dataList = keyPressSpans.Select(c => string.Format(CultureInfo.InvariantCulture, @"[{0}]{1}{2}", c.Key, System.Environment.NewLine, c.Value));
			var additionalMsg = TxtAdditionalMsg.Text;
			var environmentInfo = HotKeyMonitorProvider.GetHotKeyMonitor().GetEnvironmentInfo();

			var builder = new StringBuilder();
			builder.AppendLine(environmentInfo);
			builder.AppendLine((NoResString)"*** User Additional Message ***");
			builder.AppendLine(additionalMsg);
			builder.AppendLine(Environment.NewLine);
			var text = string.Join(Environment.NewLine, dataList);
			builder.AppendLine(text);
			return builder.ToString();
		}
	}
}
