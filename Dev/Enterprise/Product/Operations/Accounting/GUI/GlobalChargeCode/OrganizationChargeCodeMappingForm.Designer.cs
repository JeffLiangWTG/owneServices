using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	partial class OrganizationChargeCodeMappingForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OrganizationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CancelPostingButton = new Enterprise.Core.Forms.ZPostOrCancelButton();
			this.SaveAndCloseButton = new Enterprise.Core.Forms.ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping);
			// 
			// ChargeCodesGrid
			// 
			this.ChargeCodesGrid.AllowNavigation = false;
			this.ChargeCodesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeCodesGrid, "ChargeCodePivotCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping)(null)).ChargeCodePivotCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping)(null)).ChargeCodePivotCollection)).SyncRoot)).YP_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping)(null)).ChargeCodePivotCollection)).SyncRoot)).YP_TYPE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping)(null)).ChargeCodePivotCollection)).SyncRoot)).YP_YG)));
			this.ChargeCodesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo3.ColumnName = "YP_AC";
			zDropEditColumnStyleInfo2.ColumnName = "YP_TYPE";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "YP_YG";
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ChargeCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ChargeCodesGrid.GridId = "F418DF59-E942-40F9-AC2A-03B5720435CE";
			this.ChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodesGrid.LayoutKey = "DeptChargesGrid";
			this.ChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 35, true);
			this.ChargeCodesGrid.Name = "ChargeCodesGrid";
			this.ChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 142, true);
			this.ChargeCodesGrid.TabIndex = 1;
			// 
			// OrganizationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrganizationGuidFindBox, "Organization_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping)(null)).Organization_PK)));
			this.OrganizationGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrganizationChargeCodeMappingForm|00b172c0-0ccf-4cc4-b1f4-ca1f75c76965", "Organization");
			this.OrganizationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 9, true);
			this.OrganizationGuidFindBox.Name = "OrganizationGuidFindBox";
			this.OrganizationGuidFindBox.PopupCaption = null;
			this.OrganizationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.OrganizationGuidFindBox.TabIndex = 0;
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrganizationChargeCodeMappingForm|c41327ff-9840-4f80-b5f1-7781e354148f", "Cancel");
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 181, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPostingButton.TabIndex = 10;
			this.CancelPostingButton.UseVisualStyleBackColor = true;
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrganizationChargeCodeMappingForm|cb9273bc-21a6-43d7-bb8c-16f2d75be87b", "Save And Close");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 181, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.SaveAndCloseButton.TabIndex = 9;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			// 
			// OrganizationChargeCodeMappingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 234, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrganizationChargeCodeMappingForm|bfcfad14-8e74-4ba2-9c83-317a7fa69f34", "Organization Charge Code Mapping");
			this.Controls.Add(this.CancelPostingButton);
			this.Controls.Add(this.SaveAndCloseButton);
			this.Controls.Add(this.OrganizationGuidFindBox);
			this.Controls.Add(this.ChargeCodesGrid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 270, true);
			this.Name = "OrganizationChargeCodeMappingForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ChargeCodesGrid, 0);
			this.Controls.SetChildIndex(this.OrganizationGuidFindBox, 0);
			this.Controls.SetChildIndex(this.SaveAndCloseButton, 0);
			this.Controls.SetChildIndex(this.CancelPostingButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private Enterprise.ZArchitecture.ZGrid ChargeCodesGrid;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox OrganizationGuidFindBox;
		private Enterprise.Core.Forms.ZPostOrCancelButton CancelPostingButton;
		private Enterprise.Core.Forms.ZPostOrCancelButton SaveAndCloseButton;
	}
}
