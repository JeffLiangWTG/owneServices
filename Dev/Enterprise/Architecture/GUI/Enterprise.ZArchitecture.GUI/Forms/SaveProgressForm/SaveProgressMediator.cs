using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class SaveProgressMediator : ZProcessStatusFormManager<SaveProgressForm>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log for testing")]
		public SaveProgressMediator()
		{
			LogForTesting(1, "SaveProgressMediator constructed");
			InitialDelay = TimeSpan.FromSeconds(2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log for testing")]
		public void ShowModalProgressForm(Rectangle parentFormBounds, string initialStatus, int initialPercentComplete)
		{
			LogForTesting(1, "Request show form with status: '" + initialStatus + "' percentComplete: " + initialPercentComplete);
			this.parentFormBounds = parentFormBounds;
			Start();
			UpdateStatus(initialStatus, initialPercentComplete);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log for testing")]
		public void SetStatusAndPercentCompleteShowingFormIfItIsInvisible(string status, int percentComplete)
		{
			LogForTesting(1, "Request update status: '" + status + "' percentComplete: " + percentComplete);
			UpdateStatus(status, percentComplete);
			ShowForm();
		}

		protected override SaveProgressForm CreateFormCore()
		{
			form = new SaveProgressForm();
			form.Location = ControlDpiScalingHelper.NewScaledPoint(parentFormBounds.Left + (parentFormBounds.Width - form.Width) / 2, parentFormBounds.Top + (parentFormBounds.Height - form.Height) / 2, false);

#if DEBUG
			form.Shown += form_Shown;
			form.ProgressTextBox.TextChanged += ProgressTextBox_TextChanged;
			form.Disposed += form_Disposed;
#endif

			return form;
		}

		internal SaveProgressForm form;
		Rectangle parentFormBounds;

		#region Logger

#if DEBUG
		void form_Shown(object sender, EventArgs e)
		{
			LogForTesting(2, "Show form"); // Log for testing
		}

		void ProgressTextBox_TextChanged(object sender, EventArgs e)
		{
			LogForTesting(2, "Update status: '" + form.ProgressTextBox.Text + "' percentComplete: " + form.ProgressBar.Value); // Log for testing
			ProgressTextChanged.Set();
		}

		void form_Disposed(object sender, EventArgs e)
		{
			LogForTesting(2, "Dispose form"); // Log for testing
			FormDisposed.Set();
		}
#endif
		[Conditional("DEBUG")]
		void LogForTesting(int threadNum, string entry)
		{
#if DEBUG
			lock (TestingLog1)
			{
				if (TestLoggingEnabled)
				{
					if (threadNum == 1)
					{
						TestingLog1.Add(threadNum + ": " + entry);
					}
					else if (threadNum == 2)
					{
						TestingLog2.Add(threadNum + ": " + entry);
					}
					else
					{
						throw new NotSupportedException("unknown thread number");
					}
				}
			}
#endif
		}

#if DEBUG

		public class Logger : IDisposable
		{
			public Logger()
			{
				TestingLog1.Clear();
				TestingLog2.Clear();
				TestLoggingEnabled = true;
			}

			public StringCollectionX LogThread1
			{
				get { return TestingLog1; }
			}

			public StringCollectionX LogThread2
			{
				get { return TestingLog2; }
			}

			public void Dispose()
			{
				TestLoggingEnabled = false;
				TestingLog1.Clear();
				TestingLog2.Clear();
			}
		}

		[SuppressThreadStaticFieldMessage]
		static bool TestLoggingEnabled;

		[SuppressThreadStaticFieldMessage]
		static readonly StringCollectionX TestingLog1 = new StringCollectionX();
		[SuppressThreadStaticFieldMessage]
		static readonly StringCollectionX TestingLog2 = new StringCollectionX();

		public AutoResetEvent ProgressTextChanged = new AutoResetEvent(false);
		public AutoResetEvent FormDisposed = new AutoResetEvent(false);
#endif

		#endregion
	}
}
