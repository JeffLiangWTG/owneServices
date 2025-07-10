using System;
using System.Data;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.RefZone
{
	sealed class RefZoneDataFileTest : TransactionedTestCase
	{
		public void TestOnlyRowsInXmlAreLoadedFromDatabase()
		{
			var testDataFile = new RefZoneDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			var data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 2, data.Tables.Count);
		}

		public void TestWRSZonesAreNotLoadedFromDatabase()
		{
			var wrsZonePk = Guid.NewGuid();
			var wrsPivotPk = Guid.NewGuid();
			var auCountryPk = (Guid)TestConnection.ExecuteScalar($"SELECT {RefCountrySchema.Constants.PK} FROM {RefCountrySchema.Constants.SqlSchemaName}.{RefCountrySchema.Constants.TableName} WHERE {RefCountrySchema.Constants.RN_Code} = 'AU'");

			var sqlText = $@"
	INSERT {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} (
		{RefZoneHeaderSchema.Constants.PK},
		{RefZoneHeaderSchema.Constants.FZ_Code},
		{RefZoneHeaderSchema.Constants.FZ_Description},
		{RefZoneHeaderSchema.Constants.FZ_ZoneType},
		{RefZoneHeaderSchema.Constants.FZ_SystemCreateTimeUtc},
		{RefZoneHeaderSchema.Constants.FZ_SystemCreateUser},
		{RefZoneHeaderSchema.Constants.FZ_SystemLastEditTimeUtc},
		{RefZoneHeaderSchema.Constants.FZ_SystemLastEditUser}
	)
	VALUES ('{wrsZonePk}', 'Z1', 'ZoneOne', 'WRS', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

	INSERT {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} (
		{RefZonePivotSchema.Constants.PK},
		{RefZonePivotSchema.Constants.F2_FZ},
		{RefZonePivotSchema.Constants.F2_ParentID},
		{RefZonePivotSchema.Constants.F2_ParentTableCode},
		{RefZonePivotSchema.Constants.F2_SystemCreateTimeUtc},
		{RefZonePivotSchema.Constants.F2_SystemCreateUser},
		{RefZonePivotSchema.Constants.F2_SystemLastEditTimeUtc},
		{RefZonePivotSchema.Constants.F2_SystemLastEditUser}
	)
	VALUES ('{wrsPivotPk}', '{wrsZonePk}', '{auCountryPk}', '{RefCountrySchema.Constants.Prefix}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			TestConnection.ExecuteNonQuery(sqlText);
			var testDataFile = new RefZoneDataFile();

			var data = testDataFile.LoadDataFromDatabase();
			Assert(data.Tables[0].Rows.Cast<DataRow>().All(x => (Guid)x[RefZoneHeaderSchema.Constants.PK] != wrsZonePk));
			Assert(data.Tables[1].Rows.Cast<DataRow>().All(x => (Guid)x[RefZonePivotSchema.Constants.PK] != wrsPivotPk));
		}
	}
}
