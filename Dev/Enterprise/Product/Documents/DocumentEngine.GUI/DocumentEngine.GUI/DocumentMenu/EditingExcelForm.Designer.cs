using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	internal partial class EditingExcelForm : ZChildForm
	{
		private Enterprise.ZArchitecture.ZLabel EditingLabel;
		private Enterprise.ZArchitecture.GUI.ZButton OKBtn;

		new void InitializeComponent()
		{
			this.OKBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditingLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(141);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(141);
			// 
			// OKBtn
			// 
			this.OKBtn.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditingExcelForm|1a137f5b-a029-412c-ac41-00a0e8fda4df", "OK");
			this.OKBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 89, true);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKBtn.TabIndex = 0;
			this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
			// 
			// EditingLabel
			// 
			this.EditingLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditingExcelForm|688d0b23-207b-40b4-bf29-6c0d5f0d8ac1", "", "Editing Excel Template in progress. When you have finished editing the template. please save and close Excel and then press OK below.");
			this.EditingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.EditingLabel.Name = "EditingLabel";
			this.EditingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 56, true);
			this.EditingLabel.TabIndex = 1;
			// 
			// EditingExcelForm
			// 
			this.AcceptButton = this.OKBtn;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 151, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("EditingExcelForm|07160b5f-425d-488a-a9ea-faaa61a47803", "Editing Excel Template ...");
			this.Controls.Add(this.EditingLabel);
			this.Controls.Add(this.OKBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EditingExcelForm";
			this.Controls.SetChildIndex(this.OKBtn, 0);
			this.Controls.SetChildIndex(this.EditingLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
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
