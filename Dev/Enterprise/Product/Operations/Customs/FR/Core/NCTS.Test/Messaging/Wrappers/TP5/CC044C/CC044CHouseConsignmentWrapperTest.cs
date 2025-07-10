using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC044CHouseConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CHouseConsignmentWrapper>
	{
		public void TestGrossMass()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			bill.B0_Weight = 999.99m;
			bill.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("GrossMass should be mapped to B0_Weight when unloaded status is NEW.", 999.99m, wrapper.GrossMass);

			bill.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			bill.B0_GrossWeightUnloaded = 666.66m;
			wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("GrossMass should be mapped to B0_GrossWeightUnloaded when unloaded status is DIF.", 666.66m, wrapper.GrossMass);
		}

		public void TestDepartureTransportMeans()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var transportMeans1 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			var transportMeans2 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			var transportMeans3 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var transportMeans4 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans4.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var transportMeans5 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans5.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			var transportMeans6 = movementHeader.ArrivalTransportInfos.AddNew();
			transportMeans6.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;

			var wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("Only transportMeans with Status NEW and MIS are mapped.", 2, wrapper.DepartureTransportMeans.Count);
		}

		public void TestSupportingDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var supportingDocument1 = bill.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			var supportingDocument2 = bill.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var supportingDocument3 = bill.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CHouseConsignmentWrapper.New(bill);

			AssertEquals("Only SupportingDocuments with Status NEW and MIS are mapped.", 2, wrapper.SupportingDocument.Count);
		}

		public void TestTransportDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var additionalReference1 = bill.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			var additionalReference2 = bill.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var additionalReference3 = bill.AdditionalDocuments.AddNew();
			additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CHouseConsignmentWrapper.New(bill);

			AssertEquals("Only TransportDocument with Status NEW and MIS are mapped.", 2, wrapper.TransportDocument.Count);

			AssertContainsExactElementsInAnyOrder("TransportDocument should be using DocumentWrapper.", new string[] { "TRA1", "TRA2" }, Provider.TransportDocument.Select(x => x.Type));
		}

		public void TestAdditionalReference()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var additionalReference1 = bill.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			var additionalReference2 = bill.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			var additionalReference3 = bill.AdditionalDocuments.AddNew();
			additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CHouseConsignmentWrapper.New(bill);

			AssertEquals("Only AdditionalReference with Status NEW and MIS are mapped.", 2, wrapper.AdditionalReference.Count);
			AssertContainsExactElementsInAnyOrder("AdditionalReference should be using DocumentWrapper.", new string[] { "REF1", "REF2" }, Provider.AdditionalReference.Select(x => x.Type));
		}

		public void TestConsignmentItem()
		{
			var provider = GetProvider();
			AssertType<CC044CConsignmentItemWrapper>("ConsignmentItem should use ConsignmentItemWrapper for arrival.", provider.ConsignmentItem.FirstOrDefault());
			AssertEquals("ArrivaGoodsItem should be used in CC044CHouseConsignmentWrapper for Arrival.", 2, provider.ConsignmentItem.Count);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var item = bill.ArrivalGoodsItems.AddNew();

			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("goodsItem with DEC status are not sent.", 0, wrapper.ConsignmentItem.Count);

			item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("goodsItem with DEC status are not sent.", 1, wrapper.ConsignmentItem.Count);

			item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("goodsItem with DEC status are not sent.", 1, wrapper.ConsignmentItem.Count);

			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			wrapper = CC044CHouseConsignmentWrapper.New(bill);
			AssertEquals("goodsItem with DEC status are not sent.", 1, wrapper.ConsignmentItem.Count);
		}

		protected override CC044CHouseConsignmentWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			var arrivalTransportInfos = movementHeader.ArrivalTransportInfos.AddNew();
			arrivalTransportInfos.TPM_IdentificationNumber = "1234";
			arrivalTransportInfos.TPM_TypeOfIdentification = "FR";

			var bill = nctsHeader.Bills.AddNew();
			bill.B0_TransportPaymentMethod = "Z";
			bill.B0_RN_NKCountryOfExport = "FR";
			bill.B0_Weight = 999.99m;
			bill.B0_ReferenceID = "UCR";
			var item1 = bill.ArrivalGoodsItems.AddNew();
			item1.BY_LineNo = 1;
			var item2 = bill.ArrivalGoodsItems.AddNew();
			item2.BY_LineNo = 2;

			var supportingDocument1 = bill.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "SUP1";
			var supportingDocument2 = bill.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "SUP2";

			var transportDocument1 = bill.AdditionalDocuments.AddNew();
			transportDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument1.CSI_Code = "TRA1";
			var transportDocument2 = bill.AdditionalDocuments.AddNew();
			transportDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument2.CSI_Code = "TRA2";

			var additionalReference1 = bill.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Code = "REF1";
			var additionalReference2 = bill.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Code = "REF2";
			return CC044CHouseConsignmentWrapper.New(bill);
		}
	}
}
