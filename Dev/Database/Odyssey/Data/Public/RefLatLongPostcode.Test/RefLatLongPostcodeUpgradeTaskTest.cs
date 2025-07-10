using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefLatLongPostcodeUpgradeTaskTest : TransactionedTestCase
	{
		public void TestMixOfInsertUpdateDelete()
		{
			string sqlText = "DELETE dbo.RefLatLongPostcode WHERE RJ_PostCode = '2000'";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = "UPDATE dbo.RefLatLongPostcode SET RJ_CitySuburb = 'West Penno' WHERE RJ_PostCode = '2125'";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = "INSERT dbo.RefLatLongPostcode (RJ_PK, RJ_PostCode) VALUES ('17C017B7-0817-4714-8285-3F3B165F9DC3', '9999')";
			Db.Connection.ExecuteNonQuery(sqlText);

			RefLatLongPostcodeUpgradeTask task = new RefLatLongPostcodeUpgradeTask();
			task.Run();

			RefLatLongPostcodeDataFile tempFile = new RefLatLongPostcodeDataFile();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			DataRow[] unmatchingRows = data.Tables["RefLatLongPostcode"].Select("RJ_PostCode = '9999'");
			AssertEquals("Unmatching Events should have been deleted", 0, unmatchingRows.Length);

			DataRow[] reInsertedRows = data.Tables["RefLatLongPostcode"].Select("RJ_PostCode = '2000'");
			AssertEquals("Postcode 2000 should have been re-inserted", 1, reInsertedRows.Length);

			DataRow[] updatedRows = data.Tables["RefLatLongPostcode"].Select("RJ_PostCode = '2125'");
			AssertEquals("2125 Postcode description should have been changed back", "West Pennant Hills", updatedRows[0]["RJ_CitySuburb"].ToString());
		}
	}
}
