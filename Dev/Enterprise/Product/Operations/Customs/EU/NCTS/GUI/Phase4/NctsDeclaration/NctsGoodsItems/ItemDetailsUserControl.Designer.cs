namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ItemDetailsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemDetailsUserControl));
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CustomsFirstQtyDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQtyDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ItemConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ItemConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CommodityCodeTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalSupplementaryCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FiscalUnitsDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TaxOrFeeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsValueDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DescriptionOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsFirstQtyDropEdit.SuspendLayout();
			this.CustomsThirdQtyDropEdit.SuspendLayout();
			this.ItemConsigneeDocAddressControl.SuspendLayout();
			this.ItemConsignorDocAddressControl.SuspendLayout();
			this.CommodityCodeTariffFindBox.SuspendLayout();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.FeesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).BeginInit();
			this.FeesGrid.SuspendLayout();
			this.FiscalUnitsDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.CustomsValueDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.OriginCountryDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// CustomsFirstQtyDropEdit
			// 
			this.CustomsFirstQtyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsFirstQtyDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).CustomsFirstQuantityInKilograms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).CustomsFirstUnitQtyKilograms)));
			this.CustomsFirstQtyDropEdit.BindToAmount = "CustomsFirstQuantityInKilograms";
			this.CustomsFirstQtyDropEdit.BindToUnit = "CustomsFirstUnitQtyKilograms";
			this.CustomsFirstQtyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 249, true);
			this.CustomsFirstQtyDropEdit.Name = "CustomsFirstQtyDropEdit";
			this.CustomsFirstQtyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CustomsFirstQtyDropEdit.TabIndex = 9;
			this.CustomsFirstQtyDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsThirdQtyDropEdit
			// 
			this.CustomsThirdQtyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQtyDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsThirdUnitQty)));
			this.CustomsThirdQtyDropEdit.BindToAmount = "BY_CustomsThirdQuantity";
			this.CustomsThirdQtyDropEdit.BindToUnit = "BY_CustomsThirdUnitQty";
			this.CustomsThirdQtyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 274, true);
			this.CustomsThirdQtyDropEdit.Name = "CustomsThirdQtyDropEdit";
			this.CustomsThirdQtyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CustomsThirdQtyDropEdit.TabIndex = 10;
			this.CustomsThirdQtyDropEdit.UnitPreBoundMaxLength = 4;
			this.CustomsThirdQtyDropEdit.Visible = false;
			// 
			// ItemConsigneeDocAddressControl
			// 
			this.ItemConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ItemConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ItemConsigneeDocAddressControl, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Consignee)));
			this.ItemConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ItemConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b5c9361b-d83d-4662-8721-5e71f040dca3", "[8] Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ItemConsigneeDocAddressControl, false);
			this.ItemConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 19, true);
			this.ItemConsigneeDocAddressControl.Name = "ItemConsigneeDocAddressControl";
			this.ItemConsigneeDocAddressControl.ReadOnly = false;
			this.ItemConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ItemConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ItemConsigneeDocAddressControl.TabIndex = 19;
			this.ItemConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// ItemConsignorDocAddressControl
			// 
			this.ItemConsignorDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ItemConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ItemConsignorDocAddressControl, "Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Consignor)));
			this.ItemConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.ItemConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("209f6504-794b-4bc7-9ff0-b29628062a18", "[2] Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ItemConsignorDocAddressControl, false);
			this.ItemConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 19, true);
			this.ItemConsignorDocAddressControl.Name = "ItemConsignorDocAddressControl";
			this.ItemConsignorDocAddressControl.ReadOnly = false;
			this.ItemConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ItemConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ItemConsignorDocAddressControl.TabIndex = 18;
			this.ItemConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// CommodityCodeTariffFindBox
			// 
			this.CommodityCodeTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeTariffFindBox, "BY_FormattedHarmonisedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_FormattedHarmonisedTariff)));
			this.CommodityCodeTariffFindBox.ErrorForUnsupportedCountry = null;
			this.CommodityCodeTariffFindBox.GetEffectiveDate = null;
			this.CommodityCodeTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 120, true);
			this.CommodityCodeTariffFindBox.Name = "CommodityCodeTariffFindBox";
			this.CommodityCodeTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CommodityCodeTariffFindBox.ParentType = null;
			this.CommodityCodeTariffFindBox.PreBoundMaxLength = 8;
			this.CommodityCodeTariffFindBox.SelectNomenclatureModes = null;
			this.CommodityCodeTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.CommodityCodeTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.CommodityCodeTariffFindBox.TabIndex = 4;
			this.CommodityCodeTariffFindBox.TariffType = "IMP";
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1b9aed27-8470-41dd-9182-bb49de1e4c60", "Item Details");
			this.ItemDetailsGroupBox.Controls.Add(this.AdditionalSupplementaryCodesTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.AdditionalSupplementaryCodesEditButton);
			this.ItemDetailsGroupBox.Controls.Add(this.FeesGroupBox);
			this.ItemDetailsGroupBox.Controls.Add(this.FiscalUnitsDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.TaxOrFeeDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.CustomsValueDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.GrossWeightCalcDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.ItemConsigneeDocAddressControl);
			this.ItemDetailsGroupBox.Controls.Add(this.ItemConsignorDocAddressControl);
			this.ItemDetailsGroupBox.Controls.Add(this.DescriptionOfGoodsTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.DeclarationTypeDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.CountryOfDestinationDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.OriginCountryDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.CountryOfDispatchDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.ItemNumberTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.CommodityCodeTariffFindBox);
			this.ItemDetailsGroupBox.Controls.Add(this.CustomsFirstQtyDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.CustomsThirdQtyDropEdit);
			this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 526, true);
			this.ItemDetailsGroupBox.TabIndex = 0;
			this.ItemDetailsGroupBox.TabStop = false;
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("25D49C09-9EB2-4530-A671-CCE68B727913", "Calculated Duty and Tax");
			this.FeesGroupBox.Controls.Add(this.FeesGrid);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FeesGroupBox, false);
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 379, true);
			this.FeesGroupBox.Name = "FeesGroupBox";
			this.FeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 135, true);
			this.FeesGroupBox.TabIndex = 0;
			this.FeesGroupBox.TabStop = false;
			// 
			// FeesGrid
			// 
			this.FeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeesGrid, "Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeAmount)));
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("982220d1-bd60-4bdc-8dea-eec0648a5227", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "BFE_ChargeType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("03721a74-4d5d-4cbb-b348-d384c68854b7", "Method");
			zTextBoxColumnStyleInfo1.ColumnName = "BFE_MethodOfCalculation";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("35c073f0-5aa7-458e-8b11-36eeb90f57ee", "Base");
			zCalcEditColumnStyleInfo1.ColumnName = "BFE_BaseValue";
			zCalcEditColumnStyleInfo1.Decimals = 2;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("df8a6492-21ad-4e8f-8d92-f193a8a7a396", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "BFE_Rate";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4fbeff19-80a4-4d39-b9b7-c4983162f3e6", "Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "BFE_ChargeAmount";
			zCalcEditColumnStyleInfo3.Decimals = 2;
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			this.FeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FeesGrid.GridId = "F358970D-6301-4A12-A485-2BBDB8BA71DE";
			this.FeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesGrid.LayoutKey = "FeesGrid";
			this.FeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FeesGrid.Name = "FeesGrid";
			this.FeesGrid.ReadOnly = true;
			this.FeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 116, true);
			this.FeesGrid.TabIndex = 28;
			// 
			// AdditionalSupplementaryCodesTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesTextBox, "BY_Supplements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Supplements)));
			this.AdditionalSupplementaryCodesTextBox.CaptionResourceString = null;
			this.AdditionalSupplementaryCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 353, true);
			this.AdditionalSupplementaryCodesTextBox.Name = "AdditionalSupplementaryCodesTextBox";
			this.AdditionalSupplementaryCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.AdditionalSupplementaryCodesTextBox.TabIndex = 13;
			this.AdditionalSupplementaryCodesTextBox.TabStop = false;
			// 
			// AdditionalSupplementaryCodesEditButton
			// 
			this.AdditionalSupplementaryCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("AdditionalSupplementaryCodesEditButton|127119E3-4A2E-416B-A03F-691F96926793", "Additional codes...");
			this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 350, true);
			this.AdditionalSupplementaryCodesEditButton.Name = "AdditionalSupplementaryCodesEditButton";
			this.AdditionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 22, true);
			this.AdditionalSupplementaryCodesEditButton.TabIndex = 14;
			this.AdditionalSupplementaryCodesEditButton.ToolTipCaption = null;
			this.AdditionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
			// 
			// FiscalUnitsDropEdit
			// 
			this.FiscalUnitsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FiscalUnitsDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsSecondUnitQty)));
			this.FiscalUnitsDropEdit.BindToAmount = "BY_CustomsSecondQuantity";
			this.FiscalUnitsDropEdit.BindToUnit = "BY_CustomsSecondUnitQty";
			this.FiscalUnitsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 274, true);
			this.FiscalUnitsDropEdit.Name = "FiscalUnitsDropEdit";
			this.FiscalUnitsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.FiscalUnitsDropEdit.TabIndex = 10;
			this.FiscalUnitsDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeDropEdit, "BY_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_ZZF_NKTaxType)));
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 326, true);
			this.TaxOrFeeDropEdit.Name = "TaxOrFeeDropEdit";
			this.TaxOrFeeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.TaxOrFeeDropEdit.TabIndex = 12;
			// 
			// CustomsValueDropEdit
			// 
			this.CustomsValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RX_NKCurrency)));
			this.CustomsValueDropEdit.BindToAmount = "BY_MonetaryValue";
			this.CustomsValueDropEdit.BindToUnit = "BY_RX_NKCurrency";
			this.CustomsValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 301, true);
			this.CustomsValueDropEdit.Name = "CustomsValueDropEdit";
			this.CustomsValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CustomsValueDropEdit.TabIndex = 11;
			this.CustomsValueDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_NetWeightUnit)));
			this.NetWeightCalcDropEdit.BindToAmount = "BY_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "BY_NetWeightUnit";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("910e2a76-b6fd-4f92-8ed5-6fc5799594f3", "Net Weight");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 94, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.NetWeightCalcDropEdit.TabIndex = 3;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_GrossWeightUnit)));
			this.GrossWeightCalcDropEdit.BindToAmount = "BY_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "BY_GrossWeightUnit";
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("528b873e-5161-4e5d-976f-4efb35bad1cb", "Gross Weight");
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 68, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.GrossWeightCalcDropEdit.TabIndex = 2;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// DescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "BY_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Description)));
			this.DescriptionOfGoodsTextBox.CaptionResourceString = null;
			this.DescriptionOfGoodsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 42, true);
			this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
			this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.DescriptionOfGoodsTextBox.TabIndex = 1;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "BY_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Type)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 146, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.DeclarationTypeDropEdit.TabIndex = 5;
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "BY_RN_NKCountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfDestination)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 198, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.CountryOfDestinationDropEdit.TabIndex = 7;
			// 
			// OriginCountryDropEdit
			// 
			this.OriginCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCountryDropEdit, "BY_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfOrigin)));
			this.OriginCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 224, true);
			this.OriginCountryDropEdit.Name = "OriginCountryDropEdit";
			this.OriginCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.OriginCountryDropEdit.TabIndex = 8;
			// 
			// CountryOfDispatchDropEdit
			// 
			this.CountryOfDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDispatchDropEdit, "BY_RN_NKCountryOfDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfDispatch)));
			this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 172, true);
			this.CountryOfDispatchDropEdit.Name = "CountryOfDispatchDropEdit";
			this.CountryOfDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.CountryOfDispatchDropEdit.TabIndex = 6;
			// 
			// ItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberTextBox, "BY_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_LineNo)));
			this.ItemNumberTextBox.CaptionResourceString = null;
			this.ItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 16, true);
			this.ItemNumberTextBox.Name = "ItemNumberTextBox";
			this.ItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.ItemNumberTextBox.TabIndex = 0;
			// 
			// ItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemDetailsGroupBox);
			this.Name = "ItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 531, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsFirstQtyDropEdit.ResumeLayout(true);
			this.CustomsFirstQtyDropEdit.PerformLayout();
			this.CustomsThirdQtyDropEdit.ResumeLayout(true);
			this.CustomsThirdQtyDropEdit.PerformLayout();
			this.ItemConsigneeDocAddressControl.ResumeLayout(true);
			this.ItemConsigneeDocAddressControl.PerformLayout();
			this.ItemConsignorDocAddressControl.ResumeLayout(true);
			this.ItemConsignorDocAddressControl.PerformLayout();
			this.CommodityCodeTariffFindBox.ResumeLayout(true);
			this.CommodityCodeTariffFindBox.PerformLayout();
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).EndInit();
			this.FeesGrid.ResumeLayout(false);
			this.FeesGrid.PerformLayout();
			this.FeesGroupBox.ResumeLayout(false);
			this.FeesGroupBox.PerformLayout();
			this.FiscalUnitsDropEdit.ResumeLayout(true);
			this.FiscalUnitsDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.CustomsValueDropEdit.ResumeLayout(true);
			this.CustomsValueDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.OriginCountryDropEdit.ResumeLayout(true);
			this.OriginCountryDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		protected ZArchitecture.ZTextBox DescriptionOfGoodsTextBox;
		protected ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		protected ZArchitecture.GUI.ZDropEdit OriginCountryDropEdit;
		protected ZArchitecture.GUI.ZDropEdit CountryOfDispatchDropEdit;
		protected ZArchitecture.ZTextBox ItemNumberTextBox;
		protected MasterFiles.GUI.ZDocAddressControl ItemConsigneeDocAddressControl;
		protected MasterFiles.GUI.ZDocAddressControl ItemConsignorDocAddressControl;
		protected ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		protected ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		protected Universal.GUI.TariffFindBox CommodityCodeTariffFindBox;
		protected ZArchitecture.GUI.ZCalcDropEdit FiscalUnitsDropEdit;
		protected ZArchitecture.GUI.ZCalcDropEdit CustomsValueDropEdit;
		protected ZArchitecture.GUI.ZDropEdit TaxOrFeeDropEdit;
		protected ZArchitecture.ZTextBox AdditionalSupplementaryCodesTextBox;
		protected ZArchitecture.GUI.ZButton AdditionalSupplementaryCodesEditButton;
		protected ZArchitecture.GUI.ZCalcDropEdit CustomsFirstQtyDropEdit;
		protected ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQtyDropEdit;
		protected ZArchitecture.ZGrid FeesGrid;
		protected ZArchitecture.GUI.ZGroupBox FeesGroupBox;
	}
}
