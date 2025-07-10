using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class COOandFTAControlsUserControl
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
			this.IssuingPersonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingAreaNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingAgencyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.COReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.COIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.COSplitYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COIssuingCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.COLabelExemptionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COLabelTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COLabelLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CODeterminationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ProductTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryInvIssuedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExporterNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SplitOrderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SupportingDocTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IssuerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TotalNetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SequenceNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UsedUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UsedQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CoveredByCOOExporterSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.COIssueDateEdit.SuspendLayout();
			this.COSplitYNDropEdit.SuspendLayout();
			this.COCodeDropEdit.SuspendLayout();
			this.COIssuingCountryCodeFindBox.SuspendLayout();
			this.COLabelExemptionReasonDropEdit.SuspendLayout();
			this.COLabelTypeDropEdit.SuspendLayout();
			this.COLabelLocationDropEdit.SuspendLayout();
			this.CODeterminationRuleDropEdit.SuspendLayout();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.ProductTypeDropEdit.SuspendLayout();
			this.CountryInvIssuedDropEdit.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.SupportingDocTypeDropEdit.SuspendLayout();
			this.IssuerTypeDropEdit.SuspendLayout();
			this.UQDropEdit.SuspendLayout();
			this.UsedUQDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// IssuingPersonNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingPersonNameTextBox, "FilteredInvoiceLines.CertificateOfOriginPersonName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginPersonName)));
			this.IssuingPersonNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IssuingPersonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 239, true);
			this.IssuingPersonNameTextBox.Name = "IssuingPersonNameTextBox";
			this.IssuingPersonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.IssuingPersonNameTextBox.TabIndex = 14;
			// 
			// IssuingAreaNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingAreaNameTextBox, "FilteredInvoiceLines.CertificateOfOriginAreaName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginAreaName)));
			this.IssuingAreaNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IssuingAreaNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 221, true);
			this.IssuingAreaNameTextBox.Name = "IssuingAreaNameTextBox";
			this.IssuingAreaNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.IssuingAreaNameTextBox.TabIndex = 13;
			// 
			// IssuingAgencyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssuingAgencyNameTextBox, "FilteredInvoiceLines.CertificateOfOriginAgencyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginAgencyName)));
			this.IssuingAgencyNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IssuingAgencyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 203, true);
			this.IssuingAgencyNameTextBox.Name = "IssuingAgencyNameTextBox";
			this.IssuingAgencyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.IssuingAgencyNameTextBox.TabIndex = 12;
			// 
			// COReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.COReferenceNumberTextBox, "FilteredInvoiceLines.CertificateOfOriginNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginNo)));
			this.COReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 167, true);
			this.COReferenceNumberTextBox.Name = "COReferenceNumberTextBox";
			this.COReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COReferenceNumberTextBox.TabIndex = 10;
			// 
			// COIssueDateEdit
			// 
			this.COIssueDateEdit.AllowDrop = true;
			this.COIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.COIssueDateEdit, "FilteredInvoiceLines.CertificateOfOriginIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginIssueDate)));
			this.COIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 148, true);
			this.COIssueDateEdit.Name = "COIssueDateEdit";
			this.COIssueDateEdit.TabIndex = 9;
			// 
			// COSplitYNDropEdit
			// 
			this.COSplitYNDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COSplitYNDropEdit, "FilteredInvoiceLines.CertificateOfOriginStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginStatus)));
			this.COSplitYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 258, true);
			this.COSplitYNDropEdit.Name = "COSplitYNDropEdit";
			this.COSplitYNDropEdit.PreBoundMaxLength = 1;
			this.COSplitYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COSplitYNDropEdit.TabIndex = 15;
			// 
			// COCodeDropEdit
			// 
			this.COCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COCodeDropEdit, "FilteredInvoiceLines.CertificateOfOriginCriteriaCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginCriteriaCode)));
			this.COCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 185, true);
			this.COCodeDropEdit.Name = "COCodeDropEdit";
			this.COCodeDropEdit.PreBoundMaxLength = 1;
			this.COCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.COCodeDropEdit.TabIndex = 11;
			// 
			// COIssuingCountryCodeFindBox
			// 
			this.COIssuingCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COIssuingCountryCodeFindBox, "FilteredInvoiceLines.CertificateOfOriginIssuingCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginIssuingCountry)));
			this.COIssuingCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 130, true);
			this.COIssuingCountryCodeFindBox.Name = "COIssuingCountryCodeFindBox";
			this.COIssuingCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.COIssuingCountryCodeFindBox.ParentType = null;
			this.COIssuingCountryCodeFindBox.PreBoundMaxLength = 2;
			this.COIssuingCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COIssuingCountryCodeFindBox.TabIndex = 8;
			// 
			// COLabelExemptionReasonDropEdit
			// 
			this.COLabelExemptionReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COLabelExemptionReasonDropEdit, "FilteredInvoiceLines.JI_COOExemptionReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_COOExemptionReason)));
			this.COLabelExemptionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 82, true);
			this.COLabelExemptionReasonDropEdit.Name = "COLabelExemptionReasonDropEdit";
			this.COLabelExemptionReasonDropEdit.PreBoundMaxLength = 2;
			this.COLabelExemptionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COLabelExemptionReasonDropEdit.TabIndex = 20;
			// 
			// COLabelTypeDropEdit
			// 
			this.COLabelTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COLabelTypeDropEdit, "FilteredInvoiceLines.JI_COOLabelType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_COOLabelType)));
			this.COLabelTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 64, true);
			this.COLabelTypeDropEdit.Name = "COLabelTypeDropEdit";
			this.COLabelTypeDropEdit.PreBoundMaxLength = 1;
			this.COLabelTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COLabelTypeDropEdit.TabIndex = 19;
			// 
			// COLabelLocationDropEdit
			// 
			this.COLabelLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COLabelLocationDropEdit, "FilteredInvoiceLines.JI_COOLabelLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_COOLabelLocation)));
			this.COLabelLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 46, true);
			this.COLabelLocationDropEdit.Name = "COLabelLocationDropEdit";
			this.COLabelLocationDropEdit.PreBoundMaxLength = 1;
			this.COLabelLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.COLabelLocationDropEdit.TabIndex = 18;
			// 
			// CODeterminationRuleDropEdit
			// 
			this.CODeterminationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CODeterminationRuleDropEdit, "FilteredInvoiceLines.CriteriaForDeterminingCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CriteriaForDeterminingCountryOfOrigin)));
			this.CODeterminationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 28, true);
			this.CODeterminationRuleDropEdit.Name = "CODeterminationRuleDropEdit";
			this.CODeterminationRuleDropEdit.PreBoundMaxLength = 1;
			this.CODeterminationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CODeterminationRuleDropEdit.TabIndex = 17;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "FilteredInvoiceLines.JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountryOfOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 10, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.GoodsOriginCodeFindBox.TabIndex = 16;
			// 
			// ProductTypeDropEdit
			// 
			this.ProductTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductTypeDropEdit, "FilteredInvoiceLines.CertificateOfOriginProductType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginProductType)));
			this.ProductTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 10, true);
			this.ProductTypeDropEdit.Name = "ProductTypeDropEdit";
			this.ProductTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ProductTypeDropEdit.TabIndex = 21;
			// 
			// CountryInvIssuedDropEdit
			// 
			this.CountryInvIssuedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryInvIssuedDropEdit, "FilteredInvoiceLines.IssuedInThirdCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).IssuedInThirdCountry)));
			this.CountryInvIssuedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 28, true);
			this.CountryInvIssuedDropEdit.Name = "CountryInvIssuedDropEdit";
			this.CountryInvIssuedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.CountryInvIssuedDropEdit.TabIndex = 22;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "FilteredInvoiceLines.JI_RN_NKSecondCommercialInvoiceCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RN_NKSecondCommercialInvoiceCountry)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 46, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
            this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.CountryCodeFindBox.TabIndex = 23;
			// 
			// ExporterNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterNumberTextBox, "FilteredInvoiceLines.CountryOfOriginExporterNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CountryOfOriginExporterNumber)));
			this.ExporterNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 65, true);
			this.ExporterNumberTextBox.Name = "ExporterNumberTextBox";
			this.ExporterNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ExporterNumberTextBox.TabIndex = 24;
			// 
			// SplitOrderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SplitOrderCalcEdit, "FilteredInvoiceLines.COOSplitOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).COOSplitOrder)));
			this.SplitOrderCalcEdit.DecimalPlaces = 2;
			this.SplitOrderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 83, true);
			this.SplitOrderCalcEdit.Name = "SplitOrderCalcEdit";
			this.SplitOrderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.SplitOrderCalcEdit.TabIndex = 25;
			this.SplitOrderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SplitOrderCalcEdit.TrackDisposedAccess = true;
			// 
			// SupportingDocTypeDropEdit
			// 
			this.SupportingDocTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingDocTypeDropEdit, "FilteredInvoiceLines.JI_COOSupportingDocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_COOSupportingDocType)));
			this.SupportingDocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 101, true);
			this.SupportingDocTypeDropEdit.Name = "SupportingDocTypeDropEdit";
			this.SupportingDocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.SupportingDocTypeDropEdit.TabIndex = 26;
			// 
			// IssuerTypeDropEdit
			// 
			this.IssuerTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssuerTypeDropEdit, "FilteredInvoiceLines.COOIssuerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).COOIssuerType)));
			this.IssuerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 119, true);
			this.IssuerTypeDropEdit.Name = "IssuerTypeDropEdit";
			this.IssuerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.IssuerTypeDropEdit.TabIndex = 27;
			// 
			// TotalNetWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalNetWeightCalcEdit, "FilteredInvoiceLines.COOTotalNetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).COOTotalNetWeight)));
			this.TotalNetWeightCalcEdit.DecimalPlaces = 2;
			this.TotalNetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 138, true);
			this.TotalNetWeightCalcEdit.Name = "TotalNetWeightCalcEdit";
			this.TotalNetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TotalNetWeightCalcEdit.TabIndex = 28;
			this.TotalNetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalNetWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// UQDropEdit
			// 
			this.UQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UQDropEdit, "FilteredInvoiceLines.COOTotalNetWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).COOTotalNetWeightUQ)));
			this.UQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 137, true);
			this.UQDropEdit.Name = "UQDropEdit";
			this.UQDropEdit.ShowDescriptionBox = false;
			this.UQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.UQDropEdit.TabIndex = 29;
			// 
			// SequenceNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SequenceNoCalcEdit, "FilteredInvoiceLines.CertificateOfOriginLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginLineNo)));
			this.SequenceNoCalcEdit.DecimalPlaces = 2;
			this.SequenceNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 182, true);
			this.SequenceNoCalcEdit.Name = "SequenceNoCalcEdit";
			this.SequenceNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.SequenceNoCalcEdit.TabIndex = 30;
			this.SequenceNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SequenceNoCalcEdit.TrackDisposedAccess = true;
			// 
			// UsedUQDropEdit
			// 
			this.UsedUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UsedUQDropEdit, "FilteredInvoiceLines.COOUsedQuantityUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).COOUsedQuantityUQ)));
			this.UsedUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 200, true);
			this.UsedUQDropEdit.Name = "UsedUQDropEdit";
			this.UsedUQDropEdit.ShowDescriptionBox = false;
			this.UsedUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.UsedUQDropEdit.TabIndex = 33;
			// 
			// UsedQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UsedQuantityCalcEdit, "FilteredInvoiceLines.JI_CustomsFifthQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsFifthQuantity)));
			this.UsedQuantityCalcEdit.DecimalPlaces = 2;
			this.UsedQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 200, true);
			this.UsedQuantityCalcEdit.Name = "UsedQuantityCalcEdit";
			this.UsedQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.UsedQuantityCalcEdit.TabIndex = 32;
			this.UsedQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UsedQuantityCalcEdit.TrackDisposedAccess = true;
			// 
			// CoveredByCOOExporterSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CoveredByCOOExporterSystemCheckBox, "FilteredInvoiceLines.JI_CoveredByCOOExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CoveredByCOOExporter)));
			this.CoveredByCOOExporterSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CoveredByCOOExporterSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 102, true);
			this.CoveredByCOOExporterSystemCheckBox.Name = "CoveredByCOOExporterSystemCheckBox";
			this.CoveredByCOOExporterSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.CoveredByCOOExporterSystemCheckBox.TabIndex = 34;
			this.CoveredByCOOExporterSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// COOandFTAControlsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CoveredByCOOExporterSystemCheckBox);
			this.Controls.Add(this.UsedUQDropEdit);
			this.Controls.Add(this.UsedQuantityCalcEdit);
			this.Controls.Add(this.SequenceNoCalcEdit);
			this.Controls.Add(this.UQDropEdit);
			this.Controls.Add(this.TotalNetWeightCalcEdit);
			this.Controls.Add(this.IssuerTypeDropEdit);
			this.Controls.Add(this.SupportingDocTypeDropEdit);
			this.Controls.Add(this.SplitOrderCalcEdit);
			this.Controls.Add(this.ExporterNumberTextBox);
			this.Controls.Add(this.CountryCodeFindBox);
			this.Controls.Add(this.CountryInvIssuedDropEdit);
			this.Controls.Add(this.ProductTypeDropEdit);
			this.Controls.Add(this.COLabelExemptionReasonDropEdit);
			this.Controls.Add(this.COLabelTypeDropEdit);
			this.Controls.Add(this.COLabelLocationDropEdit);
			this.Controls.Add(this.CODeterminationRuleDropEdit);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.IssuingPersonNameTextBox);
			this.Controls.Add(this.IssuingAreaNameTextBox);
			this.Controls.Add(this.IssuingAgencyNameTextBox);
			this.Controls.Add(this.COReferenceNumberTextBox);
			this.Controls.Add(this.COIssueDateEdit);
			this.Controls.Add(this.COSplitYNDropEdit);
			this.Controls.Add(this.COCodeDropEdit);
			this.Controls.Add(this.COIssuingCountryCodeFindBox);
			this.Name = "COOandFTAControlsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.COIssueDateEdit.ResumeLayout(true);
			this.COIssueDateEdit.PerformLayout();
			this.COSplitYNDropEdit.ResumeLayout(true);
			this.COSplitYNDropEdit.PerformLayout();
			this.COCodeDropEdit.ResumeLayout(true);
			this.COCodeDropEdit.PerformLayout();
			this.COIssuingCountryCodeFindBox.ResumeLayout(true);
			this.COIssuingCountryCodeFindBox.PerformLayout();
			this.COLabelExemptionReasonDropEdit.ResumeLayout(true);
			this.COLabelExemptionReasonDropEdit.PerformLayout();
			this.COLabelTypeDropEdit.ResumeLayout(true);
			this.COLabelTypeDropEdit.PerformLayout();
			this.COLabelLocationDropEdit.ResumeLayout(true);
			this.COLabelLocationDropEdit.PerformLayout();
			this.CODeterminationRuleDropEdit.ResumeLayout(true);
			this.CODeterminationRuleDropEdit.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.ProductTypeDropEdit.ResumeLayout(true);
			this.ProductTypeDropEdit.PerformLayout();
			this.CountryInvIssuedDropEdit.ResumeLayout(true);
			this.CountryInvIssuedDropEdit.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.SupportingDocTypeDropEdit.ResumeLayout(true);
			this.SupportingDocTypeDropEdit.PerformLayout();
			this.IssuerTypeDropEdit.ResumeLayout(true);
			this.IssuerTypeDropEdit.PerformLayout();
			this.UQDropEdit.ResumeLayout(true);
			this.UQDropEdit.PerformLayout();
			this.UsedUQDropEdit.ResumeLayout(true);
			this.UsedUQDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox IssuingPersonNameTextBox;
		internal ZArchitecture.ZTextBox IssuingAreaNameTextBox;
		internal ZArchitecture.ZTextBox IssuingAgencyNameTextBox;
		internal ZArchitecture.ZTextBox COReferenceNumberTextBox;
		internal ZDateEdit COIssueDateEdit;
		internal ZDropEdit COSplitYNDropEdit;
		internal ZDropEdit COCodeDropEdit;
		internal ZDropEdit COLabelExemptionReasonDropEdit;
		internal ZDropEdit COLabelTypeDropEdit;
		internal ZDropEdit COLabelLocationDropEdit;
		internal ZDropEdit CODeterminationRuleDropEdit;
		internal ZCodeFindBox GoodsOriginCodeFindBox;
		internal ZCodeFindBox COIssuingCountryCodeFindBox;
		internal ZDropEdit ProductTypeDropEdit;
		internal ZDropEdit CountryInvIssuedDropEdit;
		internal ZCodeFindBox CountryCodeFindBox;
		internal ZArchitecture.ZTextBox ExporterNumberTextBox;
		internal ZArchitecture.ZCalcEdit SplitOrderCalcEdit;
		internal ZDropEdit SupportingDocTypeDropEdit;
		internal ZDropEdit IssuerTypeDropEdit;
		internal ZArchitecture.ZCalcEdit TotalNetWeightCalcEdit;
		internal ZDropEdit UQDropEdit;
		internal ZArchitecture.ZCalcEdit SequenceNoCalcEdit;
		internal ZDropEdit UsedUQDropEdit;
		internal ZArchitecture.ZCalcEdit UsedQuantityCalcEdit;
		internal ZCheckBox CoveredByCOOExporterSystemCheckBox;
	}
}
