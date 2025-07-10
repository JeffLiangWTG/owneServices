using System;
using CargoWise.Types;
using Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterData.Geography.Testing
{
	class TG_GenShapeGeography_To_GenSpatialData_DeleteIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "SHG";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoValue = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Shape, SHG_Type) values ('{0}', 'ZZ1', {1}, 'UKN')", pk, geoValue.ToPointSqlText()));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GenShapeGeography where SHG_PK = '{0}'", pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 0, ZGeography.Empty);
		}
	}
}

