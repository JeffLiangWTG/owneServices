using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class SplitTransactionLineGSTVATTest : ScriptTest
	{
		public void TestHandlesAllCases()
		{
			foreach (var field in typeof(AccTaxRate.ExtraTypes).GetFields().Where(f => f.IsLiteral))
			{
				var extraTypeString = (string)field.GetValue(null);
				if (extraTypeString == AccTaxRate.ExtraTypes.ServiceTax || extraTypeString == AccTaxRate.ExtraTypes.RegionalTax || extraTypeString == AccTaxRate.ExtraTypes.StateGST)
				{
					Assert(true);
				}
				else
				{
					var line = SetupLine(10, 1, 5, 1);
					var rz = (decimal)RunScript(line.AL_AT, extraTypeString, 100M, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2)["GST"];
					Assert($"Some kind of calculation performed for extra tax type '{extraTypeString}'",
						(rz == 0M && extraTypeString == AccTaxRate.ExtraTypes.VATRemittedByCustomer) ||
						(rz != 0M && (rz != 100M || extraTypeString == AccTaxRate.ExtraTypes.ChinaInputVATClaimed || extraTypeString == AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax)));
				}
			}
		}

		public void TestQuebecQSTAndQuebecQSTExcludingGSTInQSTBase()
		{
			var line = SetupLine(5, 1, 85, 10);
			RunScriptThenAssert(line.AL_AT, 75, 133.88, AccTaxRate.ExtraTypes.QuebecQST, 208.88, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);

			line = SetupLine(5, 1, 95, 10);
			RunScriptThenAssert(line.AL_AT, 50, 99.75, AccTaxRate.ExtraTypes.QuebecQST, 149.75, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);

			line = SetupLine(5, 1, 9975, 1000);
			RunScriptThenAssert(line.AL_AT, 50, 99.75, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 149.75, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
		}

		public void TestIndiaPrimaryAndSecondaryEducationTax()
		{
			var line = SetupLine(12, 1, 3, 1);
			RunScriptThenAssert(line.AL_AT, 94.96,2.85, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 97.81, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
		}

		public void TestChinaInputVATClaimed()
		{
			var line = SetupLine(17, 1, 7, 1);
			RunScriptThenAssert(line.AL_AT, 100,0, AccTaxRate.ExtraTypes.ChinaInputVATClaimed, 100, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);

			line = SetupLine(0, 1, 7, 1);
			RunScriptThenAssert(line.AL_AT, 0,100, AccTaxRate.ExtraTypes.ChinaInputVATClaimed, 100, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
		}

		public void TestVATRetention()
		{
			var line = SetupLine(16, 1, 4, 1);
			RunScriptThenAssert(line.AL_AT, 160, -40, AccTaxRate.ExtraTypes.VATRetention, 120, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
		}

		public void TestVATRemittedByCustomer()
		{
			var line = SetupLine(1, 1, 0, 2);
			RunScriptThenAssert(line.AL_AT, 22, -22, AccTaxRate.ExtraTypes.VATRemittedByCustomer, 0, 22, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 100);
		}

		public void TestGSTWithoutExtraType()
		{
			var line = SetupLine(10, 1, 10, 1);
			RunScriptThenAssert(line.AL_AT, 100, 0, "", 100, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2);
		}

		public void TestSTAExtraTax()
		{
			var line = SetupLine(9, 1, 9, 1);
			RunScriptThenAssert(line.AL_AT, 4.9M, 5.1M, AccTaxRate.ExtraTypes.StateGST, 10, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 2, countryCode: Core.Constants.CountryCodes.India, al_GSTVATExtra: 5.1M);
		}

		[ExpectNoExceptions]
		public void TestSTAExtraTax_ZeroRated()
		{
			var line = SetupLine(0, 1, 1, 2);
			RunScriptThenAssert(line.AL_AT,0, 0, AccTaxRate.ExtraTypes.StateGST, 0, line.AL_TaxRateNumerator, line.AL_TaxRateDenominator, line.AL_TaxExtraRateNumerator, line.AL_TaxExtraRateDenominator, 100);
		}

		#region Implementation
		void RunScriptThenAssert(ZGuid aT_PK, ZDecimal expectedGSTValue, ZDecimal expectedExtraTaxValue, ZString rateType, ZDecimal gstVat, ZInt taxRateNumerator, ZInt taxRateDenominator, ZInt extraTaxRateNumerator, ZInt extraTaxRateDenominator, ZInt numberOfDecimals
			, decimal lineAmount = 0.00m, string countryCode = "", decimal al_GSTVATExtra = 0.0M)
		{
			var row = RunScript(aT_PK, rateType, gstVat, taxRateNumerator, taxRateDenominator, extraTaxRateNumerator, extraTaxRateDenominator, numberOfDecimals, lineAmount, countryCode, al_GSTVATExtra);
			AssertEquals("Expecting SplitTransactionLineGSTVAT to return this value", expectedGSTValue, row["GST"]);
			AssertEquals("Expecting SplitTransactionLineGSTVAT to return this value", expectedExtraTaxValue, row["ExtraTax"]);
		}

		static DataRow RunScript(ZGuid aT_PK, ZString rateType, ZDecimal gstVat, ZInt taxRateNumerator, ZInt taxRateDenominator, ZInt extraTaxRateNumerator, ZInt extraTaxRateDenominator, ZInt numberOfDecimals
			, decimal lineAmount = 0.00m, string countryCode = "", decimal al_GSTVATExtra = 0.0M)
		{
			var sql = $@"SELECT * FROM dbo.SplitTransactionLineGSTVAT(
'{aT_PK}',
'{rateType}',
{gstVat}, 
{lineAmount}, 
{taxRateNumerator}, 
{taxRateDenominator}, 
{extraTaxRateNumerator}, 
{extraTaxRateDenominator}, 
{numberOfDecimals}, 
'{countryCode}', 
{al_GSTVATExtra})";

			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("One row returned", 1, dataTable.Rows.Count);
			return dataTable.Rows[0];
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

		#endregion

	}
}


