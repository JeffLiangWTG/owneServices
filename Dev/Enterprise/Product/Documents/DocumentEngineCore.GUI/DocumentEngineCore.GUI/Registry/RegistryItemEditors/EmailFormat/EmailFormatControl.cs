using System;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	internal partial class EmailFormatControl : RegistryBusinessObjectTemplateZUserControl
	{
		public EmailFormatControl()
		{
			InitializeComponent();
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			DisclaimerLabel.Text = DisclaimerLabel.CaptionResourceString.Caption + string.Format(" ({0}/{1})", DisclaimerTextBox.TextLength, DisclaimerTextBox.MaxLength);
			DisclaimerLabel.PerformLayout();
		}
	}
}
