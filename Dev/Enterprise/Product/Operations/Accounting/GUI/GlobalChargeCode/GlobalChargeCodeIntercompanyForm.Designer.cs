using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GlobalChargeCodeIntercompanyForm
	{


		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.IsActiveCheckBox = new ZCheckBox();
			this.ChargeCodeMappingGrid = new ZGrid();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.GlobalCodeTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.zTabControl1 = new ZTabControl();
			this.zTabPageGeneric = new ZTabPage();
			this.zTabPageLocalClientOverride = new ZTabPage();
			this.chargeCodeMappingWithLocalClientOverrideGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeMappingGrid)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.zTabPageGeneric.SuspendLayout();
			this.zTabPageLocalClientOverride.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chargeCodeMappingWithLocalClientOverrideGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
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
			this.BindingSource.DataSourceType = typeof(GlobalChargeCodeMapIntercompany);
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "YG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GlobalChargeCodeMapIntercompany)(null)).YG_IsActive)));
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
			this.BindingSource.SetBindingMember(this.ChargeCodeMappingGrid, "PivotWithoutOverrideLocalClientCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithoutOverrideLocalClientCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithoutOverrideLocalClientCollection)).SyncRoot)).YP_TYPE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithoutOverrideLocalClientCollection)).SyncRoot)).YP_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithoutOverrideLocalClientCollection)).SyncRoot)).YP_Description)));
			this.ChargeCodeMappingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "YP_TYPE";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "YP_AC";
			zTextBoxColumnStyleInfo1.ColumnName = "YP_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodeMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodeMappingGrid.CopySelectedRowsAllowed = true;
			this.ChargeCodeMappingGrid.GridId = "C763A712-82F4-4122-8A55-B0163A069543";
			this.ChargeCodeMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodeMappingGrid.LayoutKey = "APInvoicesGrid";
			this.ChargeCodeMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.ChargeCodeMappingGrid.Name = "ChargeCodeMappingGrid";
			this.ChargeCodeMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 130, true);
			this.ChargeCodeMappingGrid.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 234, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// GlobalCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GlobalCodeTextBox, "YG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapIntercompany)(null)).YG_Code)));
			this.GlobalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 7, true);
			this.GlobalCodeTextBox.Name = "GlobalCodeTextBox";
			this.GlobalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.GlobalCodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "YG_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapIntercompany)(null)).YG_Desc)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 33, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.zTabPageGeneric);
			this.zTabControl1.Controls.Add(this.zTabPageLocalClientOverride);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 59, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 169, true);
			this.zTabControl1.TabIndex = 9;
			// 
			// zTabPageGeneric
			// 
			this.zTabPageGeneric.Controls.Add(this.ChargeCodeMappingGrid);
			this.zTabPageGeneric.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPageGeneric.Name = "zTabPageGeneric";
			this.zTabPageGeneric.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPageGeneric.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 142, true);
			this.zTabPageGeneric.TabIndex = 0;
			this.zTabPageGeneric.Text = Res.GetString("GlobalChargeCodeIntercompanyForm|351C3D79-B947-4A75-8A40-43373386DBB0", "Generic");
			this.zTabPageGeneric.UseVisualStyleBackColor = true;
			// 
			// zTabPageLocalClientOverride
			// 
			this.zTabPageLocalClientOverride.Controls.Add(this.chargeCodeMappingWithLocalClientOverrideGrid);
			this.zTabPageLocalClientOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPageLocalClientOverride.Name = "zTabPageLocalClientOverride";
			this.zTabPageLocalClientOverride.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPageLocalClientOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 142, true);
			this.zTabPageLocalClientOverride.TabIndex = 1;
			this.zTabPageLocalClientOverride.Text = Res.GetString("GlobalChargeCodeIntercompanyForm|213E7F8A-9170-470C-864D-E24165A80FA3", "Job Local Client Overrides");
			this.zTabPageLocalClientOverride.UseVisualStyleBackColor = true;
			// 
			// chargeCodeMappingWithLocalClientOverrideGrid
			// 
			this.chargeCodeMappingWithLocalClientOverrideGrid.AllowNavigation = false;
			this.chargeCodeMappingWithLocalClientOverrideGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.chargeCodeMappingWithLocalClientOverrideGrid, "PivotWithOverrideLocalClientCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithOverrideLocalClientCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithOverrideLocalClientCollection)).SyncRoot)).YP_OH_LocalClientOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithOverrideLocalClientCollection)).SyncRoot)).YP_TYPE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithOverrideLocalClientCollection)).SyncRoot)).YP_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GlobalChargeCodeMapPivotIntercompany)(((System.Collections.IList)(((GlobalChargeCodeMapIntercompany)(null)).PivotWithOverrideLocalClientCollection)).SyncRoot)).YP_Description)));
			this.chargeCodeMappingWithLocalClientOverrideGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "YP_OH_LocalClientOverride";
			zDropEditColumnStyleInfo2.ColumnName = "YP_TYPE";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "YP_AC";
			zTextBoxColumnStyleInfo2.ColumnName = "YP_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.chargeCodeMappingWithLocalClientOverrideGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.chargeCodeMappingWithLocalClientOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.chargeCodeMappingWithLocalClientOverrideGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.chargeCodeMappingWithLocalClientOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.chargeCodeMappingWithLocalClientOverrideGrid.CopySelectedRowsAllowed = true;
			this.chargeCodeMappingWithLocalClientOverrideGrid.GridId = "CBE09F17-8A5A-4535-8D1B-6EBB11D53F25";
			this.chargeCodeMappingWithLocalClientOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.chargeCodeMappingWithLocalClientOverrideGrid.LayoutKey = "APInvoicesGrid";
			this.chargeCodeMappingWithLocalClientOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.chargeCodeMappingWithLocalClientOverrideGrid.Name = "chargeCodeMappingWithLocalClientOverrideGrid";
			this.chargeCodeMappingWithLocalClientOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 130, true);
			this.chargeCodeMappingWithLocalClientOverrideGrid.TabIndex = 4;
			// 
			// GlobalChargeCodeIntercompanyForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 284, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlobalChargeCodeIntercompanyForm|d774dd6c-23be-4b6b-b362-efefab4fea2f", "Intercompany Global Charge Code");
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.GlobalCodeTextBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.IsActiveCheckBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business.GlobalChargeCode";
			this.DataSourceType = typeof(GlobalChargeCodeMapIntercompany);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany";
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 298, true);
			this.Name = "GlobalChargeCodeIntercompanyForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.GlobalCodeTextBox, 0);
			this.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeMappingGrid)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.zTabPageGeneric.ResumeLayout(false);
			this.zTabPageLocalClientOverride.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.chargeCodeMappingWithLocalClientOverrideGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}