using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class IdTypeProviderTest : DataProviderTestCase<IdTypeProvider>
	{
		public void TestType()
		{
			AssertEquals("Type", "type", Provider.Type);
		}

		public void TestId()
		{
			AssertEquals("Id", "id", Provider.Id);
		}

		protected override IdTypeProvider GetProvider()
		{
			return IdTypeProvider.New("type", "id");
		}
	}
}
