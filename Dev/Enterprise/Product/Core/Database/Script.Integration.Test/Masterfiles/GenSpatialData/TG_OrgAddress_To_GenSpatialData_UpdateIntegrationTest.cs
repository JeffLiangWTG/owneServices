using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgAddress_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "OA";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var ohPk = Guid.NewGuid();
			var sqlText = string.Format(@"insert into dbo.OrgHeader (OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{0}', 'dummy OH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", ohPk);
			TestConnection.ExecuteNonQuery(sqlText);

			Guid oaPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', 'dummy OA address1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_Address1 = 'dummy OA address2' where OA_PK = '{0}'", oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			ZGeography geofencePolygon = ZGeography.CreatePolygon("1 2,3 4,5 6,1 2");
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeofencePolygon = {0} where OA_PK = '{1}'", geofencePolygon.ToPolygonSqlText(), oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);

			ZGeography geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeoLocation = {0} where OA_PK = '{1}'", geoLocation.ToPointSqlText(), oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);

			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeofencePolygon = convert(geography, 'POINT EMPTY') where OA_PK = '{0}'", oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeoLocation = convert(geography, 'POINT EMPTY') where OA_PK = '{0}'", oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);

			geofencePolygon = ZGeography.CreatePolygon("1 2,3 4,5 6,1 2");
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeofencePolygon = {0}, OA_GeoLocation = {1} where OA_PK = '{2}'", geofencePolygon.ToPolygonSqlText(), geoLocation.ToPointSqlText(), oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);

			TestConnection.ExecuteNonQuery(string.Format("update dbo.OrgAddress set OA_GeoLocation = convert(geography, 'POINT EMPTY'), OA_GeofencePolygon = convert(geography, 'POINT EMPTY') where OA_PK = '{0}'", oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);
		}
	}
}

