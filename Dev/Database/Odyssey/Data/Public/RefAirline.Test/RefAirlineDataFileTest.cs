using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefAirlineDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefAirlineDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestRefAirlineDataFile()
		{
			DataHelpers.ClearTable("RefAirline");
			string insertSql = @"
				INSERT dbo.RefAirline (RM_PK, RM_AirlineName1, RM_AddressLine1, RM_IsCASSControlled) VALUES ('D3FF0168-0883-4D59-AB18-28CCB15BE9EF', 'Airline 1', 'Address A', 1)
				INSERT dbo.RefAirline (RM_PK, RM_AirlineName1, RM_AddressLine1, RM_IsCASSControlled) VALUES ('1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C', 'Airline 2', 'Address A', 1)";
			Db.Connection.ExecuteNonQuery(insertSql);

			RefAirlineDataFile file = new RefAirlineDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals("RefAirline row count", 2, data.Tables["RefAirline"].Rows.Count);
			AssertEquals("AddressLine1 - Row 0", "Address A", data.Tables["RefAirline"].Rows[0]["RM_AddressLine1"].ToString());
		}
	}
}
