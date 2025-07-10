using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IdTypeProviderTest : DataProviderTestCase<IdTypeProvider>
	{
		public void TestIIdType()
		{
			Assert("Should implement IIdType", Provider is IIdType);
		}

		public void TestType()
		{
			AssertEquals("TYPE", Provider.Type);
		}

		public void TestId()
		{
			AssertEquals("ID", Provider.Id);
		}

		protected override IdTypeProvider GetProvider()
		{
			return IdTypeProvider.New("TYPE", "ID");
		}
	}
}
