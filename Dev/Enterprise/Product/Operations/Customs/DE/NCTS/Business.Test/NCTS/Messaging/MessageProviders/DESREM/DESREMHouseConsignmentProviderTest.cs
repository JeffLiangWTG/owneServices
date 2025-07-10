using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMHouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<DESREMHouseConsignmentProvider>
	{
		public void TestConstructor_NullArgument()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DESREMHouseConsignmentProvider(null));
		}

		public void TestSequenceNumber()
		{
			bill.MovementDetail.B9_SeqNo = "2";
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestGrossMass_UnloadedStateNEW()
		{
			bill.B0_Weight = 1.2m;
			bill.B0_GrossWeightUnloaded = 20m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(1.2m, Provider.GrossMass);
		}

		public void TestGrossMass_UnloadedStateDIF()
		{
			bill.B0_Weight = 1.2m;
			bill.B0_GrossWeightUnloaded = 20m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(20m, Provider.GrossMass);
		}

		public void TestGrossMass_UnloadedStateMIS()
		{
			bill.B0_Weight = 1.2m;
			bill.B0_GrossWeightUnloaded = 20m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.GrossMass);
		}

		public void TestGrossMass_ConversionAndRounding_UnloadedStateNEW()
		{
			bill.B0_Weight = 1123.567m;
			bill.B0_WeightUQ = "G";
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMass_ConversionAndRounding_UnloadedStateDIF()
		{
			bill.B0_GrossWeightUnloaded = 1123.567m;
			bill.B0_WeightUQ = "G";
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMass_Normalize()
		{
			bill.B0_Weight = 1.200m;
			bill.B0_WeightUQ = "KG";
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("1.2", Provider.GrossMass.ToString());
		}

		public void TestGrossMass_NeverZero()
		{
			bill.B0_Weight = 0m;
			bill.B0_WeightUQ = "KG";
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertNull(Provider.GrossMass);
		}

		public void TestDepartureTransportMeans()
		{
			var arrivalTransportInfos = bill.ArrivalTransportInfos;
			var transportMeans = arrivalTransportInfos.AddNew();
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			transportMeans.TPM_SequenceNumber = 1;
			var transportMeans2 = arrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			transportMeans2.TPM_SequenceNumber = 2;
			var transportMeans3 = arrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			transportMeans3.TPM_SequenceNumber = 3;
			var transportMeans4 = arrivalTransportInfos.AddNew();
			transportMeans4.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			transportMeans4.TPM_SequenceNumber = 4;
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.DepartureTransportMeans.Select(x => x.SequenceNumber));
		}

		public void TestItems_BY_UnloadedState()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem1.BY_LineNo = 1;
			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem2.BY_LineNo = 2;
			var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem3.BY_LineNo = 3;
			var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem4.BY_LineNo = 4;
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, Provider.Items.Select(x => x.SequenceNumber));
		}

		public void TestItems_B5_TypeOfDifference()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_LineNo = 1;
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_LineNo = 2;
			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
			var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_LineNo = 3;
			var package3 = goodsItem3.Packages.AddNew();
			package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(new[] { goodsItem1, goodsItem2, goodsItem3 });

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, Provider.Items.Select(x => x.SequenceNumber));
		}

		public void TestItems_B9_UnloadedState()
		{
			CombineAssertions(() =>
			{
				var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
				goodsItem1.BY_LineNo = 2;
				var package1 = goodsItem1.Packages.AddNew();
				package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
				goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				goodsItem2.BY_LineNo = 4;

				bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				AssertEquals("UnloadedState 'MIS'", Array.Empty<IDESREMConsignmentItem>(), Provider.Items);
				foreach (var unloadedState in new NctsUnloadedStateList().GetAllCodes().Except(new[] { NctsUnloadedStateList.Codes.MIS }))
				{
					bill.MovementDetail.B9_UnloadedState = unloadedState;
					AssertContainsExactElementsInAnyOrder($"UnloadedState '{unloadedState}'", new[] { 2, 4 }, GetProvider().Items.Select(x => x.SequenceNumber));
				}
			});
		}

		protected override DESREMHouseConsignmentProvider GetProvider() => new DESREMHouseConsignmentProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			bill = header.Bills.AddNew();
		}
		NctsBill bill;

		new IDESREMHouseConsignment Provider => base.Provider;

		void ModifyDefaultedUnloadedStateNEWToNotSatisfyProviderCondition(IEnumerable<NctsArrivalCargoDesc> goodsItems) => goodsItems.ForEach(x => x.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC);
	}
}
