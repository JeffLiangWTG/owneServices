using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class HarbourFeeDepartureCargoDescWrapperTest : TestCaseWithFactory
	{
		public void TestNumberOfTwentyFootContainers()
		{
			var movementHeader = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals("NumberOfTwentyFootLCLContainers should return number of 20 ft containers using LCL mode.", 2, wrapper.NumberOfTwentyFootLCLContainers);
			AssertEquals("NumberOfTwentyFootFCLContainers should return number of 20 ft containers using FCL mode.", 2, wrapper.NumberOfTwentyFootFCLContainers);
		}

		public void TestNumberOfFortyFootContainers()
		{
			var movementHeader = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals("NumberOfFortyFootLCLContainers should return number of 40 ft containers using LCL mode.", 2, wrapper.NumberOfFortyFootLCLContainers);
			AssertEquals("NumberOfFortyFootFCLContainers should return number of 40 ft containers using FCL mode.", 2, wrapper.NumberOfFortyFootFCLContainers);
		}

		public void TestNumberOfFortyFiveFootContainers()
		{
			var movementHeader = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals("NumberOfFortyFiveFootLCLContainers should return number of 45 ft containers using LCL mode, whatever their weight.", 2, wrapper.NumberOfFortyFiveFootLCLContainers);
			AssertEquals("NumberOfFortyFiveFootFCLContainers should return number of 45 ft containers using FCL mode, whatever their weight.", 2, wrapper.NumberOfFortyFiveFootFCLContainers);
		}

		public void TestTotalMassInTonnes()
		{
			var movementHeader = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals("TotalLCLContainersMassInTonnes is the sum of weights of all containers using LCL mode.", (ZDecimal)5, wrapper.TotalLCLContainersMassInTonnes);
			AssertEquals("TotalFCLContainersMassInTonnes is the sum of weights of all containers using FCL mode.", (ZDecimal)13, wrapper.TotalFCLContainersMassInTonnes);
		}

		public void TestIsDangerousGoods()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;
			var goodItem1 = movementHeader.GoodsItems.AddNew();
			var undg = goodItem1.UNDGs.AddNew();

			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			Assert(wrapper.IsDangerousGoods);

			undg.Delete();
			wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			Assert(!wrapper.IsDangerousGoods);
		}

		[TestDate(2022, 10, 9)]
		public void TestDateOfValuation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);

			AssertEquals(new DateTime(2022, 10, 9), wrapper.DateOfValuation);
		}

		public void TestCustomsValue()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals(decimal.Zero, wrapper.CustomsValue);

			var goodItem1 = movementHeader.GoodsItems.AddNew();
			goodItem1.BY_MonetaryValue = 1m;
			var goodItem2 = movementHeader.GoodsItems.AddNew();
			goodItem2.BY_MonetaryValue = 2m;
			wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals(3m, wrapper.CustomsValue);
		}

		public void TestContainerCount()
		{
			var movementHeader = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			AssertEquals(12, wrapper.ContainerCount);
		}

		public void TestContainer_Phase5()
		{
			var wrapper = CreateTestDataForContainersPhase5Test();
			AssertEquals("Container count should be equal to the number of container selected at the package level.", 2, wrapper.ContainerCount);
		}

		public void TestTotalMassInTonnes_Phase5()
		{
			var wrapper = CreateTestDataForTotalMassInTonnesPhase5Test(Core.Constants.ContainerModes.LCL);
			AssertEquals("TotalLCLContainersMassInTonnes should be equal to BM_GrossWeight when any container uses LCL mode, because one transit can’t have multiple container types.", (ZDecimal)3, wrapper.TotalLCLContainersMassInTonnes);
			AssertEquals("TotalFCLContainersMassInTonnes should be 0 when no container uses FCL mode.", (ZDecimal)0, wrapper.TotalFCLContainersMassInTonnes);

			wrapper = CreateTestDataForTotalMassInTonnesPhase5Test(Core.Constants.ContainerModes.FCL);
			AssertEquals("TotalFCLContainersMassInTonnes should be equal to BM_GrossWeight when any container uses FCL mode, because one transit can’t have multiple container types.", (ZDecimal)3, wrapper.TotalFCLContainersMassInTonnes);
			AssertEquals("TotalLCLContainersMassInTonnes should be 0 when no container uses LCL mode.", (ZDecimal)0, wrapper.TotalLCLContainersMassInTonnes);
		}

		public void TestIsDangerousGoods_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;
			var goodsItem1 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var undg = goodsItem1.UNDGs.AddNew();

			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			Assert(wrapper.IsDangerousGoods);

			undg.Delete();
			wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			Assert(!wrapper.IsDangerousGoods);
		}

		HarbourFeeDepartureCargoDescWrapper CreateTestDataForContainersPhase5Test()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();

			var headerContainer2 = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_ContainerNum = "CONTAINER2";
			var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package2 = goodsItem2.Packages.AddNew();

			var movementHeader = nctsHeader.MovementHeader;
			Factory.Save();

			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = true;
			var containerPivot2 = package2.ContainersPivotsForBindingOnly[1];
			containerPivot2.ContainerSelected = true;
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			return wrapper;
		}

		NctsDepartureMovementHeader CreateTestDataForNumberOfContainersTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			var nctsContainer20LCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL1 = Factory.New<RefContainer>();
			refContainerLCL1.RC_ContainerType = "20";
			refContainerLCL1.RC_StorageClass = "20";
			refContainerLCL1.RC_Code = "C20LCL";
			nctsContainer20LCL1.BC_RC = refContainerLCL1.PK;

			var nctsContainer20LCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL2 = Factory.New<RefContainer>();
			refContainerLCL2.RC_ContainerType = "21";
			refContainerLCL2.RC_StorageClass = "20";
			refContainerLCL2.RC_Code = "C21LCL";
			nctsContainer20LCL2.BC_RC = refContainerLCL2.PK;

			var nctsContainer20LCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL21 = Factory.New<RefContainer>();
			refContainerLCL21.RC_ContainerType = "22";
			refContainerLCL21.RC_StorageClass = "20";
			refContainerLCL21.RC_Code = "C22LCL";
			nctsContainer20LCL3.BC_RC = refContainerLCL21.PK;

			var nctsContainer40LCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL3 = Factory.New<RefContainer>();
			refContainerLCL3.RC_ContainerType = "40";
			refContainerLCL3.RC_StorageClass = "40";
			refContainerLCL3.RC_Code = "C40LCL";
			nctsContainer40LCL1.BC_RC = refContainerLCL3.PK;

			var nctsContainer40LCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL4 = Factory.New<RefContainer>();
			refContainerLCL4.RC_ContainerType = "41";
			refContainerLCL4.RC_StorageClass = "40";
			refContainerLCL4.RC_Code = "C41LCL";
			nctsContainer40LCL2.BC_RC = refContainerLCL4.PK;

			var nctsContainer40LCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL41 = Factory.New<RefContainer>();
			refContainerLCL41.RC_ContainerType = "42";
			refContainerLCL41.RC_StorageClass = "40";
			refContainerLCL41.RC_Code = "C42LCL";
			nctsContainer40LCL3.BC_RC = refContainerLCL41.PK;

			var nctsContainer45LCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL5 = Factory.New<RefContainer>();
			refContainerLCL5.RC_ContainerType = "45";
			refContainerLCL5.RC_StorageClass = "45";
			refContainerLCL5.RC_Code = "C45LCL";
			nctsContainer45LCL1.BC_RC = refContainerLCL5.PK;

			var nctsContainer45LCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL6 = Factory.New<RefContainer>();
			refContainerLCL6.RC_ContainerType = "46";
			refContainerLCL6.RC_StorageClass = "45";
			refContainerLCL6.RC_Code = "C46LCL";
			nctsContainer45LCL2.BC_RC = refContainerLCL6.PK;

			var nctsContainer45LCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerLCL61 = Factory.New<RefContainer>();
			refContainerLCL61.RC_ContainerType = "47";
			refContainerLCL61.RC_StorageClass = "45";
			refContainerLCL61.RC_Code = "C47LCL";
			nctsContainer45LCL3.BC_RC = refContainerLCL61.PK;

			var nctsContainer20FCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL1 = Factory.New<RefContainer>();
			refContainerFCL1.RC_ContainerType = "20";
			refContainerFCL1.RC_StorageClass = "20";
			refContainerFCL1.RC_Code = "C20FCL";
			nctsContainer20FCL1.BC_RC = refContainerFCL1.PK;

			var nctsContainer20FCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL2 = Factory.New<RefContainer>();
			refContainerFCL2.RC_ContainerType = "21";
			refContainerFCL2.RC_StorageClass = "20";
			refContainerFCL2.RC_Code = "C21FCL";
			nctsContainer20FCL2.BC_RC = refContainerFCL2.PK;

			var nctsContainer20FCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL21 = Factory.New<RefContainer>();
			refContainerFCL21.RC_ContainerType = "22";
			refContainerFCL21.RC_StorageClass = "20";
			refContainerFCL21.RC_Code = "C22FCL";
			nctsContainer20FCL3.BC_RC = refContainerFCL21.PK;

			var nctsContainer40FCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL3 = Factory.New<RefContainer>();
			refContainerFCL3.RC_ContainerType = "40";
			refContainerFCL3.RC_StorageClass = "40";
			refContainerFCL3.RC_Code = "C40FCL";
			nctsContainer40FCL1.BC_RC = refContainerFCL3.PK;

			var nctsContainer40FCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL4 = Factory.New<RefContainer>();
			refContainerFCL4.RC_ContainerType = "41";
			refContainerFCL4.RC_StorageClass = "40";
			refContainerFCL4.RC_Code = "C41FCL";
			nctsContainer40FCL2.BC_RC = refContainerFCL4.PK;

			var nctsContainer40FCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL41 = Factory.New<RefContainer>();
			refContainerFCL41.RC_ContainerType = "42";
			refContainerFCL41.RC_StorageClass = "40";
			refContainerFCL41.RC_Code = "C42FCL";
			nctsContainer40FCL3.BC_RC = refContainerFCL41.PK;

			var nctsContainer45FCL1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL5 = Factory.New<RefContainer>();
			refContainerFCL5.RC_ContainerType = "45";
			refContainerFCL5.RC_StorageClass = "45";
			refContainerFCL5.RC_Code = "C45FCL";
			nctsContainer45FCL1.BC_RC = refContainerFCL5.PK;

			var nctsContainer45FCL2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL6 = Factory.New<RefContainer>();
			refContainerFCL6.RC_ContainerType = "46";
			refContainerFCL6.RC_StorageClass = "45";
			refContainerFCL6.RC_Code = "C46FCL";
			nctsContainer45FCL2.BC_RC = refContainerFCL6.PK;

			var nctsContainer45FCL3 = nctsHeader.DepartureHeaderContainers.AddNew();
			var refContainerFCL61 = Factory.New<RefContainer>();
			refContainerFCL61.RC_ContainerType = "47";
			refContainerFCL61.RC_StorageClass = "45";
			refContainerFCL61.RC_Code = "C47FCL";
			nctsContainer45FCL3.BC_RC = refContainerFCL61.PK;

			var goodItem1 = movementHeader.GoodsItems.AddNew();
			goodItem1.BY_GrossWeight = 1659.1m;
			goodItem1.BY_GrossWeightUnit = "KG";
			var containersPivot20LCL11 = goodItem1.ContainersPivots.AddNew();
			containersPivot20LCL11.Container = nctsContainer20LCL1;
			containersPivot20LCL11.ContainerSelected = true;
			containersPivot20LCL11.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot20LCL12 = goodItem1.ContainersPivots.AddNew();
			containersPivot20LCL12.Container = nctsContainer20LCL2;
			containersPivot20LCL12.ContainerSelected = true;
			containersPivot20LCL12.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot20LCL13 = goodItem1.ContainersPivots.AddNew();
			containersPivot20LCL13.Container = nctsContainer20LCL3;
			containersPivot20LCL13.ContainerSelected = false;
			containersPivot20LCL13.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL11 = goodItem1.ContainersPivots.AddNew();
			containersPivot40LCL11.Container = nctsContainer40LCL1;
			containersPivot40LCL11.ContainerSelected = true;
			containersPivot40LCL11.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL12 = goodItem1.ContainersPivots.AddNew();
			containersPivot40LCL12.Container = nctsContainer40LCL2;
			containersPivot40LCL12.ContainerSelected = true;
			containersPivot40LCL12.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL13 = goodItem1.ContainersPivots.AddNew();
			containersPivot40LCL13.Container = nctsContainer40LCL3;
			containersPivot40LCL13.ContainerSelected = false;
			containersPivot40LCL13.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL11 = goodItem1.ContainersPivots.AddNew();
			containersPivot45LCL11.Container = nctsContainer45LCL1;
			containersPivot45LCL11.ContainerSelected = true;
			containersPivot45LCL11.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL12 = goodItem1.ContainersPivots.AddNew();
			containersPivot45LCL12.Container = nctsContainer45LCL2;
			containersPivot45LCL12.ContainerSelected = true;
			containersPivot45LCL12.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL13 = goodItem1.ContainersPivots.AddNew();
			containersPivot45LCL13.Container = nctsContainer45LCL3;
			containersPivot45LCL13.ContainerSelected = false;
			containersPivot45LCL13.ContainerMode = Core.Constants.ContainerModes.LCL;

			var goodItem2 = movementHeader.GoodsItems.AddNew();
			goodItem2.BY_GrossWeight = 2219.1m;
			goodItem2.BY_GrossWeightUnit = "KG";
			var containersPivot20LCL21 = goodItem2.ContainersPivots.AddNew();
			containersPivot20LCL21.Container = nctsContainer20LCL1;
			containersPivot20LCL21.ContainerSelected = true;
			containersPivot20LCL21.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot20LCL22 = goodItem2.ContainersPivots.AddNew();
			containersPivot20LCL22.Container = nctsContainer20LCL2;
			containersPivot20LCL22.ContainerSelected = true;
			containersPivot20LCL22.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot20LCL23 = goodItem2.ContainersPivots.AddNew();
			containersPivot20LCL23.Container = nctsContainer20LCL3;
			containersPivot20LCL23.ContainerSelected = false;
			containersPivot20LCL23.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL21 = goodItem2.ContainersPivots.AddNew();
			containersPivot40LCL21.Container = nctsContainer40LCL1;
			containersPivot40LCL21.ContainerSelected = true;
			containersPivot40LCL21.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL22 = goodItem2.ContainersPivots.AddNew();
			containersPivot40LCL22.Container = nctsContainer40LCL2;
			containersPivot40LCL22.ContainerSelected = true;
			containersPivot40LCL22.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot40LCL23 = goodItem2.ContainersPivots.AddNew();
			containersPivot40LCL23.Container = nctsContainer40LCL3;
			containersPivot40LCL23.ContainerSelected = false;
			containersPivot40LCL23.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL21 = goodItem2.ContainersPivots.AddNew();
			containersPivot45LCL21.Container = nctsContainer45LCL1;
			containersPivot45LCL21.ContainerSelected = true;
			containersPivot45LCL21.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL22 = goodItem2.ContainersPivots.AddNew();
			containersPivot45LCL22.Container = nctsContainer45LCL2;
			containersPivot45LCL22.ContainerSelected = true;
			containersPivot45LCL22.ContainerMode = Core.Constants.ContainerModes.LCL;

			var containersPivot45LCL23 = goodItem2.ContainersPivots.AddNew();
			containersPivot45LCL23.Container = nctsContainer45LCL3;
			containersPivot45LCL23.ContainerSelected = false;
			containersPivot45LCL23.ContainerMode = Core.Constants.ContainerModes.LCL;

			var goodItem4 = movementHeader.GoodsItems.AddNew();
			goodItem4.BY_GrossWeight = 5999.1m;
			goodItem4.BY_GrossWeightUnit = "KG";
			var containersPivot20FCL41 = goodItem4.ContainersPivots.AddNew();
			containersPivot20FCL41.Container = nctsContainer20FCL1;
			containersPivot20FCL41.ContainerSelected = true;
			containersPivot20FCL41.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot20FCL42 = goodItem4.ContainersPivots.AddNew();
			containersPivot20FCL42.Container = nctsContainer20FCL2;
			containersPivot20FCL42.ContainerSelected = true;
			containersPivot20FCL42.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot20FCL43 = goodItem4.ContainersPivots.AddNew();
			containersPivot20FCL43.Container = nctsContainer20FCL3;
			containersPivot20FCL43.ContainerSelected = false;
			containersPivot20FCL43.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL41 = goodItem4.ContainersPivots.AddNew();
			containersPivot40FCL41.Container = nctsContainer40FCL1;
			containersPivot40FCL41.ContainerSelected = true;
			containersPivot40FCL41.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL42 = goodItem4.ContainersPivots.AddNew();
			containersPivot40FCL42.Container = nctsContainer40FCL2;
			containersPivot40FCL42.ContainerSelected = true;
			containersPivot40FCL42.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL43 = goodItem4.ContainersPivots.AddNew();
			containersPivot40FCL43.Container = nctsContainer40FCL3;
			containersPivot40FCL43.ContainerSelected = false;
			containersPivot40FCL43.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL41 = goodItem4.ContainersPivots.AddNew();
			containersPivot45FCL41.Container = nctsContainer45FCL1;
			containersPivot45FCL41.ContainerSelected = true;
			containersPivot45FCL41.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL42 = goodItem4.ContainersPivots.AddNew();
			containersPivot45FCL42.Container = nctsContainer45FCL2;
			containersPivot45FCL42.ContainerSelected = true;
			containersPivot45FCL42.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL43 = goodItem4.ContainersPivots.AddNew();
			containersPivot45FCL43.Container = nctsContainer45FCL3;
			containersPivot45FCL43.ContainerSelected = false;
			containersPivot45FCL43.ContainerMode = Core.Constants.ContainerModes.FCL;

			var goodItem5 = movementHeader.GoodsItems.AddNew();
			goodItem5.BY_GrossWeight = 6999.1m;
			goodItem5.BY_GrossWeightUnit = "KG";
			var containersPivot20FCL51 = goodItem5.ContainersPivots.AddNew();
			containersPivot20FCL51.Container = nctsContainer20FCL1;
			containersPivot20FCL51.ContainerSelected = true;
			containersPivot20FCL51.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot20FCL52 = goodItem5.ContainersPivots.AddNew();
			containersPivot20FCL52.Container = nctsContainer20FCL2;
			containersPivot20FCL52.ContainerSelected = true;
			containersPivot20FCL52.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot20FCL53 = goodItem5.ContainersPivots.AddNew();
			containersPivot20FCL53.Container = nctsContainer20FCL3;
			containersPivot20FCL53.ContainerSelected = false;
			containersPivot20FCL53.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL51 = goodItem5.ContainersPivots.AddNew();
			containersPivot40FCL51.Container = nctsContainer40FCL1;
			containersPivot40FCL51.ContainerSelected = true;
			containersPivot40FCL51.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL52 = goodItem5.ContainersPivots.AddNew();
			containersPivot40FCL52.Container = nctsContainer40FCL2;
			containersPivot40FCL52.ContainerSelected = true;
			containersPivot40FCL52.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot40FCL53 = goodItem5.ContainersPivots.AddNew();
			containersPivot40FCL53.Container = nctsContainer40FCL3;
			containersPivot40FCL53.ContainerSelected = false;
			containersPivot40FCL53.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL51 = goodItem5.ContainersPivots.AddNew();
			containersPivot45FCL51.Container = nctsContainer45FCL1;
			containersPivot45FCL51.ContainerSelected = true;
			containersPivot45FCL51.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL52 = goodItem5.ContainersPivots.AddNew();
			containersPivot45FCL52.Container = nctsContainer45FCL2;
			containersPivot45FCL52.ContainerSelected = true;
			containersPivot45FCL52.ContainerMode = Core.Constants.ContainerModes.FCL;

			var containersPivot45FCL53 = goodItem5.ContainersPivots.AddNew();
			containersPivot45FCL53.Container = nctsContainer45FCL3;
			containersPivot45FCL53.ContainerSelected = false;
			containersPivot45FCL53.ContainerMode = Core.Constants.ContainerModes.FCL;

			Factory.Save();
			return movementHeader;
		}

		HarbourFeeDepartureCargoDescWrapper CreateTestDataForTotalMassInTonnesPhase5Test(ZString containerMode)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_Mode = containerMode;
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_GrossWeight = 3000m;
			Factory.Save();

			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = true;
			var wrapper = new HarbourFeeDepartureCargoDescWrapper(movementHeader);
			return wrapper;
		}
	}
}
