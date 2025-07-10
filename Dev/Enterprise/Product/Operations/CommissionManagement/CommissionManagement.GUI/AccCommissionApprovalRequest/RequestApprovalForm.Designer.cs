namespace Enterprise.CommissionManagement.GUI
{
	partial class RequestApprovalForm
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
		new void InitializeComponent()
		{
			this.Staff1CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Staff2CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AuthorizationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IncludeSummaryAsEmailAttachmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Staff1CodeFindBox.SuspendLayout();
			this.Staff2CodeFindBox.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest);
			// 
			// Staff1CodeFindBox
			// 
			this.Staff1CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Staff1CodeFindBox, "CRQ_GS_NKApprovingStaff1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).CRQ_GS_NKApprovingStaff1)));
			this.Staff1CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 30, true);
			this.Staff1CodeFindBox.Name = "Staff1CodeFindBox";
			this.Staff1CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.Staff1CodeFindBox.TabIndex = 1;
			// 
			// Staff2CodeFindBox
			// 
			this.Staff2CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Staff2CodeFindBox, "CRQ_GS_NKApprovingStaff2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).CRQ_GS_NKApprovingStaff2)));
			this.Staff2CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 50, true);
			this.Staff2CodeFindBox.Name = "Staff2CodeFindBox";
			this.Staff2CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.Staff2CodeFindBox.TabIndex = 2;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("2c1b05aa-ded1-41c7-885f-174a5bcba6e3", "Send");
			this.SendButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 103, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.SendButton.TabIndex = 4;
			this.SendButton.Click += new System.EventHandler(this.PostButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("8efe8554-a34e-4e33-9ec0-3fba66d183d5", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 103, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// AuthorizationLabel
			// 
			this.AuthorizationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AuthorizationLabel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("5b78e524-0dd9-4c06-868a-c3f7dead9996", "Authorization Staff");
			this.AuthorizationLabel.IsFontBold = true;
			this.AuthorizationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.AuthorizationLabel.Name = "AuthorizationLabel";
			this.AuthorizationLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AuthorizationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 24, true);
			this.AuthorizationLabel.TabIndex = 0;
			// 
			// IncludeSummaryAsEmailAttachmentCheckBox
			// 
			this.IncludeSummaryAsEmailAttachmentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeSummaryAsEmailAttachmentCheckBox, "IncludeSummaryAsEmailAttachment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest)(null)).IncludeSummaryAsEmailAttachment)));
			this.IncludeSummaryAsEmailAttachmentCheckBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("a1d871bf-bb53-42e2-878a-959a53385aab", "Include Summary Document as attachment");
			this.IncludeSummaryAsEmailAttachmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeSummaryAsEmailAttachmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 77, true);
			this.IncludeSummaryAsEmailAttachmentCheckBox.Name = "IncludeSummaryAsEmailAttachmentCheckBox";
			this.IncludeSummaryAsEmailAttachmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.IncludeSummaryAsEmailAttachmentCheckBox.TabIndex = 3;
			this.IncludeSummaryAsEmailAttachmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// RequestApprovalForm
			// 
			this.AcceptButton = this.SendButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("d575378e-dda0-49bf-b3b0-fdd9b0922027", "Request Approval");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 153, true);
			this.Controls.Add(this.IncludeSummaryAsEmailAttachmentCheckBox);
			this.Controls.Add(this.AuthorizationLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.Staff2CodeFindBox);
			this.Controls.Add(this.Staff1CodeFindBox);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.AccCommissionApprovalRequest);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RequestApprovalForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Staff1CodeFindBox, 0);
			this.Controls.SetChildIndex(this.Staff2CodeFindBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.AuthorizationLabel, 0);
			this.Controls.SetChildIndex(this.IncludeSummaryAsEmailAttachmentCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Staff1CodeFindBox.ResumeLayout(true);
			this.Staff1CodeFindBox.PerformLayout();
			this.Staff2CodeFindBox.ResumeLayout(true);
			this.Staff2CodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox Staff1CodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox Staff2CodeFindBox;
		protected ZArchitecture.GUI.ZButton SendButton;
		protected ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.ZLabel AuthorizationLabel;
		private ZArchitecture.GUI.ZCheckBox IncludeSummaryAsEmailAttachmentCheckBox;

	}
}