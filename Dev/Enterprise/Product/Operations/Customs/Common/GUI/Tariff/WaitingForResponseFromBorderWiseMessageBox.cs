using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Common.GUI
{
	public class WaitingForResponseFromBorderWiseMessageBox : ZMessageBox
	{
		public WaitingForResponseFromBorderWiseMessageBox(string okButtonText, string caption = "", string message = "")
			: base(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
		{
			TopMost = false;
			ShowInTaskbar = false;
			Button1.Text = okButtonText;
			MessageBoxButton1 = Button1;
		}

		public WaitingForResponseFromBorderWiseMessageBox(string okButtonText, string cancelButtonText, string caption = "", string message = "")
			: base(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
		{
			TopMost = false;
			ShowInTaskbar = false;

			ControlDpiScalingHelper.SetWidth(Button1, 200, true);
			ControlDpiScalingHelper.SetLeft(Button1, (ClientRectangle.Width - (Button1.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(6))) / 3, false);
			ControlDpiScalingHelper.SetLeft(Button2, Button1.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);

			Button1.Text = okButtonText;
			Button2.Text = cancelButtonText;

			MessageBoxButton1 = Button1;
			MessageBoxButton2 = Button2;
		}

		internal ZButton MessageBoxButton1 { get; private set; }
		internal ZButton MessageBoxButton2 { get; private set; }
	}
}
