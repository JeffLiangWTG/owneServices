using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class UserAreaControl : UserControl
	{
		public UserAreaControl()
		{
			InitializeComponent();
		}

		public void ShowMessage(string message)
		{
			outputTextBox.AppendText(string.Format("[{0}] {1}\r\n", DateTime.Now.ToLongTimeString(), message));
			outputTextBox.ScrollToCaret();
		}

		public virtual object Value
		{
			get { return string.Empty; }
		}

		public void EnableControls()
		{
			foreach (Control control in ControlsToEnableDisable)
			{
				if (control != null)
				{
					control.Enabled = true;
				}
			}
		}

		public void DisableControls()
		{
			foreach (Control control in ControlsToEnableDisable)
			{
				if (control != null)
				{
					control.Enabled = false;
				}
			}
		}

		protected virtual Control[] ControlsToEnableDisable
		{
			get
			{
				return Array.Empty<Control>();
			}
		}

		protected void ChangeStatus(TaskStatus status)
		{
			var parentForm = Parent as LogShippingSetupForm;

			if (parentForm != null)
			{
				parentForm.SetCurrentScreenStatus(status);
			}
		}

		public void ClearOutput()
		{
			outputTextBox.Text = "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected const string CannotGetInstancesMessageRaw = "Warning: Cannot get list of SQL Server instances. Please enter it manually or leave it empty for the default one. \r\nReason: {0}";
	}
}
