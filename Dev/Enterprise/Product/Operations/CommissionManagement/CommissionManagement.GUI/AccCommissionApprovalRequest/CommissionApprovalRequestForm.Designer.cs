namespace Enterprise.CommissionManagement.GUI
{
	partial class CommissionApprovalRequestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProcessPaymentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApprovingStaffPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApprovingStaffGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CRQ_GS_NKApprovingStaff1CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CRQ_GS_NKApprovingStaff2CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ApproveButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApproveButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApproveStatus2DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ApproveStatus1DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ItemGridsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.ApprovingStaffPanel.SuspendLayout();
			this.ApprovingStaffGroupBox.SuspendLayout();
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.SuspendLayout();
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 438, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.CloseButton);
			this.bottomPanel.Controls.Add(this.ProcessPaymentButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 408, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 30, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("908f5ee5-6559-4911-88a6-59e423ba386d", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 4, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ProcessPaymentButton
			// 
			this.ProcessPaymentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessPaymentButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("eae11a2e-88eb-4ab8-8330-07915931618b", "Process Payment");
			this.ProcessPaymentButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ProcessPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 4, true);
			this.ProcessPaymentButton.Name = "ProcessPaymentButton";
			this.ProcessPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.ProcessPaymentButton.TabIndex = 0;
			this.ProcessPaymentButton.Click += new System.EventHandler(this.ProcessPaymentButton_Click);
			// 
			// ApprovingStaffPanel
			// 
			this.ApprovingStaffPanel.Controls.Add(this.ApprovingStaffGroupBox);
			this.ApprovingStaffPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ApprovingStaffPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 319, true);
			this.ApprovingStaffPanel.Name = "ApprovingStaffPanel";
			this.ApprovingStaffPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 89, true);
			this.ApprovingStaffPanel.TabIndex = 4;
			// 
			// ApprovingStaffGroupBox
			// 
			this.ApprovingStaffGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("fbedfc22-9158-4a29-b834-e8bc02032308", "Authorization");
			this.ApprovingStaffGroupBox.Controls.Add(this.StatusLabel);
			this.ApprovingStaffGroupBox.Controls.Add(this.CRQ_GS_NKApprovingStaff1CodeFindBox);
			this.ApprovingStaffGroupBox.Controls.Add(this.CRQ_GS_NKApprovingStaff2CodeFindBox);
			this.ApprovingStaffGroupBox.Controls.Add(this.ApproveButton1);
			this.ApprovingStaffGroupBox.Controls.Add(this.ApproveButton2);
			this.ApprovingStaffGroupBox.Controls.Add(this.ApproveStatus2DescriptionLabel);
			this.ApprovingStaffGroupBox.Controls.Add(this.ApproveStatus1DescriptionLabel);
			this.ApprovingStaffGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovingStaffGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApprovingStaffGroupBox.Name = "ApprovingStaffGroupBox";
			this.ApprovingStaffGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 89, true);
			this.ApprovingStaffGroupBox.TabIndex = 0;
			this.ApprovingStaffGroupBox.TabStop = false;
			// 
			// StatusLabel
			// 
			this.StatusLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("af427117-2e73-42a9-be15-a936a5bfab6a", "Status");
			this.StatusLabel.IsFontBold = true;
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 13, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.StatusLabel.TabIndex = 0;
			this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CRQ_GS_NKApprovingStaff1CodeFindBox
			// 
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CRQ_GS_NKApprovingStaff1CodeFindBox, "CRQ_GS_NKApprovingStaff1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).CRQ_GS_NKApprovingStaff1)));
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 32, true);
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.Name = "CRQ_GS_NKApprovingStaff1CodeFindBox";
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.TabIndex = 1;
			// 
			// CRQ_GS_NKApprovingStaff2CodeFindBox
			// 
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CRQ_GS_NKApprovingStaff2CodeFindBox, "CRQ_GS_NKApprovingStaff2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).CRQ_GS_NKApprovingStaff2)));
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 57, true);
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.Name = "CRQ_GS_NKApprovingStaff2CodeFindBox";
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.TabIndex = 4;
			// 
			// ApproveButton1
			// 
			this.ApproveButton1.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("320a980d-3c76-45c8-8a2e-195155eeb1da", "Approve");
			this.ApproveButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 29, true);
			this.ApproveButton1.Name = "ApproveButton1";
			this.ApproveButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.ApproveButton1.TabIndex = 3;
			this.ApproveButton1.Click += new System.EventHandler(this.ApproveButton1_Click);
			// 
			// ApproveButton2
			// 
			this.ApproveButton2.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("6da08baf-c4b6-4028-898b-16796f24fd02", "Approve");
			this.ApproveButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 53, true);
			this.ApproveButton2.Name = "ApproveButton2";
			this.ApproveButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ApproveButton2.TabIndex = 6;
			this.ApproveButton2.Click += new System.EventHandler(this.ApproveButton2_Click);
			// 
			// ApproveStatus2DescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.ApproveStatus2DescriptionLabel, "ApproveStatus2Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).ApproveStatus2Description)));
			this.ApproveStatus2DescriptionLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("670052e5-93ab-4168-826e-d7a6dad7f943", "Status 2");
			this.ApproveStatus2DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 56, true);
			this.ApproveStatus2DescriptionLabel.Name = "ApproveStatus2DescriptionLabel";
			this.ApproveStatus2DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 18, true);
			this.ApproveStatus2DescriptionLabel.TabIndex = 5;
			// 
			// ApproveStatus1DescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.ApproveStatus1DescriptionLabel, "ApproveStatus1Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).ApproveStatus1Description)));
			this.ApproveStatus1DescriptionLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("1623c16f-8d32-433a-8084-05d70330658d", "Status 1");
			this.ApproveStatus1DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 31, true);
			this.ApproveStatus1DescriptionLabel.Name = "ApproveStatus1DescriptionLabel";
			this.ApproveStatus1DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 18, true);
			this.ApproveStatus1DescriptionLabel.TabIndex = 2;
			// 
			// ItemGridsPanel
			// 
			this.ItemGridsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemGridsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ItemGridsPanel.Name = "ItemGridsPanel";
			this.ItemGridsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 316, true);
			this.ItemGridsPanel.TabIndex = 0;
			// 
			// CommissionApprovalRequestForm
			// 
			this.AcceptButton = this.ProcessPaymentButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("00b77af4-b33a-40fb-8def-09d4bf0e0abc", "Commission Approval Request");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 465, true);
			this.Controls.Add(this.ItemGridsPanel);
			this.Controls.Add(this.ApprovingStaffPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 500, true);
			this.Name = "CommissionApprovalRequestForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.ApprovingStaffPanel, 0);
			this.Controls.SetChildIndex(this.ItemGridsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ApprovingStaffPanel.ResumeLayout(false);
			this.ApprovingStaffPanel.PerformLayout();
			this.ApprovingStaffGroupBox.ResumeLayout(false);
			this.ApprovingStaffGroupBox.PerformLayout();
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.ResumeLayout(true);
			this.CRQ_GS_NKApprovingStaff1CodeFindBox.PerformLayout();
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.ResumeLayout(true);
			this.CRQ_GS_NKApprovingStaff2CodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel bottomPanel;
		protected ZArchitecture.GUI.ZButton ProcessPaymentButton;
		private ZArchitecture.GUI.ZPanel ApprovingStaffPanel;
		private ZArchitecture.GUI.ZGroupBox ApprovingStaffGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CRQ_GS_NKApprovingStaff1CodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CRQ_GS_NKApprovingStaff2CodeFindBox;
		private ZArchitecture.ZLabel StatusLabel;
		protected ZArchitecture.GUI.ZButton ApproveButton1;
		protected ZArchitecture.GUI.ZButton ApproveButton2;
		private ZArchitecture.ZLabel ApproveStatus1DescriptionLabel;
		private ZArchitecture.ZLabel ApproveStatus2DescriptionLabel;
		protected ZArchitecture.GUI.ZButton CloseButton;
		protected ZArchitecture.GUI.ZPanel ItemGridsPanel;
	}
}