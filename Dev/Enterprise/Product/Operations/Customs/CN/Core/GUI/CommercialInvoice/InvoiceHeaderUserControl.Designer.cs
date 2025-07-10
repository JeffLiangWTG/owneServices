using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.CommercialInvoice
{
	public partial class InvoiceHeaderUserControl
	{
		#region Designer Generated

		void InitializeComponent()
		{
			this.NoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ContractNumbersUserControl1 = new Enterprise.Customs.CN.GUI.ContractNumbersUserControl();
			this.JZ_MarksAndNumbersLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.SpecialRelationshipConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PriceAffectConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentOfRoyaltyConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormulaPricingConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemporaryPricingConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.ContractNumbersUserControl1.SuspendLayout();
			this.JZ_MarksAndNumbersLongTextBox.SuspendLayout();
			this.SpecialRelationshipConfirmDropEdit.SuspendLayout();
			this.PriceAffectConfirmDropEdit.SuspendLayout();
			this.PaymentOfRoyaltyConfirmDropEdit.SuspendLayout();
			this.FormulaPricingConfirmDropEdit.SuspendLayout();
			this.TemporaryPricingConfirmDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// InvoiceDateDateEdit
			//
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 43, true);
			//
			// RightTabControl
			//
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 502, true);
			//
			// CustomFieldsTabPage
			//
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 475, true);
			//
			// OrdersTabPage
			//
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 475, true);
			//
			// ordersAttachUserControl
			//
			this.ordersAttachUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 475, true);
			//
			// ChargesGroupBox
			//
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 392, true);
			//
			// JZ_InvoiceCurrExRateCalcEdit
			//
			this.JZ_InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 110, true);
			//
			// GrossWeightCalcDropEdit
			//
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 154, true);
			//
			// NetWeightCalcDropEdit
			//
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 177, true);
			//
			// JZ_InvoiceAmountCurrencyControl
			//
			this.JZ_InvoiceAmountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 88, true);
			//
			// JZ_IncoTermDropDownEdit
			//
			this.JZ_IncoTermDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 132, true);
			//
			// BranchGuidFindBox
			//
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 65, true);
			//
			// IncoTermExplainButton
			//
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 131, true);
			//
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Controls.Add(this.NoOfPacksCalcDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ContractNumbersUserControl1);
			this.DetailsGroupBox.Controls.Add(this.JZ_MarksAndNumbersLongTextBox);
			this.DetailsGroupBox.Controls.Add(this.SpecialRelationshipConfirmDropEdit);
			this.DetailsGroupBox.Controls.Add(this.PriceAffectConfirmDropEdit);
			this.DetailsGroupBox.Controls.Add(this.TemporaryPricingConfirmDropEdit);
			this.DetailsGroupBox.Controls.Add(this.FormulaPricingConfirmDropEdit);
			this.DetailsGroupBox.Controls.Add(this.PaymentOfRoyaltyConfirmDropEdit);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 385, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.PaymentOfRoyaltyConfirmDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.FormulaPricingConfirmDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.TemporaryPricingConfirmDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.PriceAffectConfirmDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.SpecialRelationshipConfirmDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_MarksAndNumbersLongTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ContractNumbersUserControl1, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			//
			// InvCustomFieldsUserControl
			//
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 452, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobDeclaration);
			//
			// NoOfPacksCalcDropEdit
			//
			this.NoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).NoOfPacksPackType)));
			this.NoOfPacksCalcDropEdit.BindToAmount = "Invoices.JZ_NoOfPacks";
			this.NoOfPacksCalcDropEdit.BindToUnit = "Invoices.NoOfPacksPackType";
			this.NoOfPacksCalcDropEdit.Decimals = 3;
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 200, true);
			this.NoOfPacksCalcDropEdit.Name = "NoOfPacksCalcDropEdit";
			this.NoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.NoOfPacksCalcDropEdit.TabIndex = 9;
			this.NoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// ContractNumbersUserControl1
			//
			this.ContractNumbersUserControl1.AllowDrop = true;
			this.ContractNumbersUserControl1.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ContractNumbersUserControl1, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)))));
			this.ContractNumbersUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 223, true);
			this.ContractNumbersUserControl1.Name = "ContractNumbersUserControl1";
			this.ContractNumbersUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 20, true);
			this.ContractNumbersUserControl1.TabIndex = 10;
			//
			// JZ_MarksAndNumbersLongTextBox
			//
			this.JZ_MarksAndNumbersLongTextBox.AllowDrop = true;
			this.JZ_MarksAndNumbersLongTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("8fab03e6-e64f-4218-b5e6-a8749bb768de", "Marks & Numbers");
			this.JZ_MarksAndNumbersLongTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.JZ_MarksAndNumbersLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 246, true);
			this.JZ_MarksAndNumbersLongTextBox.Name = "JZ_MarksAndNumbersLongTextBox";
			this.JZ_MarksAndNumbersLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.JZ_MarksAndNumbersLongTextBox.TabIndex = 11;
			//
			// SpecialRelationshipConfirmDropEdit
			//
			this.SpecialRelationshipConfirmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialRelationshipConfirmDropEdit, "Invoices.JZ_SpecialRelationshipConfirm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_SpecialRelationshipConfirm)));
			this.SpecialRelationshipConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 268, true);
			this.SpecialRelationshipConfirmDropEdit.Name = "SpecialRelationshipConfirmDropEdit";
			this.SpecialRelationshipConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.SpecialRelationshipConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SpecialRelationshipConfirmDropEdit.TabIndex = 12;
			//
			// PriceAffectConfirmDropEdit
			//
			this.PriceAffectConfirmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PriceAffectConfirmDropEdit, "Invoices.JZ_PriceAffectConfirm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_PriceAffectConfirm)));
			this.PriceAffectConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 291, true);
			this.PriceAffectConfirmDropEdit.Name = "PriceAffectConfirmDropEdit";
			this.PriceAffectConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.PriceAffectConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PriceAffectConfirmDropEdit.TabIndex = 13;
			//
			// PaymentOfRoyaltyConfirmDropEdit
			//
			this.PaymentOfRoyaltyConfirmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentOfRoyaltyConfirmDropEdit, "Invoices.JZ_PaymentOfRoyaltyConfirm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_PaymentOfRoyaltyConfirm)));
			this.PaymentOfRoyaltyConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 314, true);
			this.PaymentOfRoyaltyConfirmDropEdit.Name = "PaymentOfRoyaltyConfirmDropEdit";
			this.PaymentOfRoyaltyConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentOfRoyaltyConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PaymentOfRoyaltyConfirmDropEdit.TabIndex = 14;
			//
			// FormulaPricingConfirmDropEdit
			//
			this.FormulaPricingConfirmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FormulaPricingConfirmDropEdit, "Invoices.JZ_Calc_FormulaPricingConfirm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_FormulaPricingConfirm)));
			this.FormulaPricingConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 336, true);
			this.FormulaPricingConfirmDropEdit.Name = "FormulaPricingConfirmDropEdit";
			this.FormulaPricingConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.FormulaPricingConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.FormulaPricingConfirmDropEdit.TabIndex = 15;
			//
			// TemporaryPricingConfirmDropEdit
			//
			this.TemporaryPricingConfirmDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemporaryPricingConfirmDropEdit, "Invoices.JZ_Calc_TemporaryPricingConfirm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_TemporaryPricingConfirm)));
			this.TemporaryPricingConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 358, true);
			this.TemporaryPricingConfirmDropEdit.Name = "TemporaryPricingConfirmDropEdit";
			this.TemporaryPricingConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.TemporaryPricingConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TemporaryPricingConfirmDropEdit.TabIndex = 16;
			//
			// InvoiceHeaderUserControl
			//
			this.Name = "InvoiceHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 520, true);
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
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.ContractNumbersUserControl1.ResumeLayout(true);
			this.ContractNumbersUserControl1.PerformLayout();
			this.JZ_MarksAndNumbersLongTextBox.ResumeLayout(true);
			this.JZ_MarksAndNumbersLongTextBox.PerformLayout();
			this.SpecialRelationshipConfirmDropEdit.ResumeLayout(true);
			this.SpecialRelationshipConfirmDropEdit.PerformLayout();
			this.PriceAffectConfirmDropEdit.ResumeLayout(true);
			this.PriceAffectConfirmDropEdit.PerformLayout();
			this.PaymentOfRoyaltyConfirmDropEdit.ResumeLayout(true);
			this.PaymentOfRoyaltyConfirmDropEdit.PerformLayout();
			this.FormulaPricingConfirmDropEdit.ResumeLayout(true);
			this.FormulaPricingConfirmDropEdit.PerformLayout();
			this.TemporaryPricingConfirmDropEdit.ResumeLayout(true);
			this.TemporaryPricingConfirmDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZCalcDropEdit NoOfPacksCalcDropEdit;
		ContractNumbersUserControl ContractNumbersUserControl1;
		LongTextControl JZ_MarksAndNumbersLongTextBox;
		ZDropEdit SpecialRelationshipConfirmDropEdit;
		ZDropEdit PriceAffectConfirmDropEdit;
		ZDropEdit PaymentOfRoyaltyConfirmDropEdit;
		internal ZDropEdit TemporaryPricingConfirmDropEdit;
		internal ZDropEdit FormulaPricingConfirmDropEdit;
	}
}
