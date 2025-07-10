using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_GlbStaff_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "GS";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			Guid pk;
			ZGeography geoLocation;

			pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbStaff (GS_PK, GS_CODE, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{0}', 'ZZ1', 'ZZ1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk));
			AssertAfterTriggerEvents(pk, GlbStaffSchema.Constants.GS_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbStaff (GS_PK, GS_CODE, GS_LoginName, GS_GeoLocation, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{0}', 'ZZ2', 'ZZ2', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbStaffSchema.Constants.GS_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbStaff (GS_PK, GS_CODE, GS_LoginName, GS_GeoLocation, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{0}', 'ZZ3', 'ZZ3', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbStaffSchema.Constants.GS_GeoLocation, 1, geoLocation);
		}
	}
}

