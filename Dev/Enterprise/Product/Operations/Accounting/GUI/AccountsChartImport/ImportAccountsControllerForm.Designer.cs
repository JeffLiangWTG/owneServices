using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.ImportAccountsControllerForm
{
	partial class ImportAccountsControllerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
	   // private void InitializeComponent()
		protected override void  InitializeComponent()
		{
			this.DeleteGLHeadersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteChargeCodesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportAccountsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportAlternateGLAccountsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 162, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127);
			// 
			// DeleteGLHeadersButton
			// 
			this.DeleteGLHeadersButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportAccountsControllerForm|b74ec2c3-917b-413f-8b2f-2e145a7cdef0", "Delete All GL Accounts");
			this.DeleteGLHeadersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 49, true);
			this.DeleteGLHeadersButton.Name = "DeleteGLHeadersButton";
			this.DeleteGLHeadersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.DeleteGLHeadersButton.TabIndex = 2;
			this.DeleteGLHeadersButton.ToolTipCaption = null;
			this.DeleteGLHeadersButton.UseVisualStyleBackColor = true;
			this.DeleteGLHeadersButton.Click += new System.EventHandler(this.DeleteGLHeadersButton_Click);
			// 
			// DeleteChargeCodesButton
			// 
			this.DeleteChargeCodesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportAccountsControllerForm|24d697ee-f9ae-498c-aa58-46b2b2f79e72", "Delete All Charge Codes");
			this.DeleteChargeCodesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 12, true);
			this.DeleteChargeCodesButton.Name = "DeleteChargeCodesButton";
			this.DeleteChargeCodesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.DeleteChargeCodesButton.TabIndex = 1;
			this.DeleteChargeCodesButton.ToolTipCaption = null;
			this.DeleteChargeCodesButton.UseVisualStyleBackColor = true;
			this.DeleteChargeCodesButton.Click += new System.EventHandler(this.DeleteChargeCodesButton_Click);
			// 
			// ImportAccountsButton
			// 
			this.ImportAccountsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportAccountsControllerForm|154b00f4-b212-4592-aba9-ad652079e985", "Import CSV Accounts Chart");
			this.ImportAccountsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 85, true);
			this.ImportAccountsButton.Name = "ImportAccountsButton";
			this.ImportAccountsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 26, true);
			this.ImportAccountsButton.TabIndex = 3;
			this.ImportAccountsButton.ToolTipCaption = null;
			this.ImportAccountsButton.UseVisualStyleBackColor = true;
			this.ImportAccountsButton.Click += new System.EventHandler(this.ImportAccountsButton_Click);
			// 
			// ImportAlternateGLAccountsButton
			// 
			this.ImportAlternateGLAccountsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0088AB72-1A5A-4DD1-BA59-79A8A04BF816", "Import CSV Alternate GL Accounts");
			this.ImportAlternateGLAccountsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 122, true);
			this.ImportAlternateGLAccountsButton.Name = "ImportAlternateGLAccountsButton";
			this.ImportAlternateGLAccountsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 26, true);
			this.ImportAlternateGLAccountsButton.TabIndex = 4;
			this.ImportAlternateGLAccountsButton.ToolTipCaption = null;
			this.ImportAlternateGLAccountsButton.UseVisualStyleBackColor = true;
			this.ImportAlternateGLAccountsButton.Click += new System.EventHandler(this.ImportAlternateGLAccountsButton_Click);
			this.ImportAlternateGLAccountsButton.Visible = AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value;
			// 
			// ImportAccountsControllerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ImportAccountsControllerForm|4d0c3225-e564-4559-b3b8-aacba0fd7581", "Import Chart of Accounts");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 186, true);
			this.Controls.Add(this.ImportAlternateGLAccountsButton);
			this.Controls.Add(this.DeleteChargeCodesButton);
			this.Controls.Add(this.ImportAccountsButton);
			this.Controls.Add(this.DeleteGLHeadersButton);
			this.Name = "ImportAccountsControllerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DeleteGLHeadersButton, 0);
			this.Controls.SetChildIndex(this.ImportAccountsButton, 0);
			this.Controls.SetChildIndex(this.DeleteChargeCodesButton, 0);
			this.Controls.SetChildIndex(this.ImportAlternateGLAccountsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton DeleteGLHeadersButton;
		private Enterprise.ZArchitecture.GUI.ZButton DeleteChargeCodesButton;
		private Enterprise.ZArchitecture.GUI.ZButton ImportAccountsButton;
		private ZArchitecture.GUI.ZButton ImportAlternateGLAccountsButton;
	}
}
