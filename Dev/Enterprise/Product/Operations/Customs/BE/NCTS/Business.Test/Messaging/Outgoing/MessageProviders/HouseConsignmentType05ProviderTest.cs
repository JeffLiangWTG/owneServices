using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class HouseConsignmentType05ProviderTest : Customs.Business.Testing.DataProviderTestCase<HouseConsignmentType05Provider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(3, Provider.SequenceNumber);
		}

		public void TestGrossMass_UnloadedStateNEW()
		{
			bill.B0_Weight = 1.2m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.GrossMass);
		}

		public void TestGrossMass_UnloadedStateMIS()
		{
			bill.B0_Weight = 1.2m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals(1.2m, Provider.GrossMass);
		}

		public void TestGrossMass_UnloadedStateDIF()
		{
			bill.B0_Weight = 1.2m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 10;
			goodsItem1.BY_GrossWeightUnit = "KG";

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem2.BY_GrossWeight = 2;
			goodsItem2.BY_GrossWeightUnit = "KG";
			var unloadedGoodsItem = goodsItem2.UnloadedGoodsItem;
			unloadedGoodsItem.BY_GrossWeight = 10;
			unloadedGoodsItem.BY_GrossWeightUnit = "KG";

			AssertEquals(20m, Provider.GrossMass);
		}

		public void TestGrossMass_UnloadedState_MIS()
		{
			bill.B0_Weight = 1.2m;
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals(null, Provider.GrossMass);
		}

		public void TestDepartureTransportMeans()
		{
			var transportMeans1 = bill.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_TransportState = "NEW";
			var transportMeans2 = bill.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = "MIS";
			var transportMeans3 = bill.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = "DEC";

			AssertEquals(2, Provider.DepartureTransportMeans.Count);
		}

		public void TestDepartureTransportMeans_MIS()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var transportMeans1 = bill.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_TransportState = "NEW";
			var transportMeans2 = bill.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = "MIS";
			var transportMeans3 = bill.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = "DEC";

			AssertEquals(0, Provider.DepartureTransportMeans.Count);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			var additionalReference1 = Factory.New<CusSupportingInfo>();
			additionalReference1.CSI_ParentID = bill.PK;
			additionalReference1.CSI_Type = "OTH";
			additionalReference1.CSI_SubType = "REF";
			additionalReference1.CSI_Status = "NEW";
			additionalReference1.CSI_ReferenceNumber = "NEW";

			var additionalReference2 = Factory.New<CusSupportingInfo>();
			additionalReference2.CSI_ParentID = bill.PK;
			additionalReference2.CSI_Type = "OTH";
			additionalReference2.CSI_SubType = "REF";
			additionalReference2.CSI_LineNo = 1;
			additionalReference2.CSI_Status = "MIS";

			var additionalReference3 = Factory.New<CusSupportingInfo>();
			additionalReference3.CSI_ParentID = bill.PK;
			additionalReference3.CSI_Type = "OTH";
			additionalReference3.CSI_SubType = "REF";
			additionalReference3.CSI_LineNo = 2;
			additionalReference3.CSI_Status = "DEC";

			CombineAssertions(() =>
			{
				AssertEquals("NEW included", true, Provider.AdditionalReferences.Any(a => a.ReferenceNumber == "NEW"));
				AssertEquals("MIS included", true, Provider.AdditionalReferences.Any(a => a.SequenceNumber == 1));
			});
		}

		public void TestAdditionalReferences_MIS()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var additionalReference1 = Factory.New<CusSupportingInfo>();
			additionalReference1.CSI_ParentID = bill.PK;
			additionalReference1.CSI_Type = "OTH";
			additionalReference1.CSI_SubType = "REF";
			additionalReference1.CSI_Status = "NEW";
			additionalReference1.CSI_ReferenceNumber = "NEW";

			var additionalReference2 = Factory.New<CusSupportingInfo>();
			additionalReference2.CSI_ParentID = bill.PK;
			additionalReference2.CSI_Type = "OTH";
			additionalReference2.CSI_SubType = "REF";
			additionalReference2.CSI_LineNo = 1;
			additionalReference2.CSI_Status = "MIS";

			var additionalReference3 = Factory.New<CusSupportingInfo>();
			additionalReference3.CSI_ParentID = bill.PK;
			additionalReference3.CSI_Type = "OTH";
			additionalReference3.CSI_SubType = "REF";
			additionalReference3.CSI_LineNo = 2;
			additionalReference3.CSI_Status = "DEC";

			AssertEquals(0, Provider.AdditionalReferences.Count);
		}

		public void TestConsignmentItems()
		{
			var itemTwo = bill.ArrivalGoodsItems.AddNew();
			itemTwo.BY_LineNo = 2;
			CombineAssertions(() =>
			{
				itemTwo.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DEC", 0, provider.ConsignmentItems.Count);

				itemTwo.BY_UnloadedState = NctsUnloadedStateList.Codes.DAM;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DAM", 0, provider.ConsignmentItems.Count);

				itemTwo.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("NEW", 1, provider.ConsignmentItems.Count);

				itemTwo.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("MIS", 1, provider.ConsignmentItems.Count);

				itemTwo.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DIF", 1, provider.ConsignmentItems.Count);

				var itemOne = bill.ArrivalGoodsItems.AddNew();
				itemOne.BY_LineNo = 1;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("sequence item 1", 1, provider.ConsignmentItems.First().GoodsItemNumber);
				AssertEquals("sequence item 2", 2, provider.ConsignmentItems.Last().GoodsItemNumber);
			});
		}

		public void TestConsignmentItems_MIS()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var item = bill.ArrivalGoodsItems.AddNew();
			CombineAssertions(() =>
			{
				item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DEC", 0, provider.ConsignmentItems.Count);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.DAM;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DAM", 0, provider.ConsignmentItems.Count);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("NEW", 0, provider.ConsignmentItems.Count);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("MIS", 0, provider.ConsignmentItems.Count);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				provider = new HouseConsignmentType05Provider(bill, "1");
				AssertEquals("DIF", 0, provider.ConsignmentItems.Count);
			});
		}

		public void TestConsignmentItems_Order()
		{
			var item1 = bill.ArrivalGoodsItems.AddNew();
			item1.BY_LineNo = 2;
			var item2 = bill.ArrivalGoodsItems.AddNew();
			item2.BY_LineNo = 1;
			var item3 = bill.ArrivalGoodsItems.AddNew();
			item3.BY_LineNo = 3;

			AssertContainsExactElementsInExactOrder(new[] { 1, 2, 3 }, GetProvider().ConsignmentItems.Select(x => x.GoodsItemNumber));
		}

		public void TestTransportDocuments()
		{
			var transportDocument1 = Factory.New<CusSupportingInfo>();
			transportDocument1.CSI_ParentID = bill.PK;
			transportDocument1.CSI_Type = "OTH";
			transportDocument1.CSI_SubType = "TRA";
			transportDocument1.CSI_Status = "NEW";
			transportDocument1.CSI_ReferenceNumber = "NEW";

			var transportDocument2 = Factory.New<CusSupportingInfo>();
			transportDocument2.CSI_ParentID = bill.PK;
			transportDocument2.CSI_Type = "OTH";
			transportDocument2.CSI_SubType = "TRA";
			transportDocument2.CSI_LineNo = 1;
			transportDocument2.CSI_Status = "MIS";

			var transportDocument3 = Factory.New<CusSupportingInfo>();
			transportDocument3.CSI_ParentID = bill.PK;
			transportDocument3.CSI_Type = "OTH";
			transportDocument3.CSI_SubType = "TRA";
			transportDocument3.CSI_LineNo = 2;
			transportDocument3.CSI_Status = "DEC";

			CombineAssertions(() =>
			{
				AssertEquals("NEW included", true, Provider.TransportDocuments.Any(a => a.ReferenceNumber == "NEW"));
				AssertEquals("MIS included", true, Provider.TransportDocuments.Any(a => a.SequenceNumber == 1));
			});
		}

		public void TestTransportDocuments_MIS()
		{
			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var transportDocument1 = Factory.New<CusSupportingInfo>();
			transportDocument1.CSI_ParentID = bill.PK;
			transportDocument1.CSI_Type = "OTH";
			transportDocument1.CSI_SubType = "TRA";
			transportDocument1.CSI_Status = "NEW";
			transportDocument1.CSI_ReferenceNumber = "NEW";

			var transportDocument2 = Factory.New<CusSupportingInfo>();
			transportDocument2.CSI_ParentID = bill.PK;
			transportDocument2.CSI_Type = "OTH";
			transportDocument2.CSI_SubType = "TRA";
			transportDocument2.CSI_LineNo = 1;
			transportDocument2.CSI_Status = "MIS";

			var transportDocument3 = Factory.New<CusSupportingInfo>();
			transportDocument3.CSI_ParentID = bill.PK;
			transportDocument3.CSI_Type = "OTH";
			transportDocument3.CSI_SubType = "TRA";
			transportDocument3.CSI_LineNo = 2;
			transportDocument3.CSI_Status = "DEC";

			AssertEquals(0, Provider.TransportDocuments.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			bill = header.Bills.AddNew();
			bill.MovementDetail.B9_SeqNo = "3";

			provider = new HouseConsignmentType05Provider(bill, "3");
		}

		protected override HouseConsignmentType05Provider GetProvider() => provider ??= new HouseConsignmentType05Provider(bill, "1");
		HouseConsignmentType05Provider provider;
		NctsBill bill;
	}
}
