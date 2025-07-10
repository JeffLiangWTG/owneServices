using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class AccountingPayablesServiceTest : TestCaseWithFactory
	{
		public void TestGetTaxCode_ForDifferentCountries()
		{
			AssertEquals("VAT", service.GetTaxCode("US"));
			AssertEquals("GST", service.GetTaxCode("AU"));
			AssertEquals("CON", service.GetTaxCode("JP"));
		}

		public void TestGetDueDate_ForDifferentAPPaymentTerms()
		{
			var invoiceDate = new DateTime(2024, 3, 15);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = orgHeader.PK;
			Factory.Save();

			AssertEquals(invoiceDate, service.GetDueDate(invoiceDate, company.PK, orgHeader.PK));

			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_GC = company.PK;
			orgCompanyData.OB_OH = orgHeader.PK;
			orgCompanyData.OB_APPaymentTermDays = 10;

			AssertEquals(invoiceDate, service.GetDueDate(invoiceDate, Guid.NewGuid(), Guid.NewGuid()));
			AssertDueDate(Constants.InvoiceTerms.FromInvoiceDate, new DateTime(2024, 3, 25));
			AssertDueDate(Constants.InvoiceTerms.CashOnDelivery, invoiceDate);
			AssertDueDate(Constants.InvoiceTerms.FromMonthEnd, new DateTime(2024, 3, 31));
			AssertDueDate(Constants.InvoiceTerms.FromPeriodEnd, invoiceDate);

			void AssertDueDate(ZString paymentTerm, DateTime expectedDate)
			{
				orgCompanyData.OB_APPaymentTerms = paymentTerm;
				Factory.Save();
				AssertEquals(expectedDate, service.GetDueDate(invoiceDate, company.PK, orgHeader.PK));
			}
		}

		AccountingPayablesService service;

		protected override void SetUp()
		{
			base.SetUp();
			service = new AccountingPayablesService();
		}
	}
}
