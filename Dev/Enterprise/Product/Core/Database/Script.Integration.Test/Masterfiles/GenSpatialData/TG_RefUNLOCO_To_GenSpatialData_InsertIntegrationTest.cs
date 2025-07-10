using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_RefUNLOCO_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "RL";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			Guid pk;
			ZGeography geoLocation;

			pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.RefUNLOCO (RL_PK, RL_Code) values('{0}', 'XX1')", pk));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation) values('{0}', 'XX2', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoLocation = ZGeography.CreatePoint(23.5, 31.3);
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation) values('{0}', 'XX3', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 1, geoLocation);
		}
	}
}

