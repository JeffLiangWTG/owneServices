using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS332DeclarationTypeProviderTest : DataProviderTestCase<TS332DeclarationTypeProvider>
	{
		public void TestLRN()
		{
			header.LRN = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestMRN()
		{
			header.MRN = "MRN2343234242";
			AssertEquals("MRN", "MRN2343234242", Provider.MRN);
		}

		public void TestCustomsOffices()
		{
			AssertType<TS332DeclarationTypeCustomsOfficesProvider>(Provider.CustomsOffices);
		}

		public void TestParties()
		{
			AssertType<TS332DeclarationTypePartiesProvider>(Provider.Parties);
		}

		protected override TS332DeclarationTypeProvider GetProvider() => TS332DeclarationTypeProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;
	}
}
