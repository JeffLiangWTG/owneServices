using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMTransportEquipmentProviderTest : DataProviderTestCase<DESREMTransportEquipmentProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(DESREMTransportEquipmentProvider.NewOrNull(null));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestIdentificationNumber_UnloadedStateDEC()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertNull(Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_UnloadedStateNEW()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("CN002", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_UnloadedStateDIF()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("CN002", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_UnloadedStateMIS()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.IdentificationNumber);
		}

		public void TestNumberOfSeals_BM_StateOfSealsBoolean()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			CombineAssertions(() =>
			{
				AssertNull(Provider.NumberOfSeals);
			});
		}

		public void TestNumberOfSeals()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			AssertEquals(2, Provider.NumberOfSeals);
		}

		public void TestNumberOfSeals_MIS()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			arrivalHeaderContainer.BC_UnloadedState = "MIS";
			AssertNull(Provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			AssertContainsExactElementsInAnyOrder(new[] { "S001", "S004" }, Provider.Seals.Select(x => x.Identifier));
		}

		public void TestSeals_MIS()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			arrivalHeaderContainer.BC_UnloadedState = "MIS";
			AssertEquals(0, Provider.Seals.Count);
		}

		public void TestSeals_BM_StateOfSealsBoolean()
		{
			header.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			AssertEquals(0, Provider.Seals.Count);
		}

		public void TestSeals_NumberOfSeals0()
		{
			arrivalHeaderContainer.Seals.RemoveAndDeleteAll();
			AssertEquals(0, Provider.Seals.Count);
		}

		public void TestGoodsReferences()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			package2.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			AssertEquals(2, Provider.GoodsReferences.Count);
		}

		public void TestGoodsReference_UniqueNumbers()
		{
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var containerPivot1 = package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			var package2 = goodsItem.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			package2.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			AssertEquals(2, Provider.GoodsReferences.Single());
		}

		public void TestGoodsReferences_Empty_LessThan2ContainersWhereBC_UnloadedStateNotMIS()
		{
			CreateContainer_DIF();

			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var containerPivot1 = package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		public void TestGoodsReferences_Empty_BC_UnloadedStateNotNEWorDIF()
		{
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var containerPivot1 = package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);

			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		public void TestGoodsReference_Empty_BY_UnloadedStateIsNEW()
		{
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		public void TestGoodsReference_Empty_BY_UnloadedStateIsMIS()
		{
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		public void TestGoodsReference_Empty_B5_TypeOfDifferenceIsMIS()
		{
			CreateContainer_DIF();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem.BY_DeclarationGoodsItemNumber = 2;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			package1.ContainersPivot.AddPivotFor(arrivalHeaderContainer);
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		protected override DESREMTransportEquipmentProvider GetProvider() => DESREMTransportEquipmentProvider.NewOrNull(arrivalHeaderContainer);

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeaderContainer = header.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer.BC_SequenceNumber = 2;
			arrivalHeaderContainer.BC_ContainerNum = "CN002";
			var seal = arrivalHeaderContainer.Seals.AddNew();
			seal.BK_UnloadingState = "DEC";
			seal.BK_SealNumber = "S001";
			var seal2 = arrivalHeaderContainer.Seals.AddNew();
			seal2.BK_SealNumber = "S002";
			seal2.BK_UnloadingState = "DIF";
			var seal3 = arrivalHeaderContainer.Seals.AddNew();
			seal3.BK_SealNumber = "S003";
			seal3.BK_UnloadingState = "MIS";
			var seal4 = arrivalHeaderContainer.Seals.AddNew();
			seal4.BK_SealNumber = "S004";
			seal4.BK_UnloadingState = "NEW";
		}
		NctsHeader header;
		NctsArrivalHeaderContainer arrivalHeaderContainer;

		NctsArrivalHeaderContainer CreateContainer_DIF()
		{
			var arrivalHeaderContainer = header.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			return arrivalHeaderContainer;
		}
	}
}
