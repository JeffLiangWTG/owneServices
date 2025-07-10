namespace Enterprise.Customs.AU.GUI.CommercialInvoice
{
    public partial class InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			this.invoiceOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.addInfoPermitNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zA_VALB_HiddenDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zA_HeaderREL_HiddenDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.preferenceRuleTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.preferenceSchemeTypeDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.pOCCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zA_GSTECodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.jZ_AddInfoBoundAddInfoControl = new Enterprise.Customs.AU.Declaration.GUI.AddInfoControl();
			this.exporterReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountCurrencyControl.SuspendLayout();
			this.JZ_IncoTermDropDownEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.invoiceOriginCodeFindBox.SuspendLayout();
			this.zA_VALB_HiddenDropEdit.SuspendLayout();
			this.zA_HeaderREL_HiddenDropEdit.SuspendLayout();
			this.preferenceRuleTypeDropEdit.SuspendLayout();
			this.preferenceSchemeTypeDropDown.SuspendLayout();
			this.pOCCodeFindBox.SuspendLayout();
			this.zA_GSTECodeFindBox.SuspendLayout();
			this.jZ_AddInfoBoundAddInfoControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// RightTabControl
			// 
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 10, true);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// ordersAttachUserControl
			// 
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 444, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 403, true);
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 130, true);
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 168, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 111, true);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 329, true);
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.TabIndex = 4;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.exporterReferenceTextBox);
			this.DetailsGroupBox.Controls.Add(this.zA_GSTECodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.preferenceRuleTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.preferenceSchemeTypeDropDown);
			this.DetailsGroupBox.Controls.Add(this.pOCCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.jZ_AddInfoBoundAddInfoControl);
			this.DetailsGroupBox.Controls.Add(this.zA_HeaderREL_HiddenDropEdit);
			this.DetailsGroupBox.Controls.Add(this.zA_VALB_HiddenDropEdit);
			this.DetailsGroupBox.Controls.Add(this.invoiceOriginCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.addInfoPermitNumberBoundTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 5, true);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 395, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.addInfoPermitNumberBoundTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.invoiceOriginCodeFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.zA_VALB_HiddenDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.zA_HeaderREL_HiddenDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.jZ_AddInfoBoundAddInfoControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.pOCCodeFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.preferenceSchemeTypeDropDown, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.preferenceRuleTypeDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.zA_GSTECodeFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.exporterReferenceTextBox, 0);
			// 
			// InvCustomFieldsUserControl
			// 
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 376, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// InvoiceOriginCodeFindBox
			// 
			this.invoiceOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.invoiceOriginCodeFindBox, "Invoices.AddInfo+ZA_ORG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_ORG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_ORG_List)));
			this.invoiceOriginCodeFindBox.BindToList = "Invoices.AddInfo+ZA_ORG_List";
			this.invoiceOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|d81e7d04-11ec-4b53-85bb-60db4c8e7053", "Origin", "Ctry/Rgn. of Origin", "Country/Region of Origin", "Enter the Country/Region code as listed in the reference files. This is the Country/Region from which the goods in the Declaration originated. Format is ORG=aa e.g. ORG=TW.");
			this.invoiceOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 244, true);
			this.invoiceOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.invoiceOriginCodeFindBox.Name = "InvoiceOriginCodeFindBox";
			this.invoiceOriginCodeFindBox.PreBoundMaxLength = 2;
			this.invoiceOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 16, true);
			this.invoiceOriginCodeFindBox.TabIndex = 10;
			// 
			// AddInfoPermitNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.addInfoPermitNumberBoundTextBox, "Invoices.AddInfo+ZA_PermitNumbers_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PermitNumbers_Hidden)));
			this.addInfoPermitNumberBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|a28ca8ee-83c8-49a5-b4d3-7e3efa6976ba", "Permit/Encryption", "Permit/Encryption", "Enter the Permit or Encryption Number to apply to this Commercial Invoice. Use a comma , to separate permits. Use a colon : to separate the encryption number. Example: PIL123:456,ABC12345:123. Note that Permit Numbers consist of a 3 character alphabetic Permit Issuing Authority (e.g. PIL) followed by the actual issued permit number.");
			this.addInfoPermitNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 218, true);
			this.addInfoPermitNumberBoundTextBox.Name = "AddInfoPermitNumberBoundTextBox";
			this.addInfoPermitNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 16, true);
			this.addInfoPermitNumberBoundTextBox.TabIndex = 9;
			// 
			// ZA_VALB_HiddenDropEdit
			// 
			this.zA_VALB_HiddenDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_VALB_HiddenDropEdit, "Invoices.AddInfo+ZA_VALB_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_VALB_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ValuationBasisListForCMR)));
			this.zA_VALB_HiddenDropEdit.BindToList = "Invoices.AddInfo+Lookups+ValuationBasisListForCMR";
			this.zA_VALB_HiddenDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 317, true);
			this.zA_VALB_HiddenDropEdit.Name = "ZA_VALB_HiddenDropEdit";
			this.zA_VALB_HiddenDropEdit.PreBoundMaxLength = 4;
			this.zA_VALB_HiddenDropEdit.ShouldResizeByMaxLength = true;
			this.zA_VALB_HiddenDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 16, true);
			this.zA_VALB_HiddenDropEdit.TabIndex = 15;
			// 
			// ZA_HeaderREL_HiddenDropEdit
			// 
			this.zA_HeaderREL_HiddenDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_HeaderREL_HiddenDropEdit, "Invoices.AddInfo+ZA_HeaderREL_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_HeaderREL_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_HeaderREL_List)));
			this.zA_HeaderREL_HiddenDropEdit.BindToList = "Invoices.AddInfo+Lookups+ZA_HeaderREL_List";
			this.zA_HeaderREL_HiddenDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|aacf49db-6872-4af6-a896-8f75eac878fe", "Related Transaction", "Indicates if a relationship exists between the Supplier and the Importer of the goods. It can be selected in the declaration at the invoice header level to default to all related invoice lines or it can be overridden on each invoice line. It can also be set in the Organization record as a default.");
			this.zA_HeaderREL_HiddenDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 342, true);
			this.zA_HeaderREL_HiddenDropEdit.Name = "ZA_HeaderREL_HiddenDropEdit";
			this.zA_HeaderREL_HiddenDropEdit.PreBoundMaxLength = 1;
			this.zA_HeaderREL_HiddenDropEdit.ShouldResizeByMaxLength = true;
			this.zA_HeaderREL_HiddenDropEdit.ShowDescriptionBox = false;
			this.zA_HeaderREL_HiddenDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 16, true);
			this.zA_HeaderREL_HiddenDropEdit.TabIndex = 16;
			// 
			// PreferenceRuleTypeDropEdit
			// 
			this.preferenceRuleTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.preferenceRuleTypeDropEdit, "Invoices.AddInfo+ZA_PRT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PRT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_PRT_List)));
			this.preferenceRuleTypeDropEdit.BindToList = "Invoices.AddInfo+Lookups+ZA_PRT_List";
			this.preferenceRuleTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|51371ff7-8fa0-41c5-a6c5-2eb6c93ec3f6", "Rule", "Rule", "Rule", "The preference rule type. This is required to show the reason an import declaration is claiming a particular preference scheme and while it does not affect the duty rate it is used for risk management and to analyze the effectiveness of preference measures. It is also used to prevent a preference scheme being used to claim a lower duty rate by restricting the preference scheme to only a subset of preference rules. It will default from the Invoice Header to all related Invoice Lines or can be overridden on each invoice line.");
			this.preferenceRuleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 244, true);
			this.preferenceRuleTypeDropEdit.Name = "PreferenceRuleTypeDropEdit";
			this.preferenceRuleTypeDropEdit.PreBoundMaxLength = 4;
			this.preferenceRuleTypeDropEdit.ShouldResizeByMaxLength = true;
			this.preferenceRuleTypeDropEdit.ShowDescriptionBox = false;
			this.preferenceRuleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 16, true);
			this.preferenceRuleTypeDropEdit.TabIndex = 12;
			// 
			// PreferenceSchemeTypeDropDown
			// 
			this.preferenceSchemeTypeDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.preferenceSchemeTypeDropDown, "Invoices.AddInfo+ZA_PST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_PST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_PST_List)));
			this.preferenceSchemeTypeDropDown.BindToList = "Invoices.AddInfo+Lookups+ZA_PST_List";
			this.preferenceSchemeTypeDropDown.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|b4ed1aa2-8860-4f63-9a39-528e16ad3fce", "Scheme", "Scheme", "Scheme", "Is  used to give different rates of duty for a particular tariff classification/treatment code. If entered on the Invoice header it will default to all related invoice lines or it can be overridden on each invoice line.");
			this.preferenceSchemeTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 244, true);
			this.preferenceSchemeTypeDropDown.Name = "PreferenceSchemeTypeDropDown";
			this.preferenceSchemeTypeDropDown.PreBoundMaxLength = 4;
			this.preferenceSchemeTypeDropDown.ShouldResizeByMaxLength = true;
			this.preferenceSchemeTypeDropDown.ShowDescriptionBox = false;
			this.preferenceSchemeTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 16, true);
			this.preferenceSchemeTypeDropDown.TabIndex = 11;
			// 
			// POCCodeFindBox
			// 
			this.pOCCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pOCCodeFindBox, "Invoices.AddInfo+ZA_POC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.ZA_POC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.Lookups.ZA_POC_List)));
			this.pOCCodeFindBox.BindToList = "Invoices.AddInfo+Lookups+ZA_POC_List";
			this.pOCCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 268, true);
			this.pOCCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.pOCCodeFindBox.Name = "POCCodeFindBox";
			this.pOCCodeFindBox.PreBoundMaxLength = 4;
			this.pOCCodeFindBox.ShowDescriptionBox = false;
			this.pOCCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 16, true);
			this.pOCCodeFindBox.TabIndex = 13;
			// 
			// ZA_GSTECodeFindBox
			// 
			this.zA_GSTECodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_GSTECodeFindBox, "Invoices.ZA_GSTE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ZA_GSTE)));
			this.zA_GSTECodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 292, true);
			this.zA_GSTECodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.CMRCodeLists;
			this.zA_GSTECodeFindBox.Name = "ZA_GSTECodeFindBox";
			this.zA_GSTECodeFindBox.PopupCaption = "GST Exemption Code";
			this.zA_GSTECodeFindBox.PreBoundMaxLength = 4;
			this.zA_GSTECodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 16, true);
			this.zA_GSTECodeFindBox.TabIndex = 14;
			// 
			// JZ_AddInfoBoundAddInfoControl
			// 
			this.jZ_AddInfoBoundAddInfoControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jZ_AddInfoBoundAddInfoControl, "Invoices.AddInfo+AddInfoLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).AddInfo.AddInfoLine)));
			this.jZ_AddInfoBoundAddInfoControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("InvoiceHeaderUserControl|8170579e-9ed1-4d28-9854-433204494ebd", "Add Info | MISC");
			this.jZ_AddInfoBoundAddInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 367, true);
			this.jZ_AddInfoBoundAddInfoControl.Name = "JZ_AddInfoBoundAddInfoControl";
			this.jZ_AddInfoBoundAddInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 20, true);
			this.jZ_AddInfoBoundAddInfoControl.TabIndex = 17;
			// 
			// ExporterReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.exporterReferenceTextBox, "Invoices.JZ_ExporterReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ExporterReference)));
			this.exporterReferenceTextBox.CaptionResourceString = null;
			this.exporterReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 244, true);
			this.exporterReferenceTextBox.Name = "ExporterReferenceTextBox";
			this.exporterReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 16, true);
			this.exporterReferenceTextBox.TabIndex = 10;
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1067, 536, true);
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ordersAttachUserControl.ResumeLayout(true);
			this.ordersAttachUserControl.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountCurrencyControl.PerformLayout();
			this.JZ_IncoTermDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermDropDownEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.invoiceOriginCodeFindBox.ResumeLayout(true);
			this.invoiceOriginCodeFindBox.PerformLayout();
			this.zA_VALB_HiddenDropEdit.ResumeLayout(true);
			this.zA_VALB_HiddenDropEdit.PerformLayout();
			this.zA_HeaderREL_HiddenDropEdit.ResumeLayout(true);
			this.zA_HeaderREL_HiddenDropEdit.PerformLayout();
			this.preferenceRuleTypeDropEdit.ResumeLayout(true);
			this.preferenceRuleTypeDropEdit.PerformLayout();
			this.preferenceSchemeTypeDropDown.ResumeLayout(true);
			this.preferenceSchemeTypeDropDown.PerformLayout();
			this.pOCCodeFindBox.ResumeLayout(true);
			this.pOCCodeFindBox.PerformLayout();
			this.zA_GSTECodeFindBox.ResumeLayout(true);
			this.zA_GSTECodeFindBox.PerformLayout();
			this.jZ_AddInfoBoundAddInfoControl.ResumeLayout(true);
			this.jZ_AddInfoBoundAddInfoControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal ZArchitecture.GUI.ZCodeFindBox invoiceOriginCodeFindBox;
		protected internal Declaration.GUI.AddInfoControl jZ_AddInfoBoundAddInfoControl;
		protected ZArchitecture.ZTextBox addInfoPermitNumberBoundTextBox;
		protected internal ZArchitecture.GUI.ZDropEdit zA_VALB_HiddenDropEdit;
		protected internal ZArchitecture.GUI.ZDropEdit zA_HeaderREL_HiddenDropEdit;
		internal ZArchitecture.GUI.ZDropEdit preferenceRuleTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit preferenceSchemeTypeDropDown;
		internal ZArchitecture.GUI.ZCodeFindBox pOCCodeFindBox;
		protected internal ZArchitecture.ZTextBox exporterReferenceTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox zA_GSTECodeFindBox;
	}
}
