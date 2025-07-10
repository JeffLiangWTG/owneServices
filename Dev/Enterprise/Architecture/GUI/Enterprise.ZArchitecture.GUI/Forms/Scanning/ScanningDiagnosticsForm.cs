using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;

namespace Enterprise.ZArchitecture.GUI.Scanning
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ScanningDiagnosticsForm : Form // This is a Scanning diagnostics form that relies on minimal processing of Key events.
	{
		public ScanningDiagnosticsForm()
		{
			InitializeComponent();
			UpdateCopyButtonEnabled();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			var args = new KeyEventArgs(keyData);
			HandleKey(args);

			return true; // handled
		}

		void HandleKey(KeyEventArgs e)
		{
			// prevent a scan of Alt-F4, Escape etc. from closing the form mid-scan.
			e.Handled = true;
			e.SuppressKeyPress = true;

			OutputTextBox.Text += e.KeyData + "\r\n";
			OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
			OutputTextBox.ScrollToCaret();
			UpdateCopyButtonEnabled();
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			OutputTextBox.ResetText();
			UpdateCopyButtonEnabled();
		}

		void CopyButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(OutputTextBox.Text))
			{
				SafeClipboard.Clear();
			}
			else
			{
				SafeClipboard.SetText(OutputTextBox.Text);
			}
		}

		void UpdateCopyButtonEnabled()
		{
			CopyButton.Enabled = !string.IsNullOrEmpty(OutputTextBox.Text);
		}
	}
}
