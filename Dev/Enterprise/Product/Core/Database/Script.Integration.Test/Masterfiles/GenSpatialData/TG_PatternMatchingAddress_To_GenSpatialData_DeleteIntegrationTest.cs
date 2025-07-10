using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_PatternMatchingAddress_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "PMA";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.PatternMatchingAddress (PMA_PK, PMA_ParentId, PMA_ParentTableCode, PMA_GeoLocation) values ('{0}', newid(), 'GS', {1})", pk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.PatternMatchingAddress where PMA_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, PatternMatchingAddressSchema.Constants.PMA_GeoLocation, 0, ZGeography.Empty);
		}
	}
}

