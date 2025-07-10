using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GeneralLedgerTranslatedData))]
	internal class vw_CUS__GeneralLedgerTranslatedDataTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = new[]
			{
				"AccountTypeCode",
				"AlternateChartKey",
				"AlternateGLAccount",
				"AlternateGLAccountDescription",
				"AlternateGLAccountKey",
				"Attribute_LFE",
				"Attribute_LFO",
				"Attribute_OCG",
				"Attribute_ORG",
				"Attribute_SPR",
				"Attribute_TIC",
				"BranchKey",
				"CompanyKey",
				"CompanyOfPeriodKey",
				"CurrencyTranslationForReportingBookKey",
				"CurrencyTranslationLevel",
				"DepartmentKey",
				"ExchangeRatesForReportingBookKey",
				"GeneralLedgerDataAttributeKey",
				"GeneralLedgerDataKey",
				"GLAccountKey",
				"IsReciprocal",
				"LocalAmount",
				"LocalCurrency",
				"OrganizationKey",
				"OriginalAccount",
				"OriginalAttribute_LFE",
				"OriginalAttribute_LFO",
				"OriginalAttribute_OCG",
				"OriginalAttribute_ORG",
				"OriginalAttribute_SPR",
				"OriginalAttribute_TIC",
				"PeriodManagementKey",
				"PostDate",
				"PostPeriodForReportingBook",
				"ReportingBookCode",
				"ReportingBookKey",
				"SubUnitRatio",
				"TransactionCategory",
				"TranslatedAmount",
				"TranslatedCurrency",
				"TranslatedExchangeRate",
				"TranslatedRateType",
				"Units"
			};

			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		DataTable SelectRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].vw_CUS__GeneralLedgerTranslatedData");
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
