using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AllowSpatialTypesAttributeTest : TestCaseWithDummy
	{
		public void TestAllowSpatialTypesAttribute()
		{
			var attr1 = new AllowSpatialTypesAttribute();
			var attr2 = new AllowSpatialTypesAttribute(ZGeography.SpatialType.Point, ZGeography.SpatialType.Polygon);

			AssertEquals("No specific spatial types. Wrong usage.", 0, attr1.AllowedSpatialTypes.Count);
			AssertEquals("2 spatial types.", 2, attr2.AllowedSpatialTypes.Count);
			AssertEquals("Spatial type 'Point' is allowed.", "Point", attr2.AllowedSpatialTypes[0]);
			AssertEquals("Spatial type 'Polygon' is allowed.", "Polygon", attr2.AllowedSpatialTypes[1]);
		}
	}
}
