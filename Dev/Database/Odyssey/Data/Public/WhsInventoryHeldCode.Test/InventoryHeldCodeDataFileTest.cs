using System;
using System.Data;
using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class InventoryHeldCodeDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new InventoryHeldCodeDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestInventoryHeldCodeDataFile()
		{
			DataHelpers.ClearTable("WhsInventoryHeldCode");

			var inventoryHeldCodePK = Guid.NewGuid();
			var insertInventoryHeldCodeSql = @"
INSERT dbo.WhsInventoryHeldCode (WHC_PK, WHC_Code, WHC_Description, WHC_IsSystem, WHC_SystemCreateTimeUtc, WHC_SystemCreateUser, WHC_SystemLastEditTimeUtc, WHC_SystemLastEditUser) 
VALUES (@WHC_PK, @WHC_Code, @WHC_Description, @WHC_IsSystem, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			var insertTemplateCmd = Db.Connection.Command(insertInventoryHeldCodeSql);
			insertTemplateCmd.AddParameter("@WHC_PK", SqlDbType.UniqueIdentifier, inventoryHeldCodePK);
			insertTemplateCmd.AddParameter("@WHC_Code", SqlDbType.NVarChar, "XXXX");
			insertTemplateCmd.AddParameter("@WHC_Description", SqlDbType.NVarChar, "Desc");
			insertTemplateCmd.AddParameter("@WHC_IsSystem", SqlDbType.Bit, 1);
			insertTemplateCmd.ExecuteNonQuery();

			var file = new InventoryHeldCodeDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals(data.Tables["WhsInventoryHeldCode"].Rows.Count, 1);
			Assert(data.Tables["WhsInventoryHeldCode"].Rows.Contains(inventoryHeldCodePK));

			var heldCode = data.Tables["WhsInventoryHeldCode"].Rows[0];
			AssertEquals("XXXX", heldCode["WHC_Code"]);
			AssertEquals("Desc", heldCode["WHC_Description"]);
			AssertEquals(true, heldCode["WHC_IsSystem"]);
		}
	}
}
