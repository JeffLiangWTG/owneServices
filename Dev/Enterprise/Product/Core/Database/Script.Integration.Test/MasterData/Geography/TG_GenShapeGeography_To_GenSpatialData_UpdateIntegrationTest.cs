using System;
using CargoWise.Types;
using Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterData.Geography.Testing
{
	class TG_GenShapeGeography_To_GenSpatialData_UpdateIntegrationTest : GenSpatialDataTriggersDbCreateScriptTest
	{
		public override string ParentTableCode => "SHG";

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public override void TestTrigger()
		{
			var geoValue = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0");
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format("insert into dbo.GenShapeGeography (SHG_PK, SHG_Name, SHG_Shape, SHG_Type) values ('{0}', 'ZZ1', {1}, 'UKN')", pk, geoValue.ToPolygonSqlText()));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);

			geoValue = ZGeography.CreatePolygon("0 0,2 0,2 2,0 2,0 0");
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GenShapeGeography set SHG_Shape = {0}, SHG_SystemLastEditUser = 'E', SHG_SystemLastEditTimeUtc = GetDate() WHERE SHG_PK = '{1}'", geoValue.ToPolygonSqlText(), pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);

			geoValue = ZGeography.Empty;
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GenShapeGeography set SHG_Shape = {0}, SHG_SystemLastEditUser = 'E', SHG_SystemLastEditTimeUtc = GetDate() WHERE SHG_PK = '{1}'", geoValue.ToPolygonSqlText(), pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 0, ZGeography.Empty);

			geoValue = ZGeography.CreatePolygon("0 0,3 0,3 3,0 3,0 0");
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GenShapeGeography set SHG_Shape = {0}, SHG_SystemLastEditUser = 'E', SHG_SystemLastEditTimeUtc = GetDate() WHERE SHG_PK = '{1}'", geoValue.ToPolygonSqlText(), pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);

			TestConnection.ExecuteNonQuery(string.Format("delete dbo.GenSpatialData where SPD_ParentID = '{0}'", pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 0, ZGeography.Empty);

			geoValue = ZGeography.CreatePolygon("0 0,4 0,4 4,0 4,0 0");
			TestConnection.ExecuteNonQuery(string.Format("update dbo.GenShapeGeography set SHG_Shape = {0}, SHG_SystemLastEditUser = 'E', SHG_SystemLastEditTimeUtc = GetDate() WHERE SHG_PK = '{1}'", geoValue.ToPolygonSqlText(), pk));
			AssertAfterTriggerEvents(pk, GenShapeGeographySchema.Constants.SHG_Shape, 1, geoValue);
		}
	}
}

