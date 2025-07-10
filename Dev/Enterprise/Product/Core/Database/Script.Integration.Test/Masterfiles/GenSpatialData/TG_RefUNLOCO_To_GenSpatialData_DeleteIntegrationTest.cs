using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_RefUNLOCO_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "RL";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(@"
insert into dbo.RefUNLOCO (RL_PK, RL_Code, RL_GeoLocation)
values ('{0}', 'XXX', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.RefUNLOCO where RL_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, RefUNLOCOSchema.Constants.RL_GeoLocation, 0, ZGeography.Empty);
		}
	}
}

