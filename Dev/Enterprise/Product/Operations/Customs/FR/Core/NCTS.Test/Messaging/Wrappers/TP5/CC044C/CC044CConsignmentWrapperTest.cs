using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC044CConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CConsignmentWrapper>
	{
		public void TestGrossMass()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NC5";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_GrossWeight = 9999.999m;
			movementHeader.EffectiveGrossWeightUnloaded = 6666.666m;
			movementHeader.BM_NoChangesToReport = false;
			var detail = movementHeader.MovementDetails.AddNew();
			detail.B9_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

			var wrapper =  CC044CConsignmentWrapper.New(movementHeader);
			AssertEquals("GrossMass should be mapped to EffectiveGrossWeightUnloaded everytime.", 6666.666m, wrapper.GrossMass);
		}

		public void TestTransportEquipment()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = "NC5";
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;

			var container1 = header.ArrivalHeaderContainers.AddNew();
			container1.BC_ContainerNum = "1";
			container1.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			container1.Seals.AddNew().BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var container2 = header.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "2";
			container2.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var container3 = header.ArrivalHeaderContainers.AddNew();
			container3.BC_ContainerNum = "3";
			container3.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var container4 = header.ArrivalHeaderContainers.AddNew();
			container4.BC_ContainerNum = "4";
			container4.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			container4.Seals.AddNew().BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var container5 = header.ArrivalHeaderContainers.AddNew();
			container5.BC_ContainerNum = "5";
			container5.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			container5.Seals.AddNew().BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var container6 = header.ArrivalHeaderContainers.AddNew();
			container6.BC_ContainerNum = "6";
			container6.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			container6.Seals.AddNew().BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var container7 = header.ArrivalHeaderContainers.AddNew();
			container7.BC_ContainerNum = "7";
			container7.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			container7.Seals.AddNew().BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertEquals("DEC container are mapped only if they have seal that are not DEC.", 6, wrapper.TransportEquipment.Count);
		}

		public void TestDepartureTransportMeans()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var transportMeans1 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_IdentificationNumber = "1";
			transportMeans1.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var transportMeans2 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_IdentificationNumber = "2";
			transportMeans2.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var transportMeans3 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_IdentificationNumber = "3";
			transportMeans3.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertContainsExactElementsInAnyOrder("DEC transport Means are not mapped. Identification Number for MIS is not mapped", new[] { "2", string.Empty }, wrapper.DepartureTransportMeans.Select(x => x.IdentificationNumber).ToArray());
		}

		public void TestSupportingDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var supportingDocument1 = movementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var supportingDocument2 = movementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var supportingDocument3 = movementHeader.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertEquals("Only SupportingDocuments with Status NEW and MIS are mapped.", 2, wrapper.SupportingDocument.Count);
		}

		public void TestTransportDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var additionalReference1 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var additionalReference2 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var additionalReference3 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalReference3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertEquals("Only TransportDocument with Status NEW and MIS are mapped.", 2, wrapper.TransportDocument.Count);
		}

		public void TestAdditionalReference()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var additionalReference1 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var additionalReference2 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			var additionalReference3 = movementHeader.AdditionalDocuments.AddNew();
			additionalReference3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference3.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertEquals("Only AdditionalReference with Status NEW and MIS are mapped.", 2, wrapper.AdditionalReference.Count);
		}

		public void TestHouseConsignment()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			bill.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			bill.B0_Weight = 1m;
			var bill2 = nctsHeader.Bills.AddNew();
			bill2.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
			bill2.B0_GrossWeightUnloaded = 2m;
			var bill3 = nctsHeader.Bills.AddNew();
			bill3.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
			bill3.B0_Weight = 3m;
			var bill4 = nctsHeader.Bills.AddNew();
			bill4.B0_Weight = 4m;
			bill4.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			var wrapper = CC044CConsignmentWrapper.New(movementHeader);

			AssertContainsExactElementsInAnyOrder("All house consigment but DEC are generated.", new decimal[] { 2m, 3m, 4m, }, wrapper.HouseConsignment.Select(x => x.GrossMass.Value).ToArray());
		}

		protected override CC044CConsignmentWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NC5";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var movementHeader = nctsHeader.ArrivalMovementHeader;

			return CC044CConsignmentWrapper.New(movementHeader);
		}
	}
}
