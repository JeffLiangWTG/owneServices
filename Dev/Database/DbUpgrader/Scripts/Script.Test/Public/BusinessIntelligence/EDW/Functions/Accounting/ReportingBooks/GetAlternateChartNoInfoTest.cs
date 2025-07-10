using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetAlternateChartNoInfo))]
	class GetAlternateChartNoInfoTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestGetAlternateChartNoInfo()
		{
			PrepareTestData();

			var result = Excute("123456", 1);
			AssertEquals("If IsFixedLength = 1, length of alternateChartNo must be 9", 0, result.Rows.Count);
			result = Excute("1234567890", 1);
			AssertEquals("If IsFixedLength = 1, length of alternateChartNo must be 9", 0, result.Rows.Count);
			result = Excute("123456789", 1);
			AssertEquals("If IsFixedLength = 1, length of alternateChartNo must be 9", 1, result.Rows.Count);
			AssertEquals("If IsFixedLength = 1, length of alternateChartNo must be 9", 1, result.Select("DestNo = '123-456.789' and TierLevel = 3 and ParentNo = '123-456.000'").Length);

			result = Excute("12", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12' and TierLevel = 1 and ParentNo IS NULL").Length);
			result = Excute("123", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Rows.Count);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo is null").Length);
			result = Excute("1234", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Rows.Count);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo is null").Length);
			result = Excute("12345", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12.345' and TierLevel = 2 and ParentNo = '12.000'").Length);
			result = Excute("123456789", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12.345.6789' and TierLevel = 3 and ParentNo = '12.345.0000'").Length);
			result = Excute("123450000", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12.345.0000' and TierLevel = 2 and ParentNo = '12.000.0000'").Length);

			result = Excute("12000", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12.000' and TierLevel = 1 and ParentNo is null").Length);
			result = Excute("120003456", 2);
			AssertEquals("If IsFixedLength = 0, length of alternateChartNo only can be 2 5 9", 1, result.Select("DestNo = '12.000.3456' and TierLevel = 3 and ParentNo = '12.000.0000'").Length);
		}

		DataTable Excute(string alternateChartNo, int alternateChartKey)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[dbo].[GetAlternateChartNoInfo]('{alternateChartNo}', {alternateChartKey})";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void PrepareTestData()
		{
			TestHelper.InsertALternateChart("202");
			TestHelper.InsertALternateChart("203", isFixedLength: 0);
			TestHelper.InsertALternateChartFormat("999", 1, "-", 1);
			TestHelper.InsertALternateChartFormat("999", 1, ".", 2);
			TestHelper.InsertALternateChartFormat("999", 1, "-", 3);
			TestHelper.InsertALternateChartFormat("99", 2, ".", 1);
			TestHelper.InsertALternateChartFormat("999", 2, ".", 2);
			TestHelper.InsertALternateChartFormat("9999", 2, "-", 3);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
