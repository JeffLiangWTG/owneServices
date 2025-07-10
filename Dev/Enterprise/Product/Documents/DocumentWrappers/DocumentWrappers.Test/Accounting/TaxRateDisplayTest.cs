using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class TaxRateDisplayTest : TestCaseWithFactory
	{
		InvoicingLineBase Line;
		AccTaxRate TaxRate;
		DocARInvoiceLine Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			Line = Factory.New<ARInvoiceLine>();
			Line.AL_RX_NKTransactionCurrency = "AUD";
			TaxRate = Factory.New<AccTaxRate>();
			Line.AL_LocalExTaxAmount = 1234.56789m;
			Wrapper = DocARInvoiceLine.New(Line, Factory);
		}

		void Check(string expectedOsTaxDisplay)
		{
			AssertEquals("Tax Rate Display", expectedOsTaxDisplay, (string)Wrapper.TaxRateDisplay);
			AssertEquals("Tax Rate Display", expectedOsTaxDisplay, (string)Wrapper.TaxRateDisplayAlwaysShowPercentage);
		}

		public void TestNoTax()
		{
			Line.AL_AT = ZGuid.Empty;
			Check("");
		}

		public void TestZeroTax()
		{
			TaxRate.AT_Code = "FREEVAT";
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.SetRateNumerator_ForTestOnly(0);
			Line.AL_AT = TaxRate.PK;
			Line.AL_GSTVAT = 0m;
			Check("0%");
		}

		public void TestExemptTax()
		{
			TaxRate.AT_Code = "EXEMPT";
			TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			TaxRate.SetRateNumerator_ForTestOnly(0);
			Line.AL_AT = TaxRate.PK;
			Check("");
		}

		public void TestTenPercentTax()
		{
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.AT_Code = "VAT";
			Check("10%");
		}
	}
}
