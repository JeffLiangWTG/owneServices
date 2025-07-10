using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.DocManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.DocManager
{
	[TestedType(typeof(StorageDocsMergeAllDbSizes))]
	class StorageDocsMergeAllDbSizesTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			DataTable allSdDbs = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC " + ScriptToTest.Name);
			var expectedSdDbs = TestConnection.GetDatabases(DatabaseType.SD);

			int i = 0;

			foreach (string expectedSdDb in expectedSdDbs)
			{
				string dbName = allSdDbs.Rows[i]["DbName"].ToString();
				AssertEquals("StorageDocs DB name", expectedSdDb, dbName);

				int expectedDbNumber = Convert.ToInt32(dbName.Substring(dbName.Length - 3, 3));
				AssertEquals("StorageDocs DB number", expectedDbNumber, (int)allSdDbs.Rows[i]["DbNumber"]);

				i++;
			}

			AssertEquals("StorageDocs DB count", i, allSdDbs.Rows.Count);
		}
	}
}

