using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_JobDocAddress_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "E2";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(@"
insert into dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_GeoLocation, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
values ('{0}', newid(), 'Z0', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.JobDocAddress where E2_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, JobDocAddressSchema.Constants.E2_GeoLocation, 0, ZGeography.Empty);
		}
	}
}
