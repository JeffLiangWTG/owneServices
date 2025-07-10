using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaTaxTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaTax>(), new AsycudaTaxTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaTax>(), new BusinessObjectFactory().New<AsycudaTax>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaTax), new AsycudaTaxTypeDecider().GetTypeForBinding());
		}
	}
}
