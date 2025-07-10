using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ConsignmentItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentItemWrapper>
	{
		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber should be mapped to BY_LineNo.", "77", Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("DeclarationGoodsItemNumber should be mapped to BY_DeclarationGoodsItemNumber.", "88", Provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationType()
		{
			AssertEquals("DeclarationType should be mapped to BY_Type.", "AAA", Provider.DeclarationType);
		}

		public void TestCountryOfDispatch()
		{
			AssertEquals("CountryOfDispatch should be mapped to item.BY_RN_NKCountryOfDispatch.", "MQ", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			AssertEquals("CountryOfDestination should be mapped to BY_RN_NKCountryOfDestination.", "TR", Provider.CountryOfDestination);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCR should be mapped to BY_CommercialReferenceNumber.", "Your commercial reference", Provider.ReferenceNumberUCR);
		}

		public void TestConsignee()
		{
			AssertEquals("Consignee should be using ConsigneeWrapper.", "BN CORP, FR12345678900001", $"{Provider.Consignee.Name}, {Provider.Consignee.IdentificationNumber}");

			var provider = GetProviderForHeaderTypeArrival();
			AssertNull("Consignee should be null when NCTSHeader is of type Arrival.", provider.Consignee);
		}

		public void TestCommodity()
		{
			var commodity = Provider.Commodity;
			AssertEquals("Commodity should be using CommodityWrapper.", "Description, 0145792-7", $"{commodity.DescriptionOfGoods}, {commodity.CusCode}");
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertContainsExactElementsInAnyOrder("AdditionalSupplyChainActor should be using AdditionalSupplyChainActorWrapper.", new string[] { "S1", "S2" }, Provider.AdditionalSupplyChainActor.Select(x => x.IdentificationNumber));

			var provider = GetProviderForHeaderTypeArrival();
			AssertNull("AdditionalSupplyChainActor should be null when NCTSHeader is of type Arrival.", provider.AdditionalSupplyChainActor);
		}

		public void TestPackaging()
		{
			AssertContainsExactElementsInAnyOrder("Packaging should be using PackagingWrapper.", new string[] { "CT", "BX" }, Provider.Packaging.Select(x => x.TypeOfPackages));
		}

		public void TestPreviousDocument()
		{
			AssertContainsExactElementsInAnyOrder("PreviousDocument should be using PreviousDocumentWrapper.", new string[] { "PREV1", "PREV2" }, Provider.PreviousDocument.Select(x => x.Type));

			var provider = GetProviderForHeaderTypeArrival();
			AssertNull("PreviousDocument Should be null when Header type is Arrival.", provider.PreviousDocument);
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

		ConsignmentItemWrapper GetProviderForHeaderTypeArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var bill = nctsHeader.Bills.AddNew();
			var item = bill.ArrivalGoodsItems.AddNew();
			return ConsignmentItemWrapper.New(item);
		}

		protected override ConsignmentItemWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_MethodOfPayment = "Z";
			var bill = nctsHeader.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();

			item.BY_LineNo = 77;
			item.BY_DeclarationGoodsItemNumber = 88;
			item.BY_Type = "AAA";
			item.BY_RN_NKCountryOfDispatch = "MQ";
			item.BY_RN_NKCountryOfDestination = "TR";
			item.BY_CommercialReferenceNumber = "Your commercial reference";
			item.BY_CusC4Number = "0145792-7";
			item.BY_Description = "Description";

			var package1 = item.Packages.AddNew();
			package1.B5_UnitType = "CT";
			var package2 = item.Packages.AddNew();
			package2.B5_UnitType = "BX";

			var previousDocument1 = item.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "PREV1";
			var previousDocument2 = item.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "PREV2";

			var supportingDocument1 = item.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "SUP1";
			var supportingDocument2 = item.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "SUP2";

			var transportDocument1 = item.AdditionalInfos.AddNew();
			transportDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument1.CSI_Code = "TRA1";
			var transportDocument2 = item.AdditionalInfos.AddNew();
			transportDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument2.CSI_Code = "TRA2";

			var additionalReference1 = item.AdditionalInfos.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Code = "REF1";
			var additionalReference2 = item.AdditionalInfos.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Code = "REF2";

			var additionalInformation1 = item.AdditionalInfos.AddNew();
			additionalInformation1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation1.CSI_Code = "INF1";
			var additionalInformation2 = item.AdditionalInfos.AddNew();
			additionalInformation2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation2.CSI_Code = "INF2";

			var supplyChainActor1 = item.CusSupplyChainActorReferences.AddNew();
			supplyChainActor1.CFR_Reference = "S1";
			var supplyChainActor2 = item.CusSupplyChainActorReferences.AddNew();
			supplyChainActor2.CFR_Reference = "S2";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BN CORP";
			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			item.Consignee.OrganisationPK = consignee.PK;

			return ConsignmentItemWrapper.New(item);
		}
	}
}
