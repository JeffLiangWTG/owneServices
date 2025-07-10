using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class TransportDocumentProviderTest : DataProviderTestCase<TransportDocumentProvider>
	{
		public void TestNew()
		{
			AssertNull(GenerateProvider(string.Empty));
			AssertNotNull(GenerateProvider(header.AMA_MasterBill));
		}

		public void TestIdentifier()
		{
			AssertEquals("TransportDocumentMasterLevel DocumentNumber", "ManifestNumberFromMasterBillTest", Provider.Identifier);
		}

		public void TestType()
		{
			AssertEquals("TransportDocumentMasterLevel Type", string.Empty, Provider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = "ManifestNumberFromMasterBillTest";
		}
		AsycudaManifestHeader header;

		TransportDocumentProvider GenerateProvider(string manifestNumber) => TransportDocumentProvider.NewOrNull(manifestNumber, string.Empty);

		protected sealed override TransportDocumentProvider GetProvider()
		{
			return GenerateProvider(header.AMA_MasterBill);
		}
	}
}
