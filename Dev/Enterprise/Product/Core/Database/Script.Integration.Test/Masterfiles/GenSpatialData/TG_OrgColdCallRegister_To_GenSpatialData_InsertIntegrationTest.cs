using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgColdCallRegister_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "O1";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			Guid pk;
			ZGeography geoLocation;

			pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) values ('{0}', 'ZZ1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_GeoLocation, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) values ('{0}', 'ZZ2', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_GeoLocation, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) values ('{0}', 'ZZ3', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);
		}
	}
}

