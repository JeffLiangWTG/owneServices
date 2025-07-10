using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_RefUNLOCO_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "RL";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation) values('{0}', 'XX2', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.CreatePoint(43, -8.3);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.RefUNLOCO set RL_GeoLocation = {0} where RL_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 1, geoLocation);

			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("update dbo.RefUNLOCO set RL_GeoLocation = {0} where RL_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 0, ZGeography.Empty);

			geoLocation = ZGeography.CreatePoint(-13.9, 48.99);
			TestConnection.ExecuteNonQuery(string.Format("update dbo.RefUNLOCO set RL_GeoLocation = {0} where RL_PK = '{1}'", geoLocation.ToPointSqlText(), pk));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 1, geoLocation);
		}
	}
}

