using System;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class SelfLogoffForm : ZChildForm
	{
		public SelfLogoffForm()
		{
			InitializeComponent();
			Text = Constants.ProductName;

			SetText();

			if (!DesignMode)
			{
				timer.Start();
			}
		}

		public string Message
		{
			get { return messageLabel.Text; }
			set { messageLabel.Text = value; }
		}

		public const int LogoffDelayInSeconds = 60;

		int seconds = LogoffDelayInSeconds;

		void SetText()
		{
			this.label.Text = Res.GetString("6BA59D2D-F4D3-4a6d-AAEC-C45D88AEB2DC", "{0} will shutdown in {1} seconds...", Constants.ProductName, seconds);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			seconds--;
			if (seconds <= 0)
			{
				timer.Stop();
				this.Close();
			}
			else
			{
				SetText();
			}
		}

		void exitButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
