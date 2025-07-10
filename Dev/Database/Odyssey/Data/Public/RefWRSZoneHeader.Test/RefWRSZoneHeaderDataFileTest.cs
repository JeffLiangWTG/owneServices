using System;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class RefWRSZoneHeaderDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefWRSZoneHeaderDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		// DAT must have had massive REF data and causes the test to fail intermittently with timeouts on DAT machines.
		// Local dev PC takes less than 1 second to finish.
		[SnailTest, UseSnapshotProtection(skipTransaction: true)]
		public void TestOnlyWRSZonesAreLoadedFromDatabase()
		{
			var nonWRSZoneHeaderPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 {RefZoneHeaderSchema.Constants.PK} FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.FZ_ZoneType} != 'WRS'");
			var nonWRSZonePivotPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 {RefZonePivotSchema.Constants.PK} FROM {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} WHERE {RefZonePivotSchema.Constants.F2_FZ} = '{nonWRSZoneHeaderPk}'");
			var testDataFile = new RefWRSZoneHeaderDataFile();

			var data = testDataFile.LoadDataFromDatabase();
			Assert(data.Tables[0].Rows.Cast<DataRow>().All(x => (string)x[RefZoneHeaderSchema.Constants.FZ_ZoneType] == "WRS"));
			Assert(data.Tables[0].Rows.Cast<DataRow>().All(x => (Guid)x[RefZoneHeaderSchema.Constants.PK] != nonWRSZoneHeaderPk));
			Assert(data.Tables[1].Rows.Cast<DataRow>().All(x => (Guid)x[RefZonePivotSchema.Constants.PK] != nonWRSZonePivotPk));
		}
	}
}
