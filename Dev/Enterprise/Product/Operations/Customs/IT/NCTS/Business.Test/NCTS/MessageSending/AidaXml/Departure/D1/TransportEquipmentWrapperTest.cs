using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class TransportEquipmentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When headerContainer is null", () => new TransportEquipmentWrapper(headerContainer: null));
		AssertExceptionThrown<ArgumentNullException>("When headerContainer.Header is null", () => new TransportEquipmentWrapper(Factory.New<NctsDepartureHeaderContainer>()));
	}

	public void TestContainerID()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ITransportEquipment.ContainerID), wrapper.ContainerID);

		headerContainer.BC_ContainerNum = "TURE123456";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportEquipment.ContainerID), "TURE123456", wrapper.ContainerID);

		headerContainer.BC_Mode = "NCT";
		wrapper = CreateWrapper();
		AssertNullOrEmpty("When Container mode is NCT, ContainerID", wrapper.ContainerID);
	}

	public void TestNumberOfSeals()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportEquipment.NumberOfSeals), 0, wrapper.NumberOfSeals);

		headerContainer.BC_ContainerNum = "TURE123456";
		headerContainer.Seal1 = "Seal1";
		headerContainer.Seal2 = "Seal2";

		wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportEquipment.NumberOfSeals), 2, wrapper.NumberOfSeals);

		headerContainer.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportEquipment.NumberOfSeals), 3, wrapper.NumberOfSeals);
	}

	public void TestLinkedGoodsItemNumbers()
	{
		headerContainer.BC_ContainerNum = "TURE123456";

		var wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportEquipment.LinkedGoodsItemNumbers), 0, wrapper.LinkedGoodsItemNumbers.Count);

		var houseConsignment1 = header.Bills.AddNew();
		var goodsItem1 = houseConsignment1.GoodsItems.AddNew();
		goodsItem1.BY_DeclarationGoodsItemNumber = 1;
		var package1 = goodsItem1.Packages.AddNew();
		package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
		var goodsItem2 = houseConsignment1.GoodsItems.AddNew();
		goodsItem2.BY_DeclarationGoodsItemNumber = 2;
		var package2 = goodsItem2.Packages.AddNew();
		package2.ContainersPivotsForBindingOnly[0].ContainerSelected = false;

		var houseConsignment2 = header.Bills.AddNew();
		var goodsItem3 = houseConsignment2.GoodsItems.AddNew();
		goodsItem3.BY_DeclarationGoodsItemNumber = 3;
		var package3 = goodsItem3.Packages.AddNew();
		package3.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
		var goodsItem4 = houseConsignment2.GoodsItems.AddNew();
		goodsItem4.BY_DeclarationGoodsItemNumber = 4;
		var package4 = goodsItem4.Packages.AddNew();
		package4.ContainersPivotsForBindingOnly[0].ContainerSelected = true;

		var houseConsignment3 = header.Bills.AddNew();
		var goodsItem5 = houseConsignment3.GoodsItems.AddNew();
		goodsItem5.BY_DeclarationGoodsItemNumber = 5;

		wrapper = CreateWrapper();
		AssertContainsExactElementsInAnyOrder(nameof(ITransportEquipment.LinkedGoodsItemNumbers), new[] { 1, 4 }, wrapper.LinkedGoodsItemNumbers);
	}

	public void TestSeals()
	{
		var wrapper = CreateWrapper();
		var seals = wrapper.Seals;
		AssertNotNull(seals);
		AssertSequencesEqual(Array.Empty<string>(), wrapper.Seals);
		AssertSame(seals, wrapper.Seals);

		headerContainer.BC_ContainerNum = "TURE123456";
		headerContainer.Seal1 = "Seal1";
		headerContainer.Seal2 = "Seal2";

		wrapper = CreateWrapper();
		AssertSequencesEqual(new[] { "Seal1", "Seal2" }, wrapper.Seals);

		headerContainer.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";
		wrapper = CreateWrapper();
		AssertSequencesEqual(new[] { "Seal1", "Seal2", "Seal3" }, wrapper.Seals);
	}

	protected override void SetUp()
	{
		header = Factory.NewDepartureNctsHeader();

		headerContainer = header.DepartureHeaderContainers.AddNew();
		headerContainer.BC_Mode = "CNT";
	}

	ITransportEquipment CreateWrapper() => new TransportEquipmentWrapper(headerContainer);

	NctsHeader header;
	NctsDepartureHeaderContainer headerContainer;
}
