using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__ReportingBookGeneralLedgerAggregateData))]
	internal class vw_GRP__ReportingBookGeneralLedgerAggregateDataTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var result = Execute();
			var columns = GetColumns();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Count, result.Columns.Count);
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"AccountTypeCode",
				"AlternateChartKey",
				"AlternateGLAccount",
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
				"DepartmentKey",
				"GLAccountKey",
				"GLAmountLocalBalance",
				"GLAmountLocalCredit",
				"GLAmountLocalDebit",
				"OriginalAttribute_LFE",
				"OriginalAttribute_LFO",
				"OriginalAttribute_OCG",
				"OriginalAttribute_ORG",
				"OriginalAttribute_SPR",
				"OriginalAttribute_TIC",
				"PeriodManagementKey",
				"PostPeriod",
				"ReportingBookCode",
				"ReportingBookKey",
				"TransactionCategory",
				"TranslatedCredit",
				"TranslatedDebit",
				"TranslatedBalance",
				"TranslatedCurrency",
				"CurrencyTranslationLevel",
				"TranslatedRateType",
				"IsReciprocal",
				"LocalCurrency",
				"SubUnitRatio"
			};

			return columns;
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}
	}
}
