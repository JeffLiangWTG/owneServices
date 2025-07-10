using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class EUR1BoxItemsBuilderTest : EU.Business.Documents.DocDataObjects.Testing.EUR1BoxItemsBuilderTest
	{
		protected override EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder GetNewEUR1BoxItemsBuilder(IEnumerable<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) => new EUR1BoxItemsBuilder(invoiceLines);

		protected override ZString ExpectedBox8 => @"1; M&N1, M&N2; 99 VG, 50 Carton; INVOICE1LINE1 STUFF
2; M&N2; 150 Carton; INVOICE1LINE2 STUFF
3; INVOICE2LINE1 STUFF
-------------------------------------------------------------------------------------------------------";

		protected override ZString ExpectedInvoicesBox10 => @"";

		public void TestShouldShowDescription()
		{
			var invoiceLines = new List<JobComInvoiceLine>();
			var invoiceLine1 = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine1.JI_Tariff = "111111111";
			invoiceLines.Add(invoiceLine1);
			var boxItemsBuilder = new EUR1BoxItemsBuilderForTest(invoiceLines);
			AssertEquals("ShouldShowDescription should be true when there is only one invoice.", true, boxItemsBuilder.ShouldShowDescription);

			var invoiceLine2 = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine2.JI_Tariff = "111111111";
			invoiceLines.Add(invoiceLine2);
			boxItemsBuilder = new EUR1BoxItemsBuilderForTest(invoiceLines);
			AssertEquals("ShouldShowDescription should be true when all invoice lines in the invoice share the same tariff.", true, boxItemsBuilder.ShouldShowDescription);

			var invoiceLine3 = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine3.JI_Tariff = "2222222222";
			invoiceLines.Add(invoiceLine3);
			boxItemsBuilder = new EUR1BoxItemsBuilderForTest(invoiceLines);
			AssertEquals("ShouldShowDescription should be false when all invoice lines in the invoice use more than one kind of tariff.", false, boxItemsBuilder.ShouldShowDescription);
		}

		public void TestShouldShowInvoiceNumber()
		{
			var invoiceLines = new List<JobComInvoiceLine>();
			invoiceLines.Add(Factory.NewWithValidTestData<JobComInvoiceLine>());

			var boxItemsBuilder = new EUR1BoxItemsBuilderForTest(invoiceLines);
			AssertEquals("ShouldShowInvoiceNumber should always be false.", false, boxItemsBuilder.ShouldShowInvoiceNumber);

			invoiceLines.Add(Factory.NewWithValidTestData<JobComInvoiceLine>());
			boxItemsBuilder = new EUR1BoxItemsBuilderForTest(invoiceLines);
			AssertEquals("ShouldShowInvoiceNumber should always be false, whatever the invoices count.", false, boxItemsBuilder.ShouldShowInvoiceNumber);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage(CountryCodes.France, "French");
			helper.CreateCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "Packages");
			var code = helper.CreateCusCodeList(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "CT", "CARTON", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeListLanguage(code, CountryCodes.France, "Carton");
			Factory.Save();
		}
	}

	public class EUR1BoxItemsBuilderForTest : EUR1BoxItemsBuilder
	{
		public EUR1BoxItemsBuilderForTest(IEnumerable<JobComInvoiceLine> invoiceLines) : base(invoiceLines)
		{
		}

		public new bool ShouldShowDescription => base.ShouldShowDescription;
		public new bool ShouldShowInvoiceNumber => base.ShouldShowInvoiceNumber;
	}
}
