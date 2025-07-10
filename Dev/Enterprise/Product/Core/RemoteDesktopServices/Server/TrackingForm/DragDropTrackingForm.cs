using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;

namespace Enterprise.RemoteDesktopServices.Server
{
	public partial class DragDropTrackingForm : Form
	{
		public DragDropTrackingForm()
		{
			InitializeComponent();

			TrackingInfoLogger.Instance.OnNewLog += Instance_OnNewLog;
			TrackingInfoLogger.Instance.OnShowAllDemand += Instance_OnShowAllDemand;
		}

		void RemoveEventHandlers()
		{
			TrackingInfoLogger.Instance.OnShowAllDemand -= Instance_OnShowAllDemand;
			TrackingInfoLogger.Instance.OnNewLog -= Instance_OnNewLog;
		}

		void Instance_OnShowAllDemand(object sender, EventArgs e)
		{
			this.BeginInvokeSafe(() => Visible = true);
		}

		void Instance_OnNewLog(object sender, string e)
		{
			this.BeginInvokeSafe(() =>
			{
				var newMessage = $"[{ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}] {e}";
				if (!string.IsNullOrWhiteSpace(zTextBox1.Text))
				{
					newMessage = System.Environment.NewLine + newMessage;
				}
				zTextBox1.AppendText(newMessage);
			});
		}

#if DEBUG
		public string GetTextBoxContent()
		{
			return zTextBox1.Text;
		}
#endif
	}
}
