namespace Enterprise.DocumentEngine.GUI.Registry
{
	using System;
	using System.Windows.Forms;
	using Enterprise.Registry.GUI;

	public partial class LogDocumentRendererRegistryControl : RegistryZUserControl
	{
		public LogDocumentRendererRegistryControl()
		{
			InitializeComponent();

#if WINZOR
			chooseButton.Enabled = false;
#endif
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			logFilePathTextBox.ReadOnly = readOnly;
			generateCallStacksCheckBox.ReadOnly = readOnly;
			clearButton.ReadOnly = readOnly;
			chooseButton.ReadOnly = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Diagnostics only tool does not work with Remote Desktop Services")]
		void HandleChooseButtonClick(object sender, EventArgs e)
		{
#if !WINZOR
			using (var dialog = new SaveFileDialog())
			{
				dialog.Title = Res.GetString("65ba11b7-86f3-49b0-b81c-2b3c4890accc", "Choose Log File Location");
				dialog.CheckPathExists = true;
				dialog.CreatePrompt = true;
				dialog.Filter = Res.GetString("b1e0f092-3389-42f0-8c7c-1b53adeedfd9", "Log File (*.log)|*.log");

				if (dialog.ShowDialog() != DialogResult.Cancel)
				{
					logFilePathTextBox.Text = dialog.FileName;
					logFilePathTextBox.Focus();
				}
			}
#endif
		}

		void HandleClearButtonClick(object sender, EventArgs e)
		{
			logFilePathTextBox.Clear();
			logFilePathTextBox.Focus();
		}
	}
}
