using System;
using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class WhsSystemLocationTypeDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new WhsSystemLocationTypeDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestWhsSystemLocationTypeDataFile()
		{
			DataHelpers.ClearTable("WhsLocationType");
			var locationTypePk = Guid.NewGuid();

			var insertLocationTypeSql = $@"
INSERT INTO dbo.WhsLocationType (WLT_PK, WLT_Code, WLT_Description, WLT_IsPalletIDNeutral, WLT_MaximumNumberOfProducts, WLT_LocationClass, WLT_IsSystem, WLT_IsActive, WLT_SystemLastEditTimeUtc, WLT_SystemCreateTimeUtc, WLT_SystemCreateUser, WLT_SystemLastEditUser) 
VALUES ('{locationTypePk}', 'XXX', 'Desc', 1,0, 'NOR',1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')";

			Db.Connection.ExecuteNonQuery(insertLocationTypeSql);
			var file = new WhsSystemLocationTypeDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals(data.Tables["WhsLocationType"].Rows.Count, 1);
			Assert(data.Tables["WhsLocationType"].Rows.Contains(locationTypePk));

			var locationType = data.Tables["WhsLocationType"].Rows[0];
			AssertEquals("XXX", locationType["WLT_Code"]);
			AssertEquals("PWA", locationType["WLT_DefaultCycleCountGranularity"]);
			AssertEquals("Desc", locationType["WLT_Description"]);
			AssertEquals(true, locationType["WLT_IsSystem"]);
			AssertEquals(true, locationType["WLT_IsActive"]);
			AssertEquals(true, locationType["WLT_IsPalletIDNeutral"]);
			AssertEquals(0, locationType["WLT_MaximumNumberOfProducts"]);
		}
	}
}
