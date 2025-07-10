using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class PackageLookupsTest : TestCaseWithFactory
	{
		public void TestDangerousGoods()
		{
			Package pack = Factory.New<Package>();
			AssertEquals(typeof(UNDGSubstanceCollection), pack.Lookups.DangerousGoods.GetType());
		}
	}
}
