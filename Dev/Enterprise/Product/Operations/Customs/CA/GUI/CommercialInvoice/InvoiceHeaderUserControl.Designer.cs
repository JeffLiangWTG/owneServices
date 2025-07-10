namespace Enterprise.Customs.CA.GUI
{
	public partial class InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.TabIndex = 4;
			// JZ_InvoiceNumberTextBox
			// 
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).Lookups.ChargeTypeList);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_ChargeTypeInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_ChargeType);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).ChargeCodeDescriptionInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).ChargeCodeDescription);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_Amount);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_AmountInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).Lookups.Currencies);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_RX_NKCurrencyInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_RX_NKCurrency);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsDutiable);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsDutiableInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsGSTApplicable);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsGSTApplicableInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsIncludedInITOT);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_IsIncludedInITOTInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).Lookups.PrepaidCollectList);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_PrepaidCollectInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.InvoiceCharge)(((object)(((Business.JobComInvoiceHeader)(((object)(((Business.JobDeclaration)(null)).Invoices)))).Charges)))).J7_PrepaidCollect);
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.TabIndex = 3;
			// 
			// InvoiceHeaderUserControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.CA.Business";
			this.DataSourceTypeName = "Enterprise.Customs.CA.Business.JobDeclaration";
			this.Name = "InvoiceHeaderUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
