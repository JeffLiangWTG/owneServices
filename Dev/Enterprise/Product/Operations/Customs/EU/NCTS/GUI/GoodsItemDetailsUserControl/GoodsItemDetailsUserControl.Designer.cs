using System.Windows.Forms;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemDetailsUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoodsItemDetailsUserControl));
			this.CommodityCodeTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DescriptionOfGoodsTextBox = new LongTextControl();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationGoodsItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommercialReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportChargesMethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CusC4NumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.UNDangerousGoodsUserControl = new Enterprise.Customs.EU.NCTS.GUI.UNDangerousGoodsUserControl();
			this.SupplementaryUnitsCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsFourthQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TaxOrFeeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.EU.NCTS.GUI.AdditionalSupplementaryCodesUserControl();
			this.FeesUserControl = new Enterprise.Customs.EU.NCTS.GUI.FeesGridUserControl();
			this.LinePriceCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityCodeTariffFindBox.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.CountryOfOriginDropEdit.SuspendLayout();
			this.TransportChargesMethodOfPaymentDropEdit.SuspendLayout();
			this.CusC4NumberCodeFindBox.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.UNDangerousGoodsUserControl.SuspendLayout();
			this.SupplementaryUnitsCalcDropEdit.SuspendLayout();
			this.CustomsQuantityDropEdit.SuspendLayout();
			this.CustomsThirdQuantityDropEdit.SuspendLayout();
			this.CustomsFourthQuantityDropEdit.SuspendLayout();
			this.CustomsValueCalcDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.FeesUserControl.SuspendLayout();
			this.LinePriceCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// CommodityCodeTariffFindBox
			// 
			this.CommodityCodeTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeTariffFindBox, "BY_FormattedHarmonisedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_FormattedHarmonisedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).UniversalTariffDescription)));
			this.CommodityCodeTariffFindBox.ErrorForUnsupportedCountry = null;
			this.CommodityCodeTariffFindBox.GetEffectiveDate = null;
			this.CommodityCodeTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 166, true);
			this.CommodityCodeTariffFindBox.Name = "CommodityCodeTariffFindBox";
			this.CommodityCodeTariffFindBox.NeedLoadParentDataGroup = true;
			this.CommodityCodeTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CommodityCodeTariffFindBox.ParentType = null;
			this.CommodityCodeTariffFindBox.PreBoundMaxLength = 8;
			this.CommodityCodeTariffFindBox.SelectNomenclatureModes = null;
			this.CommodityCodeTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.CommodityCodeTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CommodityCodeTariffFindBox.TabIndex = 5;
			this.CommodityCodeTariffFindBox.TariffType = "IMP";
			this.CommodityCodeTariffFindBox.BindToForDescription = "UniversalTariffDescription";
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
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 113, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
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
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 87, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 2;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// DescriptionOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionOfGoodsTextBox, "BY_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Description)));
			this.DescriptionOfGoodsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 61, true);
			this.DescriptionOfGoodsTextBox.Name = "DescriptionOfGoodsTextBox";
			this.DescriptionOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.DescriptionOfGoodsTextBox.TabIndex = 1;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "BY_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Type)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 191, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.DeclarationTypeDropEdit.TabIndex = 5;
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "BY_RN_NKCountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfDestination)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 243, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfDestinationDropEdit.TabIndex = 7;
			// 
			// CountryOfDispatchDropEdit
			// 
			this.CountryOfDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDispatchDropEdit, "BY_RN_NKCountryOfDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfDispatch)));
			this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 218, true);
			this.CountryOfDispatchDropEdit.Name = "CountryOfDispatchDropEdit";
			this.CountryOfDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfDispatchDropEdit.TabIndex = 6;
			// 
			// ItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberTextBox, "BY_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_LineNo)));
			this.ItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 35, true);
			this.ItemNumberTextBox.Name = "ItemNumberTextBox";
			this.ItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.ItemNumberTextBox.TabIndex = 0;
			// 
			// DeclarationGoodsItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationGoodsItemNumberTextBox, "BY_DeclarationGoodsItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_DeclarationGoodsItemNumber)));
			this.DeclarationGoodsItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(628, 61, true);
			this.DeclarationGoodsItemNumberTextBox.Name = "DeclarationGoodsItemNumberTextBox";
			this.DeclarationGoodsItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.DeclarationGoodsItemNumberTextBox.TabIndex = 0;
			// 
			// CountryOfOriginDropEdit
			// 
			this.CountryOfOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginDropEdit, "BY_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RN_NKCountryOfOrigin)));
			this.CountryOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 374, true);
			this.CountryOfOriginDropEdit.Name = "CountryOfOriginDropEdit";
			this.CountryOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryOfOriginDropEdit.TabIndex = 13;
			// 
			// CommercialReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialReferenceNumberTextBox, "BY_CommercialReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CommercialReferenceNumber)));
			this.CommercialReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommercialReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 400, true);
			this.CommercialReferenceNumberTextBox.Name = "CommercialReferenceNumberTextBox";
			this.CommercialReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CommercialReferenceNumberTextBox.TabIndex = 14;
			// 
			// TransportChargesMethodOfPaymentDropEdit
			// 
			this.TransportChargesMethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportChargesMethodOfPaymentDropEdit, "BY_TransportChargesMethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_TransportChargesMethodOfPayment)));
			this.TransportChargesMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 426, true);
			this.TransportChargesMethodOfPaymentDropEdit.Name = "TransportChargesMethodOfPaymentDropEdit";
			this.TransportChargesMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.TransportChargesMethodOfPaymentDropEdit.TabIndex = 10;
			// 
			// CusC4NumberCodeFindBox
			// 
			this.CusC4NumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusC4NumberCodeFindBox, "BY_CusC4Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CusC4Number)));
			this.CusC4NumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(628, 35, true);
			this.CusC4NumberCodeFindBox.Name = "CusC4NumberCodeFindBox";
			this.CusC4NumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusC4NumberCodeFindBox.ParentType = null;
			this.CusC4NumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CusC4NumberCodeFindBox.TabIndex = 14;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Consignee)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.ConsigneeList";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f9bc49f0-941e-40e1-9305-97991ced7e9e", "Consignee");
			this.ConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.ConsigneeDocAddressControl.IsCustomHeight = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocAddressControl, false);
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(825, 35, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 15;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// UNDangerousGoodsUserControl
			// 
			this.UNDangerousGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDangerousGoodsUserControl, ".");
			this.UNDangerousGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 113, true);
			this.UNDangerousGoodsUserControl.Name = "UNDangerousGoodsUserControl";
			this.UNDangerousGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.UNDangerousGoodsUserControl.TabIndex = 13;
			// 
			// SupplementaryUnitsCalcDropEdit
			// 
			this.SupplementaryUnitsCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryUnitsCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsSecondUnitQty)));
			this.SupplementaryUnitsCalcDropEdit.BindToAmount = "BY_CustomsSecondQuantity";
			this.SupplementaryUnitsCalcDropEdit.BindToUnit = "BY_CustomsSecondUnitQty";
			this.SupplementaryUnitsCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 139, true);
			this.SupplementaryUnitsCalcDropEdit.Name = "SupplementaryUnitsCalcDropEdit";
			this.SupplementaryUnitsCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SupplementaryUnitsCalcDropEdit.TabIndex = 4;
			this.SupplementaryUnitsCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// CustomsQuantityDropEdit
			// 
			this.CustomsQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).CustomsFirstQuantityInKilograms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).CustomsFirstUnitQtyKilograms)));
			this.CustomsQuantityDropEdit.BindToAmount = "CustomsFirstQuantityInKilograms";
			this.CustomsQuantityDropEdit.BindToUnit = "CustomsFirstUnitQtyKilograms";
			this.CustomsQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 113, true);
			this.CustomsQuantityDropEdit.Name = "CustomsQuantityDropEdit";
			this.CustomsQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsQuantityDropEdit.TabIndex = 16;
			this.CustomsQuantityDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsThirdQuantityDropEdit
			// 
			this.CustomsThirdQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsThirdUnitQty)));
			this.CustomsThirdQuantityDropEdit.BindToAmount = "BY_CustomsThirdQuantity";
			this.CustomsThirdQuantityDropEdit.BindToUnit = "BY_CustomsThirdUnitQty";
			this.CustomsThirdQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 113, true);
			this.CustomsThirdQuantityDropEdit.Name = "CustomsThirdQuantityDropEdit";
			this.CustomsThirdQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsThirdQuantityDropEdit.TabIndex = 17;
			this.CustomsThirdQuantityDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsFourthQuantityDropEdit
			// 
			this.CustomsFourthQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsFourthQuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsFourthQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_CustomsFourthUnitQty)));
			this.CustomsFourthQuantityDropEdit.BindToAmount = "BY_CustomsFourthQuantity";
			this.CustomsFourthQuantityDropEdit.BindToUnit = "BY_CustomsFourthUnitQty";
			this.CustomsFourthQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 113, true);
			this.CustomsFourthQuantityDropEdit.Name = "CustomsFourthQuantityDropEdit";
			this.CustomsFourthQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsFourthQuantityDropEdit.TabIndex = 17;
			this.CustomsFourthQuantityDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// CustomsValueCalcDropEdit
			// 
			this.CustomsValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_MonetaryValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RX_NKCurrency)));
			this.CustomsValueCalcDropEdit.BindToAmount = "BY_MonetaryValue";
			this.CustomsValueCalcDropEdit.BindToUnit = "Header.Company.CustomsCurrency.Code";
			this.CustomsValueCalcDropEdit.BindToList = "Lookups.Currencies";
			this.CustomsValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 139, true);
			this.CustomsValueCalcDropEdit.Name = "CustomsValueCalcDropEdit";
			this.CustomsValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsValueCalcDropEdit.TabIndex = 18;
			this.CustomsValueCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeDropEdit, "BY_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_ZZF_NKTaxType)));
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 164, true);
			this.TaxOrFeeDropEdit.Name = "TaxOrFeeDropEdit";
			this.TaxOrFeeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TaxOrFeeDropEdit.TabIndex = 19;
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, ".");
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 191, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 25;
			// 
			// FeesControl
			// 
			this.BindingSource.SetBindingMember(this.FeesUserControl, ".");
			this.FeesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(825, 223, true);
			this.FeesUserControl.Name = "FeesUserControl";
			this.FeesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 135, true);
			this.FeesUserControl.TabIndex = 27;
			// 
			// LinePriceCalcDropEdit
			// 
			this.LinePriceCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinePriceCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_RX_NKLinePriceCurrency)));
			this.LinePriceCalcDropEdit.BindToAmount = "BY_LinePrice";
			this.LinePriceCalcDropEdit.BindToUnit = "BY_RX_NKLinePriceCurrency";
			this.LinePriceCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 218, true);
			this.LinePriceCalcDropEdit.Name = "LinePriceCalcDropEdit";
			this.LinePriceCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.LinePriceCalcDropEdit.TabIndex = 28;
			this.LinePriceCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// GoodsItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinePriceCalcDropEdit);
			this.Controls.Add(this.FeesUserControl);
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Controls.Add(this.TaxOrFeeDropEdit);
			this.Controls.Add(this.CustomsValueCalcDropEdit);
			this.Controls.Add(this.CustomsQuantityDropEdit);
			this.Controls.Add(this.CustomsThirdQuantityDropEdit);
			this.Controls.Add(this.CustomsFourthQuantityDropEdit);
			this.Controls.Add(this.UNDangerousGoodsUserControl);
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Controls.Add(this.CusC4NumberCodeFindBox);
			this.Controls.Add(this.CommercialReferenceNumberTextBox);
			this.Controls.Add(this.CountryOfOriginDropEdit);
			this.Controls.Add(this.TransportChargesMethodOfPaymentDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.DescriptionOfGoodsTextBox);
			this.Controls.Add(this.DeclarationTypeDropEdit);
			this.Controls.Add(this.CountryOfDestinationDropEdit);
			this.Controls.Add(this.CountryOfDispatchDropEdit);
			this.Controls.Add(this.ItemNumberTextBox);
			this.Controls.Add(this.DeclarationGoodsItemNumberTextBox);
			this.Controls.Add(this.CommodityCodeTariffFindBox);
			this.Controls.Add(this.SupplementaryUnitsCalcDropEdit);
			this.Name = "GoodsItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1138, 515, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityCodeTariffFindBox.ResumeLayout(true);
			this.CommodityCodeTariffFindBox.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.CountryOfOriginDropEdit.ResumeLayout(true);
			this.CountryOfOriginDropEdit.PerformLayout();
			this.TransportChargesMethodOfPaymentDropEdit.ResumeLayout(true);
			this.TransportChargesMethodOfPaymentDropEdit.PerformLayout();
			this.CusC4NumberCodeFindBox.ResumeLayout(true);
			this.CusC4NumberCodeFindBox.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.UNDangerousGoodsUserControl.ResumeLayout(true);
			this.UNDangerousGoodsUserControl.PerformLayout();
			this.SupplementaryUnitsCalcDropEdit.ResumeLayout(true);
			this.SupplementaryUnitsCalcDropEdit.PerformLayout();
			this.CustomsQuantityDropEdit.ResumeLayout(true);
			this.CustomsQuantityDropEdit.PerformLayout();
			this.CustomsThirdQuantityDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityDropEdit.PerformLayout();
			this.CustomsFourthQuantityDropEdit.ResumeLayout(true);
			this.CustomsFourthQuantityDropEdit.PerformLayout();
			this.CustomsValueCalcDropEdit.ResumeLayout(true);
			this.CustomsValueCalcDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.FeesUserControl.ResumeLayout(false);
			this.FeesUserControl.PerformLayout();
			this.LinePriceCalcDropEdit.ResumeLayout(true);
			this.LinePriceCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal LongTextControl DescriptionOfGoodsTextBox;
		internal ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDispatchDropEdit;
		internal ZArchitecture.ZTextBox ItemNumberTextBox;
		internal ZArchitecture.ZTextBox DeclarationGoodsItemNumberTextBox;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal Universal.GUI.TariffFindBox CommodityCodeTariffFindBox;
		internal ZArchitecture.GUI.ZDropEdit CountryOfOriginDropEdit;
		internal ZArchitecture.ZTextBox CommercialReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit TransportChargesMethodOfPaymentDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CusC4NumberCodeFindBox;
		internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		internal UNDangerousGoodsUserControl UNDangerousGoodsUserControl;
		internal ZArchitecture.GUI.ZCalcDropEdit SupplementaryUnitsCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsFourthQuantityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsValueCalcDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TaxOrFeeDropEdit;
		internal AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
		internal FeesGridUserControl FeesUserControl;
		internal ZArchitecture.GUI.ZCalcDropEdit LinePriceCalcDropEdit;
	}
}
