using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class IETS215ConsignmentMasterLevelProviderTest : Customs.Business.Testing.DataProviderTestCase<IETS215ConsignmentMasterLevelProvider>
{
	public void TestTransportDocument()
	{
		AssertNotNull(provider.TransportDocument);
	}

	protected override IETS215ConsignmentMasterLevelProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageBill = temporaryStorageHeader.Bills.AddNew();
		temporaryStorageBill.ABL_BolType = "BOL";
		provider = new IETS215ConsignmentMasterLevelProvider(temporaryStorageBill);
	}

	EU.Business.CusTempStorage.TemporaryStorageBill temporaryStorageBill;
	IETS215ConsignmentMasterLevelProvider provider;
}
