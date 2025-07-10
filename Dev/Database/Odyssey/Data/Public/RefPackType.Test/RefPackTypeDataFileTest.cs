using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class RefPackTypeDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefPackTypeDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}
		public void TestRefPackTypeDataFile()
		{
			DataHelpers.ClearTable("RefPackType");
			string insertSql = @"
							INSERT dbo.RefPackType (F3_PK, F3_Code, F3_Description) VALUES ('e96eeb12-158b-47b7-b393-a1dd422ee464', 'REL', 'Reel')";
			Db.Connection.ExecuteNonQuery(insertSql);

			var file = new RefPackTypeDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals("RefPackType row count", 1, data.Tables["RefPackType"].Rows.Count);
			AssertEquals("F3_Code Row 1", "REL", data.Tables["RefPackType"].Rows[0]["F3_Code"].ToString());
		}
	}
}
