using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.NationalAdditionalCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NationalAdditionalCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NationalAdditionalCodesUserControl = new Enterprise.Customs.EU.GUI.NationalAdditionalCodesUserControl();
			this.SupplementaryCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormattedProcedureCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DispatchCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationUsingZZRefCusCodeListCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.EU.GUI.AdditionalSupplementaryCodesUserControl();
			this.CountryOfSupplyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AdditionalProcedureCodesUserControl = new Enterprise.Customs.EU.GUI.AdditionalProcedureCodesUserControl();
			this.PreferenceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuotaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SecondQuotaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalSupplementaryCodesAndGDMUserControl = new Enterprise.Customs.EU.GUI.AdditionalSupplementaryCodesAndGDMUserControl();
			this.CusNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuotaWithCheckLinkUserControl = new Enterprise.Customs.EU.GUI.QuotaWithCheckLinkUserControl();
			this.ValuationAdjustmentPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TransactionNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UsedGoodsCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReturnToOriginCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SecondaryTreatedProductCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReturningGoodsReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReturningGoodsReasonDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportUnionThreadCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportUnionPackCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportUnionProductionYearCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExportUnionDeferredInstallmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportUnionEcologicalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InwardProcessingLicenseLineNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryExitPurposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryExitPurposeDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessingDescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.SupplementaryCode1AndGDMUserControl = new Enterprise.Customs.EU.GUI.SupplementaryCode1AndGDMUserControl();
			this.BorderTradeStateCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportUnionAdditionalTariffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExcessStockCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommercialPaymentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ValuationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UnformattedProcedureCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RegionOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PriceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NationalAdditionalCode1DropEdit.SuspendLayout();
			this.NationalAdditionalCode2DropEdit.SuspendLayout();
			this.NationalAdditionalCodesUserControl.SuspendLayout();
			this.SupplementaryCode2DropEdit.SuspendLayout();
			this.SupplementaryCode1DropEdit.SuspendLayout();
			this.FormattedProcedureCodeFindBox.SuspendLayout();
			this.DestinationCodeFindBox.SuspendLayout();
			this.DispatchCodeFindBox.SuspendLayout();
			this.DestinationUsingZZRefCusCodeListCodeFindBox.SuspendLayout();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.CountryOfSupplyCodeFindBox.SuspendLayout();
			this.AdditionalProcedureCodesUserControl.SuspendLayout();
			this.PreferenceCodeDropEdit.SuspendLayout();
			this.QuotaDropEdit.SuspendLayout();
			this.SecondQuotaDropEdit.SuspendLayout();
			this.AdditionalSupplementaryCodesAndGDMUserControl.SuspendLayout();
			this.CusNumberCodeFindBox.SuspendLayout();
			this.QuotaWithCheckLinkUserControl.SuspendLayout();
			this.TransactionNatureDropEdit.SuspendLayout();
			this.UsedGoodsCodeDropEdit.SuspendLayout();
			this.ReturningGoodsReasonCodeDropEdit.SuspendLayout();
			this.ExportUnionThreadCodeFindBox.SuspendLayout();
			this.ExportUnionPackCodeFindBox.SuspendLayout();
			this.EntryExitPurposeCodeDropEdit.SuspendLayout();
			this.ProcessingDescriptionLongTextControl.SuspendLayout();
			this.SupplementaryCode1AndGDMUserControl.SuspendLayout();
			this.BorderTradeStateCodeFindBox.SuspendLayout();
			this.ExportUnionAdditionalTariffCodeFindBox.SuspendLayout();
			this.CommercialPaymentCodeDropEdit.SuspendLayout();
			this.ValuationCodeFindBox.SuspendLayout();
			this.UnformattedProcedureCodeFindBox.SuspendLayout();
			this.RegionOfDestinationDropEdit.SuspendLayout();
			this.PriceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// NationalAdditionalCode1DropEdit
			// 
			this.NationalAdditionalCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalAdditionalCode1DropEdit, "JI_NationalAdditionalCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_NationalAdditionalCode1)));
			this.NationalAdditionalCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 385, true);
			this.NationalAdditionalCode1DropEdit.Name = "NationalAdditionalCode1DropEdit";
			this.NationalAdditionalCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 15, true);
			this.NationalAdditionalCode1DropEdit.TabIndex = 32;
			// 
			// NationalAdditionalCode2DropEdit
			// 
			this.NationalAdditionalCode2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalAdditionalCode2DropEdit, "JI_NationalAdditionalCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_NationalAdditionalCode2)));
			this.NationalAdditionalCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 406, true);
			this.NationalAdditionalCode2DropEdit.Name = "NationalAdditionalCode2DropEdit";
			this.NationalAdditionalCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 15, true);
			this.NationalAdditionalCode2DropEdit.TabIndex = 33;
			// 
			// NationalAdditionalCodesUserControl
			// 
			this.NationalAdditionalCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NationalAdditionalCodesUserControl, ".");
			this.NationalAdditionalCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 427, true);
			this.NationalAdditionalCodesUserControl.Name = "NationalAdditionalCodesUserControl";
			this.NationalAdditionalCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 22, true);
			this.NationalAdditionalCodesUserControl.TabIndex = 34;
			// 
			// SupplementaryCode2DropEdit
			// 
			this.SupplementaryCode2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode2DropEdit, "JI_SupplementaryCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_SupplementaryCode2)));
			this.SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 89, true);
			this.SupplementaryCode2DropEdit.Name = "SupplementaryCode2DropEdit";
			this.SupplementaryCode2DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode2DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.SupplementaryCode2DropEdit.TabIndex = 3;
			// 
			// SupplementaryCode1DropEdit
			// 
			this.SupplementaryCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode1DropEdit, "JI_SupplementaryCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_SupplementaryCode1)));
			this.SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 68, true);
			this.SupplementaryCode1DropEdit.Name = "SupplementaryCode1DropEdit";
			this.SupplementaryCode1DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode1DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.SupplementaryCode1DropEdit.TabIndex = 2;
			// 
			// FormattedProcedureCodeFindBox
			// 
			this.FormattedProcedureCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FormattedProcedureCodeFindBox, "JI_FormattedProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_FormattedProcedure)));
			this.FormattedProcedureCodeFindBox.BindToList = null;
			this.FormattedProcedureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 26, true);
			this.FormattedProcedureCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			this.FormattedProcedureCodeFindBox.Name = "FormattedProcedureCodeFindBox";
			this.FormattedProcedureCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FormattedProcedureCodeFindBox.ParentType = null;
			this.FormattedProcedureCodeFindBox.PreBoundMaxLength = 3;
			this.FormattedProcedureCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 15, true);
			this.FormattedProcedureCodeFindBox.TabIndex = 0;
			// 
			// DestinationCodeFindBox
			// 
			this.DestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCodeFindBox, "ZG_CountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfDestination)));
			this.DestinationCodeFindBox.BindToList = null;
			this.DestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 304, true);
			this.DestinationCodeFindBox.Name = "DestinationCodeFindBox";
			this.DestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DestinationCodeFindBox.ParentType = null;
			this.DestinationCodeFindBox.PreBoundMaxLength = 2;
			this.DestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 15, true);
			this.DestinationCodeFindBox.TabIndex = 1;
			// 
			// DispatchCodeFindBox
			// 
			this.DispatchCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DispatchCodeFindBox, "ZG_CountryOfDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfDispatch)));
			this.DispatchCodeFindBox.BindToList = null;
			this.DispatchCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 486, true);
			this.DispatchCodeFindBox.Name = "DispatchCodeFindBox";
			this.DispatchCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DispatchCodeFindBox.ParentType = null;
			this.DispatchCodeFindBox.PreBoundMaxLength = 2;
			this.DispatchCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 15, true);
			this.DispatchCodeFindBox.TabIndex = 2;
			// 
			// DestinationUsingZZRefCusCodeListCodeFindBox
			// 
			this.DestinationUsingZZRefCusCodeListCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationUsingZZRefCusCodeListCodeFindBox, "ZG_CountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfDestination)));
			this.DestinationUsingZZRefCusCodeListCodeFindBox.BindToList = null;
			this.DestinationUsingZZRefCusCodeListCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 142, true);
			this.DestinationUsingZZRefCusCodeListCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.DestinationUsingZZRefCusCodeListCodeFindBox.Name = "DestinationUsingZZRefCusCodeListCodeFindBox";
			this.DestinationUsingZZRefCusCodeListCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DestinationUsingZZRefCusCodeListCodeFindBox.ParentType = null;
			this.DestinationUsingZZRefCusCodeListCodeFindBox.PreBoundMaxLength = 2;
			this.DestinationUsingZZRefCusCodeListCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 15, true);
			this.DestinationUsingZZRefCusCodeListCodeFindBox.TabIndex = 1;
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, ".");
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 152, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 22, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 4;
			// 
			// CountryOfSupplyCodeFindBox
			// 
			this.CountryOfSupplyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfSupplyCodeFindBox, "ZG_CountryOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfSupply)));
			this.CountryOfSupplyCodeFindBox.BindToList = null;
			this.CountryOfSupplyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 175, true);
			this.CountryOfSupplyCodeFindBox.Name = "CountryOfSupplyCodeFindBox";
			this.CountryOfSupplyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfSupplyCodeFindBox.ParentType = null;
			this.CountryOfSupplyCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfSupplyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.CountryOfSupplyCodeFindBox.TabIndex = 5;
			// 
			// AdditionalProcedureCodesUserControl
			// 
			this.AdditionalProcedureCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalProcedureCodesUserControl, ".");
			this.AdditionalProcedureCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 196, true);
			this.AdditionalProcedureCodesUserControl.Name = "AdditionalProcedureCodesUserControl";
			this.AdditionalProcedureCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.AdditionalProcedureCodesUserControl.TabIndex = 6;
			// 
			// PreferenceCodeDropEdit
			// 
			this.PreferenceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceCodeDropEdit, "JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_PrimaryPreference)));
			this.PreferenceCodeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("a5df5a9a-a8ee-4b22-9c85-cc80151b7b9d", "[36] Pref. Code");
			this.PreferenceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 217, true);
			this.PreferenceCodeDropEdit.Name = "PreferenceCodeDropEdit";
			this.PreferenceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 15, true);
			this.PreferenceCodeDropEdit.TabIndex = 7;
			// 
			// QuotaDropEdit
			// 
			this.QuotaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotaDropEdit, "JI_ConcessionOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_ConcessionOrder)));
			this.QuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 238, true);
			this.QuotaDropEdit.Name = "QuotaDropEdit";
			this.QuotaDropEdit.PreBoundMaxLength = 6;
			this.QuotaDropEdit.ShowDescriptionBox = false;
			this.QuotaDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QuotaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 15, true);
			this.QuotaDropEdit.TabIndex = 8;
			// 
			// SecondQuotaDropEdit
			// 
			this.SecondQuotaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondQuotaDropEdit, "ZG_SecondQuota");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_SecondQuota)));
			this.SecondQuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 259, true);
			this.SecondQuotaDropEdit.Name = "SecondQuotaDropEdit";
			this.SecondQuotaDropEdit.PreBoundMaxLength = 6;
			this.SecondQuotaDropEdit.ShowDescriptionBox = false;
			this.SecondQuotaDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SecondQuotaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 15, true);
			this.SecondQuotaDropEdit.TabIndex = 9;
			// 
			// AdditionalSupplementaryCodesAndGDMUserControl
			// 
			this.AdditionalSupplementaryCodesAndGDMUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesAndGDMUserControl, ".");
			this.AdditionalSupplementaryCodesAndGDMUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 168, true);
			this.AdditionalSupplementaryCodesAndGDMUserControl.Name = "AdditionalSupplementaryCodesAndGDMUserControl";
			this.AdditionalSupplementaryCodesAndGDMUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.AdditionalSupplementaryCodesAndGDMUserControl.TabIndex = 10;
			// 
			// CusNumberCodeFindBox
			// 
			this.CusNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusNumberCodeFindBox, "ZG_CusNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CusNumber)));
			this.CusNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 322, true);
			this.CusNumberCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CusNumberCodeFindBox.Name = "CusNumberCodeFindBox";
			this.CusNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusNumberCodeFindBox.ParentType = null;
			this.CusNumberCodeFindBox.PreBoundMaxLength = 25;
			this.CusNumberCodeFindBox.ShowDescriptionBox = false;
			this.CusNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 15, true);
			this.CusNumberCodeFindBox.TabIndex = 11;
			// 
			// QuotaWithCheckLinkUserControl
			// 
			this.QuotaWithCheckLinkUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotaWithCheckLinkUserControl, ".");
			this.QuotaWithCheckLinkUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 364, true);
			this.QuotaWithCheckLinkUserControl.Name = "QuotaWithCheckLinkUserControl";
			this.QuotaWithCheckLinkUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.QuotaWithCheckLinkUserControl.TabIndex = 12;
			// 
			// ValuationAdjustmentPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValuationAdjustmentPercentageCalcEdit, "JI_ValuationMarkup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_ValuationMarkup)));
			this.ValuationAdjustmentPercentageCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|9dfd52a1-58cc-4f5a-8c09-9d881fbbff13", "[45b] Valn. Adjt. Percent");
			this.ValuationAdjustmentPercentageCalcEdit.DecimalPlaces = 2;
			this.ValuationAdjustmentPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 406, true);
			this.ValuationAdjustmentPercentageCalcEdit.Name = "ValuationAdjustmentPercentageCalcEdit";
			this.ValuationAdjustmentPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 15, true);
			this.ValuationAdjustmentPercentageCalcEdit.TabIndex = 13;
			this.ValuationAdjustmentPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ValuationAdjustmentPercentageCalcEdit.TrackDisposedAccess = true;
			// 
			// TransactionNatureDropEdit
			// 
			this.TransactionNatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionNatureDropEdit, "ZG_TransNature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_TransNature)));
			this.TransactionNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 385, true);
			this.TransactionNatureDropEdit.Name = "TransactionNatureDropEdit";
			this.TransactionNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 15, true);
			this.TransactionNatureDropEdit.TabIndex = 27;
			// 
			// UsedGoodsCodeDropEdit
			// 
			this.UsedGoodsCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UsedGoodsCodeDropEdit, "ZG_UsedGoodsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_UsedGoodsCode)));
			this.UsedGoodsCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 131, true);
			this.UsedGoodsCodeDropEdit.Name = "UsedGoodsCodeDropEdit";
			this.UsedGoodsCodeDropEdit.PreBoundMaxLength = 3;
			this.UsedGoodsCodeDropEdit.ShouldResizeByMaxLength = false;
			this.UsedGoodsCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.UsedGoodsCodeDropEdit.TabIndex = 0;
			// 
			// ReturnToOriginCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReturnToOriginCheckBox, "ZG_ReturnToOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ReturnToOrigin)));
			this.ReturnToOriginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 56, true);
			this.ReturnToOriginCheckBox.Name = "ReturnToOriginCheckBox";
			this.ReturnToOriginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ReturnToOriginCheckBox.TabIndex = 0;
			// 
			// SecondaryTreatedProductCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SecondaryTreatedProductCheckBox, "ZG_SecondaryTreatedProduct");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_SecondaryTreatedProduct)));
			this.SecondaryTreatedProductCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 86, true);
			this.SecondaryTreatedProductCheckBox.Name = "SecondaryTreatedProductCheckBox";
			this.SecondaryTreatedProductCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.SecondaryTreatedProductCheckBox.TabIndex = 0;
			// 
			// ReturningGoodsReasonCodeDropEdit
			// 
			this.ReturningGoodsReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReturningGoodsReasonCodeDropEdit, "ZG_ReturningGoodsReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ReturningGoodsReasonCode)));
			this.ReturningGoodsReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 110, true);
			this.ReturningGoodsReasonCodeDropEdit.Name = "ReturningGoodsReasonCodeDropEdit";
			this.ReturningGoodsReasonCodeDropEdit.PreBoundMaxLength = 3;
			this.ReturningGoodsReasonCodeDropEdit.ShouldResizeByMaxLength = false;
			this.ReturningGoodsReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.ReturningGoodsReasonCodeDropEdit.TabIndex = 0;
			// 
			// ReturningGoodsReasonDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReturningGoodsReasonDetailTextBox, "ZG_ReturningGoodsReasonDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ReturningGoodsReasonDetail)));
			this.ReturningGoodsReasonDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 427, true);
			this.ReturningGoodsReasonDetailTextBox.Name = "ReturningGoodsReasonDetailTextBox";
			this.ReturningGoodsReasonDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 15, true);
			this.ReturningGoodsReasonDetailTextBox.TabIndex = 0;
			// 
			// ExportUnionThreadCodeFindBox
			// 
			this.ExportUnionThreadCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportUnionThreadCodeFindBox, "ZG_ExportUnionThreadCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionThreadCode)));
			this.ExportUnionThreadCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 301, true);
			this.ExportUnionThreadCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ExportUnionThreadCodeFindBox.Name = "ExportUnionThreadCodeFindBox";
			this.ExportUnionThreadCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExportUnionThreadCodeFindBox.ParentType = null;
			this.ExportUnionThreadCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 15, true);
			this.ExportUnionThreadCodeFindBox.TabIndex = 0;
			// 
			// ExportUnionPackCodeFindBox
			// 
			this.ExportUnionPackCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportUnionPackCodeFindBox, "ZG_ExportUnionPackCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionPackCode)));
			this.ExportUnionPackCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 343, true);
			this.ExportUnionPackCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ExportUnionPackCodeFindBox.Name = "ExportUnionPackCodeFindBox";
			this.ExportUnionPackCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExportUnionPackCodeFindBox.ParentType = null;
			this.ExportUnionPackCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 15, true);
			this.ExportUnionPackCodeFindBox.TabIndex = 0;
			// 
			// ExportUnionProductionYearCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionProductionYearCalcEdit, "ZG_ExportUnionProductionYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionProductionYear)));
			this.ExportUnionProductionYearCalcEdit.DecimalPlaces = 0;
			this.ExportUnionProductionYearCalcEdit.Decimals = 0;
			this.ExportUnionProductionYearCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 469, true);
			this.ExportUnionProductionYearCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.ExportUnionProductionYearCalcEdit.Name = "ExportUnionProductionYearCalcEdit";
			this.ExportUnionProductionYearCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.ExportUnionProductionYearCalcEdit.ShowGroupSeparators = false;
			this.ExportUnionProductionYearCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 15, true);
			this.ExportUnionProductionYearCalcEdit.TabIndex = 0;
			this.ExportUnionProductionYearCalcEdit.Text = "0";
			this.ExportUnionProductionYearCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExportUnionProductionYearCalcEdit.TrackDisposedAccess = true;
			// 
			// ExportUnionDeferredInstallmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionDeferredInstallmentTextBox, "ZG_ExportUnionDeferredInstallment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionDeferredInstallment)));
			this.ExportUnionDeferredInstallmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 448, true);
			this.ExportUnionDeferredInstallmentTextBox.Name = "ExportUnionDeferredInstallmentTextBox";
			this.ExportUnionDeferredInstallmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 15, true);
			this.ExportUnionDeferredInstallmentTextBox.TabIndex = 0;
			// 
			// ExportUnionEcologicalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExportUnionEcologicalCheckBox, "ZG_ExportUnionEcological");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionEcological)));
			this.ExportUnionEcologicalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 116, true);
			this.ExportUnionEcologicalCheckBox.Name = "ExportUnionEcologicalCheckBox";
			this.ExportUnionEcologicalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ExportUnionEcologicalCheckBox.TabIndex = 0;
			// 
			// InwardProcessingLicenseLineNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InwardProcessingLicenseLineNumberTextBox, "ZG_InwardProcessingLicenseLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_InwardProcessingLicenseLineNumber)));
			this.InwardProcessingLicenseLineNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 26, true);
			this.InwardProcessingLicenseLineNumberTextBox.Name = "InwardProcessingLicenseLineNumberTextBox";
			this.InwardProcessingLicenseLineNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 15, true);
			this.InwardProcessingLicenseLineNumberTextBox.TabIndex = 0;
			// 
			// EntryExitPurposeCodeDropEdit
			// 
			this.EntryExitPurposeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryExitPurposeCodeDropEdit, "ZG_EntryExitPurposeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_EntryExitPurposeCode)));
			this.EntryExitPurposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 274, true);
			this.EntryExitPurposeCodeDropEdit.Name = "EntryExitPurposeCodeDropEdit";
			this.EntryExitPurposeCodeDropEdit.PreBoundMaxLength = 3;
			this.EntryExitPurposeCodeDropEdit.ShouldResizeByMaxLength = false;
			this.EntryExitPurposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.EntryExitPurposeCodeDropEdit.TabIndex = 0;
			// 
			// EntryExitPurposeDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryExitPurposeDetailTextBox, "ZG_EntryExitPurposeDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_EntryExitPurposeDetail)));
			this.EntryExitPurposeDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 196, true);
			this.EntryExitPurposeDetailTextBox.Name = "EntryExitPurposeDetailTextBox";
			this.EntryExitPurposeDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 15, true);
			this.EntryExitPurposeDetailTextBox.TabIndex = 0;
			// 
			// ProcessingDescriptionLongTextControl
			// 
			this.ProcessingDescriptionLongTextControl.AllowDrop = true;
			this.ProcessingDescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ProcessingDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 142, true);
			this.ProcessingDescriptionLongTextControl.Name = "ProcessingDescriptionLongTextControl";
			this.ProcessingDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.ProcessingDescriptionLongTextControl.TabIndex = 30;
			// 
			// SupplementaryCode1AndGDMUserControl
			// 
			this.SupplementaryCode1AndGDMUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode1AndGDMUserControl, ".");
			this.SupplementaryCode1AndGDMUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 280, true);
			this.SupplementaryCode1AndGDMUserControl.Name = "SupplementaryCode1AndGDMUserControl";
			this.SupplementaryCode1AndGDMUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SupplementaryCode1AndGDMUserControl.TabIndex = 0;
			// 
			// BorderTradeStateCodeFindBox
			// 
			this.BorderTradeStateCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTradeStateCodeFindBox, "ZG_RW_NKBorderTradeStateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_RW_NKBorderTradeStateCode)));
			this.BorderTradeStateCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 222, true);
			this.BorderTradeStateCodeFindBox.Name = "BorderTradeStateCodeFindBox";
			this.BorderTradeStateCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BorderTradeStateCodeFindBox.ParentType = null;
			this.BorderTradeStateCodeFindBox.PreBoundMaxLength = 1;
			this.BorderTradeStateCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 15, true);
			this.BorderTradeStateCodeFindBox.TabIndex = 13;
			// 
			// ExportUnionAdditionalTariffCodeFindBox
			// 
			this.ExportUnionAdditionalTariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportUnionAdditionalTariffCodeFindBox, "ZG_ExportUnionAdditionalTariffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExportUnionAdditionalTariffCode)));
			this.ExportUnionAdditionalTariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 248, true);
			this.ExportUnionAdditionalTariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			this.ExportUnionAdditionalTariffCodeFindBox.Name = "ExportUnionAdditionalTariffCodeFindBox";
			this.ExportUnionAdditionalTariffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExportUnionAdditionalTariffCodeFindBox.ParentType = null;
			this.ExportUnionAdditionalTariffCodeFindBox.PreBoundMaxLength = 1;
			this.ExportUnionAdditionalTariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 15, true);
			this.ExportUnionAdditionalTariffCodeFindBox.TabIndex = 13;
			// 
			// ExcessStockCheckBox
			// 
			this.ExcessStockCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcessStockCheckBox, "ZG_ExcessStock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_ExcessStock)));
			this.ExcessStockCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExcessStockCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 322, true);
			this.ExcessStockCheckBox.Name = "ExcessStockCheckBox";
			this.ExcessStockCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.ExcessStockCheckBox.TabIndex = 8;
			this.ExcessStockCheckBox.UseVisualStyleBackColor = true;
			// 
			// CommercialPaymentCodeDropEdit
			// 
			this.CommercialPaymentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialPaymentCodeDropEdit, "ZG_CommercialPaymentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_CommercialPaymentCode)));
			this.CommercialPaymentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 342, true);
			this.CommercialPaymentCodeDropEdit.Name = "CommercialPaymentCodeDropEdit";
			this.CommercialPaymentCodeDropEdit.PreBoundMaxLength = 3;
			this.CommercialPaymentCodeDropEdit.ShouldResizeByMaxLength = false;
			this.CommercialPaymentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CommercialPaymentCodeDropEdit.TabIndex = 0;
			// 
			// ValuationCodeFindBox
			// 
			this.ValuationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationCodeFindBox, "JI_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_ValuationCode)));
			this.ValuationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 343, true);
			this.ValuationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ValuationCodeFindBox.Name = "ValuationCodeFindBox";
			this.ValuationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ValuationCodeFindBox.ParentType = null;
			this.ValuationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 15, true);
			this.ValuationCodeFindBox.TabIndex = 0;
			// 
			// UnformattedProcedureCodeFindBox
			// 
			this.UnformattedProcedureCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnformattedProcedureCodeFindBox, "JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_Procedure)));
			this.UnformattedProcedureCodeFindBox.BindToList = null;
			this.UnformattedProcedureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 364, true);
			this.UnformattedProcedureCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			this.UnformattedProcedureCodeFindBox.Name = "UnformattedProcedureCodeFindBox";
			this.UnformattedProcedureCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UnformattedProcedureCodeFindBox.ParentType = null;
			this.UnformattedProcedureCodeFindBox.PreBoundMaxLength = 3;
			this.UnformattedProcedureCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 15, true);
			this.UnformattedProcedureCodeFindBox.TabIndex = 31;
			// 
			// RegionOfDestinationDropEdit
			// 
			this.RegionOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionOfDestinationDropEdit, "ZG_RegionOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_RegionOfDestination)));
			this.RegionOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 50, true);
			this.RegionOfDestinationDropEdit.Name = "RegionOfDestinationDropEdit";
			this.RegionOfDestinationDropEdit.PreBoundMaxLength = 1;
			this.RegionOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 15, true);
			this.RegionOfDestinationDropEdit.TabIndex = 23;
			// 
			// PriceTypeDropEdit
			// 
			this.PriceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PriceTypeDropEdit, "ZG_PriceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).ZG_PriceType)));
			this.PriceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 385, true);
			this.PriceTypeDropEdit.Name = "PriceTypeDropEdit";
			this.PriceTypeDropEdit.PreBoundMaxLength = 2;
			this.PriceTypeDropEdit.ShouldResizeByMaxLength = false;
			this.PriceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 15, true);
			this.PriceTypeDropEdit.TabIndex = 0;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnformattedProcedureCodeFindBox);
			this.Controls.Add(this.ProcessingDescriptionLongTextControl);
			this.Controls.Add(this.InwardProcessingLicenseLineNumberTextBox);
			this.Controls.Add(this.TransactionNatureDropEdit);
			this.Controls.Add(this.FormattedProcedureCodeFindBox);
			this.Controls.Add(this.DestinationCodeFindBox);
			this.Controls.Add(this.DispatchCodeFindBox);
			this.Controls.Add(this.DestinationUsingZZRefCusCodeListCodeFindBox);
			this.Controls.Add(this.SupplementaryCode1DropEdit);
			this.Controls.Add(this.SupplementaryCode2DropEdit);
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Controls.Add(this.CountryOfSupplyCodeFindBox);
			this.Controls.Add(this.AdditionalProcedureCodesUserControl);
			this.Controls.Add(this.PreferenceCodeDropEdit);
			this.Controls.Add(this.QuotaDropEdit);
			this.Controls.Add(this.SecondQuotaDropEdit);
			this.Controls.Add(this.AdditionalSupplementaryCodesAndGDMUserControl);
			this.Controls.Add(this.CusNumberCodeFindBox);
			this.Controls.Add(this.QuotaWithCheckLinkUserControl);
			this.Controls.Add(this.ValuationAdjustmentPercentageCalcEdit);
			this.Controls.Add(this.UsedGoodsCodeDropEdit);
			this.Controls.Add(this.ReturnToOriginCheckBox);
			this.Controls.Add(this.SecondaryTreatedProductCheckBox);
			this.Controls.Add(this.ReturningGoodsReasonCodeDropEdit);
			this.Controls.Add(this.ReturningGoodsReasonDetailTextBox);
			this.Controls.Add(this.ExportUnionEcologicalCheckBox);
			this.Controls.Add(this.ExportUnionDeferredInstallmentTextBox);
			this.Controls.Add(this.ExportUnionProductionYearCalcEdit);
			this.Controls.Add(this.ExportUnionPackCodeFindBox);
			this.Controls.Add(this.ExportUnionThreadCodeFindBox);
			this.Controls.Add(this.EntryExitPurposeCodeDropEdit);
			this.Controls.Add(this.EntryExitPurposeDetailTextBox);
			this.Controls.Add(this.SupplementaryCode1AndGDMUserControl);
			this.Controls.Add(this.BorderTradeStateCodeFindBox);
			this.Controls.Add(this.ExportUnionAdditionalTariffCodeFindBox);
			this.Controls.Add(this.ExcessStockCheckBox);
			this.Controls.Add(this.CommercialPaymentCodeDropEdit);
			this.Controls.Add(this.ValuationCodeFindBox);
			this.Controls.Add(this.NationalAdditionalCode2DropEdit);
			this.Controls.Add(this.NationalAdditionalCode1DropEdit);
			this.Controls.Add(this.NationalAdditionalCodesUserControl);
			this.Controls.Add(this.RegionOfDestinationDropEdit);
			this.Controls.Add(this.PriceTypeDropEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NationalAdditionalCode1DropEdit.ResumeLayout(true);
			this.NationalAdditionalCode1DropEdit.PerformLayout();
			this.NationalAdditionalCode2DropEdit.ResumeLayout(true);
			this.NationalAdditionalCode2DropEdit.PerformLayout();
			this.NationalAdditionalCodesUserControl.ResumeLayout(true);
			this.NationalAdditionalCodesUserControl.PerformLayout();
			this.SupplementaryCode2DropEdit.ResumeLayout(true);
			this.SupplementaryCode2DropEdit.PerformLayout();
			this.SupplementaryCode1DropEdit.ResumeLayout(true);
			this.SupplementaryCode1DropEdit.PerformLayout();
			this.FormattedProcedureCodeFindBox.ResumeLayout(true);
			this.FormattedProcedureCodeFindBox.PerformLayout();
			this.DestinationCodeFindBox.ResumeLayout(true);
			this.DestinationCodeFindBox.PerformLayout();
			this.DispatchCodeFindBox.ResumeLayout(true);
			this.DispatchCodeFindBox.PerformLayout();
			this.DestinationUsingZZRefCusCodeListCodeFindBox.ResumeLayout(true);
			this.DestinationUsingZZRefCusCodeListCodeFindBox.PerformLayout();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.CountryOfSupplyCodeFindBox.ResumeLayout(true);
			this.CountryOfSupplyCodeFindBox.PerformLayout();
			this.AdditionalProcedureCodesUserControl.ResumeLayout(true);
			this.AdditionalProcedureCodesUserControl.PerformLayout();
			this.PreferenceCodeDropEdit.ResumeLayout(true);
			this.PreferenceCodeDropEdit.PerformLayout();
			this.QuotaDropEdit.ResumeLayout(true);
			this.QuotaDropEdit.PerformLayout();
			this.SecondQuotaDropEdit.ResumeLayout(true);
			this.SecondQuotaDropEdit.PerformLayout();
			this.AdditionalSupplementaryCodesAndGDMUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesAndGDMUserControl.PerformLayout();
			this.CusNumberCodeFindBox.ResumeLayout(true);
			this.CusNumberCodeFindBox.PerformLayout();
			this.QuotaWithCheckLinkUserControl.ResumeLayout(true);
			this.QuotaWithCheckLinkUserControl.PerformLayout();
			this.TransactionNatureDropEdit.ResumeLayout(true);
			this.TransactionNatureDropEdit.PerformLayout();
			this.UsedGoodsCodeDropEdit.ResumeLayout(true);
			this.UsedGoodsCodeDropEdit.PerformLayout();
			this.ReturningGoodsReasonCodeDropEdit.ResumeLayout(true);
			this.ReturningGoodsReasonCodeDropEdit.PerformLayout();
			this.ExportUnionThreadCodeFindBox.ResumeLayout(true);
			this.ExportUnionThreadCodeFindBox.PerformLayout();
			this.ExportUnionPackCodeFindBox.ResumeLayout(true);
			this.ExportUnionPackCodeFindBox.PerformLayout();
			this.EntryExitPurposeCodeDropEdit.ResumeLayout(true);
			this.EntryExitPurposeCodeDropEdit.PerformLayout();
			this.ProcessingDescriptionLongTextControl.ResumeLayout(true);
			this.ProcessingDescriptionLongTextControl.PerformLayout();
			this.SupplementaryCode1AndGDMUserControl.ResumeLayout(true);
			this.SupplementaryCode1AndGDMUserControl.PerformLayout();
			this.BorderTradeStateCodeFindBox.ResumeLayout(true);
			this.BorderTradeStateCodeFindBox.PerformLayout();
			this.ExportUnionAdditionalTariffCodeFindBox.ResumeLayout(true);
			this.ExportUnionAdditionalTariffCodeFindBox.PerformLayout();
			this.CommercialPaymentCodeDropEdit.ResumeLayout(true);
			this.CommercialPaymentCodeDropEdit.PerformLayout();
			this.ValuationCodeFindBox.ResumeLayout(true);
			this.ValuationCodeFindBox.PerformLayout();
			this.UnformattedProcedureCodeFindBox.ResumeLayout(true);
			this.UnformattedProcedureCodeFindBox.PerformLayout();
			this.RegionOfDestinationDropEdit.ResumeLayout(true);
			this.RegionOfDestinationDropEdit.PerformLayout();
			this.PriceTypeDropEdit.ResumeLayout(true);
			this.PriceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDropEdit NationalAdditionalCode1DropEdit;
		internal ZArchitecture.GUI.ZDropEdit NationalAdditionalCode2DropEdit;
		internal NationalAdditionalCodesUserControl NationalAdditionalCodesUserControl;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FormattedProcedureCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode2DropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode1DropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DispatchCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationUsingZZRefCusCodeListCodeFindBox;
		internal AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfSupplyCodeFindBox;
		internal AdditionalProcedureCodesUserControl AdditionalProcedureCodesUserControl;
		internal ZArchitecture.GUI.ZDropEdit PreferenceCodeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit QuotaDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SecondQuotaDropEdit;
		internal AdditionalSupplementaryCodesAndGDMUserControl AdditionalSupplementaryCodesAndGDMUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox CusNumberCodeFindBox;
		internal QuotaWithCheckLinkUserControl QuotaWithCheckLinkUserControl;
		internal Enterprise.ZArchitecture.ZCalcEdit ValuationAdjustmentPercentageCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit TransactionNatureDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit UsedGoodsCodeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ReturnToOriginCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox SecondaryTreatedProductCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ReturningGoodsReasonCodeDropEdit;
		internal ZArchitecture.ZTextBox ReturningGoodsReasonDetailTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox ExportUnionThreadCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox ExportUnionPackCodeFindBox;
		internal ZArchitecture.ZCalcEdit ExportUnionProductionYearCalcEdit;
		internal ZArchitecture.ZTextBox ExportUnionDeferredInstallmentTextBox;
		internal ZArchitecture.GUI.ZCheckBox ExportUnionEcologicalCheckBox;
		internal ZArchitecture.ZTextBox InwardProcessingLicenseLineNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit EntryExitPurposeCodeDropEdit;
		internal ZArchitecture.ZTextBox EntryExitPurposeDetailTextBox;
		internal Customs.GUI.LongTextControl ProcessingDescriptionLongTextControl;
		internal SupplementaryCode1AndGDMUserControl SupplementaryCode1AndGDMUserControl;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox BorderTradeStateCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ExportUnionAdditionalTariffCodeFindBox;
		internal ZArchitecture.GUI.ZCheckBox ExcessStockCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CommercialPaymentCodeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ValuationCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox UnformattedProcedureCodeFindBox;
		internal ZDropEdit RegionOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PriceTypeDropEdit;
	}
}
