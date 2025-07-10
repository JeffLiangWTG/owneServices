
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESREMHeaderProvider))]
	sealed class DESREMHeaderProviderTest : NCTSHeaderProviderAbstractTest<DESREMHeaderProvider>
	{
		public void TestMRN()
		{
			const string mrn = "DE231234567898765";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = mrn;

			AssertEquals(mrn, Provider.MRN);
		}

		public void TestOtherThingsToReport()
		{
			const string otherThingsToReport = "ABC";
			nctsHeader.ArrivalMovementHeader.OtherThingsToReport = otherThingsToReport;

			AssertEquals(otherThingsToReport, Provider.OtherThingsToReport);
		}

		public void TestCustomsOfficeOfDestinationActualID()
		{
			const string data = "ABC";
			var officeCode = nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			officeCode.CY_Data = data;

			AssertEquals(data, Provider.CustomsOfficeOfDestinationActualID);
		}

		public void TestUnloadingRemarkConform()
		{
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
			AssertEquals("0", Provider.UnloadingRemarkConform);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			AssertEquals("1", Provider.UnloadingRemarkConform);
		}

		public void TestUnloadingRemarkStateOfSeals_NoArrivalHeaderContainers()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			CombineAssertions(() =>
			{
				AssertEquals("Precondition, no containers: containers count", 0, nctsHeader.ArrivalHeaderContainers.Count);
				AssertNullOrEmpty("Result null", Provider.UnloadingRemarkStateOfSeals);
			});
		}

		public void TestUnloadingRemarkStateOfSeals_BC_Seal1()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			CombineAssertions(() =>
			{
				var arrivalContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
				arrivalContainer.BC_Seal1 = "123";
				AssertEquals("BC_Seal1 not empty, BM_StateOfSealsBoolean true", "1", Provider.UnloadingRemarkStateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				AssertEquals("BC_Seal1 not empty, BM_StateOfSealsBoolean false", "0", Provider.UnloadingRemarkStateOfSeals);
			});
		}

		public void TestUnloadingRemarkStateOfSeals_BC_Seal2()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			CombineAssertions(() =>
			{
				var arrivalContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
				arrivalContainer.BC_Seal2 = "123";

				AssertEquals("BC_Seal2 not empty, BM_StateOfSealsBoolean true", "1", Provider.UnloadingRemarkStateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				AssertEquals("BC_Seal2 not empty, BM_StateOfSealsBoolean false", "0", Provider.UnloadingRemarkStateOfSeals);
			});
		}

		public void TestUnloadingRemarkStateOfSeals_AdditionalSeals()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			CombineAssertions(() =>
			{
				var arrivalContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
				AssertEquals("No AdditionalSeals", null, Provider.UnloadingRemarkStateOfSeals);

				arrivalContainer.Seals.AddNew();
				AssertEquals("Has AdditionalSeals, BM_StateOfSealsBoolean true", "1", Provider.UnloadingRemarkStateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				AssertEquals("Has AdditionalSeals, BM_StateOfSealsBoolean false", "0", Provider.UnloadingRemarkStateOfSeals);
			});
		}

		public void TestUnloadingRemark()
		{
			const string remarks = "Remark1";
			nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks = remarks;

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			AssertEquals(remarks, Provider.UnloadingRemark);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			AssertEquals(remarks, Provider.UnloadingRemark);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			AssertNullOrEmpty(remarks, Provider.UnloadingRemark);
		}

		public void TestGrossMass()
		{
			CombineAssertions(() =>
			{
				AssertEquals("no value assigned", 0m, Provider.GrossMass);

				nctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded = 1m;
				AssertEquals("value assigned", 1m, Provider.GrossMass);
			});
		}

		public void TestGrossMassRounding()
		{
			nctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded = 1.1235;
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMassNormalize()
		{
			nctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded = 1.100;
			AssertEquals("1.1", Provider.GrossMass.ToString());
		}

		public void TestTransportEquipments()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			var arrivalHeaderContainer1 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer1.BC_ContainerNum = "CN1";
			arrivalHeaderContainer1.BC_UnloadedState = "NEW";
			var arrivalHeaderContainer2 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer2.BC_ContainerNum = "CN2";
			arrivalHeaderContainer1.BC_UnloadedState = "DIF";
			var arrivalHeaderContainer3 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer3.BC_ContainerNum = "CN3";
			arrivalHeaderContainer3.BC_UnloadedState = "MIS";
			var arrivalHeaderContainer4 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer4.BC_ContainerNum = "CN4";
			arrivalHeaderContainer4.BC_UnloadedState = "DEC";
			arrivalHeaderContainer4.Seals.AddNew().BK_UnloadingState = "DEC";

			var arrivalHeaderContainer5 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer5.BC_ContainerNum = "CN5";
			arrivalHeaderContainer5.BC_UnloadedState = "DEC";
			arrivalHeaderContainer5.Seals.AddNew().BK_UnloadingState = "MIS";

			var arrivalHeaderContainer6 = nctsHeader.ArrivalHeaderContainers.AddNew();
			arrivalHeaderContainer6.BC_ContainerNum = "CN6";
			arrivalHeaderContainer6.BC_UnloadedState = "DEC";
			arrivalHeaderContainer6.Seals.AddNew().BK_UnloadingState = "NEW";

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3, 5, 6 }, Provider.TransportEquipments.Select(x => x.SequenceNumber));
			AssertContainsExactElementsInAnyOrder(new[] { "CN1", "CN2", null, null, null }, Provider.TransportEquipments.Select(x => x.IdentificationNumber));
		}

		public void TestDepartureTransportMeans()
		{
			var arrivalTransportInfos = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos;
			var transportMeans = arrivalTransportInfos.AddNew();
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			transportMeans.SequenceNumber = 1;
			var transportMeans2 = arrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			transportMeans2.SequenceNumber = 2;
			var transportMeans3 = arrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			transportMeans3.SequenceNumber = 3;
			var transportMeans4 = arrivalTransportInfos.AddNew();
			transportMeans4.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			transportMeans4.SequenceNumber = 4;
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.DepartureTransportMeans.Select(x => x.SequenceNumber));
		}

		public void TestHouseConsignments_B9_UnloadedState()
		{
			var bill = nctsHeader.Bills.AddNew();
			var temp = bill.MovementDetail.B9_UnloadedState;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			bill.MovementDetail.B9_SeqNo = "1";
			var bill2 = nctsHeader.Bills.AddNew();
			bill2.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			bill2.MovementDetail.B9_SeqNo = "2";
			var bill3 = nctsHeader.Bills.AddNew();
			bill3.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			bill3.MovementDetail.B9_SeqNo = "3";
			var bill4 = nctsHeader.Bills.AddNew();
			bill4.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			bill4.MovementDetail.B9_SeqNo = "4";
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.HouseConsignments.Select(x => x.SequenceNumber));
		}

		public void TestHouseConsignments_TPM_TransportState()
		{
			var bill = nctsHeader.Bills.AddNew();
			var transportMeans = bill.ArrivalTransportInfos.AddNew();
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			bill.MovementDetail.B9_SeqNo = "1";
			var bill2 = nctsHeader.Bills.AddNew();
			var transportMeans2 = bill2.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			bill2.MovementDetail.B9_SeqNo = "2";
			var bill3 = nctsHeader.Bills.AddNew();
			var transportMeans3 = bill3.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			bill3.MovementDetail.B9_SeqNo = "3";
			var bill4 = nctsHeader.Bills.AddNew();
			var transportMeans4 = bill4.ArrivalTransportInfos.AddNew();
			transportMeans4.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			bill4.MovementDetail.B9_SeqNo = "4";
			ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(new[] { bill.MovementDetail, bill2.MovementDetail, bill3.MovementDetail, bill4.MovementDetail });

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.HouseConsignments.Select(x => x.SequenceNumber));
		}

		public void TestHouseConsignments_BY_UnloadedState()
		{
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			var temp = goodsItem.BY_UnloadedState;
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			bill.MovementDetail.B9_SeqNo = "1";
			var bill2 = nctsHeader.Bills.AddNew();
			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			bill2.MovementDetail.B9_SeqNo = "2";
			var bill3 = nctsHeader.Bills.AddNew();
			var goodsItem3 = bill3.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			bill3.MovementDetail.B9_SeqNo = "3";
			var bill4 = nctsHeader.Bills.AddNew();
			var goodsItem4 = bill4.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			bill4.MovementDetail.B9_SeqNo = "4";
			ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(new[] { bill.MovementDetail, bill2.MovementDetail, bill3.MovementDetail, bill4.MovementDetail });

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.HouseConsignments.Select(x => x.SequenceNumber));
		}

		public void TestHouseConsignments_B5_TypeOfDifference()
		{
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			bill.MovementDetail.B9_SeqNo = "1";
			var bill2 = nctsHeader.Bills.AddNew();
			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			bill2.MovementDetail.B9_SeqNo = "2";
			var bill3 = nctsHeader.Bills.AddNew();
			var goodsItem3 = bill3.ArrivalGoodsItems.AddNew();
			var package3 = goodsItem3.Packages.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			bill3.MovementDetail.B9_SeqNo = "3";
			ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(new[] { bill.MovementDetail, bill2.MovementDetail, bill3.MovementDetail }, new[] { goodsItem, goodsItem2, goodsItem3 });

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, Provider.HouseConsignments.Select(x => x.SequenceNumber));
		}

		protected override DESREMHeaderProvider GetHeaderProvider() => new DESREMHeaderProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		}
		NctsHeader nctsHeader;

		new IDESREMHeader Provider => base.Provider;

		void ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(IEnumerable<CusInBondMoveDetail> movementDetails, IEnumerable<NctsArrivalCargoDesc> goodsItems = null)
		{
			movementDetails.ForEach(x => x.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC);
			goodsItems?.ForEach(x => x.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC);
		}
	}
}
