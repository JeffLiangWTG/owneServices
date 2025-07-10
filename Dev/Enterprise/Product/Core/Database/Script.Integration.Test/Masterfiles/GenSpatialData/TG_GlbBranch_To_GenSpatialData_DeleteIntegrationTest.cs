using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	class TG_GlbBranchTo_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "GB";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var gcPk = Guid.NewGuid();
			var sqlText = string.Format(@"insert into dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{0}', 'XYZ', 'AU company', 'AU', 'AUD')", gcPk);
			TestConnection.ExecuteNonQuery(sqlText);

			var geoLocation = ZGeography.CreatePoint(23.5, 31.2);
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GlbBranch (GB_PK, GB_GC, GB_CODE, GB_GeoLocation) values ('{0}', '{1}', 'ZZ1', {2})", pk, gcPk, geoLocation.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GlbBranchSchema.Constants.GB_GeoLocation, 1, geoLocation);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GlbBranch where GB_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, GlbBranchSchema.Constants.GB_GeoLocation, 0, ZGeography.Empty);
		}
	}
}

