using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class CountdownMessageBox : ZMessageBox
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public CountdownMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, int seconds)
			: base(message, caption, buttons, icon)
		{
			InitializeForm(seconds);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public CountdownMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, int seconds)
			: base(message, caption, buttons, icon)
		{
			InitializeForm(seconds);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void InitializeForm(int seconds)
		{
			this.Seconds = seconds;
			InitializeComponent();
			InitializeProgressBarAndTimer();
			Button3.Text = Res.GetString("21d8db63-ae65-4214-b391-85cf3f3fb2de", "Refresh");
		}

		Timer timer;
		new IContainer components;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		int Seconds;

		void InitializeProgressBarAndTimer()
		{
			progressBar.Maximum = Seconds;
			progressBar.Value = Seconds;

			timer.Start();
		}

		void UpdateMessageAndProgressBar()
		{
			CountdownLabel.Text = Res.GetString("0e6a208d-49bd-489f-8e93-c4a8d9ad2161", "This form will close in {0} seconds.", Seconds);
			progressBar.Value--;
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if (Seconds == 0)
			{
				timer.Stop();
				Close();
				DialogResult = DialogResult.Cancel;
			}
			else
			{
				Seconds--;
				UpdateMessageAndProgressBar();
			}
		}
	}
}
