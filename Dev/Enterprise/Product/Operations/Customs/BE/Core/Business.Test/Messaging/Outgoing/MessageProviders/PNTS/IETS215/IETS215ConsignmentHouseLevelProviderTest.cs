using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class IETS215ConsignmentHouseLevelProviderTest : Customs.Business.Testing.DataProviderTestCase<IETS215ConsignmentHouseLevelProvider>
{
	public void TestReferenceNumUCR()
	{
		temporaryStorageBill.ABL_UCRNumber = "TestUCR";
		AssertEquals("TestUCR", provider.ReferenceNumUCR);
	}

	public void TestTotalGrossMass()
	{
		temporaryStorageBill.ABL_GrossWeight = 12134m;
		AssertEquals(12134m, provider.TotalGrossMass);
	}

	public void TestConsignor()
	{
		temporaryStorageBill.ABL_ShipperName = "TestShipper";
		AssertNotNull(provider.Consignor);
	}

	public void TestConsignor_NoShipper()
	{
		AssertNull(provider.Consignor);
	}

	public void TestConsignee()
	{
		temporaryStorageBill.ABL_ConsigneeName = "TestConsignee";
		AssertNotNull(provider.Consignee);
	}

	public void TestConsignor_NoConsignee()
	{
		AssertNull(provider.Consignee);
	}

	public void TestNotify()
	{
		temporaryStorageBill.ABL_NotifyPartyName = "NotifyParty";
		AssertNotNull(provider.Notify);
	}

	public void TestNotify_NoNotifyParty()
	{
		AssertNull(provider.Notify);
	}

	public void TestTransportDocument()
	{
		AssertNotNull(provider.TransportDocument);
	}

	public void TestTransportEquipments()
	{
		var pack1 = temporaryStorageBill.Packs.AddNew();
		var pack2 = temporaryStorageBill.Packs.AddNew();
		var pack3 = temporaryStorageBill.Packs.AddNew();

		var container1 = Factory.New<EU.Business.CusTempStorage.TemporaryStorageContainer>();
		var container2 = Factory.New<EU.Business.CusTempStorage.TemporaryStorageContainer>();

		container1.ACN_ContainerNumber = "CONTAINER1";
		container2.ACN_ContainerNumber = "CONTAINER2";

		pack1.ContainerPK = container1.PK;
		pack2.ContainerPK = container2.PK;
		pack3.ContainerPK = container1.PK;

		AssertEquals(2, provider.TransportEquipments.Count);
		AssertEquals("CONTAINER1", provider.TransportEquipments.First().ContainerIdentificationNumber);
		AssertEquals("CONTAINER2", provider.TransportEquipments.Last().ContainerIdentificationNumber);
	}

	public void TestAdditionalInformations()
	{
		var additionalInfo1 = temporaryStorageBill.AdditionalInfos.AddNew();
		var additionalInfo2 = temporaryStorageBill.AdditionalInfos.AddNew();
		var additionalInfo3 = temporaryStorageBill.AdditionalInfos.AddNew();
		additionalInfo1.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		additionalInfo2.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		additionalInfo3.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;

		AssertEquals(2, provider.AdditionalInformations.Count);
	}

	public void TestAdditionalSupplyChainActors()
	{
		temporaryStorageBill.SupplyChainActors.AddNew();
		temporaryStorageBill.SupplyChainActors.AddNew();

		AssertEquals(2, provider.AdditionalSupplyChainActors.Count);
	}

	public void TestSupportingDocuments()
	{
		temporaryStorageBill.SupportingDocuments.AddNew();
		temporaryStorageBill.SupportingDocuments.AddNew();

		AssertEquals(2, provider.SupportingDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalReference1 = temporaryStorageBill.AdditionalInfos.AddNew();
		var additionalReference2 = temporaryStorageBill.AdditionalInfos.AddNew();
		var additionalInfo = temporaryStorageBill.AdditionalInfos.AddNew();

		additionalReference1.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		additionalReference2.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;

		AssertEquals(2, provider.AdditionalReferences.Count);
	}

	public void TestConsignmentItemHouseLevels()
	{
		AssertArrayEqualsByElements(Array.Empty<IIETS215ConsignmentItemHouseLevel>(), provider.ConsignmentItemHouseLevels.ToArray());
	}

	protected override IETS215ConsignmentHouseLevelProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageBill = temporaryStorageHeader.Bills.AddNew();
		temporaryStorageBill.ABL_BolType = "HWB";
		provider = new IETS215ConsignmentHouseLevelProvider(temporaryStorageBill);
	}

	EU.Business.CusTempStorage.TemporaryStorageBill temporaryStorageBill;
	IETS215ConsignmentHouseLevelProvider provider;
}
