using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class HouseConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<HouseConsignmentWrapper>
	{
		public void TestCountryofDispatch()
		{
			AssertEquals("CountryofDispatch should be mapped to B0_RN_NKCountryOfExport.", "FR", Provider.CountryofDispatch);
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass should be mapped to B0_Weight.", 999.99m, Provider.GrossMass);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCR should be mapped to B0_ReferenceID.", "UCR", Provider.ReferenceNumberUCR);
		}

		public void TestConsignor()
		{
			var consignor = Provider.Consignor;
			AssertEquals("Consignor should be using OrganizationWithContactWrapper.", "SJ CORP, FR12345678900002, Contact1", $"{consignor.Name}, {consignor.IdentificationNumber}, {consignor.ContactPerson.Name}");
		}

		public void TestConsignee()
		{
			var consigee = Provider.Consignee;
			AssertEquals("Consignee should be using OrganizationWrapper.", "BN CORP, FR12345678900001", $"{consigee.Name}, {consigee.IdentificationNumber}");
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertContainsExactElementsInAnyOrder("AdditionalSupplyChainActor should be using AdditionalSupplyChainActorWrapper.", new string[] { "S1", "S2" }, Provider.AdditionalSupplyChainActor.Select(x => x.IdentificationNumber));
		}

		public void TestDepartureTransportMeans()
		{
			AssertContainsExactElementsInAnyOrder("DepartureTransportMeans should be using DepartureTransportMeansWrapper.", new string[] { "9" }, Provider.DepartureTransportMeans.Select(x => x.TypeOfIdentification));

			var provider = GetProviderForHeaderTypeArrival();
			AssertContainsExactElementsInAnyOrder("DepartureTransportMeans should be using ArrivalTransportMeansWrapper when Header type is Arrival.", new string[] { "FR" }, provider.DepartureTransportMeans.Select(x => x.TypeOfIdentification));
		}

		public void TestPreviousDocument()
		{
			AssertContainsExactElementsInAnyOrder("PreviousDocument should be using PreviousDocumentWrapper.", new string[] { "PREV1", "PREV2" }, Provider.PreviousDocument.Select(x => x.Type));
		}

		public void TestSupportingDocument()
		{
			AssertContainsExactElementsInAnyOrder("SupportingDocument should be using SupportingDocumentWrapper.", new string[] { "SUP1", "SUP2" }, Provider.SupportingDocument.Select(x => x.Type));
		}

		public void TestTransportDocument()
		{
			AssertContainsExactElementsInAnyOrder("TransportDocument should be using DocumentWrapper.", new string[] { "TRA1", "TRA2" }, Provider.TransportDocument.Select(x => x.Type));
		}

		public void TestAdditionalReference()
		{
			AssertContainsExactElementsInAnyOrder("AdditionalReference should be using DocumentWrapper.", new string[] { "REF1", "REF2" }, Provider.AdditionalReference.Select(x => x.Type));
		}

		public void TestAdditionalInformation()
		{
			AssertContainsExactElementsInAnyOrder("AdditionalInformation should be using AdditionalInformationWrapper.", new string[] { "INF1", "INF2" }, Provider.AdditionalInformation.Select(x => x.Code));
		}

		public void TestTransportCharges()
		{
			AssertEquals("TransportCharges should be using TransportChargesWrapper.", "Z", Provider.TransportCharges.MethodOfPayment);
		}

		public void TestConsignmentItem()
		{
			AssertContainsExactElementsInExactOrder("For departure headers, ConsignmentItem should be mapped with header.Bills.GoodsItems ordered by BY_LineNo.", new string[] { "1", "2", "3" }, Provider.ConsignmentItem.Select(x => x.GoodsItemNumber));

			var provider = GetProviderForHeaderTypeArrival();
			AssertType<ConsignmentItemWrapper>("ConsignmentItem should use ConsignmentItemWrapper for arrival.", provider.ConsignmentItem.FirstOrDefault());
			AssertContainsExactElementsInExactOrder("For arrival headers, ConsignmentItem should be mapped with header.Bills.ArrivalGoodsItems ordered by BY_LineNo.", new string[] { "1", "2", "3" }, provider.ConsignmentItem.Select(x => x.GoodsItemNumber));
		}

		HouseConsignmentWrapper GetProviderForHeaderTypeArrival()
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
			item1.BY_LineNo = 2;
			var item2 = bill.ArrivalGoodsItems.AddNew();
			item2.BY_LineNo = 1;
			var item3 = bill.ArrivalGoodsItems.AddNew();
			item3.BY_LineNo = 3;

			return HouseConsignmentWrapper.New(bill);
		}

		protected override HouseConsignmentWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_TransportAtDepartureType = "9";
			var bill = nctsHeader.Bills.AddNew();
			bill.B0_TransportPaymentMethod = "Z";
			bill.B0_RN_NKCountryOfExport = "FR";
			bill.B0_Weight = 999.99m;
			bill.B0_ReferenceID = "UCR";
			var item1 = bill.GoodsItems.AddNew();
			item1.BY_LineNo = 2;
			var item2 = bill.GoodsItems.AddNew();
			item2.BY_LineNo = 1;
			var item3 = bill.GoodsItems.AddNew();
			item3.BY_LineNo = 3;

			var previousDocument1 = bill.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "PREV1";
			var previousDocument2 = bill.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "PREV2";

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

			var additionalInformation1 = bill.AdditionalDocuments.AddNew();
			additionalInformation1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation1.CSI_Code = "INF1";
			var additionalInformation2 = bill.AdditionalDocuments.AddNew();
			additionalInformation2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation2.CSI_Code = "INF2";

			var supplyChainActor1 = bill.CusSupplyChainActorReferences.AddNew();
			supplyChainActor1.CFR_Reference = "S1";
			var supplyChainActor2 = bill.CusSupplyChainActorReferences.AddNew();
			supplyChainActor2.CFR_Reference = "S2";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BN CORP";
			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			bill.Consignee.OrganisationPK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "SJ CORP";
			consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignor.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00002", Core.Constants.CountryCodes.France);
			bill.Consignor.OrganisationPK = consignor.PK;

			var contact1 = consignor.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			return HouseConsignmentWrapper.New(bill);
		}
	}
}
