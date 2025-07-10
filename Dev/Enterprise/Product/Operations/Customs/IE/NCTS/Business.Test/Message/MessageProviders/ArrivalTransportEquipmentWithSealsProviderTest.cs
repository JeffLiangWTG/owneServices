using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class ArrivalTransportEquipmentWithSealsProviderTest : Customs.Business.Testing.DataProviderTestCase<ArrivalTransportEquipmentWithSealsProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ArrivalTransportEquipmentWithSealsProvider(null));
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
			var seal4 = container.Seals.AddNew();
			seal4.BK_SealNumber = "ADD4";
			seal4.BK_UnloadingState = "DAM";

			AssertArrayEqualsByElements(new[] { "456", "789" }, Provider.Seals.ToArray());
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

			AssertContainsExactElementsInAnyOrder(new[] { "2", "3" }, Provider.GoodsReferences);
		}

		public void TestContainerIsFull()
		{
			AssertEquals("Container is full", false, Provider.ContainerIsFull);
		}

		protected override ArrivalTransportEquipmentWithSealsProvider GetProvider() => new ArrivalTransportEquipmentWithSealsProvider(container);

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
