using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class ArrivalTransportEquipmentProviderTest : DataProviderTestCase<ArrivalTransportEquipmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ArrivalTransportEquipmentProvider(null));
		}

		public void TestSequenceNumber()
		{
			container.BC_SequenceNumber = 4;
			AssertEquals(4, Provider.SequenceNumber);
		}

		public void TestContainerIdentificationNumber()
		{
			container.BC_ContainerNum = "1234";
			AssertEquals("1234", Provider.ContainerIdentificationNumber);
		}

		public void TestNumberOfSeals()
		{
			var seal1 = container.Seals.AddNew();
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			var seal2 = container.Seals.AddNew();
			seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			var seal3 = container.Seals.AddNew();
			seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(2, Provider.NumberOfSeals);

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			var seal1 = container.Seals.AddNew();
			seal1.BK_SequenceNumber = 1;
			seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			seal1.BK_SealNumber = "123";
			var seal2 = container.Seals.AddNew();
			seal2.BK_SequenceNumber = 1;
			seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			seal2.BK_SealNumber = "456";
			var seal3 = container.Seals.AddNew();
			seal3.BK_SequenceNumber = 1;
			seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
			seal3.BK_SealNumber = "789";

			AssertArrayEqualsByElements(new[] { "456", "789" }, Provider.Seals.Select(x => x.Identifier).ToArray());

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(0, Provider.Seals.Count);
		}

		public void TestGoodsReferences()
		{
			var bill = header.Bills.AddNew();
			container = header.ArrivalHeaderContainers.AddNew();
			var container2 = header.ArrivalHeaderContainers.AddNew();

			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodsItem1.Packages.AddNew();
			package1.ContainersPivot.AddPivotFor(container);

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = goodsItem2.Packages.AddNew();
			package2.ContainersPivot.AddPivotFor(container);

			var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem3.BY_DeclarationGoodsItemNumber = 3;
			var package3 = goodsItem3.Packages.AddNew();
			package3.ContainersPivot.AddPivotFor(container);

			var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem4.BY_DeclarationGoodsItemNumber = 4;
			var package4 = goodsItem4.Packages.AddNew();
			package4.ContainersPivot.AddPivotFor(container2);

			AssertContainsExactElementsInAnyOrder(new[] { 2, 3 }, Provider.GoodsReferences.Select(x => x.DeclarationGoodsItemNumber));

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		protected override ArrivalTransportEquipmentProvider GetProvider() => new ArrivalTransportEquipmentProvider(container);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			container = header.ArrivalHeaderContainers.AddNew();
		}
		NctsHeader header;
		NctsArrivalHeaderContainer container;
	}
}
