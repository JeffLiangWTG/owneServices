using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Customs.Tests
{
	sealed class FreightWrapperCustomsInfoAdapterTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<Enterprise.Customs.Business.BaseJobDeclaration>();
			var freightWrapper = FreightWrapper.New(declaration, Factory)[0];
			ICustomsInfo adapter = new FreightWrapperCustomsInfoAdapter(freightWrapper);

			AssertEquals(declaration, adapter.Declaration);
		}
	}
}
