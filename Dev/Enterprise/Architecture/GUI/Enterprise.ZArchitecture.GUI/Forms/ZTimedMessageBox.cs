using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[CodeAlive("It will be used for counting down.")]
	public partial class ZTimedMessageBox : ZMessageBox
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public ZTimedMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, int seconds)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1, seconds)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public ZTimedMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, int seconds)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1, seconds)
		{
			Button1.Text = button1Text;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public ZTimedMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, int seconds)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitialiseMessageBox(seconds);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public ZTimedMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, int seconds)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1, seconds)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public ZTimedMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, int seconds)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitialiseMessageBox(seconds);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void InitialiseMessageBox(int seconds)
		{
			countdownSeconds = seconds;
			InitializeComponent();
			countdownLabel.Text = CountdownMessage;
			SetCountdownLabelHeightWidth();
			timer.Start();
		}

		void SetCountdownLabelHeightWidth()
		{
			var textSize = TextRenderer.MeasureText(countdownLabel.Text, countdownLabel.Font);

			if (ClientRectangle.Width < textSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(16))
			{
				ControlDpiScalingHelper.SetWidth(this, textSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(16) + (Width - ClientRectangle.Width), false);
				UpdateButtonsLocation();
			}

			countdownLabel.Location = ControlDpiScalingHelper.NewScaledPoint((ClientRectangle.Width - textSize.Width) / 2, Button1.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
		}

		void UpdateButtonsLocation()
		{
			switch (Buttons)
			{
				case MessageBoxButtons.OK:
					UpdateOneButtonLocation();
					break;
				case MessageBoxButtons.OKCancel:
				case MessageBoxButtons.RetryCancel:
				case MessageBoxButtons.YesNo:
					UpdateTwoButtonsLocation();
					break;
				case MessageBoxButtons.YesNoCancel:
				case MessageBoxButtons.AbortRetryIgnore:
					UpdateThreeButtonsLocation();
					break;
			}
		}

		void UpdateOneButtonLocation()
		{
			if (ClientRectangle.Width < Button1.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20))
			{
				ControlDpiScalingHelper.SetWidth(this, ControlDpiScalingHelper.ScaleToCurrentDpiX(20) + Button1.Width + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(ref Button1, (ClientRectangle.Width - Button1.Width) / 2, false);
		}

		void UpdateTwoButtonsLocation()
		{
			if (ClientRectangle.Width < Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(26))
			{
				ControlDpiScalingHelper.SetWidth(this, Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(26) + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(Button1, (ClientRectangle.Width - (Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(6))) / 2, false);
			ControlDpiScalingHelper.SetLeft(Button2, Button1.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
		}
		void UpdateThreeButtonsLocation()
		{
			if (ClientRectangle.Width < Button1.Width * 3 + ControlDpiScalingHelper.ScaleToCurrentDpiX(32))
			{
				ControlDpiScalingHelper.SetWidth(this, ControlDpiScalingHelper.ScaleToCurrentDpiX(32) + Button1.Width * 3 + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(Button1, (ClientRectangle.Width - (Button1.Width * 3 + ControlDpiScalingHelper.ScaleToCurrentDpiX(12))) / 2, false);
			ControlDpiScalingHelper.SetLeft(Button2, Button1.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
			ControlDpiScalingHelper.SetLeft(Button3, Button2.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
		}

		void TimerTick_Event(object sender, EventArgs e)
		{
			if (countdownSeconds == 0)
			{
				Close();
			}
			else
			{
				countdownSeconds--;
				countdownLabel.Text = CountdownMessage;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			timer.Stop();

			base.OnClosing(e);
		}

		int countdownSeconds;

		string CountdownMessage => countdownSeconds > 1 ? Res.GetString("294AE47A-4846-424F-9F2A-27A0C3E9162A", "This form will close in {0} seconds", countdownSeconds)
			: Res.GetString("E794E0C4-3AB0-485B-B8B5-AB01C5B374D3", "This form will close in {0} second", countdownSeconds);
	}
}
