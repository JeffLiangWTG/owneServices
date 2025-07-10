using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	partial class IntercompanyChargeCodeMappingForm
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.ChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
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
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompanyCollection);
			// 
			// ChargeCodesGrid
			// 
			this.ChargeCodesGrid.AllowNavigation = false;
			this.ChargeCodesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeCodesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompany)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompany)(null)).YP_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompany)(null)).YP_TYPE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompany)(null)).YP_YG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompany)(null)).YP_OH_LocalClientOverride)));
			this.ChargeCodesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "YP_AC";
			zDropEditColumnStyleInfo1.ColumnName = "YP_TYPE";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "YP_YG";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "YP_OH_LocalClientOverride";
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargeCodesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ChargeCodesGrid.CopySelectedRowsAllowed = true;
			this.ChargeCodesGrid.GridId = "A28AB71C-B074-4A6A-8FDE-5EE5E41FD114";
			this.ChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodesGrid.LayoutKey = "DeptChargesGrid";
			this.ChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 6, true);
			this.ChargeCodesGrid.Name = "ChargeCodesGrid";
			this.ChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 142, true);
			this.ChargeCodesGrid.TabIndex = 0;
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyChargeCodeMappingForm|c41327ff-9840-4f80-b5f1-7781e354148f", "Cancel");
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 152, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPostingButton.TabIndex = 10;
			this.CancelPostingButton.UseVisualStyleBackColor = true;
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyChargeCodeMappingForm|cb9273bc-21a6-43d7-bb8c-16f2d75be87b", "Save And Close");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 152, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.SaveAndCloseButton.TabIndex = 9;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			// 
			// IntercompanyChargeCodeMappingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 205, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyChargeCodeMappingForm|bfcfad14-8e74-4ba2-9c83-317a7fa69f34", "Intercompany Charge Code Mapping");
			this.Controls.Add(this.CancelPostingButton);
			this.Controls.Add(this.SaveAndCloseButton);
			this.Controls.Add(this.ChargeCodesGrid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapPivotIntercompanyCollection);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 241, true);
			this.Name = "IntercompanyChargeCodeMappingForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ChargeCodesGrid, 0);
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
		private Enterprise.Core.Forms.ZPostOrCancelButton CancelPostingButton;
		private Enterprise.Core.Forms.ZPostOrCancelButton SaveAndCloseButton;
	}
}
