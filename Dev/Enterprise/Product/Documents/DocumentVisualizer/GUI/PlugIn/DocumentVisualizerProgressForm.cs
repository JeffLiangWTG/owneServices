using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class DocumentVisualizerProgressForm : ProgressForm, IProcessStatus
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040", Justification = "false positive")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1042", Justification = "false positive")]
		public DocumentVisualizerProgressForm()
		{
			ShowCancelButton = false;
			ShowProgressBar = true;

			ProgressBar.Value = 0;

			ProgressBar.Maximum = maxTimeInSeconds * numberOfStepsInOneSecond;
			ProgressBar.Step = 1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "no need to use TimeSpan")]
		int maxTimeInSeconds = 5;
		const int numberOfStepsInOneSecond = 4;

		Timer timer;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "no need to use TimeSpan")]
		public bool SetMaxTimeInSeconds(int numberOfSeconds)
		{
			if (numberOfSeconds >= 0)
			{
				maxTimeInSeconds = numberOfSeconds;
				return true;
			}

			return false;
		}

		protected override void OnLoad(EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				base.OnLoad(e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040", Justification = "false positive")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1042", Justification = "false positive")]
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			void TimerCallback(object state)
			{
				var nextProgressBarValue = ProgressBar.Value + 1;
				if (nextProgressBarValue < ProgressBar.Maximum)
				{
#if WINZOR
					ProgressBar.Invoke(() => ProgressBar.Value = nextProgressBarValue);
#else
					ProgressBar.Value = nextProgressBarValue;
#endif
				}
			}

			var timerStep = 1000 / numberOfStepsInOneSecond;
			timer = new Timer(TimerCallback, null, timerStep, timerStep);
		}

		public void UpdateStatus(string status, int progressValue) => ProgressLabel.Text = status;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				timer?.Dispose();
				timer = null;
			}

			base.Dispose(isNotFinalizing);
		}
	}
}
