using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestHeaderValidationTest : ExportCustomsManifestValidationAbstractTest
	{
		public void TestValidateED_OA_CTOAddress()
		{
			SetupSeaDEP();
			header.Validation.ValidateAll();
			AssertHasErrors(header.ED_OA_CTOAddressInfo);
			OrgAddress address = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress;
			address.LocalControlledPremisesID = "9122P";
			header.ED_OA_CTOAddress = address.PK;
			AssertNoNotifications(header.ED_OA_CTOAddressInfo);
			address.LocalControlledPremisesID = ZString.Empty;
			header.Validation.ValidateAll();
			AssertHasMessageError(header.ED_OA_CTOAddressInfo, "This address is missing a Premise ID (CCP). Please enter one on the Organisation's Config tab.");
			header.ED_OA_CTOAddress = ZGuid.Empty;
			AssertHasErrors(header.ED_OA_CTOAddressInfo);
			SetupSeaEMM();
			header.Validation.ValidateAll();
			AssertNoNotifications(header.ED_OA_CTOAddressInfo);
		}

		public void TestValidateED_TransportMode()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Assert("NoNotifications", !header.ED_TransportModeInfo.HasNotifications());
			header.ED_TransportMode = "XXX";
			Assert("MessageError", header.ED_TransportModeInfo.HasMessageErrors());
		}

		public void TestValidateED_DepartureDate()
		{
			SetupAirESM();
			header.ED_DepartureDate = ZDateTime.Today;
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(1);
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-1);
			Assert("MessageErrors", header.ED_DepartureDateInfo.HasMessageErrors());
			header.ED_DepartureDate = ZDateTime.Empty;
			Assert("MessageErrors", header.ED_DepartureDateInfo.HasMessageErrors());

			SetupSeaEMM();
			header.ED_DepartureDate = ZDateTime.Today;
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(1);
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-1);
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-3);
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-4);
			Assert("MessageErrors", header.ED_DepartureDateInfo.HasMessageErrors());
			header.ED_DepartureDate = ZDateTime.Empty;
			Assert("MessageErrors", header.ED_DepartureDateInfo.HasMessageErrors());
		}

		public void TestValidateED_DepartureDateForSlot()
		{
			SetupAirESM();
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header.ED_DepartureDate = ZDateTime.Today;
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(1);
			Assert("NoNotifications", !header.ED_DepartureDateInfo.HasNotifications());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-1);
			Assert("MessageErrors", !header.ED_DepartureDateInfo.HasMessageErrors());
			header.ED_DepartureDate = ZDateTime.Today.AddDays(-4);
			Assert("MessageErrors", header.ED_DepartureDateInfo.HasMessageErrors());
		}

		public void TestValidateED_NoOfPacks()
		{
			header.ED_NoOfPacks = -1;
			Assert("MessageErrors", header.ED_NoOfPacksInfo.HasMessageErrors());
			SetupAirESM();
			header.ED_NoOfPacks = 0;
			Assert("MessageErrors", header.ED_NoOfPacksInfo.HasMessageErrors());
			line.EL_NumberOfPackages = 1;
			header.ED_NoOfPacks = 1;
			Assert("NoNotifications", !header.ED_NoOfPacksInfo.HasNotifications());

			SetupSeaESM();
			line.EL_NumberOfPackages = 0;

			header.ED_NoOfContainer = 0;
			header.ED_NoOfPacks = 0;
			Assert("MessageErrors", header.ED_NoOfPacksInfo.HasMessageErrors());
			header.ED_NoOfContainer = 1;
			header.ED_NoOfPacks = 0;
			Assert("NoNotifications", !header.ED_NoOfPacksInfo.HasNotifications());

			SetupAirEMM();
			header.ED_NoOfPacks = 0;
			Assert("NoNotifications", !header.ED_NoOfPacksInfo.HasNotifications());

			SetupSeaEMM();
			header.ED_NoOfContainer = 0;
			header.ED_NoOfPacks = 0;
			Assert("NoNotifications", !header.ED_NoOfPacksInfo.HasNotifications());

			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.Validation.ValidateAll();
			AssertNoNotifications(header.ED_NoOfPacksInfo);
		}

		public void TestValidateED_NoOfPacksNotValidatedForDeparture()
		{
			SetupSeaDEP();
			header.ED_NoOfPacks = -1;
			AssertNoNotifications(header.ED_NoOfPacksInfo);
		}

		public void TestValidateED_NoOfContainers()
		{
			header.ED_NoOfContainer = -1;
			Assert("MessageErrors", header.ED_NoOfContainerInfo.HasMessageErrors());
			SetupAirESM();
			header.ED_NoOfPacks = 1;
			header.ED_NoOfContainer = 0;
			Assert("NoNotifications", !header.ED_NoOfContainerInfo.HasNotifications());
			header.ED_NoOfContainer = 1;
			Assert("MessageErrors", header.ED_NoOfContainerInfo.HasMessageErrors());

			SetupSeaESM();
			header.ED_NoOfPacks = 0;
			header.ED_NoOfContainer = 0;
			Assert("MessageErrors", header.ED_NoOfContainerInfo.HasMessageErrors());
			header.ED_NoOfPacks = 1;
			header.ED_NoOfContainer = 0;
			Assert("NoNotifications", !header.ED_NoOfContainerInfo.HasNotifications());

			SetupAirEMM();
			header.ED_NoOfContainer = 0;
			Assert("NoNotifications", !header.ED_NoOfContainerInfo.HasNotifications());
			header.ED_NoOfContainer = 1;
			Assert("MessageErrors", header.ED_NoOfContainerInfo.HasMessageErrors());

			SetupSeaEMM();
			header.ED_NoOfPacks = 0;
			header.ED_NoOfContainer = 0;
			Assert("NoNotifications", !header.ED_NoOfContainerInfo.HasNotifications());
		}

		public void TestValidateED_NoOfContainersNotValidatedForDeparture()
		{
			SetupSeaDEP();
			header.ED_NoOfContainer = -1;
			AssertNoNotifications(header.ED_NoOfContainerInfo);
		}

		public void TestValidateED_NoOfEmptyContainers()
		{
			header.ED_NoOfEmptyContainers = -1;
			Assert("MessageErrors", header.ED_NoOfEmptyContainersInfo.HasMessageErrors());
			SetupAirESM();
			header.ED_NoOfEmptyContainers = 1;
			Assert("MessageErrors", header.ED_NoOfEmptyContainersInfo.HasMessageErrors());

			SetupSeaESM();
			header.ED_NoOfContainer = 1;
			header.ED_NoOfEmptyContainers = 1;
			Assert("MessageErrors", header.ED_NoOfEmptyContainersInfo.HasMessageErrors());

			SetupSeaSlotESM();
			header.ED_NoOfContainer = 1;
			header.ED_NoOfEmptyContainers = 1;
			Assert("NoNotifications", !header.ED_NoOfEmptyContainersInfo.HasNotifications());
			header.ED_NoOfEmptyContainers = 2;
			Assert("NoNotifications", !header.ED_NoOfEmptyContainersInfo.HasMessageErrors());

			SetupAirEMM();
			header.ED_NoOfContainer = 0;
			header.ED_NoOfEmptyContainers = 1;
			Assert("MessageErrors", header.ED_NoOfEmptyContainersInfo.HasMessageErrors());

			SetupSeaEMM();
			header.ED_NoOfContainer = 1;
			header.ED_NoOfEmptyContainers = 1;
			Assert("NoNotifications", !header.ED_NoOfEmptyContainersInfo.HasNotifications());
		}

		public void TestValidateED_NoOfEmptyContainersNotValidatedForDeparture()
		{
			SetupSeaDEP();
			header.ED_NoOfEmptyContainers = -1;
			AssertNoNotifications(header.ED_NoOfEmptyContainersInfo);
		}

		public void TestValidateED_ManifestType()
		{
			header.ED_ManifestType = "XXX";
			Assert("Error", header.ED_ManifestTypeInfo.HasErrors());
			header.ED_ManifestType = ZString.Empty;
			Assert("Error", header.ED_ManifestTypeInfo.HasErrors());

			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Assert("NoNotifications", !header.ED_ManifestTypeInfo.HasNotifications());

			OrgAddress address = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress;
			address.LocalControlledPremisesID = "9122P";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "1234567";

			ExportCustomsManifestHeader dupHeader1 = Factory.New<ExportCustomsManifestHeader>();
			dupHeader1.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			dupHeader1.ED_BGMReference = "K1";
			dupHeader1.ED_TransportMode = Core.Constants.TransportModes.Sea;
			dupHeader1.ED_OA_CTOAddress = address.PK;
			dupHeader1.ED_DepartureDate = new ZDateTime(2009, 9, 1, 12, 1, 2);
			dupHeader1.ED_VesselName = vessel.RV_Code;
			dupHeader1.ED_VoyageNumber = "23";

			ExportCustomsManifestHeader dupHeader2 = Factory.New<ExportCustomsManifestHeader>();
			dupHeader2.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			dupHeader2.ED_BGMReference = "K2";
			dupHeader2.ED_TransportMode = Core.Constants.TransportModes.Air;
			dupHeader2.ED_OA_CTOAddress = address.PK;
			dupHeader2.ED_DepartureDate = new ZDateTime(2009, 9, 1, 12, 1, 3);
			dupHeader2.ED_FlightNumber = "QF123";

			header.ED_BGMReference = "K3";
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_OA_CTOAddress = address.PK;
			header.ED_DepartureDate = new ZDateTime(2009, 9, 1, 11, 1, 4);
			header.ED_VesselName = vessel.RV_Code;
			header.ED_VoyageNumber = "23";
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report, the same CTO, Departure Date and Transport details are recorded on K1");
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report, the same CTO, Departure Date and Transport details are recorded on K1");
			header.ED_OA_CTOAddress = ZGuid.Empty;
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_OA_CTOAddress = address.PK;
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_DepartureDate = new ZDateTime(2009, 9, 2, 11, 1, 4);
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_DepartureDate = new ZDateTime(2009, 9, 1, 14, 1, 4);
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_VesselName = ZString.Empty;
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_VesselName = vessel.RV_Code;
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_VoyageNumber = "22";
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_VoyageNumber = "23";
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");

			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report");
			header.ED_VesselName = ZString.Empty;
			header.ED_VoyageNumber = ZString.Empty;
			header.ED_FlightNumber = "QF123";
			AssertHasErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report, the same CTO, Departure Date and Transport details are recorded on K2");
			header.ED_FlightNumber = "QF124";
			AssertNoErrorContaining(header.ED_ManifestTypeInfo, "This is a duplicate departure report, the same CTO, Departure Date and Transport details are recorded on K2");
		}

		public void TestValidateED_FlightNumber()
		{
			SetupAirEMM();
			header.ED_FlightNumber = "QF123";
			Assert("NoNotifications", !header.ED_FlightNumberInfo.HasNotifications());
			header.ED_FlightNumber = ZString.Empty;
			Assert("MessageErrors", header.ED_FlightNumberInfo.HasMessageErrors());
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header.Validation.ValidateAll();
			AssertNoNotifications(header.ED_FlightNumberInfo);
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.Validation.ValidateAll();
			AssertNoNotifications(header.ED_FlightNumberInfo);
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			header.Validation.ValidateAll();
			AssertHasMessageErrors(header.ED_FlightNumberInfo);
			header.ED_FlightNumber = "QF394";
			AssertNoNotifications(header.ED_FlightNumberInfo);
		}

		public void TestValidateED_VesselName()
		{
			SetupSeaEMM();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "1234567";
			header.ED_VesselName = vessel.RV_Code;
			Assert("NoMessageErrors", !header.ED_VesselNameInfo.HasMessageErrors());
			vessel.RV_LloydsNumber = ZString.Empty;
			header.ED_VesselName = vessel.RV_Code;
			Assert("MessageErrors", header.ED_VesselNameInfo.HasMessageErrors());
			header.ED_VesselName = ZString.Empty;
			Assert("MessageErrors", header.ED_VesselNameInfo.HasMessageErrors());

			vessel.RV_LloydsNumber = "1234567";
			header.ED_VesselName = vessel.RV_Code;
			Assert("NoMessageErrors", !header.ED_VesselNameInfo.HasMessageErrors());

			ExportCustomsManifestHeader otherHeader = Factory.New<ExportCustomsManifestHeader>();
			otherHeader.ED_VesselName = vessel.RV_Code;
			otherHeader.ED_VoyageNumber = "123";
			otherHeader.ED_RL_NKPortOfDeparture = "AUSYD";
			otherHeader.ED_RN_NKCountryOfDestination = "US";
			otherHeader.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_VoyageNumber = "123";
			header.ED_RL_NKPortOfDeparture = "AUSYD";
			header.ED_RN_NKCountryOfDestination = "US";
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertHasErrorContaining(header.ED_VesselNameInfo, "An Export Main Manifest with the same Vessel, Voyage, Departure Port and Destination Country/Region has already been registered.");
			header.ED_RN_NKCountryOfDestination = "GB";
			AssertNoErrorContaining(header.ED_VesselNameInfo, "An Export Main Manifest with the same Vessel, Voyage, Departure Port and Destination Country/Region has already been registered.");
		}

		public void TestValidateED_VesselNameForDeparture()
		{
			SetupSeaDEP();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "1234567";
			header.ED_VesselName = vessel.RV_Code;
			Assert("NoMessageErrors", !header.ED_VesselNameInfo.HasMessageErrors());
			vessel.RV_LloydsNumber = ZString.Empty;
			header.ED_VesselName = vessel.RV_Code;
			Assert("MessageErrors", header.ED_VesselNameInfo.HasMessageErrors());
			header.ED_VesselName = ZString.Empty;
			Assert("MessageErrors", header.ED_VesselNameInfo.HasMessageErrors());
		}

		public void TestValidateED_VoyageNumber()
		{
			SetupSeaEMM();
			header.ED_VoyageNumber = "123";
			Assert("NoMessageErrors", !header.ED_VoyageNumberInfo.HasMessageErrors());
			header.ED_VoyageNumber = ZString.Empty;
			Assert("MessageErrors", header.ED_VoyageNumberInfo.HasMessageErrors());
		}

		public void TestValidateED_VoyageNumberForDeparture()
		{
			SetupSeaDEP();
			header.ED_VoyageNumber = "123";
			Assert("NoMessageErrors", !header.ED_VoyageNumberInfo.HasMessageErrors());
			header.ED_VoyageNumber = ZString.Empty;
			Assert("MessageErrors", header.ED_VoyageNumberInfo.HasMessageErrors());
		}

		public void TestValidateED_RL_NKPortOfDeparture()
		{
			SetupSeaEMM();
			header.ED_RL_NKPortOfDeparture = ZString.Empty;
			Assert("MessageErrors", header.ED_RL_NKPortOfDepartureInfo.HasMessageErrors());
			header.ED_RL_NKPortOfDeparture = "AUSYD";
			Assert("NoMessageErrors", !header.ED_RL_NKPortOfDepartureInfo.HasMessageErrors());
			header.ED_RL_NKPortOfDeparture = "NZAKL";
			Assert("MessageErrors", header.ED_RL_NKPortOfDepartureInfo.HasMessageErrors());
			header.ED_RL_NKPortOfDeparture = "ZZZZZ";
			Assert("MessageErrors", header.ED_RL_NKPortOfDepartureInfo.HasMessageErrors());

			ZString expected = "Customs Export Manifest for 'Test Vessel'-'123'-'AUSYD' already exists with Folio References: 'Test Ref 1', 'Test Ref 2'.\r\nWe recommend to use a single manifest for this 'Test Vessel'-'123'-'AUSYD'";

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Test Vessel";

			ExportCustomsManifestHeader header1 = Factory.New<ExportCustomsManifestHeader>();
			header1.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header1.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header1.ED_VesselName = vessel.RV_Code;
			header1.ED_VoyageNumber = "123";
			header1.ED_FolioReference = "Test Ref 1";
			header1.ED_RL_NKPortOfDeparture = "AUSYD";

			ExportCustomsManifestHeader header2 = Factory.New<ExportCustomsManifestHeader>();
			header2.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header2.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header2.ED_VesselName = vessel.RV_Code;
			header2.ED_VoyageNumber = "123";
			header2.ED_FolioReference = "Test Ref 2";
			header2.ED_RL_NKPortOfDeparture = "AUSYD";

			ExportCustomsManifestHeader header3 = Factory.New<ExportCustomsManifestHeader>();
			header3.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header3.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			header3.ED_VesselName = vessel.RV_Code;
			header3.ED_VoyageNumber = "123";
			header3.ED_FolioReference = "Test Ref 3";
			header3.ED_RL_NKPortOfDeparture = "AUSYD";

			AssertHasWarning(header3.ED_RL_NKPortOfDepartureInfo, expected);
			header3.ED_RL_NKPortOfDeparture = "";
			AssertNoNotifications(header3.ED_RL_NKPortOfDepartureInfo);
		}

		public void TestValidateED_RN_NKCountryOfDestination()
		{
			SetupSeaEMM();
			header.ED_RN_NKCountryOfDestination = ZString.Empty;
			Assert("MessageErrors", header.ED_RN_NKCountryOfDestinationInfo.HasMessageErrors());
			header.ED_RN_NKCountryOfDestination = "NZ";
			Assert("NoMessageErrors", !header.ED_RN_NKCountryOfDestinationInfo.HasMessageErrors());
			header.ED_RN_NKCountryOfDestination = "ZZ";
			Assert("MessageErrors", header.ED_RN_NKCountryOfDestinationInfo.HasMessageErrors());
		}

		public void TestValidateED_RL_NKPortOfDestination()
		{
			SetupSeaDEP();
			header.ED_RL_NKPortOfDestination = "AUSYD";
			Assert("NoMessageErrors", !header.ED_RL_NKPortOfDestinationInfo.HasMessageErrors());
			header.ED_RL_NKPortOfDestination = ZString.Empty;
			Assert("MessageErrors", header.ED_RL_NKPortOfDestinationInfo.HasMessageErrors());
			header.ED_RL_NKPortOfDestination = "NZAKL";
			Assert("NoMessageErrors", !header.ED_RL_NKPortOfDestinationInfo.HasMessageErrors());
		}

		public void TestSlotOnlyAllowedForSeaManifests()
		{
			SetupSeaSlotESM();
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Assert("MessageErrors", header.ED_TransportModeInfo.HasMessageErrors());
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("!MessageErrors", !header.ED_TransportModeInfo.HasMessageErrors());
		}

		public void TestValidateContainersIfWeChangePacks()
		{
			SetupSeaESM();
			header.ED_NoOfPacks = 0;
			header.ED_NoOfContainer = 0;
			Assert("ContainerMessageErrors", header.ED_NoOfContainerInfo.HasMessageErrors());
			header.ED_NoOfPacks = 1;
			Assert("NoContainerMessageErrors", !header.ED_NoOfContainerInfo.HasMessageErrors());
		}

		public void TestValidatePacksIfWeChangeContainers()
		{
			SetupSeaESM();
			header.ED_NoOfContainer = 0;
			header.ED_NoOfPacks = 0;
			Assert("ED_NoOfPacksMessageErrors", header.ED_NoOfPacksInfo.HasMessageErrors());
			header.ED_NoOfContainer = 1;
			Assert("NoED_NoOfPacksMessageErrors", !header.ED_NoOfPacksInfo.HasMessageErrors());
		}

		public void TestNoMessageErrorsForZeroContainersForAir()
		{
			SetupAirESM();
			header.ED_NoOfContainer = 0;
			header.ED_NoOfPacks = 0;
			Assert("NoED_NoOfContainerMessageErrors", !header.ED_NoOfContainerInfo.HasMessageErrors());
		}

		public void TestLineContainersAddsUpToHeaderContainers()
		{
			SetupSeaEMM();
			SetupCANLine();
			line.EL_NumberOfContainers = 5;
			header.ED_NoOfEmptyContainers = 2;
			header.ED_NoOfContainer = 1;
			Assert("CountainerMessageError", header.ED_NoOfContainerInfo.HasMessageErrors());
			header.ED_NoOfContainer = 5;
			Assert("NoCountainerMessageErrors", !header.ED_NoOfContainerInfo.HasMessageErrors());
		}

		public void TestLinePackagesAddsUpToHeaderPackages()
		{
			SetupSeaESM();
			SetupCANLine();
			line.EL_NumberOfPackages = 5;
			header.ED_NoOfPacks = 2;
			Assert("PacksMessageError", header.ED_NoOfPacksInfo.HasMessageErrors());
			header.ED_NoOfPacks = 5;
			Assert("NoPacksMessageError", !header.ED_NoOfPacksInfo.HasMessageErrors());
		}

		public void TestDepartureDateIsMandatoryForCTO()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			header.ED_TransportMode = Core.Constants.TransportCodes.Air;

			header.ED_DepartureDate = ZDateTime.Now;
			AssertNoNotifications(header.ED_DepartureDateInfo);

			header.ED_DepartureDate = ZDateTime.Empty;
			AssertHasError(header.ED_DepartureDateInfo, "Please enter a Date of Departure.");
		}

		public void TestDepartureDateIsMandatoryForAirCTOConsolidationESM()
		{
			AirCTOExportCustomsManifestHeader header = Factory.New<AirCTOExportCustomsManifestHeader>();
			header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportCodes.Air;

			header.ED_DepartureDate = ZDateTime.Now;
			AssertNoNotifications(header.ED_DepartureDateInfo);

			header.ED_DepartureDate = ZDateTime.Empty;
			AssertHasMessageError(header.ED_DepartureDateInfo, "Please enter a Date of Departure.");
		}

		public void TestCTOAddressRequiredOnESMWhenParentIsAirCTOHeader()
		{
			AirCTOExportCustomsManifestHeader header = Factory.New<AirCTOExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_OA_CTOAddress = ZGuid.Empty;
			AssertHasErrorContaining(header.ED_OA_CTOAddressInfo, "Please enter a CTO Address");
			header.ED_OA_CTOAddress = ZGuid.NewZGuid();
			AssertNoNotifications(header.ED_OA_CTOAddressInfo);
		}

		public void TestCTOAddressRequiredOnEMMWhenParentIsAirCTOHeader()
		{
			AirCTOExportCustomsManifestHeader header = Factory.New<AirCTOExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_OA_CTOAddress = ZGuid.Empty;
			AssertHasErrorContaining(header.ED_OA_CTOAddressInfo, "Please enter a CTO Address");
			header.ED_OA_CTOAddress = ZGuid.NewZGuid();
			AssertNoNotifications(header.ED_OA_CTOAddressInfo);
		}
	}
}
