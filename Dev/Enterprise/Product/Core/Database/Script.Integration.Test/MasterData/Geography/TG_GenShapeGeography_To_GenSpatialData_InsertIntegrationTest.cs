using System;
using CargoWise.Types;
using Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterData.Geography.Testing
{
	class TG_GenShapeGeography_To_GenSpatialData_InsertIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "SHG";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			Guid pk;
			ZGeography geoValue;

			pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Type) values ('{0}', 'ZZ1', 'UKN')", pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoValue = new ZGeography("POLYGON EMPTY");
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Shape, SHG_Type) values ('{0}', 'ZZ2', {1}, 'UKN')", pk, geoValue.ToPolygonSqlText()));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 0, ZGeography.Empty);

			pk = Guid.NewGuid();
			geoValue = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Shape, SHG_Type) values ('{0}', 'ZZ3', {1}, 'UKN')", pk, geoValue.ToPolygonSqlText()));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);
		}
	}
}

