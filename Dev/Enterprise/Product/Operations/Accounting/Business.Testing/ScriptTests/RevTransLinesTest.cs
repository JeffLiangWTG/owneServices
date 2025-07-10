using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class RevTransLinesTest : ScriptTest
	{
		public void TestGSTVAT_WithDecimal()
		{
			var line = SetupLine(11, 10, 0, 1);
			var result = RunScript(line.AL_AT, "AU", "RVS", "", 1000m, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, "CST", 2);
			AssertEquals("Should have 2 rows", 2, result.Rows.Count);

			var inputRow = result.Rows[0];
			AssertEquals("LineAmount", 1000.0000m, inputRow["LineAmount"]);
			AssertEquals("GSTVAT", 11.00m, inputRow["GSTVAT"]);
			AssertEquals("GSTVAT", 0m, inputRow["ExtraTax"]);

			var outputRow = result.Rows[1];
			AssertEquals("LineAmount", -1000.0000m, outputRow["LineAmount"]);
			AssertEquals("GSTVAT", -11.00m, outputRow["GSTVAT"]);
			AssertEquals("GSTVAT", 0m, outputRow["ExtraTax"]);

			line = SetupLine(6, 1, 6, 1);
			result = RunScript(line.AL_AT, "IN", "RVS", "STA", 1000m, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, "CST", 2);
			AssertEquals("Should have 2 rows", 2, result.Rows.Count);

			inputRow = result.Rows[0];
			AssertEquals("LineAmount", 1000.0000m, inputRow["LineAmount"]);
			AssertEquals("GSTVAT", 60.00m, inputRow["GSTVAT"]);
			AssertEquals("GSTVAT", 60.00m, inputRow["ExtraTax"]);

			outputRow = result.Rows[1];
			AssertEquals("LineAmount", -1000.0000m, outputRow["LineAmount"]);
			AssertEquals("GSTVAT", -60.00m, outputRow["GSTVAT"]);
			AssertEquals("GSTVAT", -60.00m, outputRow["ExtraTax"]);
		}

		static DataTable RunScript(ZGuid aT_PK, string countryCode, string taxType, string extraTaxType, decimal lineAmount, int taxRateNumerator, int taxRateDenominator, int extraTaxRateNumberator, int extraTaxRateDenominator, string lineType, int localCurrencyDecimalPlaces)
		{
			var script = $@"
SELECT * 
FROM REVTransLines(	
'{aT_PK}',
'{countryCode}',				--@AT_RN_NKCountry
'{taxType}',					--@TaxType
'{extraTaxType}',				--@ExtraTaxType
{lineAmount},					--@LineAmount
{taxRateNumerator},				--@TaxRateNumerator
{taxRateDenominator},			--@TaxRateDenominator
{extraTaxRateNumberator},		--@AT_ExtraTaxRateNumerator
{extraTaxRateDenominator},		--@AT_ExtraTaxRateDenominator
'{lineType}',					--@LineType
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
