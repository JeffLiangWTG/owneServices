using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	partial class InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			this.OtherDetailsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.JZ_RelatedIndicatorDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ExchangeHedgeFinancialInstitutionFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.ExchangeHedgeReasonDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ExchangeHedgeTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ExchangeHedgeROFBACENNumberTextBox = new ZArchitecture.ZTextBox();
			this.ExchangeHedgeValueCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JZ_ValuationCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountCurrencyControl.SuspendLayout();
			this.JZ_IncoTermDropDownEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherDetailsGroupBox.SuspendLayout();
			this.JZ_RelatedIndicatorDropEdit.SuspendLayout();
			this.ExchangeHedgeFinancialInstitutionFindBox.SuspendLayout();
			this.ExchangeHedgeReasonDropEdit.SuspendLayout();
			this.ExchangeHedgeTypeDropEdit.SuspendLayout();
			this.JZ_ValuationCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 445, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 445, true);
			// 
			// ordersAttachUserControl
			// 
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 445, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 425, true);
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 116, true);
			this.ChargesGroupBox.TabIndex = 6;
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.Captions = new string[0];
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.Captions = new string[0];
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 97, true);
			// 
			// InvCustomFieldsUserControl
			// 
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 423, true);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.JZ_ValuationCodeDropEdit);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 420, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_ValuationCodeDropEdit, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// OtherDetailsGroupBox
			// 
			this.OtherDetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2D2B41C6-C5AE-433F-B10C-2874DA94301A", "Other Details");
			this.OtherDetailsGroupBox.Controls.Add(this.JZ_RelatedIndicatorDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.ExchangeHedgeFinancialInstitutionFindBox);
			this.OtherDetailsGroupBox.Controls.Add(this.ExchangeHedgeReasonDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.ExchangeHedgeTypeDropEdit);
			this.OtherDetailsGroupBox.Controls.Add(this.ExchangeHedgeROFBACENNumberTextBox);
			this.OtherDetailsGroupBox.Controls.Add(this.ExchangeHedgeValueCalcEdit);
			this.OtherDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 247, true);
			this.OtherDetailsGroupBox.Name = "OtherDetailsGroupBox";
			this.OtherDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 171, true);
			this.OtherDetailsGroupBox.TabIndex = 4;
			this.OtherDetailsGroupBox.TabStop = false;
			// 
			// JZ_RelatedIndicatorDropEdit
			// 
			this.JZ_RelatedIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_RelatedIndicatorDropEdit, "Invoices.JZ_RelatedIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RelatedIndicator);
			this.JZ_RelatedIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 17, true);
			this.JZ_RelatedIndicatorDropEdit.Name = "JZ_RelatedIndicatorDropEdit";
			this.JZ_RelatedIndicatorDropEdit.PreBoundMaxLength = 10;
			this.JZ_RelatedIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.JZ_RelatedIndicatorDropEdit.TabIndex = 0;
			// 
			// ExchangeHedgeFinancialInstitutionFindBox
			// 
			this.ExchangeHedgeFinancialInstitutionFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeHedgeFinancialInstitutionFindBox, "Invoices.ExchangeHedgeFinancialInstitution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ExchangeHedgeFinancialInstitution);
			this.ExchangeHedgeFinancialInstitutionFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 67, true);
			this.ExchangeHedgeFinancialInstitutionFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ExchangeHedgeFinancialInstitutionFindBox.Name = "ExchangeHedgeFinancialInstitutionFindBox";
			this.ExchangeHedgeFinancialInstitutionFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExchangeHedgeFinancialInstitutionFindBox.ParentType = null;
			this.ExchangeHedgeFinancialInstitutionFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.ExchangeHedgeFinancialInstitutionFindBox.TabIndex = 2;
			// 
			// ExchangeHedgeReasonDropEdit
			// 
			this.ExchangeHedgeReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeHedgeReasonDropEdit, "Invoices.ExchangeHedgeReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ExchangeHedgeReason);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.ReasonTypeList);
			this.ExchangeHedgeReasonDropEdit.BindToList = "Invoices.Lookups.ReasonTypeList";
			this.ExchangeHedgeReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 92, true);
			this.ExchangeHedgeReasonDropEdit.Name = "ExchangeHedgeReasonDropEdit";
			this.ExchangeHedgeReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.ExchangeHedgeReasonDropEdit.TabIndex = 3;
			// 
			// ExchangeHedgeTypeDropEdit
			// 
			this.ExchangeHedgeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeHedgeTypeDropEdit, "Invoices.ExchangeHedgeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ExchangeHedgeType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.ExchangeHedgeList);
			this.ExchangeHedgeTypeDropEdit.BindToList = "Invoices.Lookups.ExchangeHedgeList";
			this.ExchangeHedgeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 42, true);
			this.ExchangeHedgeTypeDropEdit.Name = "ExchangeHedgeTypeDropEdit";
			this.ExchangeHedgeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.ExchangeHedgeTypeDropEdit.TabIndex = 1;
			// 
			// ExchangeHedgeROFBACENNumberTextBox
			// 
			this.ExchangeHedgeROFBACENNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExchangeHedgeROFBACENNumberTextBox, "Invoices.ExchangeHedgeROFBACENNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ExchangeHedgeROFBACENNumber);
			this.ExchangeHedgeROFBACENNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 142, true);
			this.ExchangeHedgeROFBACENNumberTextBox.Name = "ExchangeHedgeROFBACENNumberTextBox";
			this.ExchangeHedgeROFBACENNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.ExchangeHedgeROFBACENNumberTextBox.TabIndex = 5;
			// 
			// ExchangeHedgeValueCalcEdit
			// 
			this.ExchangeHedgeValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExchangeHedgeValueCalcEdit, "Invoices.ExchangeHedgeValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ExchangeHedgeValue);
			this.ExchangeHedgeValueCalcEdit.DecimalPlaces = 2;
			this.ExchangeHedgeValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 117, true);
			this.ExchangeHedgeValueCalcEdit.Name = "ExchangeHedgeValueCalcEdit";
			this.ExchangeHedgeValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.ExchangeHedgeValueCalcEdit.TabIndex = 4;
			this.ExchangeHedgeValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JZ_ValuationCodeDropEdit
			// 
			this.JZ_ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_ValuationCodeDropEdit, "Invoices.JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.ValuationCodeList);
			this.JZ_ValuationCodeDropEdit.BindToList = "Invoices.Lookups.ValuationCodeList";
			this.JZ_ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 216, true);
			this.JZ_ValuationCodeDropEdit.Name = "JZ_ValuationCodeDropEdit";
			this.JZ_ValuationCodeDropEdit.PreBoundMaxLength = 10;
			this.JZ_ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.JZ_ValuationCodeDropEdit.TabIndex = 9;
			// 
			// InvoiceHeaderUserControl
			// 
			this.Controls.Add(this.OtherDetailsGroupBox);
			this.Name = "InvoiceHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 546, true);
			this.Controls.SetChildIndex(this.RightTabControl, 0);
			this.Controls.SetChildIndex(this.ChargesGroupBox, 0);
			this.Controls.SetChildIndex(this.SupplierOrganisationControl, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControl, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ShipmentTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.OtherDetailsGroupBox, 0);
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
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
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherDetailsGroupBox.ResumeLayout(false);
			this.OtherDetailsGroupBox.PerformLayout();
			this.JZ_RelatedIndicatorDropEdit.ResumeLayout(true);
			this.JZ_RelatedIndicatorDropEdit.PerformLayout();
			this.ExchangeHedgeFinancialInstitutionFindBox.ResumeLayout(true);
			this.ExchangeHedgeFinancialInstitutionFindBox.PerformLayout();
			this.ExchangeHedgeReasonDropEdit.ResumeLayout(true);
			this.ExchangeHedgeReasonDropEdit.PerformLayout();
			this.ExchangeHedgeTypeDropEdit.ResumeLayout(true);
			this.ExchangeHedgeTypeDropEdit.PerformLayout();
			this.JZ_ValuationCodeDropEdit.ResumeLayout(true);
			this.JZ_ValuationCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
