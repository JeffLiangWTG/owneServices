using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GlobalChargeCodeOrganizationForm
	{
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.IsActiveCheckBox = new ZCheckBox();
			this.ChargeCodeMappingGrid = new ZGrid();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.GlobalCodeTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.SortByGroupBox = new ZGroupBox();
			this.OrganizationGuidFindBox = new ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeMappingGrid)).BeginInit();
			this.SortByGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 267, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 21, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GlobalChargeCodeMapOrganization);
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "YG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GlobalChargeCodeMapOrganization)(null)).YG_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 10, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.IsActiveCheckBox.TabIndex = 2;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChargeCodeMappingGrid
			// 
			this.ChargeCodeMappingGrid.AllowNavigation = false;
			this.ChargeCodeMappingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeCodeMappingGrid, "PivotCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GlobalChargeCodeMapOrganization)(null)).PivotCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((GlobalChargeCodeMapOrganization)(null)).PivotCollection)).SyncRoot)).YP_TYPE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((GlobalChargeCodeMapOrganization)(null)).PivotCollection)).SyncRoot)).YP_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotOrganization)(((System.Collections.IList)(((GlobalChargeCodeMapOrganization)(null)).PivotCollection)).SyncRoot)).YP_Description)));
			this.ChargeCodeMappingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "YP_TYPE";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "YP_AC";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlobalChargeCodeOrganizationForm|3337bb02-442e-4122-814c-1498ba3b33fc", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "YP_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodeMappingGrid.GridId = "4D9DFACA-E273-44C5-B2B6-65B3E0146A8A";
			this.ChargeCodeMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodeMappingGrid.LayoutKey = "APInvoicesGrid";
			this.ChargeCodeMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ChargeCodeMappingGrid.Name = "ChargeCodeMappingGrid";
			this.ChargeCodeMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 125, true);
			this.ChargeCodeMappingGrid.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 238, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// GlobalCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GlobalCodeTextBox, "YG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapOrganization)(null)).YG_Code)));
			this.GlobalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 7, true);
			this.GlobalCodeTextBox.Name = "GlobalCodeTextBox";
			this.GlobalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.GlobalCodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "YG_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapOrganization)(null)).YG_Desc)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 33, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// SortByGroupBox
			// 
			this.SortByGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SortByGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlobalChargeCodeOrganizationForm|270192c9-75d5-4a45-9979-70f0cafbcc48", "Charge Code Mapping");
			this.SortByGroupBox.Controls.Add(this.ChargeCodeMappingGrid);
			this.SortByGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 85, true);
			this.SortByGroupBox.Name = "SortByGroupBox";
			this.SortByGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 147, true);
			this.SortByGroupBox.TabIndex = 8;
			this.SortByGroupBox.TabStop = false;
			// 
			// OrganizationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrganizationGuidFindBox, "YG_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GlobalChargeCodeMapOrganization)(null)).YG_OH)));
			this.OrganizationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 59, true);
			this.OrganizationGuidFindBox.Name = "OrganizationGuidFindBox";
			this.OrganizationGuidFindBox.PopupCaption = null;
			this.OrganizationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 20, true);
			this.OrganizationGuidFindBox.TabIndex = 9;
			// 
			// GlobalChargeCodeOrganizationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 288, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlobalChargeCodeOrganizationForm|d774dd6c-23be-4b6b-b362-efefab4fea2f", "Organization Global Charge Code");
			this.Controls.Add(this.OrganizationGuidFindBox);
			this.Controls.Add(this.SortByGroupBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.GlobalCodeTextBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.IsActiveCheckBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business.GlobalChargeCode";
			this.DataSourceType = typeof(GlobalChargeCodeMapOrganization);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapOrganization";
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 324, true);
			this.Name = "GlobalChargeCodeOrganizationForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.GlobalCodeTextBox, 0);
			this.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.SortByGroupBox, 0);
			this.Controls.SetChildIndex(this.OrganizationGuidFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeMappingGrid)).EndInit();
			this.SortByGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
