using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS332GoodsShipmentTypeProviderTest : DataProviderTestCase<TS332GoodsShipmentTypeProvider>
	{
		public void TestGoodsLocation()
		{
			AssertType<GoodsLocationProvider>("GoodsLocation", Provider.GoodsLocation);
		}

		public void TestTransportInformation()
		{
			AssertType<MeansIdentityAtBorderMandatoryProvider>("TransportInformation", Provider.TransportInformation);
		}

		public void TestDocumentsAuthorisations()
		{
			Assert("Should be IReadOnlyCollection<SimplifiedDeclarationDocumentWritingOffProvider>",
				Provider.DocumentsAuthorisations is IReadOnlyCollection<SimplifiedDeclarationDocumentWritingOffProvider>);
			AssertEquals("Documents Count", 1, Provider.DocumentsAuthorisations.Count);
		}

		protected override TS332GoodsShipmentTypeProvider GetProvider() => TS332GoodsShipmentTypeProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.PreviousDocuments.AddNew();
		}
		TemporaryStorageHeader header;
	}
}
