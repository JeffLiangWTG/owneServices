using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgAddress_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "OA";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var ohPk = Guid.NewGuid();
			var sqlText = string.Format(@"insert into dbo.OrgHeader (OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{0}', 'dummy OH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", ohPk);
			TestConnection.ExecuteNonQuery(sqlText);

			Guid oaPk;
			ZGeography geoLocation;
			ZGeography geofencePolygon;

			oaPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', 'dummy OA address1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			oaPk = Guid.NewGuid();
			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeoLocation, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '1', 'dummy OA address1', {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			oaPk = Guid.NewGuid();
			geofencePolygon = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeofencePolygon, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '2', 'dummy OA address2', {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geofencePolygon.ToPolygonSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			oaPk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeoLocation, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '3', 'dummy OA address3', {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			oaPk = Guid.NewGuid();
			geofencePolygon = ZGeography.CreatePolygon("1 2,3 4,5 6,1 2");
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeofencePolygon, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '4', 'dummy OA address4', {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geofencePolygon.ToPolygonSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);

			oaPk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			geofencePolygon = ZGeography.CreatePolygon("1 2,3 4,5 6,1 2");
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeoLocation, OA_GeofencePolygon, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '5', 'dummy OA address5', {2}, {3}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geoLocation.ToPointSqlText(), geofencePolygon.ToPolygonSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);
		}
	}
}
