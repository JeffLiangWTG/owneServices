using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class DeclarationReportTestHelper
	{
		public static List<string> GetParametersValuesListExport(string applicationCode = null)
		{
			return new List<string>
			{
				$"'{GlbCompany.CurrentCompany.PK}'", // 0 @CurrentCompany
				"null", // 1 @TransportMode
				"null", // 2 @Origin
				"null", // 3 @Destination
				"null", // 4 @Loading 
				"null", // 5 @Arrival
				ReportFunctionalTestCase.QuoteParameter(ZDateTime.BrettsBirthday.ToISO8601String()), // 6 @CreateDateFrom
				ReportFunctionalTestCase.QuoteParameter(ZDateTime.Today.AddDays(1).ToISO8601String()), // 7 @CreateDateTo
				"null", // 8  @ImporterPk
				"null", // 9  @SupplierPk
				"null", // 10 BranchPk
				"null", // 11 @DepartureDateFrom
				"null", // 12 @DepartureDateTo AS smalldatetime,
				"null", // 13 @ArrivalDateFrom AS smalldatetime,
				"null", // 14 @ArrivalDateTo AS smalldatetime,
				"null", // 15 @Route
				"null", // 16 @DeclarationType
				"null", // 17 @Badge
				"null", // 18 @Csp
				"null", // 19 @DeclarantType
				applicationCode == null ? "null" : $"'{applicationCode}'", // 20 @ApplicationCode
				"default", // 21 @MessageType
				"null" // 22 @CdsLocationOfGoods
			};
		}

		public static List<string> GetParametersValuesListImport(string applicationCode = null)
		{
			var shared = GetParametersValuesListExport(applicationCode);

			shared.AddRange(new List<string>
			{
				"null", // 23 @LocationOfGoods
				"null", // 24 @Shed
				"null", // 25 @IRC
				"null", // 26 @FirstDAN
				"null", // 27 @SecondDAN
				"null", // 28 @FirstDANType
				"null", // 29 @SecondDANType
			});
			return shared;
		}

		public static void MakeDeclaration(BusinessObjectFactory factory, out JobDeclaration declaration, bool isShipmentLinked = false)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "B", "11", "11", "111", "One", "EXP", group: "EFD");

			if (Shed.LoadByCode(factory, Core.Constants.CountryCodes.UnitedKingdom, "LHRCAX") == null)
			{
				ShedTest.CreateShed(factory, Core.Constants.CountryCodes.UnitedKingdom, "LHRCAX", "CargoWise", acpCode: "H", isDEP: false, isETSF: true, chiefShed: "ERT", portName: "Heathrow");
			}

			factory.Save();

			declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			if (isShipmentLinked)
			{
				var shipment = factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = true;
			}
			declaration.JE_OH_Supplier = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS")).PK;
			declaration.JE_OH_Importer = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BACCRI")).PK;
			declaration.SupplierPickupAddress.E2_OA_Address = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "PACAKL")).MainAddress.PK;
			declaration.JE_MessageType = "EXP";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "5GB1234-B1234";
			entry2.CH_BGMReference = "5GB1234-B1234/1";
			entry1.EntryNumber = "071-111111A";
			entry2.EntryNumber = "071-222222A";
			entry1.LRN = "LRN1";
			entry2.LRN = "LRN2";

			entry1.CH_EntryStatus = "ABC";
			entry1.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday.AddDays(-4);
			entry1.CH_ImportClearanceStatusICS = "01";
			entry1.CH_RouteOfEntry = "6";
			entry1.CH_StyleOfEntrySOE = "7";
			entry2.CH_EntryStatus = "ABC";
			entry2.CusEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday.AddDays(-3);
			entry2.CH_ImportClearanceStatusICS = "01";
			entry2.CH_RouteOfEntry = "6";
			entry2.CH_StyleOfEntrySOE = "7";
			declaration.JE_MasterUCR = "A:12345678900";
			declaration.JE_RL_NKPortOfLoading = "GBLHR";
			declaration.JE_RL_NKPortOfArrival = "USATL";
			declaration.JE_RL_NKOrigin = "GBMIK";
			declaration.JE_RL_NKFinalDestination = "USMSP";
			declaration.JE_CustomsProfile = "DJC";
			declaration.ZG_Gateway = "CCSUK";
			declaration.ZG_ShipmentType = "BAS";
			declaration.JE_OA_DeclarantAddress = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "COUNTR")).MainAddress.PK;
			declaration.JE_DeclarantType = "DIR";
			declaration.JE_RN_NKTransportNationality = "AU";
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_TransportModeInland = "RAI";
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "CAX";
			declaration.SupervisingOfficeDocAddress.E2_OA_Address = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DAVJDI")).MainAddress.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "EASKUL")).MainAddress.PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.FindOrAddNew("CCP", "GB123456A", "GB");
			declaration.JE_UCR = "5GB123456798000-B00010";
			declaration.JE_OH_ShippingLine = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FAICAE")).PK;
			declaration.JE_OH_Forwarder = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "INTMOT")).PK;
			declaration.JE_ContainerMode = "ULD";
			declaration.JE_GS_NKCusAgent = "E";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "DANU1234567";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "MSCU7654321";
			declaration.ZG_CTStatusID = "X";
			var gatcon = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GATCON"));
			gatcon.OH_IsShippingProvider = true;
			gatcon.OH_IsLocalTransport = true;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = gatcon.MainAddress.PK;
			declaration.DepotDocAddress.E2_OA_Address = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "HARFRE")).MainAddress.PK;
			declaration.JE_GoodsDescription = "Ducks in a line";
			declaration.JE_HouseBill = "12345678";

			declaration.JE_EntryStatus = "ABC";
			declaration.JE_MasterBill = "12587654321";
			declaration.JE_CustomsOffice = "NL012345";
			declaration.JE_GBRouteOfEntry = "6";
			declaration.JE_RouteFRequested = true;
			declaration.JE_RS_NKServiceLevel = "STD";
			declaration.ZG_StyleOfEntrySOE = "7";
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_VoyageFlightNo = "BA123";
			declaration.JE_DateAtOrigin = new ZDateTime(2015, 08, 22, 14, 00, 0); // ETD
			declaration.JE_DateAtFinalDestination = new ZDateTime(1979, 08, 09, 09, 56, 0); // ETA
			declaration.JE_OwnerRef = "Daniel";
			declaration.JE_TotalNoOfPacks = 6;
			declaration.JE_TotalWeight = 35m;
			declaration.JE_TotalVolume = 36m;
			declaration.ZG_LCPInspect = new ZDateTime(1979, 08, 09, 09, 57, 0);
			declaration.ZG_LCPDepart = new ZDateTime(1979, 08, 09, 09, 58, 0);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(1979, 08, 09, 09, 59, 0);
			declaration.JE_MessageSubType = "EX ";
		}
	}
}
