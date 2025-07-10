using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class ConsignmentType06ProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentType06Provider>
	{
		public void TestGrossMass()
		{
			var bill1 = header.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem1.BY_GrossWeight = 260m;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem2.BY_GrossWeight = 25.2175m;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem3.UnloadedGoodsItem.BY_GrossWeight = 0.8715m;
			goodsItem3.UnloadedGoodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;

			var bill2 = header.Bills.AddNew();
			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItem4.BY_GrossWeight = 1.2500m;
			goodsItem4.BY_GrossWeightUnit = "KG";

			var bill3 = header.Bills.AddNew();
			bill3.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var goodsItem5 = bill3.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem4.BY_GrossWeight = 1.2500m;
			goodsItem4.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			AssertEquals(898.2275m, Provider.GrossMass);
		}

		public void TestTransportEquipments()
		{
			var container1 = header.ArrivalHeaderContainers.AddNew();
			container1.BC_SequenceNumber = 1;
			container1.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			container1.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			var container2 = header.ArrivalHeaderContainers.AddNew();
			container2.BC_SequenceNumber = 2;
			container2.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var container3 = header.ArrivalHeaderContainers.AddNew();
			container3.BC_SequenceNumber = 3;
			container3.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			var container4 = header.ArrivalHeaderContainers.AddNew();
			container4.BC_SequenceNumber = 4;
			container4.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			container4.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
			var container5 = header.ArrivalHeaderContainers.AddNew();
			container5.BC_SequenceNumber = 5;
			container5.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			container5.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
			AssertArrayEqualsByElements(new[] { 2, 3, 4, 5 }, Provider.TransportEquipments.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestDepartureTransportMeans()
		{
			var transportMeans1 = header.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_SequenceNumber = 1;
			transportMeans1.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var transportMeans2 = header.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_SequenceNumber = 2;
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			var transportMeans3 = header.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_SequenceNumber = 3;
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;

			AssertArrayEqualsByElements(new[] { 2, 3 }, Provider.DepartureTransportMeans.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestSupportingDocuments()
		{
			var doc1 = header.ArrivalMovementHeader.SupportingDocuments.AddNew();
			doc1.CSI_LineNo = 1;
			doc1.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var doc2 = header.ArrivalMovementHeader.SupportingDocuments.AddNew();
			doc2.CSI_LineNo = 2;
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var doc3 = header.ArrivalMovementHeader.SupportingDocuments.AddNew();
			doc3.CSI_LineNo = 3;
			doc3.CSI_Status = NctsUnloadedStateList.Codes.NEW;

			AssertArrayEqualsByElements(new[] { 2, 3 }, Provider.SupportingDocuments.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestTransportDocuments()
		{
			var doc1 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc1.CSI_SubType = Constants.CusSupportingInfoSubTypes.TransportDocument;
			doc1.CSI_LineNo = 1;
			doc1.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var doc2 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc2.CSI_SubType = Constants.CusSupportingInfoSubTypes.TransportDocument;
			doc2.CSI_LineNo = 2;
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var doc3 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc3.CSI_SubType = Constants.CusSupportingInfoSubTypes.TransportDocument;
			doc3.CSI_LineNo = 3;
			doc3.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			var doc4 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc4.CSI_SubType = Constants.CusSupportingInfoSubTypes.AdditionalReference;
			doc4.CSI_LineNo = 4;
			doc4.CSI_Status = NctsUnloadedStateList.Codes.NEW;

			AssertArrayEqualsByElements(new[] { 2, 3 }, Provider.TransportDocuments.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestAdditionalReferences()
		{
			var doc1 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc1.CSI_SubType = Constants.CusSupportingInfoSubTypes.AdditionalReference;
			doc1.CSI_LineNo = 1;
			doc1.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var doc2 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc2.CSI_SubType = Constants.CusSupportingInfoSubTypes.AdditionalReference;
			doc2.CSI_LineNo = 2;
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var doc3 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc3.CSI_SubType = Constants.CusSupportingInfoSubTypes.AdditionalReference;
			doc3.CSI_LineNo = 3;
			doc3.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			var doc4 = header.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			doc4.CSI_SubType = Constants.CusSupportingInfoSubTypes.TransportDocument;
			doc4.CSI_LineNo = 4;
			doc4.CSI_Status = NctsUnloadedStateList.Codes.NEW;

			AssertArrayEqualsByElements(new[] { 2, 3 }, Provider.AdditionalReferences.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestHouseConsignments()
		{
			var bill1 = header.Bills.AddNew();
			bill1.UnloadedStatus = NctsUnloadedStateList.Codes.DEC;

			var bill2 = header.Bills.AddNew();
			bill2.UnloadedStatus = NctsUnloadedStateList.Codes.NEW;

			var bill3 = header.Bills.AddNew();
			bill3.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;

			var bill4 = header.Bills.AddNew();
			bill4.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;

			AssertEquals(3, Provider.HouseConsignments.Count);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentType06Provider(null));
		}

		protected override ConsignmentType06Provider GetProvider() => new ConsignmentType06Provider(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader header;
	}
}
