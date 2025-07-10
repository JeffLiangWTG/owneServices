using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Customs
{
	[TestedType(typeof(vw_ZACusEntryInstruction))]
	sealed class vw_ZACusEntryInstructionTest : BiCreateScriptTest
	{
		public void TestView()
		{
			EDWTestDataCreator.CreateCusEntryInstruction("US", 5, "US DataModel", "ZZZ", "CustomsOfficeOverride=ABC");
			var (entryInstructionID, entryInstructionKey) = EDWTestDataCreator.CreateCusEntryInstruction("ZA", 10, "Some Description", "XYZ", "CustomsOfficeOverride=BBB");
			var sql = $"SELECT * FROM [{ScriptDbName}].[dbo].[vw_ZACusEntryInstruction]";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("BBB", result.Rows[0]["Customs Office Override"]);
			AssertEquals(10L, result.Rows[0]["Declaration Key"]);
			AssertEquals("Some Description", result.Rows[0]["Description"]);
			AssertEquals(entryInstructionID, result.Rows[0]["Entry Instruction ID"]);
			AssertEquals(entryInstructionKey, result.Rows[0]["Entry Instruction Key"]);
			AssertEquals("XYZ", result.Rows[0]["Style"]);
		}

		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
