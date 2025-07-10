using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISPackageLookupsTest : TestCaseWithFactory
	{
		public void TestAQISPackageTypeList()
		{
			var package = new AQISPackage(Factory);
			AssertNotNull("Package Type List", package.Lookups.AQISPackageTypeList);
			AssertEquals("Pacakge Type List count", true, package.Lookups.AQISPackageTypeList.Count > 0);
			AssertNull("CTN not in package", package.Lookups.AQISPackageTypeList.GetDescriptionFromCode("CTN"));
			AssertEquals("BB in list", "BASE BOX", package.Lookups.AQISPackageTypeList.GetDescriptionFromCode("BB"));
		}
	}
}
