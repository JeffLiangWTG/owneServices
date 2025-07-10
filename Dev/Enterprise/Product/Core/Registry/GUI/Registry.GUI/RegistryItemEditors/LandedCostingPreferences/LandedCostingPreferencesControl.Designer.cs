namespace Enterprise.Registry.GUI
{
	public partial class LandedCostingPreferencesControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZLabel ChargeGroupsAndChargeCodesLabel;
		internal Enterprise.ZArchitecture.ZLabel LandedCostingGroupsLabel;
		protected internal Enterprise.ZArchitecture.ZGrid ChargeGroupsAndChargeCodesGrid;
		protected internal Enterprise.ZArchitecture.ZGrid LandedCostingGroupsGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChargeGroupsAndChargeCodesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LandedCostingGroupsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChargeGroupsAndChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LandedCostingGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsAndChargeCodesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingGroupsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.LandedCostingGroup);
			// 
			// ChargeGroupsAndChargeCodesLabel
			// 
			this.ChargeGroupsAndChargeCodesLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|d0831fe2-68e1-4661-9c07-87bfbad64a2f", "Charge Groups and Charge Codes");
			this.ChargeGroupsAndChargeCodesLabel.IsFontBold = true;
			this.ChargeGroupsAndChargeCodesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.ChargeGroupsAndChargeCodesLabel.Name = "ChargeGroupsAndChargeCodesLabel";
			this.ChargeGroupsAndChargeCodesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ChargeGroupsAndChargeCodesLabel.TabIndex = 9;
			// 
			// LandedCostingGroupsLabel
			// 
			this.LandedCostingGroupsLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|b66e5c76-f0b3-4e8f-9bb7-93ff9293c0cb", "Landed Costing Groups");
			this.LandedCostingGroupsLabel.IsFontBold = true;
			this.LandedCostingGroupsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LandedCostingGroupsLabel.Name = "LandedCostingGroupsLabel";
			this.LandedCostingGroupsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.LandedCostingGroupsLabel.TabIndex = 8;
			// 
			// ChargeGroupsAndChargeCodesGrid
			// 
			this.ChargeGroupsAndChargeCodesGrid.AllowNavigation = false;
			this.ChargeGroupsAndChargeCodesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeGroupsAndChargeCodesGrid, "Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeGroupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeGroupList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ChargeGroupAndChargeCode)(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).Charges)).SyncRoot)).IsExcluded)));
			this.ChargeGroupsAndChargeCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "ChargeGroupList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|88e2279b-71d2-413f-bcbf-3e2e0010853e", "Chg Grp.");
			zDropEditColumnStyleInfo1.ColumnName = "ChargeGroupCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|e9ff8acb-ad2d-41d2-b7b1-61aabb093055", "Charge Group");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeGroupDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCodeList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|a7c9212e-9cd6-4dce-bdfc-f20b384653e0", "Chg Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|44137480-0edb-45aa-8846-9b28216bec52", "Charge Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|7d049da7-5e5d-4bac-b8e4-57104263980d", "Exclude");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsExcluded";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ChargeGroupsAndChargeCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeGroupsAndChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeGroupsAndChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeGroupsAndChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeGroupsAndChargeCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargeGroupsAndChargeCodesGrid.GridId = "368b13b8-8993-4c06-b408-47d0837af781";
			this.ChargeGroupsAndChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupsAndChargeCodesGrid.LayoutKey = "LandedCostingChargesGrid";
			this.ChargeGroupsAndChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.ChargeGroupsAndChargeCodesGrid.Name = "ChargeGroupsAndChargeCodesGrid";
			this.ChargeGroupsAndChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 168, true);
			this.ChargeGroupsAndChargeCodesGrid.TabIndex = 6;
			// 
			// LandedCostingGroupsGrid
			// 
			this.LandedCostingGroupsGrid.AllowNavigation = false;
			this.LandedCostingGroupsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LandedCostingGroupsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).GroupID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).GroupName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).CostDistributionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).CostDistributionList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LandedCostingGroup)(null)).CostDistributionDescription)));
			this.LandedCostingGroupsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|64c5125e-a432-4cfb-9d91-47b37e29f083", "ID");
			zCalcEditColumnStyleInfo1.ColumnName = "GroupID";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|f8943d58-ad83-4924-97c3-c4a1a46cc1ca", "Group Name");
			zTextBoxColumnStyleInfo3.ColumnName = "GroupName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zDropEditColumnStyleInfo2.BindToList = "CostDistributionList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|e8dc63b8-c3b8-49d8-a1fa-bf8a35e5d049", "Cost Distribution");
			zDropEditColumnStyleInfo2.ColumnName = "CostDistributionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("LandedCostingPreferencesControl|28f08048-a895-4d2b-b72d-fa09533c7965", "Cost Distribution");
			zTextBoxColumnStyleInfo4.ColumnName = "CostDistributionDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			this.LandedCostingGroupsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LandedCostingGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LandedCostingGroupsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LandedCostingGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LandedCostingGroupsGrid.GridId = "4cfce23f-3687-45c9-aacf-11216ebb240d";
			this.LandedCostingGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LandedCostingGroupsGrid.LayoutKey = "zGrid1";
			this.LandedCostingGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.LandedCostingGroupsGrid.Name = "LandedCostingGroupsGrid";
			this.LandedCostingGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 128, true);
			this.LandedCostingGroupsGrid.TabIndex = 5;
			// 
			// LandedCostingPreferencesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargeGroupsAndChargeCodesLabel);
			this.Controls.Add(this.LandedCostingGroupsLabel);
			this.Controls.Add(this.ChargeGroupsAndChargeCodesGrid);
			this.Controls.Add(this.LandedCostingGroupsGrid);
			this.Name = "LandedCostingPreferencesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsAndChargeCodesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingGroupsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
