using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_JobDocAddress_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "E2";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_GeoLocation, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser) values('{0}', newid(), 'Z0', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.CreatePoint(43, -8.3);
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobDocAddress
SET
	E2_GeoLocation = {0},
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP'
WHERE
	E2_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobDocAddress
SET
	E2_GeoLocation = {0},
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP'
WHERE
	E2_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(-13.9, 48.99);
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobDocAddress
SET
	E2_GeoLocation = {0},
	E2_SystemLastEditTimeUtc = GETUTCDATE(),
	E2_SystemLastEditUser = '~BP'
WHERE
	E2_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 1, geoLocation);
		}
	}
}

