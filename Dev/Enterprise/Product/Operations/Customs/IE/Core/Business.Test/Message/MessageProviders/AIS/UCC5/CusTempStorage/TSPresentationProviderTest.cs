using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TSPresentationProviderTest : DataProviderTestCase<TSPresentationProvider>
	{
		public void TestPresentationTrader()
		{
			var presentater = Factory.New<OrgHeader>();
			presentater.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			Header.AMA_OA_Presenter = presentater.MainAddress.PK;
			AssertEquals("PresentationTrader=>EOR", "IE123456789", Provider.PresentationTrader);
		}

		public void TestFirstEntryCustomsOffice()
		{
			Header.CustomsOfficeOfFirstEntry = "IEA";
			AssertEquals("FirstEntryCustomsOffice", "IEA", Provider.FirstEntryCustomsOffice);
		}

		public void TestActiveBorderTransportMeansId()
		{
			Header.AMA_TransportMeans = "10";
			Header.AMA_VesselName = "IMO8712345";
			var activeBorderTransportMeansId = Provider.ActiveBorderTransportMeansId;
			AssertType<IdTypeProvider>(activeBorderTransportMeansId);
			AssertEquals("Type", "10", activeBorderTransportMeansId.Type);
			AssertEquals("Id", "IMO8712345", activeBorderTransportMeansId.Id);
		}

		protected override TSPresentationProvider GetProvider()
		{
			return TSPresentationProvider.New(Header);
		}

		TemporaryStorageHeader Header => header ??= Factory.New<TemporaryStorageHeader>();
		TemporaryStorageHeader header;
	}
}
