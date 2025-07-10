using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class APTermsAndDueDateCalculationProviderTest : TermsAndDueDateCalculationProviderTest
	{
		protected override TermsAndDueDateCalculationProvider GetCalculationProvider(Invoice invoice)
		{
			return new APTermsAndDueDateCalculationProvider(invoice);
		}

		protected override Invoice GetInvoice()
		{
			return Factory.New<APInvoice>();
		}

		protected override void SetupOrganisation()
		{
			base.SetupOrganisation();
			Organisation.CompanyData.OB_APPaymentTerms = Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			Organisation.CompanyData.OB_APPaymentTermDays = 0;
		}

		protected override void SetupInvoice()
		{
			base.SetupInvoice();
			Invoice.AH_InvoiceDate = ZDateTime.Now;
			Invoice.AH_PostDate = ZDateTime.Now;
		}

		public override void TestSetInvoiceTerms()
		{
			CalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("Invoice Terms", Core.Constants.InvoiceTerms.CashOnDelivery, Invoice.AH_InvoiceTerm);
			AssertEquals("Term Days", (ZByte)0, Invoice.AH_InvoiceTermDays);

			Organisation.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			Organisation.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

			Organisation.APSettlementGroup.CompanyData.OB_APPaymentTerms = Enterprise.Core.Constants.InvoiceTerms.FromInvoiceDate;
			Organisation.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 10;

			CalculationProvider.SetInvoiceTermsAndDays();
			AssertEquals("Invoice Terms", Core.Constants.InvoiceTerms.FromInvoiceDate, Invoice.AH_InvoiceTerm);
			AssertEquals("Term Days", (ZByte)10, Invoice.AH_InvoiceTermDays);
		}
	}
}