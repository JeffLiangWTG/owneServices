using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class CusPersonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGlobalPersonsList()
		{
			var person = Factory.New<CusPerson>();
			AssertType<GlbPersonCollection>(new CusPersonLookups(person).GlobalPersonsList);
		}
	}
}
