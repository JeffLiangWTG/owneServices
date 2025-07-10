using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class SourceDirectoryControl : UserAreaControl
	{
		public SourceDirectoryControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get { return directoryTextBox.Text; }
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ?? new Control[] { directoryTextBox };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		void SourceDirectoryControl_VisibleChanged(object sender, EventArgs e)
		{
			LogShippingSetupForm parentForm = this.ParentForm as LogShippingSetupForm;
			if (this.Visible && parentForm != null && parentForm.NextStep)
			{
				directoryTextBox.Text = parentForm.Navigator.SetupInfo.BackupSourceDirectory;
			}
		}
	}
}
