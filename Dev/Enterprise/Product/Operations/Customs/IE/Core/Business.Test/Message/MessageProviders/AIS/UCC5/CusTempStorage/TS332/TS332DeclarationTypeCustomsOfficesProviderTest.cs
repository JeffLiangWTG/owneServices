using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS332DeclarationTypeCustomsOfficesProviderTest : DataProviderTestCase<TS332DeclarationTypeCustomsOfficesProvider>
	{
		public void TestFirstEntryCustomsOffice()
		{
			header.CustomsOfficeOfFirstEntry = "Test 123";
			AssertEquals("FirstEntryCustomsOffice", "Test 123", Provider.FirstEntryCustomsOffice);
		}

		protected override TS332DeclarationTypeCustomsOfficesProvider GetProvider() => TS332DeclarationTypeCustomsOfficesProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;
	}
}
