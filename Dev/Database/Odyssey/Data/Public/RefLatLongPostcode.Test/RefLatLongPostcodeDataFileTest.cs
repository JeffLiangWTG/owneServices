using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefLatLongPostcodeDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefLatLongPostcodeDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestRefLatLongPostcodeDataFile()
		{
			DataHelpers.ClearTable("RefLatLongPostcode");

			string insertSql = @"
				INSERT dbo.RefLatLongPostcode (RJ_PK, RJ_RN_NKCountry, RJ_Postcode) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC3', 'AU', '2015')
				INSERT dbo.RefLatLongPostcode (RJ_PK, RJ_RN_NKCountry, RJ_Postcode) VALUES ('183EF542-B544-4121-8686-6EF7755E918C', 'AU', '2125')";
			Db.Connection.ExecuteNonQuery(insertSql);

			RefLatLongPostcodeDataFile file = new RefLatLongPostcodeDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals("RefLatLongPostcode row count", 2, data.Tables["RefLatLongPostcode"].Rows.Count);
			AssertEquals("Postcode - Row 0", "2015", data.Tables["RefLatLongPostcode"].Rows[0]["RJ_Postcode"].ToString());
			AssertEquals("Postcode - Row 1", "2125", data.Tables["RefLatLongPostcode"].Rows[1]["RJ_Postcode"].ToString());
		}
	}
}
