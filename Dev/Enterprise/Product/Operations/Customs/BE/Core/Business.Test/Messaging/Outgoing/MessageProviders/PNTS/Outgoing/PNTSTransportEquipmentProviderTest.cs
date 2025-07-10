using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSTransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSTransportEquipmentProvider>
{
	public void TestContainerIdentificationNumber()
	{
		container.ACN_ContainerNumber = "CONTAINER";
		AssertEquals("CONTAINER", provider.ContainerIdentificationNumber);
	}

	public void TestContainerPackedStatus()
	{
		container.ACN_EmptyFullIndicator = "E";
		AssertEquals("E", provider.ContainerPackedStatus);
	}

	public void TestNumberOfSeals()
	{
		var pack1 = bill.Packs.AddNew();
		var pack2 = bill.Packs.AddNew();
		var pack3 = bill.Packs.AddNew();

		pack1.ContainerPK = container.PK;
		pack2.ContainerPK = container.PK;

		pack1.APA_PackQty = 12;
		pack2.APA_PackQty = 34;
		pack3.APA_PackQty = 56;

		AssertEquals(46, provider.NumberOfSeals);
	}

	public void TestSeals()
	{
		container.ACN_Seal1 = "SEAL1";
		container.ACN_Seal2 = "SEAL2";
		container.ACN_Seal3 = "SEAL3";

		var additionalSeal1 = container.AdditionalSeals.AddNew();
		var additionalSeal2 = container.AdditionalSeals.AddNew();

		additionalSeal1.BK_SealNumber = "SEAL4";
		additionalSeal2.BK_SealNumber = "SEAL5";

		AssertEquals(5, provider.Seals.Count);
	}

	protected override PNTSTransportEquipmentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		container = Factory.New<TemporaryStorageContainer>();
		bill = Factory.New<TemporaryStorageBill>();
		provider = new PNTSTransportEquipmentProvider(container, bill);
	}

	TemporaryStorageContainer container;
	TemporaryStorageBill bill;
	PNTSTransportEquipmentProvider provider;
}
