using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class OsTaxDisplayTest : TestCaseWithFactory
	{
		InvoicingLineBase Line;
		RefCurrency Currency;
		AccTaxRate TaxRate;
		DocARInvoiceLine Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			Line = Factory.New<ARInvoiceLine>();
			Currency = Factory.New<RefCurrency>();
			Currency.RX_Code = "NEW";
			Line.AL_RX_NKTransactionCurrency = Currency.RX_Code;
			TaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			Line.AL_OSExTaxAmount = 1234.56789m;
			Wrapper = DocARInvoiceLine.New(Line, Factory);
		}

		void CheckOsTaxDisplay(string expectedOsTaxDisplay)
		{
			AssertEquals("OS Tax Display", expectedOsTaxDisplay, (string)Wrapper.OSTaxDisplay);
		}

		public void TestNoTax()
		{
			Line.AL_AT = ZGuid.Empty;
			CheckOsTaxDisplay("N/A");
		}

		public void TestZeroTax()
		{
			// note: AccTaxRate with empty AT_Type cannot be saved.
			TaxRate.AT_Type = ZString.Empty;
			TaxRate.SetRateNumerator_ForTestOnly(0);
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 0m;
			CheckOsTaxDisplay("0%=0.00");
		}

		public void TestTenPercentTax()
		{
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			CheckOsTaxDisplay("10%=123.46");
		}

		public void Test3DecimalPlaceTax()
		{
			TaxRate.SetRate_ForTestOnly(12346, 1000);
			Line.AL_AT = TaxRate.PK;
			CheckOsTaxDisplay("12.346%=152.42");
		}

		public void TestNoDecimalPlaceCurrency()
		{
			Currency.RX_SubUnitRatio = 1;
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 113m;
			CheckOsTaxDisplay("10%=113");
		}

		public void TestFourDecimalPlaceCurrency()
		{
			Currency.RX_SubUnitRatio = 10000;
			TaxRate.SetRateNumerator_ForTestOnly(10);
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 112.2335m;
			CheckOsTaxDisplay("10%=112.2335");
		}

		public void TestHideOsTaxAmountOnLineForCountriesThatHaveTaxCalculatedAtHeaderLevel()
		{
			RefCountry australia = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			RefCountry taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			try
			{
				Currency.RX_SubUnitRatio = 100;
				TaxRate.SetRateNumerator_ForTestOnly(10);
				Line.AL_AT = TaxRate.PK;
				Line.AL_OSTaxAmount = 112.23m;
				CheckOsTaxDisplay("10%=112.23");

				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Wrapper = DocARInvoiceLine.New(Line, Factory);
				AssertEquals("IncludeTaxAmountInOsTaxDisplay", false, Wrapper.IncludeTaxAmountInOsTaxDisplay);
				CheckOsTaxDisplay("10%");

				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Wrapper = DocARInvoiceLine.New(Line, Factory);
				AssertEquals("IncludeTaxAmountInOsTaxDisplay", true, Wrapper.IncludeTaxAmountInOsTaxDisplay);
				CheckOsTaxDisplay("10%=112.23");
			}
			finally
			{
				Line.Branch.Company.GC_RX_NKLocalCurrency = originalCountry;
			}
		}

		public void TestGSTAndQST()
		{
			TaxRate.SetRateNumerator_ForTestOnly(5);
			TaxRate.SetExtraRate_ForTestOnly(75, 10);
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 12.88m;
			CheckOsTaxDisplay("GST 5%=5.00,\r\nQST 7.5%=7.88");
		}

		public void TestGSTAndQST_QCT()
		{
			TaxRate.SetRateNumerator_ForTestOnly(5);
			TaxRate.SetExtraRate_ForTestOnly(75, 10);
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 12.5m;
			CheckOsTaxDisplay("GST 5%=5.00,\r\nQST 7.5%=7.50");
		}

		public void TestRET()
		{
			TaxRate.SetRateNumerator_ForTestOnly(16);
			TaxRate.SetExtraRate_ForTestOnly(4, 1);
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 12m;
			CheckOsTaxDisplay("IVA 16%=16.00\r\n- Withheld 4%=4.00");
		}

		public void TestREF()
		{
			TaxRate.SetRateNumerator_ForTestOnly(16);
			TaxRate.SetExtraRate_ForTestOnly(1, 4);
			TaxRate.AT_Type = AccTaxRate.Types.Rated;
			TaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 12m;
			CheckOsTaxDisplay("IVA 16%=16.00\r\n- Withheld 4%=4.00");
		}

		public void TestReverseTaxIDs()
		{
			TaxRate.SetRateNumerator_ForTestOnly(10);
			TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 0m;
			CheckOsTaxDisplay("Reverse Charge");

			TaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			CheckOsTaxDisplay("Reverse Charge");
		}

		public void TestSuspendedTaxIDs()
		{
			TaxRate.SetRateNumerator_ForTestOnly(12);
			TaxRate.AT_Type = AccTaxRate.Types.Suspended;
			Line.AL_AT = TaxRate.PK;
			Line.AL_OSTaxAmount = 0m;
			CheckOsTaxDisplay("Suspended");
		}
	}
}
