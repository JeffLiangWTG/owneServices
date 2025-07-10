using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_PatternMatchingAddress_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "PMA";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.PatternMatchingAddress (PMA_PK, PMA_ParentId, PMA_ParentTableCode, PMA_GeoLocation) values ('{0}', newid(), 'GS', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.CreatePoint(43, -8.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.PatternMatchingAddress set PMA_GeoLocation = {0} where PMA_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("update dbo.PatternMatchingAddress set PMA_GeoLocation = {0} where PMA_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(-13.9, 48.99);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.PatternMatchingAddress set PMA_GeoLocation = {0} where PMA_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GenSpatialData where SPD_ParentID = '{0}'", pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(51.3, -81.12);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.PatternMatchingAddress set PMA_GeoLocation = {0} where PMA_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 1, geoLocation);
		}
	}
}

