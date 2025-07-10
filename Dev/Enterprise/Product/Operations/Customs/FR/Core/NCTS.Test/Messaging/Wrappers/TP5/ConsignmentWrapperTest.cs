using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentWrapper>
	{
		public void TestCountryOfDispatch()
		{
			AssertEquals("CountryOfDispatch should be mapped to BM_RN_NKCountryOfDispatch.", "TR", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			AssertEquals("CountryOfDestination should be mapped to BM_RL_NKDestinationPort.", "CH", Provider.CountryOfDestination);
		}

		public void TestContainerIndicator()
		{
			AssertEquals("ContainerIndicator should be true when one of Goods Item has a container pivot.", true, Provider.ContainerIndicator);
		}

		public void TestInlandModeOfTransport()
		{
			AssertEquals("InlandModeOfTransport should be mapped to InlandTransportModeAtDeparture.", "3", Provider.InlandModeOfTransport);

			var provider = GetProviderForHeaderTypeArrival();
			AssertEquals("InlandModeOfTransport should be empty when Header type is Arrival.", ZString.Empty, provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			AssertEquals("ModeOfTransportAtTheBorder should be mapped to BM_ExportTransportMode.", "4", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass should be mapped to BM_GrossWeight.", 9999.999m, Provider.GrossMass);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCR should be mapped to BM_UniqueConsignmentReference.", "TOTO", Provider.ReferenceNumberUCR);
		}

		public void TestCarrier()
		{
			AssertEquals("Carrier should be using OrganizationWithContactWrapper.", "CARRIER CORP, FR12345678900003, Contact2", $"{Provider.Carrier.Name}, {Provider.Carrier.IdentificationNumber}, {Provider.Carrier.ContactPerson.Name}");

			var provider = GetProviderForHeaderTypeArrival();
			AssertNull("Carrier should be null when Header type is Arrival.", provider.Carrier);
		}

		public void TestConsignor()
		{
			AssertEquals("Consignor should be using OrganizationWithContactWrapper.", "SJ CORP, FR12345678900002, Contact1", $"{Provider.Consignor.Name}, {Provider.Consignor.IdentificationNumber}, {Provider.Consignor.ContactPerson.Name}");
		}

		public void TestIncident()
		{
			var provider = GetProviderForHeaderTypeArrival();
			AssertEquals("There should be 2 incidents.", 2, provider.Incident.Count);
			AssertType<EnRouteIncidentWrapper>("Incident should be of type EnRouteIncidentWrapper.", provider.Incident.ElementAt(0));
		}

		public void TestConsignee()
		{
			AssertEquals("Consignee should be using movementHeader in OrganizationWrapper.", "BN CORP, FR12345678900001", $"{Provider.Consignee.Name}, {Provider.Consignee.IdentificationNumber}");

			var actualConsignee = Factory.New<OrgHeader>();
			actualConsignee.OH_FullName = "AK CORP";
			actualConsignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			actualConsignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var provider = ConsignmentWrapper.New(Factory.New<NctsDepartureMovementHeader>(), actualConsignee);

			AssertEquals("Consignee should be using actualConsignee in OrganizationWrapper.", "AK CORP, FR12345678900001", $"{provider.Consignee.Name}, {provider.Consignee.IdentificationNumber}");
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertContainsExactElementsInAnyOrder("AdditionalSupplyChainActor should be using AdditionalSupplyChainActorWrapper.", new string[] { "S1", "S2" }, Provider.AdditionalSupplyChainActor.Select(x => x.IdentificationNumber));
		}

		public void TestTransportEquipment()
		{
			AssertContainsExactElementsInAnyOrder("TransportEquipment should be using TransportEquipmentWrapper.", new string[] { "CONT1, 7", "CONT2, 84" }, Provider.TransportEquipment.Select(x => x.ContainerIdentificationNumber + ", " + x.GoodsReference.First().DeclarationGoodsItemNumber));

			var provider = GetProviderForHeaderTypeArrival();
			AssertContainsExactElementsInAnyOrder("TransportEquipment should be using TransportEquipmentsWrapper for Arrival.", new string[] { "CONT1, 7", "CONT2, 84" }, provider.TransportEquipment.Select(x => x.ContainerIdentificationNumber + ", " + x.GoodsReference.Last().DeclarationGoodsItemNumber));
		}

		public void TestLocationOfGoods()
		{
			AssertEquals("LocationOfGoods should be using LocationOfGoodsWrapper.", "A", Provider.LocationOfGoods.TypeOfLocation);
		}

		public void TestDepartureTransportMeans()
		{
			AssertContainsExactElementsInAnyOrder("DepartureTransportMeans should be using DepartureTransportMeansWrapper.", new string[] { "9" }, Provider.DepartureTransportMeans.Select(x => x.TypeOfIdentification));

			var provider = GetProviderForHeaderTypeArrival();
			AssertContainsExactElementsInAnyOrder("DepartureTransportMeans should be using ArrivalTransportMeansWrapper.", new string[] { "1234" }, provider.DepartureTransportMeans.Select(x => x.IdentificationNumber));
		}

		public void TestCountryOfRoutingofConsignment()
		{
			AssertContainsExactElementsInAnyOrder("CountryOfRoutingofConsignment should be using CountryOfRoutingofConsignmentWrapper.", new string[] { "LU", "DE" }, Provider.CountryOfRoutingofConsignment.Select(x => x.Country));
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertContainsExactElementsInAnyOrder("ActiveBorderTransportMeans should be using ActiveBorderTransportMeansWrapper.", new string[] { "FR000040" }, Provider.ActiveBorderTransportMeans.Select(x => x.CustomsOfficeAtBorderReference));
		}

		public void TestPlaceofLoading()
		{
			AssertEquals("PlaceofLoading.UNLoCode should be mapped to BM_PortOfPresentationCode.", "FRPAR", Provider.PlaceofLoading.UNLoCode);
			AssertEquals("PlaceofLoading.Location should be mapped to BM_PlaceOfLoading.", "Paris", Provider.PlaceofLoading.Location);
		}

		public void TestPlaceofUnloading()
		{
			AssertEquals("PlaceofUnloading.UNLoCode should be mapped to BM_ForeignDestPortKCode.", "CNBJS", Provider.PlaceofUnloading.UNLoCode);
			AssertEquals("PlaceofUnloading.Location should be mapped to BM_PlaceOfUnloading.", "Beijing", Provider.PlaceofUnloading.Location);
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

		public void TestHouseConsignment()
		{
			AssertContainsExactElementsInExactOrder("HouseConsignment should use HouseConsignmentWrapper and bills should be ordered by their sequence number.", new string[] { "BE", "ES" }, Provider.HouseConsignment.Select(x => x.CountryofDispatch));
		}

		ConsignmentWrapper GetProviderForHeaderTypeArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NC5";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			nctsHeader.EnRouteIncidents.AddNew();
			nctsHeader.EnRouteIncidents.AddNew();

			var movementHeader = nctsHeader.ArrivalMovementHeader;

			var arrivalTransportInfos = movementHeader.ArrivalTransportInfos.AddNew();
			arrivalTransportInfos.TPM_IdentificationNumber = "1234";

			var container1 = nctsHeader.ArrivalHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONT2";

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_RN_NKCountryOfExport = "ES";
			bill1.SequenceNumber = 4;

			var item1 = bill1.ArrivalGoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 7;
			var package1 = item1.Packages.AddNew();
			var containerPivot1 = package1.ContainersPivot.AddNew();
			containerPivot1.XX_Relation2ID = container1.PK;

			var item2 = bill1.ArrivalGoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 84;
			var package2 = item2.Packages.AddNew();
			var containerPivot2 = package2.ContainersPivot.AddNew();
			containerPivot2.XX_Relation2ID = container2.PK;

			return ConsignmentWrapper.New(movementHeader);
		}

		protected override ConsignmentWrapper GetProvider()
		{
			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_RN_NKCountryCode = "BE";
			unloco2.RL_Code = "BEANV";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var movementHeader = nctsHeader.MovementHeader;

			var locationGoods = movementHeader.GoodsLocation;
			locationGoods.CGL_Type = "A";

			var countryOfRouting1 = nctsHeader.CountriesOfRouting.AddNew();
			countryOfRouting1.CY_Data = "LU";
			var countryOfRouting2 = nctsHeader.CountriesOfRouting.AddNew();
			countryOfRouting2.CY_Data = "DE";

			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONT2";

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_RN_NKCountryOfExport = "ES";
			bill1.SequenceNumber = 4;
			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_RN_NKCountryOfExport = "BE";
			bill2.SequenceNumber = 3;

			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_DeclarationGoodsItemNumber = 7;
			var package = item1.Packages.AddNew();
			var containerPivot1 = package.ContainersPivot.AddNew();
			containerPivot1.XX_Relation2ID = container1.PK;
			package.ContainersPivotsForBindingOnly[0].ContainerSelected = true;

			var item2 = bill1.GoodsItems.AddNew();
			item2.BY_DeclarationGoodsItemNumber = 84;
			var package2 = item2.Packages.AddNew();
			var containerPivot2 = package2.ContainersPivot.AddNew();
			containerPivot2.XX_Relation2ID = container2.PK;
			package2.ContainersPivotsForBindingOnly[0].ContainerSelected = true;

			var previousDocument1 = nctsHeader.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "PREV1";
			var previousDocument2 = nctsHeader.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "PREV2";

			var supportingDocument1 = movementHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "SUP1";
			var supportingDocument2 = movementHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "SUP2";

			var transportDocument1 = nctsHeader.AdditionalDocuments.AddNew();
			transportDocument1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument1.CSI_Code = "TRA1";
			var transportDocument2 = nctsHeader.AdditionalDocuments.AddNew();
			transportDocument2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDocument2.CSI_Code = "TRA2";

			var additionalReference1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference1.CSI_Code = "REF1";
			var additionalReference2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalReference2.CSI_Code = "REF2";

			var additionalInformation1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInformation1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation1.CSI_Code = "INF1";
			var additionalInformation2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInformation2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInformation2.CSI_Code = "INF2";

			var supplyChainActor1 = nctsHeader.CusSupplyChainActors.AddNew();
			supplyChainActor1.CFR_Reference = "S1";
			var supplyChainActor2 = nctsHeader.CusSupplyChainActors.AddNew();
			supplyChainActor2.CFR_Reference = "S2";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BN CORP";
			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			nctsHeader.Consignee.OrganisationPK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "SJ CORP";
			consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignor.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00002", Core.Constants.CountryCodes.France);
			nctsHeader.Consignor.OrganisationPK = consignor.PK;

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "Contact1";
			consignorContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRIER CORP";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			carrier.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00003", Core.Constants.CountryCodes.France);
			movementHeader.Carrier.OrganisationPK = carrier.PK;

			var carrierContact = carrier.Contacts.AddNew();
			carrierContact.OC_ContactName = "Contact2";
			carrierContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			movementHeader.BM_MethodOfPayment = "Z";
			movementHeader.BM_PortOfPresentationCode = "FRPAR";
			movementHeader.BM_PlaceOfLoading = "Paris";
			movementHeader.BM_ForeignDestPortKCode = "CNBJS";
			movementHeader.BM_PlaceOfUnloading = "Beijing";
			movementHeader.BM_UniqueConsignmentReference = "TOTO";
			movementHeader.BM_GrossWeight = 9999.999m;
			movementHeader.BM_RN_NKCountryOfDispatch = "TR";
			movementHeader.BM_InlandTransportMode = "3";
			movementHeader.BM_ExportTransportMode = "4";
			movementHeader.BM_TransportAtDepartureType = "9";
			movementHeader.BM_CustomsOfficeAtBorder = "FR000040";
			movementHeader.BM_RL_NKDestinationPort = "CH";

			return ConsignmentWrapper.New(movementHeader);
		}
	}
}
