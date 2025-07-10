using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocTaxSummaryLineTest : TestCaseWithFactory
	{
		public void TestTaxDescriptionWithDescriptionOverride()
		{
			TaxSummaryLine taxSummaryLine = new TaxSummaryLine(AccTaxRate.Types.Rated, 10, "", 0, 1, 100, 100, 10, 10, 110, 110, 0, 0);
			DocTaxSummaryLine docSummaryLine = DocTaxSummaryLine.New(taxSummaryLine, Factory);
			AssertEquals("10%", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.CapitalRated;
			AssertEquals("10%", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				taxSummaryLine.TaxRateType = AccTaxRate.Types.ServiceTax;
				AssertEquals("10%", docSummaryLine.TaxDescriptionWithDescriptionOverride);
			}

			taxSummaryLine.TaxRateType = AccTaxRate.Types.NotReportable;
			AssertEquals("Not Applicable", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.Exempt;
			AssertEquals("Exempt", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse Charge", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.ReportableUnderBusinessTax;
			AssertEquals("Reportable Under Business Tax", docSummaryLine.TaxDescriptionWithDescriptionOverride);

			taxSummaryLine.TaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				AssertEquals("Exon.", docSummaryLine.TaxDescriptionWithDescriptionOverride);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AssertEquals("SPV", docSummaryLine.TaxDescriptionWithDescriptionOverride);
			}
		}

		public void TestTaxDescription()
		{
			TaxSummaryLine taxSummaryLine = new TaxSummaryLine(AccTaxRate.Types.Rated, 10, "", 0, 1, 100, 100, 10, 10, 110, 110, 0, 0);
			DocTaxSummaryLine docSummaryLine = DocTaxSummaryLine.New(taxSummaryLine, Factory);
			AssertEquals("10%", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.CapitalRated;
			AssertEquals("10%", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.NotReportable;
			AssertEquals("N/A", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.Exempt;
			AssertEquals("Exempt", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse", docSummaryLine.TaxDescription);

			taxSummaryLine.TaxRateType = AccTaxRate.Types.ReportableUnderBusinessTax;
			AssertEquals("N/A", docSummaryLine.TaxDescription);
		}

		public void TestExtraTaxDescription()
		{
			TaxSummaryLine taxSummaryLine = new TaxSummaryLine("RAT", 10, "QST", 0, 1, 100, 100, 10, 10, 110, 110, 0, 0);
			DocTaxSummaryLine docSummaryLine = DocTaxSummaryLine.New(taxSummaryLine, Factory);
			AssertEquals("QST", docSummaryLine.ExtraTaxDescription);

			taxSummaryLine.TaxExtraRateType = "QCT";
			AssertEquals("QST", docSummaryLine.ExtraTaxDescription);

			taxSummaryLine.TaxExtraRateType = "RET";
			AssertEquals("Retention", docSummaryLine.ExtraTaxDescription);

			taxSummaryLine.TaxExtraRateType = "EDU";
			AssertEquals("CESS", docSummaryLine.ExtraTaxDescription);

			taxSummaryLine.TaxExtraRateType = "INP";
			AssertEquals("Input", docSummaryLine.ExtraTaxDescription);

			taxSummaryLine.TaxExtraRateType = "";
			AssertEquals("", docSummaryLine.ExtraTaxDescription);
		}

		public void TestExtraTaxRate()
		{
			TaxSummaryLine taxSummaryLine = new TaxSummaryLine("RAT", 10, "QST", 25, 10, 100, 100, 10, 10, 110, 110, 0, 0);
			DocTaxSummaryLine docSummaryLine = DocTaxSummaryLine.New(taxSummaryLine, Factory);
			AssertEquals("2.5%", docSummaryLine.ExtraTaxRate);

			//taxSummaryLine.TaxExtraRate = 0;
			taxSummaryLine.TaxExtraRateNumerator = 0;
			AssertEquals("0%", docSummaryLine.ExtraTaxRate);
		}

		public void TestVariousTaxAmount()
		{
			TaxSummaryLine taxSummaryLine = new TaxSummaryLine("RAT", 10, "QST", 25, 10, 100, 10, 12.5, 1.25, 112.5, 11.25, 2.5, 0.25);
			DocTaxSummaryLine docSummaryLine = DocTaxSummaryLine.New(taxSummaryLine, Factory);

			AssertEquals(100m, docSummaryLine.TotalExcludeTaxAmountInOSCurrency);
			AssertEquals(10m, docSummaryLine.TotalExcludeTaxAmountInLocalCurrency);
			AssertEquals(12.5m, docSummaryLine.TotalTaxAmountInOSCurrency);
			AssertEquals(1.25m, docSummaryLine.TotalTaxAmountInLocalCurrency);
			AssertEquals(112.5m, docSummaryLine.TotalIncludeTaxAmountInOSCurrency);
			AssertEquals(11.25m, docSummaryLine.TotalIncludeTaxAmountInLocalCurrency);
			AssertEquals(2.5m, docSummaryLine.TotalExtraTaxAmountInOSCurrency);
			AssertEquals(0.25m, docSummaryLine.TotalExtraTaxAmountInLocalCurrency);
			AssertEquals(10m, docSummaryLine.TotalTaxAmountExcludeExtraTaxAmountInOSCurrency);
			AssertEquals(1m, docSummaryLine.TotalTaxAmountExcludeExtraTaxAmountInLocalCurrency);
		}

		public void TestIsSPVLine()
		{
			var spvTaxSummaryLine = new TaxSummaryLine("SPV", 10, "QST", 25, 10, 100, 10, 12.5, 1.25, 112.5, 11.25, 2.5, 0.25);
			var docSummaryLine = DocTaxSummaryLine.New(spvTaxSummaryLine, Factory);
			Assert("SPV tax rate type", docSummaryLine.IsSPVLine);

			var otherTaxSummaryLine = new TaxSummaryLine("TES", 10, "QST", 25, 10, 100, 10, 12.5, 1.25, 112.5, 11.25, 2.5, 0.25);
			docSummaryLine = DocTaxSummaryLine.New(otherTaxSummaryLine, Factory);
			Assert("not SPV tax rate type", !docSummaryLine.IsSPVLine);
		}
	}
}
