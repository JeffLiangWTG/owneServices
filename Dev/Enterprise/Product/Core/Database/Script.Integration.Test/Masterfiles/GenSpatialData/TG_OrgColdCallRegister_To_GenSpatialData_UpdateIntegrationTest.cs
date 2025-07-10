using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgColdCallRegister_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "O1";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_GeoLocation, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) values ('{0}', 'ZZ1', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.CreatePoint(43, -8.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgColdCallRegister set O1_GeoLocation = {0}, O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' where O1_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgColdCallRegister set O1_GeoLocation = {0}, O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' where O1_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(-13.9, 48.99);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgColdCallRegister set O1_GeoLocation = {0}, O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' where O1_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GenSpatialData where SPD_ParentID = '{0}'", pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(51.3, -81.12);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgColdCallRegister set O1_GeoLocation = {0}, O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' where O1_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, OrgColdCallRegisterSchema.Constants.O1_GeoLocation, 1, geoLocation);
		}
	}
}
