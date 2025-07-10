using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSConsignmentHeaderMasterLevelProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSConsignmentHeaderMasterLevelProvider>
{
	public void TestPlaceOfUnloadingUNLocode()
	{
		temporaryStorageBill.ABL_RL_NKPortOfDischarge = "ABC";
		AssertEquals(temporaryStorageBill.ABL_RL_NKPortOfDischarge, provider.PlaceOfUnloadingUNLocode);
	}

	public void TestConsignmentMasterLevel()
	{
		AssertNull(provider.ConsignmentMasterLevel);
	}

	public void TestConsignmentHouseLevels()
	{
		AssertArrayEqualsByElements(Array.Empty<IConsignmentHouseLevel>(), provider.ConsignmentHouseLevels.ToArray());
	}

	public void TestLocationOfGoods()
	{
		AssertType<PNTSLocationOfGoodsProvider>(provider.LocationOfGoods);
	}

	public void TestArrivalTransportMeans()
	{
		AssertType<PNTSArrivalTransportMeansProvider>(provider.ArrivalTransportMeans);
	}

	public void TestWarehouse()
	{
		CusAuthorizationUsage.New(temporaryStorageHeader);
		AssertType<PNTSWarehouseProvider>(provider.Warehouse);
	}

	public void TestCarrier()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		temporaryStorageHeader.AMA_OA_Carrier = orgAddress.PK;
		AssertType<PNTSCarrierProvider>(provider.Carrier);
	}

	protected override PNTSConsignmentHeaderMasterLevelProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageBill = temporaryStorageHeader.MasterBill;
		provider = new PNTSConsignmentHeaderMasterLevelProvider(temporaryStorageHeader);
	}

	TemporaryStorageHeader temporaryStorageHeader;
	EU.Business.CusTempStorage.TemporaryStorageBill temporaryStorageBill;
	PNTSConsignmentHeaderMasterLevelProvider provider;
}
