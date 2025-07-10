using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_GlbCompany_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "GC";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			Guid pk;
			ZGeography geoLocation;

			pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{0}', 'ZZ1', 'AU company', 'AU', 'AUD')", pk));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_GeoLocation, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{0}', 'ZZ2', 'AU company', {1}, 'AU', 'AUD')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_GeoLocation, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{0}', 'ZZ3', 'AU company', {1}, 'AU', 'AUD')", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbCompanySchema.Constants.GC_GeoLocation, 1, geoLocation);
		}
	}
}

