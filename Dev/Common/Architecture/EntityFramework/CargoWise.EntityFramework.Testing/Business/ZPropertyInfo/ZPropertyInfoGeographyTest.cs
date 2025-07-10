using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoGeographyTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Geography = ZGeography.Empty;

			AssertEquals(true, Dummy.Z0_GeographyInfo.SetValueFromString("-122 47"));
			AssertEquals(new ZGeography("-122 47"), Dummy.Z0_Geography);

			AssertEquals(true, Dummy.Z0_GeographyInfo.SetValueFromString("-122.1,48"));
			AssertEquals(new ZGeography("-122.1 48"), Dummy.Z0_Geography);

			AssertEquals(true, Dummy.Z0_GeographyInfo.SetValueFromString("POINT (-122.3 49.1)"));
			AssertEquals(new ZGeography("-122.3 49.1"), Dummy.Z0_Geography);
		}

		public void TestAllowedSpatialTypes()
		{
			var propertyInfo = Dummy.Z0_GeographyInfo as ZPropertyInfoGeography;
			AssertNotNull(propertyInfo);

			AssertNotNull("Z0_Geography is maked with AllowGeographyTypesAttribute", propertyInfo.AllowedSpatialTypes);
			AssertEquals("Z0_Geography has 2 allowed geography types.", 2, propertyInfo.AllowedSpatialTypes.Count);
		}
	}
}
