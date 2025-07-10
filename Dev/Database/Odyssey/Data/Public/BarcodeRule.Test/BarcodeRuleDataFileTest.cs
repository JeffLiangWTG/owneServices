using System.IO;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class BarcodeRuleDataFileTest : TransactionedTestCase
	{
		#region TestBarcodeRuleDataFile

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new BarcodeRuleDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestBarcodeRuleDataFile()
		{
			DataHelpers.ClearTable("BarcodeRuleComponent");
			DataHelpers.ClearTable("BarcodeRule");
			DataHelpers.ClearTable("BarcodeRuleSet");

			var ruleSetSystemDefined = new BarcodeRuleSet("XYZ") { BRS_IsSystem = true }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeRule(ruleSetSystemDefined, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			new BarcodeRuleComponent(rule1, "N", "PID") { BRC_Sequence = 0, BRC_ApplicationID = "00", BRC_MinLength = 18, BRC_MaxLength = 18 }.Insert(TestConnection);
			new BarcodeRuleComponent(rule1, "N", "QTY") { BRC_Sequence = 0, BRC_ApplicationID = "30", BRC_MinLength = 1, BRC_MaxLength = 8 }.Insert(TestConnection);
			var rule2 = new BarcodeRule(ruleSetSystemDefined, "Test rule 2", 2) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			new BarcodeRuleComponent(rule2, "N", "PRC") { BRC_Sequence = 0, BRC_ApplicationID = "01", BRC_MinLength = 14, BRC_MaxLength = 14 }.Insert(TestConnection);
			new BarcodeRuleComponent(rule2, "N", "PRC") { BRC_Sequence = 0, BRC_ApplicationID = "02", BRC_MinLength = 14, BRC_MaxLength = 14 }.Insert(TestConnection);
			new BarcodeRuleComponent(rule2, "AN", "PRC") { BRC_Sequence = 0, BRC_ApplicationID = "241", BRC_MinLength = 1, BRC_MaxLength = 30 }.Insert(TestConnection);

			var ruleSetNonSystemDefined = new BarcodeRuleSet("BLA") { BRS_IsSystem = false }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeRule(ruleSetNonSystemDefined, "Test rule 3", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			new BarcodeRuleComponent(rule3, "N", "PID") { BRC_Sequence = 0, BRC_ApplicationID = "00", BRC_MinLength = 18, BRC_MaxLength = 18 }.Insert(TestConnection);

			var file = new BarcodeRuleDataFile();
			var data = file.LoadDataFromDatabase();
			AssertEquals("Table Count", 3, data.Tables.Count);
			AssertEquals(1, data.Tables["BarcodeRuleSet"].Rows.Count);
			AssertEquals(true, data.Tables["BarcodeRuleSet"].Rows[0]["BRS_IsSystem"]);
			AssertEquals(2, data.Tables["BarcodeRule"].Rows.Count);
			AssertEquals(5, data.Tables["BarcodeRuleComponent"].Rows.Count);
		}

		#endregion
	}
}
