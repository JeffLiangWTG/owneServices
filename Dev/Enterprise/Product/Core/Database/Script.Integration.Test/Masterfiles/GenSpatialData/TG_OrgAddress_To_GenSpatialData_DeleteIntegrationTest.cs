using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_OrgAddress_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "OA";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var ohPk = Guid.NewGuid();
			var sqlText = string.Format(@"insert into dbo.OrgHeader (OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{0}', 'dummy OH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", ohPk);
			TestConnection.ExecuteNonQuery(sqlText);

			ZGeography geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			ZGeography geofencePolygon = ZGeography.CreatePolygon("1 2,3 4,5 6,1 2");
			Guid oaPk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.OrgAddress (OA_PK, OA_OH, OA_CODE, OA_Address1, OA_GeoLocation, OA_GeofencePolygon, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{0}', '{1}', '1', 'dummy OA address1', {2}, {3}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", oaPk, ohPk, geoLocation.ToPointSqlText(), geofencePolygon.ToPolygonSqlText()));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 1, geoLocation);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 1, geofencePolygon);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.OrgAddress where OA_PK = '{0}'", oaPk));
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeoLocation, 0, ZGeography.Empty);
			AssertAfterTriggerEvents(oaPk, OrgAddressSchema.Constants.OA_GeofencePolygon, 0, ZGeography.Empty);
		}
	}
}

