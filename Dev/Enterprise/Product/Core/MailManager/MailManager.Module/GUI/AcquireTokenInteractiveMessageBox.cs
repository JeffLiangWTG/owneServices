using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Res = MailManager.Module.Res;

namespace Enterprise.MailManager.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class AcquireTokenInteractiveMessageBox : ZMessageBox
	{
		public AcquireTokenInteractiveMessageBox(string message, string code, string url)
			: base(message, "", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
		{
			Code = code;
			Url = url;
			SetupCopyButtons();
		}

		public string Code { get; private set; }
		public string Url { get; private set; }

		ZButton SignButton;

		void SetupCopyButtons()
		{
			var padding = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
			SignButton = new ZButton();
			SignButton.Text = Res.GetString("e1177fcd-f9ec-4d90-9918-d4b6407ad51a", "Sign into your account");
			SignButton.Click += new EventHandler(SignButton_Click);
			SignButton.Name = "SignButton";
			SignButton.AutoSize = true;
			SignButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			ControlDpiScalingHelper.SetTop(ref SignButton, Button1.Top, false);
			Controls.Add(SignButton);

			Button1.Anchor = AnchorStyles.Left | AnchorStyles.Top;

			ControlDpiScalingHelper.SetLeft(ref Button1, Button1.Left - Button1.Width - padding, false);
			ControlDpiScalingHelper.SetLeft(ref SignButton, Button1.Right + padding, false);
		}

		public void SignButton_Click(object sender, EventArgs e)
		{
			SafeClipboard.SetText(Code);
			WebUrlLauncher.Launch(Url);
		}
	}
}
