namespace Enterprise.Customs.DE.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CessionManagementFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginFederalStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuotaQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TobaccoStampTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsMainPackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UsualReplacementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReimportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DecisiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OutwardMRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutwardDecisiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BondedWHSOrderNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondedWHSOrderLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetPriceCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.DgSubstanceUserControl = new Enterprise.Customs.DE.GUI.DgSubstanceUserControl();
			this.DescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.FixedMaxLengthEntryInstructionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.FixedMaxLengthWithDescriptionTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.FixedMaxLengthInvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CessionManagementFlagDropEdit.SuspendLayout();
			this.OriginFederalStateDropEdit.SuspendLayout();
			this.InvoiceNumberDropEdit.SuspendLayout();
			this.QuotaQtyCalcDropEdit.SuspendLayout();
			this.ReimportDateEdit.SuspendLayout();
			this.ExportCountryCodeFindBox.SuspendLayout();
			this.DecisiveDateEdit.SuspendLayout();
			this.OutwardDecisiveDateEdit.SuspendLayout();
			this.NetPriceCurrencyCalcFindBox.SuspendLayout();
			this.DgSubstanceUserControl.SuspendLayout();
			this.DescriptionLongTextControl.SuspendLayout();
			this.FixedMaxLengthEntryInstructionGuidDropEdit.SuspendLayout();
			this.FixedMaxLengthWithDescriptionTariffFindBox.SuspendLayout();
			this.FixedMaxLengthInvoiceNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine);
			// 
			// CessionManagementFlagDropEdit
			// 
			this.CessionManagementFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CessionManagementFlagDropEdit, "JI_CessionFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_CessionFlag)));
			this.CessionManagementFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 15, true);
			this.CessionManagementFlagDropEdit.Name = "CessionManagementFlagDropEdit";
			this.CessionManagementFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.CessionManagementFlagDropEdit.TabIndex = 0;
			// 
			// SupplementaryInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplementaryInformationTextBox, "SupplementaryInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).SupplementaryInformation)));
			this.SupplementaryInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupplementaryInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 40, true);
			this.SupplementaryInformationTextBox.Name = "SupplementaryInformationTextBox";
			this.SupplementaryInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 20, true);
			this.SupplementaryInformationTextBox.TabIndex = 1;
			// 
			// OriginFederalStateDropEdit
			// 
			this.OriginFederalStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginFederalStateDropEdit, "JI_StateOrRegionOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_StateOrRegionOfOrigin)));
			this.OriginFederalStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 65, true);
			this.OriginFederalStateDropEdit.Name = "OriginFederalStateDropEdit";
			this.OriginFederalStateDropEdit.PreBoundMaxLength = 1;
			this.OriginFederalStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.OriginFederalStateDropEdit.TabIndex = 2;
			// 
			// InvoiceNumberDropEdit
			// 
			this.InvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberDropEdit, "JI_Calc_Invoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_Calc_Invoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).Lookups.SortedInvoiceList)));
			this.InvoiceNumberDropEdit.BindToList = "Lookups.SortedInvoiceList";
			this.InvoiceNumberDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4AFE426D-BAD7-4990-8E8C-555D1EFCACBD", "Invoice Number");
			this.InvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 90, true);
			this.InvoiceNumberDropEdit.Name = "InvoiceNumberDropEdit";
			this.InvoiceNumberDropEdit.PreBoundMaxLength = 35;
			this.InvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.InvoiceNumberDropEdit.TabIndex = 4;
			// 
			// QuotaQtyCalcDropEdit
			// 
			this.QuotaQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotaQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).ZG_QuotaQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).ZG_QuotaUQ)));
			this.QuotaQtyCalcDropEdit.BindToAmount = "ZG_QuotaQty";
			this.QuotaQtyCalcDropEdit.BindToUnit = "ZG_QuotaUQ";
			this.QuotaQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 115, true);
			this.QuotaQtyCalcDropEdit.Name = "QuotaQtyCalcDropEdit";
			this.QuotaQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.QuotaQtyCalcDropEdit.TabIndex = 5;
			this.QuotaQtyCalcDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// TobaccoStampTextBox
			// 
			this.BindingSource.SetBindingMember(this.TobaccoStampTextBox, "JI_TobaccoStamp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_TobaccoStamp)));
			this.TobaccoStampTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 190, true);
			this.TobaccoStampTextBox.Name = "TobaccoStampTextBox";
			this.TobaccoStampTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TobaccoStampTextBox.TabIndex = 9;
			// 
			// IsMainPackCheckBox
			// 
			this.IsMainPackCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsMainPackCheckBox, "JI_IsMainPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_IsMainPack)));
			this.IsMainPackCheckBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8D9B7F4C-A9DB-4EA7-B9B5-FB7827E16DB9", "Is Main Pack?");
			this.IsMainPackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 215, true);
			this.IsMainPackCheckBox.Name = "IsMainPackCheckBox";
			this.IsMainPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.IsMainPackCheckBox.TabIndex = 10;
			this.IsMainPackCheckBox.UseVisualStyleBackColor = true;
			// 
			// UsualReplacementCheckBox
			// 
			this.UsualReplacementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UsualReplacementCheckBox, "ZG_UsualReplacement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).ZG_UsualReplacement)));
			this.UsualReplacementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 235, true);
			this.UsualReplacementCheckBox.Name = "UsualReplacementCheckBox";
			this.UsualReplacementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.UsualReplacementCheckBox.TabIndex = 11;
			this.UsualReplacementCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReimportDateEdit
			// 
			this.ReimportDateEdit.AllowDrop = true;
			this.ReimportDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReimportDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReimportDateEdit, "ZG_ReimportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).ZG_ReimportDate)));
			this.ReimportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 263, true);
			this.ReimportDateEdit.Name = "ReimportDateEdit";
			this.ReimportDateEdit.TabIndex = 12;
			// 
			// ExportCountryCodeFindBox
			// 
			this.ExportCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportCountryCodeFindBox, "JI_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_RN_NKCountryOfExport)));
			this.ExportCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 293, true);
			this.ExportCountryCodeFindBox.Name = "ExportCountryCodeFindBox";
			this.ExportCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExportCountryCodeFindBox.ParentType = null;
			this.ExportCountryCodeFindBox.PreBoundMaxLength = 1;
			this.ExportCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.ExportCountryCodeFindBox.TabIndex = 13;
			// 
			// DecisiveDateEdit
			// 
			this.DecisiveDateEdit.AllowDrop = true;
			this.DecisiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.DecisiveDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DecisiveDateEdit, "JI_CustomDate1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_CustomDate1)));
			this.DecisiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 320, true);
			this.DecisiveDateEdit.Name = "DecisiveDateEdit";
			this.DecisiveDateEdit.TabIndex = 14;
			// 
			// OutwardMRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutwardMRNTextBox, "OutwardMRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).OutwardMRN)));
			this.OutwardMRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 346, true);
			this.OutwardMRNTextBox.Name = "OutwardMRNTextBox";
			this.OutwardMRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OutwardMRNTextBox.TabIndex = 15;
			// 
			// OutwardDecisiveDateEdit
			// 
			this.OutwardDecisiveDateEdit.AllowDrop = true;
			this.OutwardDecisiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.OutwardDecisiveDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OutwardDecisiveDateEdit, "OutwardDecisiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).OutwardDecisiveDate)));
			this.OutwardDecisiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 372, true);
			this.OutwardDecisiveDateEdit.Name = "OutwardDecisiveDateEdit";
			this.OutwardDecisiveDateEdit.TabIndex = 16;
			// 
			// BondedWHSOrderNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderNumberTextBox, "JI_BondedWHSOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_BondedWHSOrderNumber)));
			this.BondedWHSOrderNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 398, true);
			this.BondedWHSOrderNumberTextBox.Name = "BondedWHSOrderNumberTextBox";
			this.BondedWHSOrderNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.BondedWHSOrderNumberTextBox.TabIndex = 17;
			// 
			// BondedWHSOrderLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BondedWHSOrderLineNumberCalcEdit, "JI_BondedWHSOrderLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_BondedWHSOrderLineNumber)));
			this.BondedWHSOrderLineNumberCalcEdit.DecimalPlaces = 0;
			this.BondedWHSOrderLineNumberCalcEdit.Decimals = 0;
			this.BondedWHSOrderLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 424, true);
			this.BondedWHSOrderLineNumberCalcEdit.Name = "BondedWHSOrderLineNumberCalcEdit";
			this.BondedWHSOrderLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.BondedWHSOrderLineNumberCalcEdit.TabIndex = 18;
			this.BondedWHSOrderLineNumberCalcEdit.Text = "0";
			this.BondedWHSOrderLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BondedWHSOrderLineNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// NetPriceCurrencyCalcFindBox
			// 
			this.NetPriceCurrencyCalcFindBox.AllowDrop = true;
			this.NetPriceCurrencyCalcFindBox.BindToAmount = "JI_NetPrice";
			this.NetPriceCurrencyCalcFindBox.BindToList = "Lookups+CurrencyList";
			this.NetPriceCurrencyCalcFindBox.BindToUnit = "JI_RX_NKNetPriceCurr";
			this.NetPriceCurrencyCalcFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("CF38E1E1-8798-4310-829C-82472A3DF9C4", "Net Price");
			this.NetPriceCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.NetPriceCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 446, true);
			this.NetPriceCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.NetPriceCurrencyCalcFindBox.Name = "NetPriceCurrencyCalcFindBox";
			this.NetPriceCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.NetPriceCurrencyCalcFindBox.TabIndex = 19;
			// 
			// DgSubstanceUserControl
			// 
			this.DgSubstanceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DgSubstanceUserControl, ".");
			this.DgSubstanceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 472, true);
			this.DgSubstanceUserControl.Name = "DgSubstanceUserControl";
			this.DgSubstanceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			this.DgSubstanceUserControl.TabIndex = 20;
			// 
			// DescriptionLongTextControl
			// 
			this.DescriptionLongTextControl.AllowDrop = true;
			this.DescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 495, true);
			this.DescriptionLongTextControl.Name = "DescriptionLongTextControl";
			this.DescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.DescriptionLongTextControl.TabIndex = 3;
			// 
			// FixedMaxLengthEntryInstructionGuidDropEdit
			// 
			this.FixedMaxLengthEntryInstructionGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FixedMaxLengthEntryInstructionGuidDropEdit, "JI_CEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_CEI)));
			this.FixedMaxLengthEntryInstructionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 495, true);
			this.FixedMaxLengthEntryInstructionGuidDropEdit.Name = "FixedMaxLengthEntryInstructionGuidDropEdit";
			this.FixedMaxLengthEntryInstructionGuidDropEdit.PreBoundMaxLength = 10;
			this.FixedMaxLengthEntryInstructionGuidDropEdit.ShouldResizeByMaxLength = false;
			this.FixedMaxLengthEntryInstructionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.FixedMaxLengthEntryInstructionGuidDropEdit.TabIndex = 21;
			// 
			// FixedMaxLengthWithDescriptionTariffFindBox
			// 
			this.FixedMaxLengthWithDescriptionTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FixedMaxLengthWithDescriptionTariffFindBox, "JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_Tariff)));
			this.FixedMaxLengthWithDescriptionTariffFindBox.ErrorForUnsupportedCountry = null;
			this.FixedMaxLengthWithDescriptionTariffFindBox.GetEffectiveDate = null;
			this.FixedMaxLengthWithDescriptionTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 520, true);
			this.FixedMaxLengthWithDescriptionTariffFindBox.Name = "FixedMaxLengthWithDescriptionTariffFindBox";
			this.FixedMaxLengthWithDescriptionTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FixedMaxLengthWithDescriptionTariffFindBox.ParentType = null;
			this.FixedMaxLengthWithDescriptionTariffFindBox.PreBoundMaxLength = 10;
			this.FixedMaxLengthWithDescriptionTariffFindBox.SelectNomenclatureModes = null;
			this.FixedMaxLengthWithDescriptionTariffFindBox.ShouldResize = false;
			this.FixedMaxLengthWithDescriptionTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.FixedMaxLengthWithDescriptionTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.FixedMaxLengthWithDescriptionTariffFindBox.TabIndex = 22;
			this.FixedMaxLengthWithDescriptionTariffFindBox.TariffType = null;
			// 
			// FixedMaxLengthInvoiceNumberDropEdit
			// 
			this.FixedMaxLengthInvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FixedMaxLengthInvoiceNumberDropEdit, "JI_Calc_Invoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).JI_Calc_Invoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).Lookups.SortedInvoiceList)));
			this.FixedMaxLengthInvoiceNumberDropEdit.BindToList = "Lookups.SortedInvoiceList";
			this.FixedMaxLengthInvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 545, true);
			this.FixedMaxLengthInvoiceNumberDropEdit.Name = "FixedMaxLengthInvoiceNumberDropEdit";
			this.FixedMaxLengthInvoiceNumberDropEdit.PreBoundMaxLength = 10;
			this.FixedMaxLengthInvoiceNumberDropEdit.ShouldResizeByMaxLength = false;
			this.FixedMaxLengthInvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.FixedMaxLengthInvoiceNumberDropEdit.TabIndex = 23;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DgSubstanceUserControl);
			this.Controls.Add(this.OutwardDecisiveDateEdit);
			this.Controls.Add(this.OutwardMRNTextBox);
			this.Controls.Add(this.DecisiveDateEdit);
			this.Controls.Add(this.ReimportDateEdit);
			this.Controls.Add(this.UsualReplacementCheckBox);
			this.Controls.Add(this.CessionManagementFlagDropEdit);
			this.Controls.Add(this.SupplementaryInformationTextBox);
			this.Controls.Add(this.OriginFederalStateDropEdit);
			this.Controls.Add(this.InvoiceNumberDropEdit);
			this.Controls.Add(this.QuotaQtyCalcDropEdit);
			this.Controls.Add(this.TobaccoStampTextBox);
			this.Controls.Add(this.IsMainPackCheckBox);
			this.Controls.Add(this.ExportCountryCodeFindBox);
			this.Controls.Add(this.BondedWHSOrderNumberTextBox);
			this.Controls.Add(this.BondedWHSOrderLineNumberCalcEdit);
			this.Controls.Add(this.NetPriceCurrencyCalcFindBox);
			this.Controls.Add(this.DescriptionLongTextControl);
			this.Controls.Add(this.FixedMaxLengthEntryInstructionGuidDropEdit);
			this.Controls.Add(this.FixedMaxLengthWithDescriptionTariffFindBox);
			this.Controls.Add(this.FixedMaxLengthInvoiceNumberDropEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CessionManagementFlagDropEdit.ResumeLayout(true);
			this.CessionManagementFlagDropEdit.PerformLayout();
			this.OriginFederalStateDropEdit.ResumeLayout(true);
			this.OriginFederalStateDropEdit.PerformLayout();
			this.InvoiceNumberDropEdit.ResumeLayout(true);
			this.InvoiceNumberDropEdit.PerformLayout();
			this.QuotaQtyCalcDropEdit.ResumeLayout(true);
			this.QuotaQtyCalcDropEdit.PerformLayout();
			this.ReimportDateEdit.ResumeLayout(true);
			this.ReimportDateEdit.PerformLayout();
			this.ExportCountryCodeFindBox.ResumeLayout(true);
			this.ExportCountryCodeFindBox.PerformLayout();
			this.DecisiveDateEdit.ResumeLayout(true);
			this.DecisiveDateEdit.PerformLayout();
			this.OutwardDecisiveDateEdit.ResumeLayout(true);
			this.OutwardDecisiveDateEdit.PerformLayout();
			this.NetPriceCurrencyCalcFindBox.ResumeLayout(true);
			this.NetPriceCurrencyCalcFindBox.PerformLayout();
			this.DgSubstanceUserControl.ResumeLayout(true);
			this.DgSubstanceUserControl.PerformLayout();
			this.DescriptionLongTextControl.ResumeLayout(true);
			this.DescriptionLongTextControl.PerformLayout();
			this.FixedMaxLengthEntryInstructionGuidDropEdit.ResumeLayout(true);
			this.FixedMaxLengthEntryInstructionGuidDropEdit.PerformLayout();
			this.FixedMaxLengthWithDescriptionTariffFindBox.ResumeLayout(true);
			this.FixedMaxLengthWithDescriptionTariffFindBox.PerformLayout();
			this.FixedMaxLengthInvoiceNumberDropEdit.ResumeLayout(true);
			this.FixedMaxLengthInvoiceNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit CessionManagementFlagDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox SupplementaryInformationTextBox;
		internal ZArchitecture.GUI.ZDropEdit OriginFederalStateDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceNumberDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit FixedMaxLengthInvoiceNumberDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit QuotaQtyCalcDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox TobaccoStampTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox IsMainPackCheckBox;
		internal ZArchitecture.GUI.ZCheckBox UsualReplacementCheckBox;
		internal ZArchitecture.GUI.ZDateEdit ReimportDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ExportCountryCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit DecisiveDateEdit;
		internal ZArchitecture.ZTextBox OutwardMRNTextBox;
		internal ZArchitecture.GUI.ZDateEdit OutwardDecisiveDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox BondedWHSOrderNumberTextBox;
		internal ZArchitecture.ZCalcEdit BondedWHSOrderLineNumberCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcFindBox NetPriceCurrencyCalcFindBox;
		internal Enterprise.Customs.GUI.LongTextControl DescriptionLongTextControl;
		internal DgSubstanceUserControl DgSubstanceUserControl;
		internal Enterprise.ZArchitecture.GUI.ZGuidDropEdit FixedMaxLengthEntryInstructionGuidDropEdit;
		internal Universal.GUI.TariffFindBox FixedMaxLengthWithDescriptionTariffFindBox;
	}
}
