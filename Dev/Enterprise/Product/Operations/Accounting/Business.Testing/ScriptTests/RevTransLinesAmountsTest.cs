using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class RevTransLinesAmountsTest : ScriptTest
	{
		public void TestGSTVAT_WithDecimal()
		{
			var line = SetupLine(11, 10, 0, 1);
			DataTable result = RunScript(line.AL_AT,"AU", "RAT", "", 1, 1000m, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
			AssertEquals("Should have 1 rows", 1, result.Rows.Count);
			var row = result.Rows[0];
			AssertEquals("LineAmount", 1000.0000m, row["LineAmount"]);
			AssertEquals("GSTVAT", 11.00m, row["GSTVAT"]);
			AssertEquals("ExtraTax", 0m, row["ExtraTax"]);

			line = SetupLine(6, 1, 6, 1);
			result = RunScript(line.AL_AT, "IN", "RVS", "STA", 1, 1000m, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
			AssertEquals("Should have 1 rows", 1, result.Rows.Count);
			row = result.Rows[0];
			AssertEquals("LineAmount", 1000.0000m, row["LineAmount"]);
			AssertEquals("GSTVAT", 60m, row["GSTVAT"]);
			AssertEquals("ExtraTax", 60m, row["ExtraTax"]);
		}

		DataTable RunScript(ZGuid aT_PK, string countryCode, string taxRateType, string extraTaxRateType, int multiplier, decimal lineAmount, int taxRateNumerator, int taxRateDenominator, int extraTaxRateNumberator, int extraTaxRateDenominator, int localCurrencyDecimalPlaces)
		{
			var script = $@"
SELECT * 
FROM RevTransLinesAmounts(	
'{aT_PK}',
'{countryCode}',				--@AT_RN_NKCountry
'{taxRateType}',				--@AT_TaxRateType
'{extraTaxRateType}',			--@AT_ExtraTaxRateType
{multiplier},					--@Multiplier
{lineAmount},					--@LineAmount
{taxRateNumerator},				--@TaxRateNumerator
{taxRateDenominator},			--@TaxRateDenominator
{extraTaxRateNumberator},		--@AT_ExtraTaxRateNumerator
{extraTaxRateDenominator},		--@AT_ExtraTaxRateDenominator
{localCurrencyDecimalPlaces}	--@LocalCurrencyDecimalPlaces
)";
			return DataUtils.GetDataTableFromQuery(Db.Connection, script);
		}

		InvoicingLineBase SetupLine(int taxRateNumerator, int taxRateDenominator, int taxExtraRateNumerator, int taxExtraRateDenominator)
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1M, 35M, 25M, 4M, 3M, 7M, 2M);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M);
			line.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			line.AL_TaxRateNumerator = taxRateNumerator;
			line.AL_TaxRateDenominator = taxRateDenominator;
			line.AL_TaxExtraRateNumerator = taxExtraRateNumerator;
			line.AL_TaxExtraRateDenominator = taxExtraRateDenominator;
			return line;
		}
	}
}
