using System;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup.Tools
{
	public partial class NetworkMonitorForm : ZChildForm
	{
		public NetworkMonitorForm() : base()
		{
			InitializeComponent();
			monitor = new NetworkMonitor();
			monitor.PingResponseEvent += PingResponseEventHandler;
			radioButton1.Checked = true;
			chart1.ChartAreas[0].AxisX.RoundAxisValues();

			string fqDNName = null;
			try
			{
				fqDNName = EnterpriseChannel.Instance?.SendMessage<string>(EnterpriseChannelMessageTypes.GetFullyQualifiedMachineName, Array.Empty<byte>());
			}
			catch (OperationCanceledException)
			{
			}
			var chartTitlePrefix = fqDNName ?? Res.GetString("5E74F3C9-ADB5-4F30-9116-A777AE889F5D", "Network Monitor");
			chart1.Titles[0].Text = $"{chartTitlePrefix} ({monitor.IPAddress?.ToString()})";
			chart1.GetToolTipText += Chart1_GetToolTipText;
		}
		readonly NetworkMonitor monitor;

		void Chart1_GetToolTipText(object sender, ToolTipEventArgs e)
		{
			// For some reason this is required for tooltips to show up
		}

		protected override void OnResize(EventArgs e)
		{
			if (chart1 != null)
			{
				chart1.Location = ControlDpiScalingHelper.NewScaledPoint(12, 110, true);
				chart1.Size = ControlDpiScalingHelper.NewScaledSize(Math.Max(Size.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(24), 0), Math.Max(Size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(180), 0), false);
			}
			base.OnResize(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		void PingResponseEventHandler(object sender, EventArgs e)
		{
			if (IsDisposed)
			{ 
				return; 
			}

			if (IsHandleCreated)
			{
				if (InvokeRequired)
				{
					Invoke(new Action(() =>
					{
						chart1.Series[0].Points.DataBind(monitor.TcpPingResponsesList, "ElapsedSeconds", (NoResString)"Average", (NoResString)"Tooltip=Tooltip");
					}));
				}
				else
				{
					chart1.Series[0].Points.DataBind(monitor.TcpPingResponsesList, "ElapsedSeconds", "Average", "Tooltip=Tooltip");
				}
			}
		}

		void button1_Click(object sender, EventArgs e)
		{
			chart1.ChartAreas[0].Visible = true;
			chart1.ChartAreas[1].Visible = false;
			chart1.Legends[0].Enabled = true;
			chart1.Legends[1].Enabled = false;

			if (!monitor.IsRunning)
			{
				chart1.Series[0].Points.Clear();
				chart1.Series[0].Points.AddXY(0, 0);
				monitor.Start();
				button1.Text = Res.GetString("A8369FEA-D206-4E8A-BBEE-21EA020F6D45", "Stop");
				button2.Enabled = false;
			}
			else
			{
				monitor.Stop();
				button1.Text = Start;
				button2.Enabled = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No translation needed")]
		void button2_Click(object sender, EventArgs e)
		{
			chart1.ChartAreas[0].Visible = false;
			chart1.ChartAreas[1].Visible = true;
			chart1.Legends[0].Enabled = false;
			chart1.Legends[1].Enabled = true;
			button1.Enabled = false;

			var result = ThroughputTester.TestThroughput(radioButton1.Checked ? 1024 : 10240);
			chart1.Series[1].Points.Clear();

#if NET8_0_OR_GREATER // DataPoint is not IDisposable in .NET8
			{
				var uplinkPoint = new DataPoint();
#else
			using (var uplinkPoint = new DataPoint())
			{
#endif
				uplinkPoint.SetValueXY(1, result.UplinkSpeed);
				uplinkPoint.AxisLabel = Res.GetString("3B666F25-45FE-4FDE-BB03-FF1A9715D4DE", "Up-link Speed");
				uplinkPoint.ToolTip = string.Format(CultureInfo.CurrentCulture, "{0:N1} MB/s", result.UplinkSpeed);
				chart1.Series[1].Points.Add(uplinkPoint);
			}

#if NET8_0_OR_GREATER // DataPoint is not IDisposable in .NET8
			{
				var downlinkPoint = new DataPoint();
#else
			using (var downlinkPoint = new DataPoint())
			{
#endif
				downlinkPoint.SetValueXY(2, result.DownlinkSpeed);
				downlinkPoint.AxisLabel = Res.GetString("4EB4D954-06B7-4FDD-B8D7-5860E2237A18", "Down-link Speed");
				downlinkPoint.ToolTip = string.Format(CultureInfo.CurrentCulture, "{0:N1} MB/s", result.DownlinkSpeed);
				chart1.Series[1].Points.Add(downlinkPoint);
			}
			button1.Enabled = true;
		}

		static string Start => Res.GetString("F04D582F-8241-49E2-86B3-0440493D21A2", "Start");
	}
}
