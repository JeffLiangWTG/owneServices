using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_GlbCompany_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "GC";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_GeoLocation, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{0}', 'ZZ1', 'AU company', {1}, 'AU', 'AUD')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.CreatePoint(43, -8.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GlbCompany set GC_GeoLocation = {0}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GlbCompany set GC_GeoLocation = {0}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(-13.9, 48.99);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GlbCompany set GC_GeoLocation = {0}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GenSpatialData where SPD_ParentID = '{0}'", pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(51.3, -81.12);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GlbCompany set GC_GeoLocation = {0}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 1, geoLocation);
		}
	}
}

