using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AssignClaimPopupForm
	{

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
		protected ZArchitecture.ZTextBox CommentTextBox;
		private readonly System.ComponentModel.IContainer components = null;
		private ZGuidFindBox BranchFindBox;
		private ZButton OKButton;
		private ZCodeFindBox StaffMemberFindbox;
		private ZButton CancelButtonX;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			this.CommentTextBox = new ZArchitecture.ZTextBox();
			this.BranchFindBox = new ZGuidFindBox();
			this.OKButton = new ZButton();
			this.CancelButtonX = new ZButton();
			this.StaffMemberFindbox = new ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 255, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 24, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccQueryClaimReassignAction);
			// 
			// CommentTextBox
			// 
			this.CommentTextBox.AcceptsReturn = true;
			this.CommentTextBox.AcceptsTab = true;
			this.CommentTextBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommentTextBox, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaimReassignAction)(null)).Comment)));
			this.CommentTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|979897b0-568e-4311-bd07-33eb5c58bbd3", "Comment");
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 87, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 133, true);
			this.CommentTextBox.TabIndex = 2;
			// 
			// BranchFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchFindBox, "BranchPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaimReassignAction)(null)).BranchPK)));
			this.BranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|b59ec0c1-b218-48ac-bc4f-e938b7d96496", "Branch");
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.BranchFindBox.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|48cc863f-b774-4353-87b9-31d2db28c9e4", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 226, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|b6abc783-58fe-4065-b814-56fab7870580", "Cancel");
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 226, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 4;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new EventHandler(this.CancelButton_Click);
			// 
			// StaffMemberFindbox
			// 
			this.BindingSource.SetBindingMember(this.StaffMemberFindbox, "StaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaimReassignAction)(null)).StaffCode)));
			this.StaffMemberFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|35e73d80-5f9e-48c4-9f32-fdf450e588e0", "Staff Member");
			this.StaffMemberFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 38, true);
			this.StaffMemberFindbox.Name = "StaffMemberFindbox";
			this.StaffMemberFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.StaffMemberFindbox.TabIndex = 1;
			// 
			// AssignClaimPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 279, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AssignClaimPopupForm|db321e8c-abca-4e96-ae81-5a824dab4754", "Reassign Claim");
			this.Controls.Add(this.StaffMemberFindbox);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.BranchFindBox);
			this.Controls.Add(this.CommentTextBox);
			this.DataSourceType = typeof(AccQueryClaimReassignAction);
			this.Name = "AssignClaimPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CommentTextBox, 0);
			this.Controls.SetChildIndex(this.BranchFindBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.StaffMemberFindbox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
