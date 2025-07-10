using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.OCR
{
	public partial class OCRResultForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZTextBox ResultTextBox;

		new void InitializeComponent()
		{
			this.ResultTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 22, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(186);
			// 
			// ResultTextBox
			// 
			this.ResultTextBox.AcceptsReturn = true;
			this.ResultTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ResultTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ResultTextBox, false);
			this.ResultTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultTextBox.Multiline = true;
			this.ResultTextBox.Name = "ResultTextBox";
			this.ResultTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ResultTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 176, true);
			this.ResultTextBox.TabIndex = 1;
			this.ResultTextBox.WordWrap = false;
			// 
			// OCRResultForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 176, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("OCRResultForm|3fde91de-0cee-42c7-9b01-36e58a6a850c", "Converted Text");
			this.Controls.Add(this.ResultTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "OCRResultForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Controls.SetChildIndex(this.ResultTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
