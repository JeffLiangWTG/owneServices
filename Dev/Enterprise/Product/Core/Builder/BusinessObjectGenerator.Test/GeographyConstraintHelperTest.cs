using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class GeographyConstraintHelperTest : TestCase
	{
		public void TestGetAllowedSpatialTypes()
		{
			var helper = new GeographyConstraintHelper();

			AssertContainsExactElementsInAnyOrder("",
				new List<string>
				{
					GeographyConstraintHelper.SpatialType.Point,
				},
				helper.GetAllowedSpatialTypes("TestGeographyConstraints", "XY_Geography1"));

			AssertContainsExactElementsInAnyOrder("",
				new List<string>
				{
					GeographyConstraintHelper.SpatialType.Point,
					GeographyConstraintHelper.SpatialType.Polygon,
				},
				helper.GetAllowedSpatialTypes("TestGeographyConstraints", "XY_Geography2"));

			AssertContainsExactElementsInAnyOrder("",
				GeographyConstraintHelper.SpatialType.All,
				helper.GetAllowedSpatialTypes("TestGeographyConstraints", "XY_Geography3"));
		}

		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery(@"
CREATE TABLE TestGeographyConstraints(
	XY_Geography1 geography NOT NULL,
	XY_Geography2 geography NOT NULL,
	XY_Geography3 geography NOT NULL,
)

ALTER TABLE TestGeographyConstraints ADD  CONSTRAINT DF_TestGeographyConstraints_XY_Geography1 DEFAULT (CONVERT(geography,'POINT EMPTY')) FOR XY_Geography1
ALTER TABLE TestGeographyConstraints ADD  CONSTRAINT DF_TestGeographyConstraints_XY_Geography2 DEFAULT (CONVERT(geography,'POINT EMPTY')) FOR XY_Geography2
ALTER TABLE TestGeographyConstraints ADD  CONSTRAINT DF_TestGeographyConstraints_XY_Geography3 DEFAULT (CONVERT(geography,'POINT EMPTY')) FOR XY_Geography3

ALTER TABLE TestGeographyConstraints
ADD CONSTRAINT Constraint_XY_Geography1 CHECK (XY_Geography1.STIsValid() = 1 and XY_Geography1.STGeometryType() = 'Point');
ALTER TABLE TestGeographyConstraints
ADD CONSTRAINT Constraint_XY_Geography2 CHECK (XY_Geography2.STIsValid() = 1 and (XY_Geography2.STIsEmpty() = 1 or XY_Geography2.STGeometryType() in ('Point', 'Polygon')));
ALTER TABLE TestGeographyConstraints
ADD CONSTRAINT Constraint_XY_Geography3 CHECK (XY_Geography3.STIsValid() = 1);
");
		}

		protected override void TearDown()
		{
			Db.Connection.ExecuteNonQuery(@"DROP TABLE TestGeographyConstraints");
		}
	}
}
