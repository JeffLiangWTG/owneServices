using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgColdCallRegister_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "O1";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_GeoLocation, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) values ('{0}', 'ZZ1', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.OrgColdCallRegister where O1_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 0, ZGeography.Empty);
		}
	}
}

