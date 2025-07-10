using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class TaxAmountDisplayTest : TestCaseWithFactory
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
			AssertEquals("Tax Amount Display", expectedOsTaxDisplay, (string)Wrapper.TaxAmountDisplay);
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
			Line.AL_OSTaxAmount = 0m;
			Check("Zero Rated");
		}

		public void TestExemptTax()
		{
			TaxRate.AT_Code = "EXEMPT";
			TaxRate.AT_Type = AccTaxRate.Types.Exempt;
			TaxRate.SetRateNumerator_ForTestOnly(0);
			Line.AL_AT = TaxRate.PK;
			Check("Exempt Rated");
		}

		public void TestTenPercentTax()
		{
			TaxRate.AT_Code = "VAT";
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 123.46m;
			Check("123.46");
		}

		public void TestReverseTax()
		{
			TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			Check("Reverse " + Country.GetConsumptionTaxDescription(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		public void TestSuspendedTax()
		{
			TaxRate.AT_Type = AccTaxRate.Types.Suspended;
			TaxRate.SetRateNumerator_ForTestOnly(12);
			Line.AL_AT = TaxRate.PK;
			Check("Suspended");
		}
	}
}
