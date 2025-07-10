using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CTransportEquipmentWrapper))]
sealed class CC044CTransportEquipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CTransportEquipmentWrapper>
{
	public void TestContainerIdentificationNumber() => CombineAssertions(() =>
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var container = header.ArrivalHeaderContainers.AddNew();
		var provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
		container.BC_ContainerNum = "MSCU1234566";
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
		AssertNullOrEmpty(provider.ContainerIdentificationNumber);
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
		AssertEquals("NctsUnloadedStateList.Codes.NEW", "MSCU1234566", provider.ContainerIdentificationNumber);
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
		AssertNullOrEmpty(provider.ContainerIdentificationNumber);
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
		AssertEquals("NctsUnloadedStateList.Codes.DIF", "MSCU1234566", provider.ContainerIdentificationNumber);
	});

	public void TestSeals()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var container = header.ArrivalHeaderContainers.AddNew();
		var seal1 = container.Seals.AddNew();
		seal1.BK_SealNumber = "seal1";
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		var seal2 = container.Seals.AddNew();
		seal2.BK_SealNumber = "seal2";
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		var seal3 = container.Seals.AddNew();
		seal3.BK_SealNumber = "seal3";
		seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		var seal4 = container.Seals.AddNew();
		seal4.BK_SealNumber = "seal4";
		seal4.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });

		AssertContainsExactElementsInExactOrder(new string[] { "seal2" }, Provider.Seal.Where(x => x.Identifier != null).Select(x => x.Identifier).ToArray());
	}

	public void TestGoodsReferences()
	{
		var bill = container.NctsArrival.Bills.AddNew();

		var cusInBondCargoDesc1 = bill.ArrivalGoodsItems.AddNew();
		var package1 = cusInBondCargoDesc1.Packages.AddNew();
		cusInBondCargoDesc1.BY_DeclarationGoodsItemNumber = 1;
		package1.ContainersPivot.AddPivotFor(container);

		var cusInBondCargoDesc2 = bill.ArrivalGoodsItems.AddNew();
		var package2 = cusInBondCargoDesc2.Packages.AddNew();
		cusInBondCargoDesc2.BY_DeclarationGoodsItemNumber = 2;
		cusInBondCargoDesc2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		package2.ContainersPivot.AddPivotFor(container);

		var cusInBondCargoDesc3 = bill.ArrivalGoodsItems.AddNew();
		var package3 = cusInBondCargoDesc3.Packages.AddNew();
		cusInBondCargoDesc3.BY_DeclarationGoodsItemNumber = 3;
		cusInBondCargoDesc3.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		package3.ContainersPivot.AddPivotFor(container);

		var cusInBondCargoDesc4 = bill.ArrivalGoodsItems.AddNew();
		var package4 = cusInBondCargoDesc4.Packages.AddNew();
		cusInBondCargoDesc4.BY_DeclarationGoodsItemNumber = 4;
		cusInBondCargoDesc4.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		package4.ContainersPivot.AddPivotFor(container);

		AssertContainsExactElementsInAnyOrder(new[] { "1", "2" }, Provider.GoodsReference.Select(x => x.DeclarationGoodsItemNumber));
	}

	protected override CC044CTransportEquipmentWrapper GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		container = header.ArrivalHeaderContainers.AddNew();
		provider = CC044CTransportEquipmentWrapper.New(container, new List<string> { "1", "2", "3", "4" });
	}
	NctsArrivalHeaderContainer container;
	CC044CTransportEquipmentWrapper provider;
}
