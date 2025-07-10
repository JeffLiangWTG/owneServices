using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class IETS215ConsignmentHeaderMasterLevelProviderTest : Customs.Business.Testing.DataProviderTestCase<IETS215ConsignmentHeaderMasterLevelProvider>
{
	public void TestConsignmentMasterLevel()
	{
		AssertNotNull(provider.ConsignmentMasterLevel);
	}

	public void TestConsignmentHouseLevels()
	{
		var bill1 = temporaryStorageHeader.Bills.AddNew();
		var bill2 = temporaryStorageHeader.Bills.AddNew();
		var bill3 = temporaryStorageHeader.Bills.AddNew();

		bill1.ABL_BolType = "HWB";
		bill2.ABL_BolType = "HWB";
		bill3.ABL_BolType = "XXX";

		AssertEquals("Only House Bills", 2, provider.ConsignmentHouseLevels.Count);
	}

	protected override IETS215ConsignmentHeaderMasterLevelProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		provider = new IETS215ConsignmentHeaderMasterLevelProvider(temporaryStorageHeader);
	}

	TemporaryStorageHeader temporaryStorageHeader;
	IETS215ConsignmentHeaderMasterLevelProvider provider;
}
