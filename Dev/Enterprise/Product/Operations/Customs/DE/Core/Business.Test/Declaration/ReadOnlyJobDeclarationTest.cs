using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ReadOnlyJobDeclarationTest : TestCaseWithFactory
	{
		public void TestIsWarehouseAdjustment()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertEquals("JE_MessageType is 'WAD'", true, declaration.IsWarehouseAdjustment);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("JE_MessageType isn't 'WAD'", false, declaration.IsWarehouseAdjustment);
			});
		}

		public void TestIsInwardProcessingAVABR()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			CombineAssertions(() =>
			{
				AssertEquals("Entry instruction is 'AVABR'", true, declaration.IsInwardProcessingAVABR);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertEquals("Entry instruction is NOT 'AVABR'", false, declaration.IsInwardProcessingAVABR);
			});
		}

		public void TestClearFields_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.SupplierDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.ZG_BorderTransportMeans = "00";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;

			declaration.JE_MasterBill = "123";
			declaration.JE_VoyageFlightNo = "QF123";
			declaration.JE_VesselName = "MYVESSEL";
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Australia;
			declaration.JE_RL_NKPortOfLoading = "AU";
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
			declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			declaration.JE_RL_NKPortOfArrival = "ITCEJ";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.ZG_Box18TransportID = "1";
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.Turkey;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			declaration.InlandTransports.AddNew();

			declaration.JE_HouseBill = "H12345";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_GoodsOrigin = "AU";
			declaration.JE_DateAtOrigin = ZDateTime.Today;
			declaration.JE_RL_NKFinalDestination = "GBLON";
			declaration.JE_GoodsDestination = "GB";
			declaration.JE_DateAtFinalDestination = ZDateTime.Today;

			declaration.JE_GoodsDescription = "ABCDE";
			declaration.JE_LocationOfGoods = "LHR";
			declaration.JE_TotalNoOfPacks = 5;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Bag;
			declaration.JE_TotalWeight = 6.6;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;
			declaration.JE_TotalVolume = 8.8;
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicFeet;
			declaration.JE_ShipmentIncoTermPlace = "X";
			declaration.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;

			declaration.JE_AgentsReference = "Agent";
			declaration.JE_UCR = "DUCR123";

			declaration.ZG_PresentationStartDate = ZDateTime.Today;
			declaration.ZG_PresentationEndDate = ZDateTime.Today;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertEquals("ImporterDocumentaryAddress", ZGuid.Empty, declaration.ImporterDocumentaryAddress.OrganisationPK);
				AssertEquals("JE_EntryStyle", ZString.Empty, declaration.JE_EntryStyle);
				AssertEquals("JE_TransportMode", ZString.Empty, declaration.JE_TransportMode);
				AssertEquals("ZG_BorderTransportMeans", ZString.Empty, declaration.ZG_BorderTransportMeans);
				AssertEquals("JE_ContainerMode", ZString.Empty, declaration.JE_ContainerMode);
				AssertEquals("ZG_SpecificCircumstanceIndicator", ZString.Empty, declaration.ZG_SpecificCircumstanceIndicator);

				AssertEquals("JE_MasterBill", ZString.Empty, declaration.JE_MasterBill);
				AssertEquals("JE_VoyageFlightNo", ZString.Empty, declaration.JE_VoyageFlightNo);
				AssertEquals("JE_VesselName", ZString.Empty, declaration.JE_VesselName);
				AssertEquals("JE_RN_NKTransportNationality", ZString.Empty, declaration.JE_RN_NKTransportNationality);
				AssertEquals("JE_RL_NKPortOfLoading", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("JE_ExportDate", ZDateTime.Empty, declaration.JE_ExportDate);
				AssertEquals("JE_RL_NKPortOfFirstArrival", ZString.Empty, declaration.JE_RL_NKPortOfFirstArrival);
				AssertEquals("JE_DateOfFirstArrival", ZDateTime.Empty, declaration.JE_DateOfFirstArrival);
				AssertEquals("JE_RL_NKPortOfArrival", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
				AssertEquals("JE_DateOfArrival", ZDateTime.Empty, declaration.JE_DateOfArrival);
				AssertEquals("ZG_Box18TransportID", ZString.Empty, declaration.ZG_Box18TransportID);
				AssertEquals("ZG_Box18TransportNationality", ZString.Empty, declaration.ZG_Box18TransportNationality);
				AssertEquals("JE_TransportModeInland", ZString.Empty, declaration.JE_TransportModeInland);
				AssertEquals("InlandTransports", false, declaration.InlandTransports.Any());

				AssertEquals("JE_HouseBill", ZString.Empty, declaration.JE_HouseBill);
				AssertEquals("JE_RL_NKOrigin", ZString.Empty, declaration.JE_RL_NKOrigin);
				AssertEquals("JE_GoodsOrigin", ZString.Empty, declaration.JE_GoodsOrigin);
				AssertEquals("JE_DateAtOrigin", ZDateTime.Empty, declaration.JE_DateAtOrigin);
				AssertEquals("JE_RL_NKFinalDestination", ZString.Empty, declaration.JE_RL_NKFinalDestination);
				AssertEquals("JE_GoodsDestination", ZString.Empty, declaration.JE_GoodsDestination);
				AssertEquals("JE_DateAtFinalDestination", ZDateTime.Empty, declaration.JE_DateAtFinalDestination);

				AssertEquals("JE_GoodsDescription", ZString.Empty, declaration.JE_GoodsDescription);
				AssertEquals("JE_LocationOfGoods", ZString.Empty, declaration.JE_LocationOfGoods);
				AssertEquals("JE_TotalNoOfPacks", ZInt.Zero, declaration.JE_TotalNoOfPacks);
				AssertEquals("JE_TotalNoOfPacksPackType", ZString.Empty, declaration.JE_TotalNoOfPacksPackType);
				AssertEquals("JE_TotalWeight", ZDecimal.Zero, declaration.JE_TotalWeight);
				AssertEquals("JE_TotalWeightUnit", ZString.Empty, declaration.JE_TotalWeightUnit);
				AssertEquals("JE_ShipmentIncoTerm", ZString.Empty, declaration.JE_ShipmentIncoTerm);
				AssertEquals("JE_TotalVolume", ZDecimal.Zero, declaration.JE_TotalVolume);
				AssertEquals("JE_TotalVolumeUnit", ZString.Empty, declaration.JE_TotalVolumeUnit);
				AssertEquals("JE_ShipmentIncoTermPlace", ZString.Empty, declaration.JE_ShipmentIncoTermPlace);
				AssertEquals("ZG_AgreedPlaceCode", ZString.Empty, declaration.ZG_AgreedPlaceCode);

				AssertEquals("JE_AgentsReference", ZString.Empty, declaration.JE_AgentsReference);
				AssertEquals("JE_UCR", ZString.Empty, declaration.JE_UCR);

				AssertEquals("ZG_PresentationStartDate", ZDateTime.Empty, declaration.ZG_PresentationStartDate);
				AssertEquals("ZG_PresentationEndDate", ZDateTime.Empty, declaration.ZG_PresentationEndDate);
			});
		}

		public void TestTestReadOnlyFields_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertEquals(true, declaration.JE_TransportModeInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_BorderTransportMeansInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_SpecificCircumstanceIndicatorInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ExportDateInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateOfFirstArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateOfArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_Box18TransportIDInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_Box18TransportNationalityInfo.ReadOnly);
			AssertEquals(true, declaration.JE_HouseBillInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals(true, declaration.JE_GoodsOriginInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateAtOriginInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateAtFinalDestinationInfo.ReadOnly);
			AssertEquals(true, declaration.JE_GoodsDescriptionInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalNoOfPacksInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalWeightInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalWeightUnitInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ShipmentIncoTermInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalVolumeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalVolumeUnitInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ShipmentIncoTermPlaceInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_AgreedPlaceCodeInfo.ReadOnly);
			AssertEquals(string.Empty, DataBoundResourceStrings.GetDataForProperty(declaration.ZG_AgreedPlaceCodeInfo).FullDescription);
			AssertEquals(true, declaration.JE_AgentsReferenceInfo.ReadOnly);
			AssertEquals(true, declaration.JE_UCRInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TransportModeInlandInfo.ReadOnly);
			AssertEquals(true, declaration.JE_LocationOfGoodsInfo.ReadOnly);
			AssertEquals(true, declaration.JE_VesselNameInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ContainerModeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_EntryStyleInfo.ReadOnly);
			AssertEquals(true, declaration.JE_GoodsDestinationInfo.ReadOnly);
		}

		public void TestSupplierMandatory() => CombineAssertions(() =>
		{
			var expectedMessage = "Supplier Documentary Address is required for Inventory Management integration.";

			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, expectedMessage);

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_OA_Warehouse = Factory.NewWithValidTestData<OrgHeader>().PK;

			declaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, expectedMessage);
		});

		public void TestClearFields_InwardProcessingAVABR()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.SupplierDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.A;
			declaration.ZG_BorderTransportMeans = "00";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.ZG_CTStatusID = "00001";
			declaration.ZG_IsHighValueOvrd = true;

			declaration.JE_VesselName = "MYVESSEL";
			declaration.JE_RL_NKPortOfLoading = "AU";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
			declaration.JE_RL_NKPortOfArrival = "ITCEJ";
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Australia;
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.ZG_Box18TransportID = "1";
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.Turkey;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;

			declaration.JE_HouseBill = "H12345";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "GBLON";
			declaration.JE_DateAtOrigin = ZDateTime.Today;
			declaration.JE_DateAtFinalDestination = ZDateTime.Today;
			declaration.JE_GoodsDescription = "ABCDE";
			declaration.JE_OwnerRef = "123";
			declaration.JE_TotalNoOfPacks = 5;
			declaration.JE_TotalWeight = 6.6;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_TotalVolume = 8.8;
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicFeet;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtFrontier;
			declaration.JE_ShipmentIncoTermPlace = "X";

			declaration.JE_UCR = "DUCR123";
			declaration.JE_CustomsOffice = "DE058";

			declaration.JE_OA_Representative = Factory.New<OrgHeader>().PK;
			declaration.JE_OA_Representative_ZAddress.OrgPK = Factory.New<OrgHeader>().PK;
			declaration.RepresentativeDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			declaration.DefermentPartyDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;

			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
				AssertEquals("SupplierDocumentaryAddress", ZGuid.Empty, declaration.SupplierDocumentaryAddress.OrganisationPK);
				AssertEquals("JE_DeclarantType", ZString.Empty, declaration.JE_DeclarantType);
				AssertEquals("JE_TransportMode", ZString.Empty, declaration.JE_TransportMode);
				AssertEquals("ZG_MethodOfPayment", ZString.Empty, declaration.ZG_MethodOfPayment);
				AssertEquals("ZG_BorderTransportMeans", ZString.Empty, declaration.ZG_BorderTransportMeans);
				AssertEquals("JE_ContainerMode", ZString.Empty, declaration.JE_ContainerMode);
				AssertEquals("ZG_SpecificCircumstanceIndicator", ZString.Empty, declaration.ZG_SpecificCircumstanceIndicator);
				AssertEquals("ZG_CTStatusID", ZString.Empty, declaration.ZG_CTStatusID);
				AssertEquals("ZG_IsHighValueOvrd", false, declaration.ZG_IsHighValueOvrd);

				AssertEquals("JE_VesselName", ZString.Empty, declaration.JE_VesselName);
				AssertEquals("JE_RL_NKPortOfLoading", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("JE_RL_NKPortOfFirstArrival", ZString.Empty, declaration.JE_RL_NKPortOfFirstArrival);
				AssertEquals("JE_RL_NKPortOfArrival", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
				AssertEquals("JE_RN_NKTransportNationality", ZString.Empty, declaration.JE_RN_NKTransportNationality);
				AssertEquals("JE_ExportDate", ZDateTime.Empty, declaration.JE_ExportDate);
				AssertEquals("JE_DateOfFirstArrival", ZDateTime.Empty, declaration.JE_DateOfFirstArrival);
				AssertEquals("JE_DateOfArrival", ZDateTime.Empty, declaration.JE_DateOfArrival);
				AssertEquals("ZG_Box18TransportID", ZString.Empty, declaration.ZG_Box18TransportID);
				AssertEquals("ZG_Box18TransportNationality", ZString.Empty, declaration.ZG_Box18TransportNationality);
				AssertEquals("JE_TransportModeInland", ZString.Empty, declaration.JE_TransportModeInland);

				AssertEquals("JE_HouseBill", ZString.Empty, declaration.JE_HouseBill);
				AssertEquals("JE_RL_NKOrigin", ZString.Empty, declaration.JE_RL_NKOrigin);
				AssertEquals("JE_RL_NKFinalDestination", ZString.Empty, declaration.JE_RL_NKFinalDestination);
				AssertEquals("JE_DateAtOrigin", ZDateTime.Empty, declaration.JE_DateAtOrigin);
				AssertEquals("JE_DateAtFinalDestination", ZDateTime.Empty, declaration.JE_DateAtFinalDestination);
				AssertEquals("JE_GoodsDescription", ZString.Empty, declaration.JE_GoodsDescription);
				AssertEquals("JE_OwnerRef", ZString.Empty, declaration.JE_OwnerRef);
				AssertEquals("JE_TotalNoOfPacks", ZInt.Zero, declaration.JE_TotalNoOfPacks);
				AssertEquals("JE_TotalWeight", ZDecimal.Zero, declaration.JE_TotalWeight);
				AssertEquals("JE_TotalWeightUnit", ZString.Empty, declaration.JE_TotalWeightUnit);
				AssertEquals("JE_TotalVolume", ZDecimal.Zero, declaration.JE_TotalVolume);
				AssertEquals("JE_TotalVolumeUnit", ZString.Empty, declaration.JE_TotalVolumeUnit);
				AssertEquals("JE_ShipmentIncoTerm", ZString.Empty, declaration.JE_ShipmentIncoTerm);
				AssertEquals("JE_ShipmentIncoTermPlace", ZString.Empty, declaration.JE_ShipmentIncoTermPlace);

				AssertEquals("JE_GoodsOrigin", ZString.Empty, declaration.JE_GoodsOrigin);
				AssertEquals("JE_GoodsDestination", ZString.Empty, declaration.JE_GoodsDestination);
				AssertEquals("JE_LocationOfGoods", ZString.Empty, declaration.JE_LocationOfGoods);
				AssertEquals("JE_TotalNoOfPacksPackType", ZString.Empty, declaration.JE_TotalNoOfPacksPackType);
				AssertEquals("ZG_AgreedPlaceCode", ZString.Empty, declaration.ZG_AgreedPlaceCode);

				AssertEquals("JE_UCR", ZString.Empty, declaration.JE_UCR);
				AssertEquals("JE_CustomsOffice", ZString.Empty, declaration.JE_CustomsOffice);

				AssertEquals("JE_OA_Representative", ZGuid.Empty, declaration.JE_OA_Representative);
				AssertEquals("JE_OA_Representative_ZAddress.OrgPK", ZGuid.Empty, declaration.JE_OA_Representative_ZAddress.OrgPK);
				AssertEquals("RepresentativeDocAddress", ZGuid.Empty, declaration.RepresentativeDocAddress.OrganisationPK);
				AssertEquals("DefermentPartyDocAddress", ZGuid.Empty, declaration.DefermentPartyDocAddress.OrganisationPK);
			});
		}

		public void TestReadOnlyFields_InwardProcessing_AVABR()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertEquals(true, declaration.SupplierDocumentaryAddress.ReadOnly);
			AssertEquals(true, declaration.JE_DeclarantTypeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TransportModeInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_MethodOfPaymentInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_BorderTransportMeansInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ContainerModeInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_CTStatusIDInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_IsHighValueOvrdInfo.ReadOnly);
			AssertEquals(true, declaration.JE_VesselNameInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKPortOfFirstArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ExportDateInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateOfFirstArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateOfArrivalInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_Box18TransportIDInfo.ReadOnly);
			AssertEquals(true, declaration.ZG_Box18TransportNationalityInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TransportModeInlandInfo.ReadOnly);
			AssertEquals(true, declaration.JE_HouseBillInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateAtOriginInfo.ReadOnly);
			AssertEquals(true, declaration.JE_DateAtFinalDestinationInfo.ReadOnly);
			AssertEquals(true, declaration.JE_GoodsDescriptionInfo.ReadOnly);
			AssertEquals(true, declaration.JE_OwnerRefInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalNoOfPacksInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalWeightInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalWeightUnitInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalVolumeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_TotalVolumeUnitInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ShipmentIncoTermInfo.ReadOnly);
			AssertEquals(true, declaration.JE_ShipmentIncoTermPlaceInfo.ReadOnly);
			AssertEquals(true, declaration.JE_UCRInfo.ReadOnly);
			AssertEquals(true, declaration.JE_CustomsOfficeInfo.ReadOnly);
			AssertEquals(true, declaration.JE_OA_RepresentativeInfo.ReadOnly);
			AssertEquals(true, declaration.DefermentPartyDocAddress.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
