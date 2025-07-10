namespace Enterprise.Customs.BR.GUI
{
	public partial class InvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.CPCSecondDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPCThirdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPCFourthDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BRNFENumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BRNFEItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ComplementaryDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.CargoPriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CPCGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NFeLinePriceCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ImportLicenseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NaladiNccaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NaladiHsCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsConditionSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.GoodsConditionOperationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UsedMaterialRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManufacturerIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BrandGoodsConditionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SerialNumberGoodsConditionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelGoodsConditionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.YearGoodsConditionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DutyTaxRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyTaxRegimeSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.DutyLegalBaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportLicenseFineSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ImportLicenseAuthorizationDateTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportLicenseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportLicenseFeeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffAgreementSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.TariffAgreementDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsApplicationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FullGoodsDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.RequiresImportLicenseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntryInstructionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CPCSecondDropEdit.SuspendLayout();
			this.CPCThirdDropEdit.SuspendLayout();
			this.CPCFourthDropEdit.SuspendLayout();
			this.ComplementaryDescriptionTextBox.SuspendLayout();
			this.CargoPriorityDropEdit.SuspendLayout();
			this.CountryDestinationCodeFindBox.SuspendLayout();
			this.CPCGroupBox.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.NFeLinePriceCalcEdit.SuspendLayout();
			this.NaladiNccaCodeFindBox.SuspendLayout();
			this.NaladiHsCodeFindBox.SuspendLayout();
			this.GoodsConditionSeparatorUserControl.SuspendLayout();
			this.GoodsConditionOperationTypeDropEdit.SuspendLayout();
			this.UsedMaterialRegimeDropEdit.SuspendLayout();
			this.ManufacturerIndicatorDropEdit.SuspendLayout();
			this.DutyTaxRegimeDropEdit.SuspendLayout();
			this.DutyTaxRegimeSeparatorUserControl.SuspendLayout();
			this.DutyLegalBaseDropEdit.SuspendLayout();
			this.ImportLicenseFineSeparatorUserControl.SuspendLayout();
			this.ImportLicenseAuthorizationDateTextBox.SuspendLayout();
			this.ImportLicenseTypeDropEdit.SuspendLayout();
			this.ImportLicenseFeeTypeDropEdit.SuspendLayout();
			this.TariffAgreementSeparatorUserControl.SuspendLayout();
			this.TariffAgreementDropEdit.SuspendLayout();
			this.GoodsApplicationDropEdit.SuspendLayout();
			this.GoodsConditionDropEdit.SuspendLayout();
			this.FullGoodsDescriptionTextBox.SuspendLayout();
			this.EntryInstructionGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// CPCSecondDropEdit
			// 
			this.CPCSecondDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCSecondDropEdit, "JI_SecondCPC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_SecondCPC)));
			this.CPCSecondDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 38, true);
			this.CPCSecondDropEdit.Name = "CPCSecondDropEdit";
			this.CPCSecondDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.CPCSecondDropEdit.TabIndex = 2;
			// 
			// CPCThirdDropEdit
			// 
			this.CPCThirdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCThirdDropEdit, "JI_ThirdCPC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ThirdCPC)));
			this.CPCThirdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 63, true);
			this.CPCThirdDropEdit.Name = "CPCThirdDropEdit";
			this.CPCThirdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.CPCThirdDropEdit.TabIndex = 3;
			// 
			// CPCFourthDropEdit
			// 
			this.CPCFourthDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCFourthDropEdit, "JI_FourthCPC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_FourthCPC)));
			this.CPCFourthDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 89, true);
			this.CPCFourthDropEdit.Name = "CPCFourthDropEdit";
			this.CPCFourthDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.CPCFourthDropEdit.TabIndex = 4;
			// 
			// BRNFENumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BRNFENumberTextBox, "JI_NFeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_NFeNumber)));
			this.BRNFENumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 68, true);
			this.BRNFENumberTextBox.Name = "BRNFENumberTextBox";
			this.BRNFENumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 20, true);
			this.BRNFENumberTextBox.TabIndex = 8;
			// 
			// BRNFEItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BRNFEItemNumberTextBox, "JI_NFeItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_NFeItemNumber)));
			this.BRNFEItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 92, true);
			this.BRNFEItemNumberTextBox.Name = "BRNFEItemNumberTextBox";
			this.BRNFEItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.BRNFEItemNumberTextBox.TabIndex = 9;
			// 
			// ComplementaryDescriptionTextBox
			// 
			this.ComplementaryDescriptionTextBox.AllowDrop = true;
			this.ComplementaryDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ComplementaryDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 42, true);
			this.ComplementaryDescriptionTextBox.Name = "ComplementaryDescriptionTextBox";
			this.ComplementaryDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 20, true);
			this.ComplementaryDescriptionTextBox.TabIndex = 7;
			// 
			// CargoPriorityDropEdit
			// 
			this.CargoPriorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoPriorityDropEdit, "JI_CargoPriority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_CargoPriority)));
			this.CargoPriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 117, true);
			this.CargoPriorityDropEdit.Name = "CargoPriorityDropEdit";
			this.CargoPriorityDropEdit.PreBoundMaxLength = 4;
			this.CargoPriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 20, true);
			this.CargoPriorityDropEdit.TabIndex = 11;
			// 
			// CountryDestinationCodeFindBox
			// 
			this.CountryDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryDestinationCodeFindBox, "JI_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_RN_NKCountryOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).Lookups.CountryList)));
			this.CountryDestinationCodeFindBox.BindToList = "Lookups+CountryList";
			this.CountryDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 146, true);
			this.CountryDestinationCodeFindBox.Name = "CountryDestinationCodeFindBox";
			this.CountryDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryDestinationCodeFindBox.ParentType = null;
			this.CountryDestinationCodeFindBox.PreBoundMaxLength = 2;
			this.CountryDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 20, true);
			this.CountryDestinationCodeFindBox.TabIndex = 10;
			// 
			// CPCGroupBox
			// 
			this.CPCGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("42DA18A5-340C-4640-AB60-0F208485F105", "CPC");
			this.CPCGroupBox.Controls.Add(this.CPCDropEdit);
			this.CPCGroupBox.Controls.Add(this.CPCFourthDropEdit);
			this.CPCGroupBox.Controls.Add(this.CPCThirdDropEdit);
			this.CPCGroupBox.Controls.Add(this.CPCSecondDropEdit);
			this.CPCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 198, true);
			this.CPCGroupBox.Name = "CPCGroupBox";
			this.CPCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 120, true);
			this.CPCGroupBox.TabIndex = 12;
			this.CPCGroupBox.TabStop = false;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_Procedure)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 13, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.CPCDropEdit.TabIndex = 1;
			// 
			// NFeLinePriceCalcEdit
			// 
			this.NFeLinePriceCalcEdit.AllowDrop = true;
			this.NFeLinePriceCalcEdit.BindToAmount = "JI_NFeLinePrice";
			this.NFeLinePriceCalcEdit.BindToUnit = "JI_NFeLinePriceCurrency";
			this.NFeLinePriceCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.NFeLinePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 324, true);
			this.NFeLinePriceCalcEdit.Name = "NFeLinePriceCalcEdit";
			this.NFeLinePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.NFeLinePriceCalcEdit.TabIndex = 5;
			// 
			// ImportLicenseNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportLicenseNumberTextBox, "ImportLicenseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ImportLicenseNumber)));
			this.ImportLicenseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 13, true);
			this.ImportLicenseNumberTextBox.Name = "ImportLicenseNumberTextBox";
			this.ImportLicenseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 20, true);
			this.ImportLicenseNumberTextBox.TabIndex = 14;
			// 
			// NaladiNccaCodeFindBox
			// 
			this.NaladiNccaCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NaladiNccaCodeFindBox, "NaladiNcca");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).NaladiNcca)));
			this.NaladiNccaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 39, true);
			this.NaladiNccaCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			this.NaladiNccaCodeFindBox.Name = "NaladiNccaCodeFindBox";
			this.NaladiNccaCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NaladiNccaCodeFindBox.ParentType = null;
			this.NaladiNccaCodeFindBox.PreBoundMaxLength = 8;
			this.NaladiNccaCodeFindBox.ShowDescriptionBox = false;
			this.NaladiNccaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.NaladiNccaCodeFindBox.TabIndex = 15;
			// 
			// NaladiHsCodeFindBox
			// 
			this.NaladiHsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NaladiHsCodeFindBox, "NaladiHs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).NaladiHs)));
			this.NaladiHsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 65, true);
			this.NaladiHsCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			this.NaladiHsCodeFindBox.Name = "NaladiHsCodeFindBox";
			this.NaladiHsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NaladiHsCodeFindBox.ParentType = null;
			this.NaladiHsCodeFindBox.PreBoundMaxLength = 8;
			this.NaladiHsCodeFindBox.ShowDescriptionBox = false;
			this.NaladiHsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.NaladiHsCodeFindBox.TabIndex = 16;
			// 
			// GoodsConditionSeparatorUserControl
			// 
			this.GoodsConditionSeparatorUserControl.AllowDrop = true;
			this.GoodsConditionSeparatorUserControl.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9CF6188B-6538-474F-BEB1-D2A3604ACEF0", "Goods Condition");
			this.GoodsConditionSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 350, true);
			this.GoodsConditionSeparatorUserControl.Name = "GoodsConditionSeparatorUserControl";
			this.GoodsConditionSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.GoodsConditionSeparatorUserControl.TabIndex = 0;
			// 
			// GoodsConditionOperationTypeDropEdit
			// 
			this.GoodsConditionOperationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsConditionOperationTypeDropEdit, "JI_UsedMaterialOperationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_UsedMaterialOperationType)));
			this.GoodsConditionOperationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 393, true);
			this.GoodsConditionOperationTypeDropEdit.Name = "GoodsConditionOperationTypeDropEdit";
			this.GoodsConditionOperationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.GoodsConditionOperationTypeDropEdit.TabIndex = 1;
			// 
			// UsedMaterialRegimeDropEdit
			// 
			this.UsedMaterialRegimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UsedMaterialRegimeDropEdit, "JI_UsedMaterialRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_UsedMaterialRegime)));
			this.UsedMaterialRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 369, true);
			this.UsedMaterialRegimeDropEdit.Name = "UsedMaterialRegimeDropEdit";
			this.UsedMaterialRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.UsedMaterialRegimeDropEdit.TabIndex = 0;
			// 
			// ManufacturerIndicatorDropEdit
			// 
			this.ManufacturerIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerIndicatorDropEdit, "JI_ManufacturerIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ManufacturerIndicator)));
			this.ManufacturerIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 91, true);
			this.ManufacturerIndicatorDropEdit.Name = "ManufacturerIndicatorDropEdit";
			this.ManufacturerIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ManufacturerIndicatorDropEdit.TabIndex = 17;
			// 
			// BrandGoodsConditionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandGoodsConditionTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_BrandName)));
			this.BrandGoodsConditionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 416, true);
			this.BrandGoodsConditionTextBox.Name = "BrandGoodsConditionTextBox";
			this.BrandGoodsConditionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.BrandGoodsConditionTextBox.TabIndex = 18;
			// 
			// SerialNumberGoodsConditionTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNumberGoodsConditionTextBox, "JI_UsedMaterialSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_UsedMaterialSerialNumber)));
			this.SerialNumberGoodsConditionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 416, true);
			this.SerialNumberGoodsConditionTextBox.Name = "SerialNumberGoodsConditionTextBox";
			this.SerialNumberGoodsConditionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.SerialNumberGoodsConditionTextBox.TabIndex = 19;
			// 
			// ModelGoodsConditionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelGoodsConditionTextBox, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_Model)));
			this.ModelGoodsConditionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 437, true);
			this.ModelGoodsConditionTextBox.Name = "ModelGoodsConditionTextBox";
			this.ModelGoodsConditionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ModelGoodsConditionTextBox.TabIndex = 20;
			// 
			// YearGoodsConditionTextBox
			// 
			this.BindingSource.SetBindingMember(this.YearGoodsConditionTextBox, "JI_UsedMaterialManufactureYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_UsedMaterialManufactureYear)));
			this.YearGoodsConditionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 437, true);
			this.YearGoodsConditionTextBox.Name = "YearGoodsConditionTextBox";
			this.YearGoodsConditionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.YearGoodsConditionTextBox.TabIndex = 21;
			// 
			// DutyTaxRegimeDropEdit
			// 
			this.DutyTaxRegimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyTaxRegimeDropEdit, "DutyTaxRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyTaxRegime)));
			this.DutyTaxRegimeDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("d605aeaa-f2df-4646-a313-30a0cc0be60f", "Tax Regime");
			this.DutyTaxRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 146, true);
			this.DutyTaxRegimeDropEdit.Name = "DutyTaxRegimeDropEdit";
			this.DutyTaxRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.DutyTaxRegimeDropEdit.TabIndex = 22;
			// 
			// DutyTaxRegimeSeparatorUserControl
			// 
			this.DutyTaxRegimeSeparatorUserControl.AllowDrop = true;
			this.DutyTaxRegimeSeparatorUserControl.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("74bb7d4e-e3fd-449e-996b-c1fb71735d3d", "Tax Regime");
			this.DutyTaxRegimeSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 119, true);
			this.DutyTaxRegimeSeparatorUserControl.Name = "DutyTaxRegimeSeparatorUserControl";
			this.DutyTaxRegimeSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.DutyTaxRegimeSeparatorUserControl.TabIndex = 23;
			// 
			// DutyLegalBaseDropEdit
			// 
			this.DutyLegalBaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyLegalBaseDropEdit, "DutyLegalBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyLegalBase)));
			this.DutyLegalBaseDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("11ef74fa-6aed-4f83-a19c-beb191c996d9", "Legal Basis");
			this.DutyLegalBaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 172, true);
			this.DutyLegalBaseDropEdit.Name = "DutyLegalBaseDropEdit";
			this.DutyLegalBaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.DutyLegalBaseDropEdit.TabIndex = 24;
			// 
			// ImportLicenseFineSeparatorUserControl
			// 
			this.ImportLicenseFineSeparatorUserControl.AllowDrop = true;
			this.ImportLicenseFineSeparatorUserControl.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a7897290-a90b-4e4a-ad07-29955aedf062", "Import License Fine");
			this.ImportLicenseFineSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 298, true);
			this.ImportLicenseFineSeparatorUserControl.Name = "ImportLicenseFineSeparatorUserControl";
			this.ImportLicenseFineSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.ImportLicenseFineSeparatorUserControl.TabIndex = 28;
			// 
			// ImportLicenseAuthorizationDateTextBox
			// 
			this.ImportLicenseAuthorizationDateTextBox.AllowDrop = true;
			this.ImportLicenseAuthorizationDateTextBox.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ImportLicenseAuthorizationDateTextBox, "ImportLicenseAuthorizationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ImportLicenseAuthorizationDate)));
			this.ImportLicenseAuthorizationDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 345, true);
			this.ImportLicenseAuthorizationDateTextBox.Name = "ImportLicenseAuthorizationDateTextBox";
			this.ImportLicenseAuthorizationDateTextBox.TabIndex = 26;
			// 
			// ImportLicenseTypeDropEdit
			// 
			this.ImportLicenseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportLicenseTypeDropEdit, "ImportLicenseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ImportLicenseType)));
			this.ImportLicenseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 319, true);
			this.ImportLicenseTypeDropEdit.Name = "ImportLicenseTypeDropEdit";
			this.ImportLicenseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ImportLicenseTypeDropEdit.TabIndex = 25;
			// 
			// ImportLicenseFeeTypeDropEdit
			// 
			this.ImportLicenseFeeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportLicenseFeeTypeDropEdit, "ImportLicenseFeeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ImportLicenseFeeType)));
			this.ImportLicenseFeeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 371, true);
			this.ImportLicenseFeeTypeDropEdit.Name = "ImportLicenseFeeTypeDropEdit";
			this.ImportLicenseFeeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ImportLicenseFeeTypeDropEdit.TabIndex = 27;
			// 
			// TariffAgreementSeparatorUserControl
			// 
			this.TariffAgreementSeparatorUserControl.AllowDrop = true;
			this.TariffAgreementSeparatorUserControl.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0b92266f-8276-42f5-9ef4-b7d0c6e86f0f", "Tariff Agreement");
			this.TariffAgreementSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 416, true);
			this.TariffAgreementSeparatorUserControl.Name = "TariffAgreementSeparatorUserControl";
			this.TariffAgreementSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.TariffAgreementSeparatorUserControl.TabIndex = 29;
			// 
			// TariffAgreementDropEdit
			// 
			this.TariffAgreementDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffAgreementDropEdit, "JI_SecondaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_SecondaryPreference)));
			this.TariffAgreementDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("75653e6a-4c18-4a62-bcd4-393cee06f2e7", "Agreement");
			this.TariffAgreementDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 437, true);
			this.TariffAgreementDropEdit.Name = "TariffAgreementDropEdit";
			this.TariffAgreementDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.TariffAgreementDropEdit.TabIndex = 30;
			// 
			// GoodsApplicationDropEdit
			// 
			this.GoodsApplicationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsApplicationDropEdit, "JI_GoodsApplication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_GoodsApplication)));
			this.GoodsApplicationDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("92c22aa5-25c0-428c-8a50-5db62766b6e0", "Goods Application");
			this.GoodsApplicationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 172, true);
			this.GoodsApplicationDropEdit.Name = "GoodsApplicationDropEdit";
			this.GoodsApplicationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.GoodsApplicationDropEdit.TabIndex = 29;
			// 
			// GoodsConditionDropEdit
			// 
			this.GoodsConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsConditionDropEdit, "JI_GoodsCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_GoodsCondition)));
			this.GoodsConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 207, true);
			this.GoodsConditionDropEdit.Name = "GoodsConditionDropEdit";
			this.GoodsConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.GoodsConditionDropEdit.TabIndex = 31;
			// 
			// FullGoodsDescriptionTextBox
			// 
			this.FullGoodsDescriptionTextBox.AllowDrop = true;
			this.FullGoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.FullGoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 235, true);
			this.FullGoodsDescriptionTextBox.Name = "FullGoodsDescriptionTextBox";
			this.FullGoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.FullGoodsDescriptionTextBox.TabIndex = 32;
			// 
			// RequiresImportLicenseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RequiresImportLicenseCheckBox, "JI_RequiresImportLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_RequiresImportLicense)));
			this.RequiresImportLicenseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(892, 11, true);
			this.RequiresImportLicenseCheckBox.Name = "RequiresImportLicenseCheckBox";
			this.RequiresImportLicenseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 24, true);
			this.RequiresImportLicenseCheckBox.TabIndex = 34;
			this.RequiresImportLicenseCheckBox.UseVisualStyleBackColor = true;
			// 
			// EntryInstructionGuidDropEdit
			// 
			this.EntryInstructionGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryInstructionGuidDropEdit, "JI_CEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_CEI)));
			this.EntryInstructionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 463, true);
			this.EntryInstructionGuidDropEdit.Name = "EntryInstructionGuidDropEdit";
			this.EntryInstructionGuidDropEdit.ShowDescriptionBox = false;
			this.EntryInstructionGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.EntryInstructionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.EntryInstructionGuidDropEdit.TabIndex = 35;
			this.EntryInstructionGuidDropEdit.UseFullWidthForCodeBox = true;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryInstructionGuidDropEdit);
			this.Controls.Add(this.RequiresImportLicenseCheckBox);
			this.Controls.Add(this.FullGoodsDescriptionTextBox);
			this.Controls.Add(this.GoodsConditionDropEdit);
			this.Controls.Add(this.TariffAgreementDropEdit);
			this.Controls.Add(this.TariffAgreementSeparatorUserControl);
			this.Controls.Add(this.GoodsApplicationDropEdit);
			this.Controls.Add(this.ImportLicenseFeeTypeDropEdit);
			this.Controls.Add(this.ImportLicenseFineSeparatorUserControl);
			this.Controls.Add(this.ImportLicenseAuthorizationDateTextBox);
			this.Controls.Add(this.ImportLicenseTypeDropEdit);
			this.Controls.Add(this.DutyLegalBaseDropEdit);
			this.Controls.Add(this.DutyTaxRegimeSeparatorUserControl);
			this.Controls.Add(this.DutyTaxRegimeDropEdit);
			this.Controls.Add(this.YearGoodsConditionTextBox);
			this.Controls.Add(this.ModelGoodsConditionTextBox);
			this.Controls.Add(this.SerialNumberGoodsConditionTextBox);
			this.Controls.Add(this.BrandGoodsConditionTextBox);
			this.Controls.Add(this.ManufacturerIndicatorDropEdit);
			this.Controls.Add(this.GoodsConditionSeparatorUserControl);
			this.Controls.Add(this.UsedMaterialRegimeDropEdit);
			this.Controls.Add(this.GoodsConditionOperationTypeDropEdit);
			this.Controls.Add(this.NaladiNccaCodeFindBox);
			this.Controls.Add(this.NaladiHsCodeFindBox);
			this.Controls.Add(this.ImportLicenseNumberTextBox);
			this.Controls.Add(this.CPCGroupBox);
			this.Controls.Add(this.BRNFENumberTextBox);
			this.Controls.Add(this.BRNFEItemNumberTextBox);
			this.Controls.Add(this.ComplementaryDescriptionTextBox);
			this.Controls.Add(this.CargoPriorityDropEdit);
			this.Controls.Add(this.CountryDestinationCodeFindBox);
			this.Controls.Add(this.NFeLinePriceCalcEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CPCSecondDropEdit.ResumeLayout(true);
			this.CPCSecondDropEdit.PerformLayout();
			this.CPCThirdDropEdit.ResumeLayout(true);
			this.CPCThirdDropEdit.PerformLayout();
			this.CPCFourthDropEdit.ResumeLayout(true);
			this.CPCFourthDropEdit.PerformLayout();
			this.ComplementaryDescriptionTextBox.ResumeLayout(true);
			this.ComplementaryDescriptionTextBox.PerformLayout();
			this.CargoPriorityDropEdit.ResumeLayout(true);
			this.CargoPriorityDropEdit.PerformLayout();
			this.CountryDestinationCodeFindBox.ResumeLayout(true);
			this.CountryDestinationCodeFindBox.PerformLayout();
			this.CPCGroupBox.ResumeLayout(false);
			this.CPCGroupBox.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.NFeLinePriceCalcEdit.ResumeLayout(true);
			this.NFeLinePriceCalcEdit.PerformLayout();
			this.NaladiNccaCodeFindBox.ResumeLayout(true);
			this.NaladiNccaCodeFindBox.PerformLayout();
			this.NaladiHsCodeFindBox.ResumeLayout(true);
			this.NaladiHsCodeFindBox.PerformLayout();
			this.GoodsConditionSeparatorUserControl.ResumeLayout(true);
			this.GoodsConditionSeparatorUserControl.PerformLayout();
			this.GoodsConditionOperationTypeDropEdit.ResumeLayout(true);
			this.GoodsConditionOperationTypeDropEdit.PerformLayout();
			this.UsedMaterialRegimeDropEdit.ResumeLayout(true);
			this.UsedMaterialRegimeDropEdit.PerformLayout();
			this.ManufacturerIndicatorDropEdit.ResumeLayout(true);
			this.ManufacturerIndicatorDropEdit.PerformLayout();
			this.DutyTaxRegimeDropEdit.ResumeLayout(true);
			this.DutyTaxRegimeDropEdit.PerformLayout();
			this.DutyTaxRegimeSeparatorUserControl.ResumeLayout(true);
			this.DutyTaxRegimeSeparatorUserControl.PerformLayout();
			this.DutyLegalBaseDropEdit.ResumeLayout(true);
			this.DutyLegalBaseDropEdit.PerformLayout();
			this.ImportLicenseFineSeparatorUserControl.ResumeLayout(true);
			this.ImportLicenseFineSeparatorUserControl.PerformLayout();
			this.ImportLicenseAuthorizationDateTextBox.ResumeLayout(true);
			this.ImportLicenseAuthorizationDateTextBox.PerformLayout();
			this.ImportLicenseTypeDropEdit.ResumeLayout(true);
			this.ImportLicenseTypeDropEdit.PerformLayout();
			this.ImportLicenseFeeTypeDropEdit.ResumeLayout(true);
			this.ImportLicenseFeeTypeDropEdit.PerformLayout();
			this.TariffAgreementSeparatorUserControl.ResumeLayout(true);
			this.TariffAgreementSeparatorUserControl.PerformLayout();
			this.TariffAgreementDropEdit.ResumeLayout(true);
			this.TariffAgreementDropEdit.PerformLayout();
			this.GoodsApplicationDropEdit.ResumeLayout(true);
			this.GoodsApplicationDropEdit.PerformLayout();
			this.GoodsConditionDropEdit.ResumeLayout(true);
			this.GoodsConditionDropEdit.PerformLayout();
			this.FullGoodsDescriptionTextBox.ResumeLayout(true);
			this.FullGoodsDescriptionTextBox.PerformLayout();
			this.EntryInstructionGuidDropEdit.ResumeLayout(true);
			this.EntryInstructionGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal Customs.GUI.LongTextControl ComplementaryDescriptionTextBox;
		internal ZArchitecture.ZTextBox BRNFEItemNumberTextBox;
		internal ZArchitecture.ZTextBox BRNFENumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit CargoPriorityDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CountryDestinationCodeFindBox;
		internal ZArchitecture.GUI.ZGroupBox CPCGroupBox;
		internal ZArchitecture.GUI.ZDropEdit CPCDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CPCFourthDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CPCThirdDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CPCSecondDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcFindBox NFeLinePriceCalcEdit;
		internal ZArchitecture.ZTextBox ImportLicenseNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox NaladiNccaCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox NaladiHsCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl GoodsConditionSeparatorUserControl;
		internal ZArchitecture.GUI.ZDropEdit GoodsConditionOperationTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit UsedMaterialRegimeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ManufacturerIndicatorDropEdit;
		internal ZArchitecture.ZTextBox BrandGoodsConditionTextBox;
		internal ZArchitecture.ZTextBox SerialNumberGoodsConditionTextBox;
		internal ZArchitecture.ZTextBox ModelGoodsConditionTextBox;
		internal ZArchitecture.ZTextBox YearGoodsConditionTextBox;
		internal ZArchitecture.GUI.ZDropEdit DutyTaxRegimeDropEdit;
		internal ZArchitecture.GUI.SeparatorUserControl DutyTaxRegimeSeparatorUserControl;
		internal ZArchitecture.GUI.ZDropEdit DutyLegalBaseDropEdit;
		internal ZArchitecture.GUI.SeparatorUserControl ImportLicenseFineSeparatorUserControl;
		internal ZArchitecture.GUI.ZDateEdit ImportLicenseAuthorizationDateTextBox;
		internal ZArchitecture.GUI.ZDropEdit ImportLicenseTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ImportLicenseFeeTypeDropEdit;
		internal ZArchitecture.GUI.SeparatorUserControl TariffAgreementSeparatorUserControl;
		internal ZArchitecture.GUI.ZDropEdit TariffAgreementDropEdit;
		internal ZArchitecture.GUI.ZDropEdit GoodsApplicationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit GoodsConditionDropEdit;
		internal Customs.GUI.LongTextControl FullGoodsDescriptionTextBox;
		internal ZArchitecture.GUI.ZCheckBox RequiresImportLicenseCheckBox;
		internal ZArchitecture.GUI.ZGuidDropEdit EntryInstructionGuidDropEdit;
	}
}
