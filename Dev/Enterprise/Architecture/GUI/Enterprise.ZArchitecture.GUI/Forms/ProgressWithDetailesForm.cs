using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ProgressWithDetailesForm : ProgressForm
	{
		public ProgressWithDetailesForm()
		{
			InitializeComponent();
		}

		public void AddLog(string message)
		{
			LogTextBox.AppendText(System.Environment.NewLine + message);
		}

		public void ResetLog()
		{
			LogTextBox.ResetText();
		}

		public string Log
		{
			get { return LogTextBox.Text; }
		}

		public event CancelEventHandler CancellingQuery;

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!IsCloseActivated)
			{
				e.Cancel = true;

				if (!IsCancelling)
				{
					var cancellingQueryArgs = new CancelEventArgs();
					CancellingQuery?.Invoke(this, cancellingQueryArgs);
					if (!cancellingQueryArgs.Cancel)
					{
						CancelProgressButton.Enabled = false;
						CancelProgressButton.Text = Res.GetString("1E8ED947-C60F-49FA-9F30-E772863AEEBE", "Canceling");
						IsCancelling = true;
					}
				}
			}

			base.OnClosing(e);
		}

		public void ActivateCloseButton()
		{
			CancelProgressButton.Enabled = true;
			CancelProgressButton.Text = Res.GetString("991a18db-eadc-47e5-9a94-fa3c58668286", "Close");
			IsCloseActivated = true;
			IsCancelling = false;
		}

		bool IsCancelling;
		bool IsCloseActivated;
	}
}
