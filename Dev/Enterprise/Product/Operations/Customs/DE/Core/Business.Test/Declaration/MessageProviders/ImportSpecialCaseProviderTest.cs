using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportSpecialCaseProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportSpecialCaseProvider>
	{
		public void TestGroup()
		{
			AssertEquals("AB1", dataProvider.Group);
		}

		public void TestApplicationType()
		{
			AssertEquals("A1", dataProvider.ApplicationType);
		}

		public void TestRateOrAmountOrFactor()
		{
			AssertEquals(1.01235m, dataProvider.RateOrAmountOrFactor);
		}

		public void TestRateOrAmountOrFactor_Empty()
		{
			tax.JLT_Rate = 0;
			AssertEquals(decimal.Zero, dataProvider.RateOrAmountOrFactor);
		}

		protected override void SetUp()
		{
			var line = Factory.New<JobComInvoiceLine>();
			tax = line.Taxes.AddNew();
			tax.JLT_Type = "AB1";
			tax.JLT_MethodOfCalculation = "A1";
			tax.JLT_Rate = 1.012345m;
			dataProvider = new ImportSpecialCaseProvider(tax);
		}
		ImportSpecialCaseProvider dataProvider;
		JobComInvoiceLineTax tax;

		protected override ImportSpecialCaseProvider GetProvider() => dataProvider;
	}
}
