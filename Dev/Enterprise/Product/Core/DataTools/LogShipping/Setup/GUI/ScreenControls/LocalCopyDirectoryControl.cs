#define SuppressResourceStringsCheckRegion

using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class LocalCopyDirectoryControl : UserAreaControl
	{
		public LocalCopyDirectoryControl()
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
				return controlsToEnableDisable = controlsToEnableDisable ??
					new Control[] { directoryTextBox, browseButton };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Standalone tool, no reference to ZArchitecture")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Standalone tool, no reference to ZArchitecture")]
		void browseButton_Click(object sender, EventArgs e)
		{
			ChangeStatus(TaskStatus.Setup);
			using (var folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						directoryTextBox.Text = folderBrowserDialog.SelectedPath;
					}
					catch (NotSupportedException ex)
					{
						ShowMessage(string.Format("Error: {0}.", ex.Message));
						ChangeStatus(TaskStatus.Failure);
					}
				}
			}
		}

		void LocalCopyDirectoryControl_VisibleChanged(object sender, EventArgs e)
		{
			LogShippingSetupForm parentForm = this.ParentForm as LogShippingSetupForm;
			if (this.Visible && parentForm != null && parentForm.NextStep)
			{
				directoryTextBox.Text = parentForm.Navigator.SetupInfo.BackupLocalCopyDirectory;
			}
		}
	}
}
