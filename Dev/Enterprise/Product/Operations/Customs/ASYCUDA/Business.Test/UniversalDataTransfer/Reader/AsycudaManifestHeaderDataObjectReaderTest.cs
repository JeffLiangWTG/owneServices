using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaManifestHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingAsycudaManifestData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var carrierCode = "OTD1";
			var billIssuerCode = "BIC1";

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM");
			headerDataObject.MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.Consolidator };

			var headerEntryHeader = help.SetupCountryHeaderEntryHeader("ZA", 1);
			var headerEntryInstruction = help.SetupCountryHeaderEntryInstruction(1, portOfFirstArrival, carrierCode, "ZA", "IAM", "NT1");

			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = help.SetupCountryBillEntryHeader("ZA", 1, "CLR", "Sender Reference1");

			var billCountryEntryInstruction = help.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", "ZA", billIssuerCode, 200.01, 300.01, "Q", "A", "TEST", ZDateTime.Empty, ZString.Empty);
			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);

			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.AddSafe(headerEntryHeader);

			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.AddSafe(headerEntryInstruction);

			bill.SetEntryHeaderCollection(() => new List<EntryHeader>());
			bill.EntryHeaderCollection.AddSafe(billCountryEntryHeader);

			bill.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			bill.EntryInstructionCollection.AddSafe(billCountryEntryInstruction);

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(bill);
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			headerDataObject.ContainerCollection.Add(container);

			var deConsolidatorOrganizationAddress = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsContainerYardAddress);
			var deConsolidator = new OrganisationDataObjectReader(deConsolidatorOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			deConsolidator.OA_Address1 = "1 ATLAS ROAD";
			deConsolidator.OA_PostCode = "1619";
			deConsolidator.OA_Address2 = "JOHANNESBURG INTERNATIONAL AIRPORT";
			deConsolidator.OA_City = "KEMPTON PARK";
			deConsolidator.CompanyName = "ZA DECONSOLIDATOR";

			var dischargeTerminalOrganizationAddress = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsContainerTerminalOperatorAddress);
			var dischargeTerminal = new OrganisationDataObjectReader(dischargeTerminalOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			dischargeTerminal.OA_Address1 = "1 ATLAS ROAD";
			dischargeTerminal.OA_PostCode = "1619";
			dischargeTerminal.OA_City = "KEMPTON PARK";
			dischargeTerminal.CompanyName = "ZA TERMINAL";
			headerDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			headerDataObject.OrganizationAddressCollection.AddSafe(deConsolidatorOrganizationAddress);
			headerDataObject.OrganizationAddressCollection.AddSafe(dischargeTerminalOrganizationAddress);
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);

			AssertNotNull(headerBO);
			#region Check Contents of header Business Object
			//header
			AssertEquals("headerBO.AMA_ApplicationCode", ApplicationCodeTypeList.Codes.Consolidator, headerBO.AMA_ApplicationCode);
			AssertEquals("headerBO.AMA_MasterBill", "MAN00001", headerBO.AMA_MasterBill);
			AssertEquals("headerBO.AMA_TransportMode", TransportTypeList.Codes.Air, headerBO.AMA_TransportMode);
			AssertEquals("headerBO.AMA_Voyage", "QTX370", headerBO.AMA_Voyage);
			AssertEquals("headerBO.AMA_RL_NKPortOfLoading", airLocalPort1.RL_Code, headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("headerBO.AMA_RL_NKPortOfDischarge", airLocalPort2.RL_Code, headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("headerBO.AMA_VesselName", "TestVessel", headerBO.AMA_VesselName);
			AssertEquals("headerBO.AMA_RN_NKConveyanceNationality", "XX", headerBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("headerBO.AMA_MasterInformation", "Captain Kirk", headerBO.AMA_MasterInformation);
			AssertEquals("headerBO.AMA_E_DEP", departureTime, headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", arrivalTime, headerBO.AMA_E_ARV);
			AssertEquals("headerBO..AMA_MasterBillIssueDate", ZDateTime.Today.AddDays(4), headerBO.AMA_MasterBillIssueDate);
			AssertEquals("headerBO.AMA_OverrideFreightDefaults", false, headerBO.AMA_OverrideFreightDefaults);
			AssertEquals("headerBO.AMA_ManifestType", "IAM", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", carrierCode, headerBO.AMA_CarrierCode);
			AssertEquals("headerBO.AMA_Trailer1RegNo", "TRAILER001", headerBO.AMA_Trailer1RegNo);
			AssertEquals("headerBO.AMA_Trailer2RegNo", "TRAILER002", headerBO.AMA_Trailer2RegNo);
			AssertEquals("headerBO.AMA_RN_NKTrailer1RegCountry", "TR", headerBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("headerBO.AMA_RN_NKTrailer2RegCountry", "ZA", headerBO.AMA_RN_NKTrailer2RegCountry);
			AssertEquals("headerBO.AMA_AgentType", Core.Constants.AgentType.Agent, headerBO.AMA_AgentType);
			AssertEquals("headerBO.AMA_RadioCallSign", "9064384", headerBO.AMA_RadioCallSign);
			AssertEquals("headerBO.AMA_ContainerMode", "CNT", headerBO.AMA_ContainerMode);
			AssertEquals("headerBO.AMA_IsBuyersConsolidation", ZBool.True, headerBO.AMA_IsBuyersConsolidation);
			AssertEquals("DE-Consolidator Address", deConsolidator.PK, headerBO.AMA_OA_DeconsolidateAddress);
			AssertEquals("Discharge Terminal Address", dischargeTerminal.PK, headerBO.AMA_OA_DischargeTerminalAddress);

			//bill
			AssertEquals("headerBO has 1 bill", 1, headerBO.Bills.Count);
			var billBO = headerBO.Bills[0];
			AssertEquals("billBO.ABL_RL_NKOrigin", airLocalPort1.RL_Code, billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", airLocalPort2.RL_Code, billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 300m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 3m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_CarrierReference", "Carrier Reference", billBO.ABL_CarrierReference);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType);
			AssertEquals("billBO.ABL_PrepaidCollect", "PRE", billBO.ABL_PrepaidCollect);
			AssertEquals("billBO.ABL_BillStatus", "", billBO.ABL_BillStatus);
			AssertEquals("billBO.ABL_SenderReference", "", billBO.ABL_SenderReference);
			AssertEquals("billBO.ABL_LocationInformation", "Location Information1", billBO.ABL_LocationInformation);
			AssertEquals("billBO.ABL_GoodsLocation", "GOODS LOCATION1", billBO.ABL_GoodsLocation);
			AssertEquals("billBO.ABL_ShipmentType", "IMP", billBO.ABL_ShipmentType);
			AssertEquals("billBO.ABL_BillIssuer", billIssuerCode, billBO.ABL_BillIssuer);

			//container
			AssertEquals("headerBO has 1 container", 1, headerBO.Containers.Count);
			var containerBO = headerBO.Containers[0];
			AssertEquals("containerBO.ACN_ContainerNumber", "CONT00001", containerBO.ACN_ContainerNumber);
			AssertEquals("containerBO.ACN_Seal1", "ABC", containerBO.ACN_Seal1);
			AssertEquals("containerBO.ACN_GoodsWeight", 200m, containerBO.ACN_GoodsWeight);
			AssertEquals("containerBO.ACN_CommodityCode", "CC1", containerBO.ACN_CommodityCode);
			AssertEquals("containerBO.ACN_StowageLocation", "STO", containerBO.ACN_StowageLocation);
			AssertEquals("containerBO.ACN_EmptyFullIndicator", "F", containerBO.ACN_EmptyFullIndicator);
			AssertEquals("containerBO.ACN_SealingPartyName", "SPN", containerBO.ACN_SealingPartyName);
			AssertEquals("containerBO.ACN_SealingPartyType", "SPT", containerBO.ACN_SealingPartyType);
			AssertEquals("containerBO.ACN_NumberOfPackages", 10, containerBO.ACN_NumberOfPackages);
			#endregion
		}

		public void TestUpdateAsycudaManifestData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			var headerPK = oldHeader.PK;

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();
			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(readerHeaderBO);
			AssertEquals("Manifest Header is updated.", headerPK, readerHeaderBO.PK);
			AssertEquals("readerHeaderBO.AMA_MasterBill", "MAN00001", readerHeaderBO.AMA_MasterBill);
			AssertEquals("readerHeaderBO.AMA_TransportMode", TransportTypeList.Codes.Air, readerHeaderBO.AMA_TransportMode);
			AssertEquals("readerHeaderBO.AMA_Voyage", "QTX370", readerHeaderBO.AMA_Voyage);
			AssertEquals("readerHeaderBO.AMA_RL_NKPortOfLoading", airLocalPort1.RL_Code, readerHeaderBO.AMA_RL_NKPortOfLoading);
			AssertEquals("readerHeaderBO.AMA_RL_NKPortOfDischarge", airLocalPort2.RL_Code, readerHeaderBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("readerHeaderBO.AMA_VesselName", "TestVessel", readerHeaderBO.AMA_VesselName);
			AssertEquals("readerHeaderBO.AMA_RN_NKConveyanceNationality", "XX", readerHeaderBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("readerHeaderBO.AMA_MasterInformation", "Captain Kirk", readerHeaderBO.AMA_MasterInformation);
			AssertEquals("readerHeaderBO.AMA_E_DEP", departureTime, readerHeaderBO.AMA_E_DEP);
			AssertEquals("readerHeaderBO.AMA_E_ARV", arrivalTime, readerHeaderBO.AMA_E_ARV);
			AssertEquals("readerHeaderBO.AMA_OverrideFreightDefaults", false, readerHeaderBO.AMA_OverrideFreightDefaults);
			AssertEquals("readerHeaderBO.AMA_Trailer1RegNo", "TRAILER001", readerHeaderBO.AMA_Trailer1RegNo);
			AssertEquals("readerHeaderBO.AMA_Trailer2RegNo", "TRAILER002", readerHeaderBO.AMA_Trailer2RegNo);
			AssertEquals("readerHeaderBO.AMA_RN_NKTrailer1RegCountry", "TR", readerHeaderBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("readerHeaderBO.AMA_RN_NKTrailer2RegCountry", "ZA", readerHeaderBO.AMA_RN_NKTrailer2RegCountry);
		}

		public void TestUpdateAsycudaManifestData_FillVesselConveyanceNationality_FromVesselCountryOfRegistration()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();
			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			headerDataObject.AddInfoCollection.Remove(headerDataObject.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == AddInfoConstants.Header.ConveyanceNationality));
			headerDataObject.VesselCountryOfRegistration = new Country { Code = "YY" };
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertEquals("readerHeaderBO.AMA_RN_NKConveyanceNationality", "YY", readerHeaderBO.AMA_RN_NKConveyanceNationality);
		}

		public void TestUpdateAsycudaManifestData_FillVesselConveyanceNationality_WillNotOverrideExisting()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			oldHeader.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.China;
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();
			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			headerDataObject.AddInfoCollection.Remove(headerDataObject.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == AddInfoConstants.Header.ConveyanceNationality));
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertEquals("readerHeaderBO.AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.China, readerHeaderBO.AMA_RN_NKConveyanceNationality);
		}

		public void TestUpdateAsycudaManifestData_FillVoyageFlightNo_WhenVoyageFlightNoIsNull()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			oldHeader.AMA_TransportMode = TransportModes.Air;
			oldHeader.AMA_Voyage = "VOY001";
			Factory.SaveForTesting();

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			headerDataObject.TransportMode = new CodeDescriptionPair
			{
				Code = TransportModes.Air,
				Description = TransportModeDescriptions.Air
			};
			headerDataObject.VoyageFlightNo = null;

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertEquals("readerHeaderBO.AMA_Voyage", "VOY001", readerHeaderBO.AMA_Voyage);
		}

		public void TestUpdateAsycudaManifestData_FillVoyageFlightNo_TransportMode_Road()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			oldHeader.AMA_TransportMode = TransportModes.Road;
			oldHeader.AMA_Voyage = "VOY001";
			oldHeader.AMA_VehicleRegistration = "REG001";
			Factory.SaveForTesting();

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			headerDataObject.TransportMode = new CodeDescriptionPair
			{
				Code = TransportModes.Road,
				Description = TransportModeDescriptions.Road
			};
			headerDataObject.VoyageFlightNo = "REG002";

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertEquals("readerHeaderBO.AMA_Voyage", "VOY001", readerHeaderBO.AMA_Voyage);
			AssertEquals("readerHeaderBO.AMA_Voyage", "REG002", readerHeaderBO.AMA_VehicleRegistration);
		}

		public void TestUpdateAsycudaManifestData_FillVoyageFlightNo_TransportMode_Sea()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			oldHeader.AMA_TransportMode = TransportModes.Sea;
			oldHeader.AMA_Voyage = "VOY001";
			oldHeader.AMA_VehicleRegistration = "REG001";
			Factory.SaveForTesting();

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			headerDataObject.TransportMode = new CodeDescriptionPair
			{
				Code = TransportModes.Sea,
				Description = TransportModeDescriptions.Sea
			};
			headerDataObject.VoyageFlightNo = "VOY002";

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertEquals("readerHeaderBO.AMA_Voyage", "VOY002", readerHeaderBO.AMA_Voyage);
			AssertEquals("readerHeaderBO.AMA_Voyage", "REG001", readerHeaderBO.AMA_VehicleRegistration);
		}

		public void TestCreateNewManifestIfCountryIsDifferent()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = Factory.New<AsycudaManifestHeader>();
			var existingHeaderPK = existingHeader.PK;
			existingHeader.AMA_JobReference = "MAN001";
			existingHeader.MasterBill.ABL_BillNumber = "MST001";
			existingHeader.AMA_RN_NKCountry = "BD";

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			var carrierCode1 = "OTD1";
			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader("MST001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM");
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction1 = help.SetupCountryHeaderEntryInstruction(1, portOfFirstArrival, carrierCode1, "US", "MA1", "NT1");
			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.AddSafe(headerEntryHeader1);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.AddSafe(headerEntryInstruction1);
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertNotEquals("Header should not be updated", existingHeaderPK, headerBO.PK);
			AssertEquals("headerBO.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
			AssertEquals("headerBO.AMA_ManifestType", "MA1", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", carrierCode1, headerBO.AMA_CarrierCode);
		}

		public void TestUpdateACountryWhichHasSentMessage()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var existingHeaderPK = existingHeader.PK;
			existingHeader.AMA_JobReference = "MAN001";
			existingHeader.MasterBill.ABL_BillNumber = "MST001";
			existingHeader.AMA_MessageStatus = MessageStatusList.Codes.Sent;

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			var carrierCode1 = "OTD1";
			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader("MST001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction1 = help.SetupCountryHeaderEntryInstruction(1, portOfFirstArrival, carrierCode1, "US", "MA1", "NT1");
			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.AddSafe(headerEntryHeader1);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.AddSafe(headerEntryInstruction1);
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertEquals("Header is updated", existingHeaderPK, headerBO.PK);
			AssertEquals("headerBO.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
			AssertEquals("headerBO.AMA_ManifestType", "MA1", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", carrierCode1, headerBO.AMA_CarrierCode);
		}

		public void TestImportHeaderForMasterBill()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var existingHeaderPK = existingHeader.PK;
			existingHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			existingHeader.AMA_TransportMode = "SEA";
			var consol = Factory.New<ForwardingConsol>();
			existingHeader.SetParent(consol);
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader(null, portOfLoading, portOfDischarge, arrivalTime, departureTime, existingHeader.AMA_JobReference, "IAM");
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertEquals("Header is updated", existingHeaderPK, headerBO.PK);

			AssertEquals("headerBO.AMA_TransportMode", TransportTypeList.Codes.Air, headerBO.AMA_TransportMode);
			AssertEquals("headerBO.AMA_Voyage", "QTX370", headerBO.AMA_Voyage);
			AssertEquals("headerBO.AMA_RL_NKPortOfLoading", "USAAF", headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("headerBO.AMA_RL_NKPortOfDischarge", "USAAZ", headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("headerBO.AMA_VesselName", "TestVessel", headerBO.AMA_VesselName);
			AssertEquals("headerBO.AMA_RN_NKConveyanceNationality", "XX", headerBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("headerBO.AMA_MasterInformation", "Captain Kirk", headerBO.AMA_MasterInformation);
			AssertEquals("headerBO.AMA_E_DEP", departureTime, headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", arrivalTime, headerBO.AMA_E_ARV);
			AssertEquals("headerBO.AMA_Trailer1RegNo", "TRAILER001", headerBO.AMA_Trailer1RegNo);
			AssertEquals("headerBO.AMA_Trailer2RegNo", "TRAILER002", headerBO.AMA_Trailer2RegNo);
			AssertEquals("headerBO.AMA_RN_NKTrailer1RegCountry", "TR", headerBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("headerBO.AMA_RN_NKTrailer2RegCountry", "ZA", headerBO.AMA_RN_NKTrailer2RegCountry);

			AssertNotNull(headerBO.MasterBill);
			AssertEquals("headerBO has no bill", 0, headerBO.Bills.Count);
		}

		public void TestImportHeaderWhenWayBillNumberIsEmpty()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var existingHeaderPK = existingHeader.PK;
			var consol = Factory.New<ForwardingConsol>();
			existingHeader.SetParent(consol);
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			var carrierCode1 = "OTD1";
			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader("", portOfLoading, portOfDischarge, arrivalTime, departureTime, existingHeader.AMA_JobReference, "IAM");
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction1 = help.SetupCountryHeaderEntryInstruction(1, portOfFirstArrival, carrierCode1, "US", "MA1", "NT1");
			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.AddSafe(headerEntryHeader1);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.AddSafe(headerEntryInstruction1);
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertEquals("Header is updated", existingHeaderPK, headerBO.PK);
			AssertEquals("headerBO.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
			AssertEquals("headerBO.AMA_ManifestType", "MA1", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", carrierCode1, headerBO.AMA_CarrierCode);
		}

		public void TestOverrideFreightDefaults()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			oldHeader.MasterBill.ABL_BillNumber = "MAN00001";
			var headerPK = oldHeader.PK;
			var consol = Factory.New<ForwardingConsol>();
			oldHeader.SetParent(consol);

			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM", "IMP");
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			AssertNotNull(readerHeaderBO);
			AssertEquals("Manifest Header is updated.", headerPK, readerHeaderBO.PK);

			AssertEquals("readerHeaderBO.AMA_OverrideFreightDefaults", true, readerHeaderBO.AMA_OverrideFreightDefaults);
		}

		public void TestImportingAsycudaManifestDataFromEDIMessage()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var message = GetQueuedUniversalShipmentMessage(embeddedResourceRetriever.GetString(help.GetTestFilePathFor("UniversalXmlManifestHeader.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MAST0006");
			query.AddSubQuery(subQuery, JoinCondition.And);
			var headerBO = Factory.LoadTop1<AsycudaManifestHeader>(query);

			AssertNotNull(headerBO);

			#region Check Contents of header Business Object
			//header
			AssertEquals("headerBO.AMA_MasterBill", "MAST0006", headerBO.AMA_MasterBill);
			AssertEquals("headerBO.AMA_TransportMode", TransportTypeList.Codes.Air, headerBO.AMA_TransportMode);
			AssertEquals("headerBO.AMA_Voyage", "QTX370", headerBO.AMA_Voyage);
			AssertEquals("headerBO.AMA_RL_NKPortOfLoading", "BDAKH", headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("headerBO.AMA_RL_NKPortOfDischarge", "BDCDG", headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("headerBO.AMA_VesselName", "TestVessel", headerBO.AMA_VesselName);
			AssertEquals("headerBO.AMA_RN_NKConveyanceNationality", "XX", headerBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("headerBO.AMA_MasterInformation", "Captain Kirk", headerBO.AMA_MasterInformation);
			AssertEquals("headerBO.AMA_E_DEP", new ZDateTime("2017-06-12T00:00:00"), headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", new ZDateTime("2017-06-13T11:26:00"), headerBO.AMA_E_ARV);
			AssertEquals("headerBO.AMA_OverrideFreightDefaults", false, headerBO.AMA_OverrideFreightDefaults);
			AssertEquals("headerBO.AMA_ManifestType", "IAM", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", new ZDateTime("2017-06-12T00:00:00"), headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", "OTD1", headerBO.AMA_CarrierCode);
			AssertEquals("headerBO.AMA_Trailer1RegNo", "TRAILER001", headerBO.AMA_Trailer1RegNo);
			AssertEquals("headerBO.AMA_Trailer2RegNo", "TRAILER002", headerBO.AMA_Trailer2RegNo);
			AssertEquals("headerBO.AMA_RN_NKTrailer1RegCountry", "TR", headerBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("headerBO.AMA_RN_NKTrailer2RegCountry", "ZA", headerBO.AMA_RN_NKTrailer2RegCountry);

			//bill
			AssertEquals("headerBO has 1 bill", 1, headerBO.Bills.Count);
			var billBO = headerBO.Bills[0];
			AssertEquals("billBO.ABL_BillNumber", "BIL0002", billBO.ABL_BillNumber);
			AssertEquals("billBO.ABL_RL_NKOrigin", "BDKHL", billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", "BDMGL", billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 10000m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 0m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType);
			AssertEquals("billBO.ABL_BillStatus", "", billBO.ABL_BillStatus);
			AssertEquals("billBO.ABL_SenderReference", "", billBO.ABL_SenderReference);
			AssertEquals("billBO.ABL_LocationInformation", "LI", billBO.ABL_LocationInformation);
			AssertEquals("billBO.ABL_ShipmentType", "IMP", billBO.ABL_ShipmentType);
			#endregion
		}

		public void TestImportIntoAsycudaContainerBillOrPackageLink()
		{
			AssertNoExceptionThrown(() =>
			{
				var query = new ZDBOnlyQuery(typeof(AsycudaBill));
				query.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MSCU9077");
				var billBO = Factory.LoadTop1<AsycudaBill>(query);
				AssertEquals("Pre-req - no bill MSCU9077 exists", null, billBO);

				var help = new AsycudaManifestDataObjectReaderTestHelper();
				var messageForNewManifest = GetQueuedUniversalShipmentMessage(
					embeddedResourceRetriever.GetString(
						help.GetTestFilePathFor("RealError_AsycudaContainerBillOrPackageLink.xml")));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(messageForNewManifest);
				AssertContains("A successful import - no AsycudaContainerBillOrPackageLink constraint error", "Successfully saved Manifest Pack Consignment Reference 1 with 1 x AsycudaContainer, 1 x AsycudaManifestHeader, 1 x AsycudaBill.", serviceTaskLog.ToString());

				billBO = Factory.LoadTop1<AsycudaBill>(query);
				var headerBO = billBO.Header;

				AssertEquals("Container count", 1, headerBO.Containers.Count);
				AssertEquals("Container Number", "MSCU1234566", headerBO.Containers[0].ACN_ContainerNumber);
				AssertEquals("Bill count", 1, headerBO.Bills.Count);
				AssertEquals("Bill Number", "MSCU9077", headerBO.Bills[0].ABL_BillNumber);
				AssertEquals("Pack count", 1, headerBO.Bills[0].Packs.Count);
				AssertEquals("Pack item count", 100, headerBO.Bills[0].Packs[0].APA_PackQty);
				AssertEquals("Pack item type", "NO", headerBO.Bills[0].Packs[0].APA_PackUQ);
				AssertEquals("Pack link to container", "MSCU1234566", headerBO.Bills[0].Packs[0].Container.ACN_ContainerNumber);
				AssertEquals("No link to Bill", ZGuid.Empty, headerBO.Bills[0].Packs[0].Pivot.APC_ABL_Bill);
			});
		}

		public void TestImportHeaderWithChildBill()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var existingHeaderPK = existingHeader.PK;
			existingHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			existingHeader.AMA_JobReference = "C001";
			existingHeader.AMA_TransportMode = "SEA";
			var consol = Factory.New<ForwardingConsol>();
			existingHeader.SetParent(consol);
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			Factory.SaveForTesting();
			AssertEquals("C00001000_1", existingHeader.AMA_JobReference);
			var headerDataObject = help.SetupManifestHeader(null, portOfLoading, portOfDischarge, arrivalTime, departureTime, existingHeader.AMA_JobReference, "IAM");
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertEquals("Header is updated", existingHeaderPK, headerBO.PK);

			AssertEquals("headerBO.AMA_TransportMode", TransportTypeList.Codes.Air, headerBO.AMA_TransportMode);
			AssertEquals("headerBO.AMA_Voyage", "QTX370", headerBO.AMA_Voyage);
			AssertEquals("headerBO.AMA_RL_NKPortOfLoading", "USAAF", headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("headerBO.AMA_RL_NKPortOfDischarge", "USAAZ", headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("headerBO.AMA_VesselName", "TestVessel", headerBO.AMA_VesselName);
			AssertEquals("headerBO.AMA_RN_NKConveyanceNationality", "XX", headerBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("headerBO.AMA_MasterInformation", "Captain Kirk", headerBO.AMA_MasterInformation);
			AssertEquals("headerBO.AMA_E_DEP", departureTime, headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", arrivalTime, headerBO.AMA_E_ARV);
			AssertEquals("headerBO.AMA_Trailer1RegNoNo`", "TRAILER001", headerBO.AMA_Trailer1RegNo);
			AssertEquals("headerBO.AMA_Trailer2RegNo", "TRAILER002", headerBO.AMA_Trailer2RegNo);
			AssertEquals("headerBO.AMA_RN_NKTrailer1RegCountry", "TR", headerBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("headerBO.AMA_RN_NKTrailer2RegCountry", "ZA", headerBO.AMA_RN_NKTrailer2RegCountry);

			AssertNotNull(headerBO.MasterBill);
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_ExcludeZAOutturnGateInOut()
		{
			using (FreightDataRegistry.Instance.MAWBRecyclePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var helper = new AsycudaManifestDataObjectReaderTestHelper();
				var testHeader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				testHeader1.AMA_JobReference = "VV1";
				var bill1 = testHeader1.Bills.AddNew();
				bill1.ABL_BolType = AsycudaBill.ChildBolCode;
				bill1.ABL_BillNumber = "A123";
				var testHeader2 = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				testHeader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				testHeader2.AMA_ApplicationCode = "OUT";
				testHeader2.AMA_JobReference = "VV2";
				var bill2 = testHeader2.Bills.AddNew();
				bill2.ABL_BolType = AsycudaBill.ChildBolCode;
				bill2.ABL_BillNumber = "B567";

				Factory.SaveForTesting();

				var shippy = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "A123",
				};
				shippy.SetEntryHeaderCollection(() => new List<EntryHeader>(new[] { helper.SetupCountryHeaderEntryHeader("ER", 1) }));
				shippy.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO() { Code = "ERZZZ" }, "OTT1", "ER", "MA1", "NT1") }));
				var reader1 = new AsycudaManifestHeaderDataObjectReader(shippy, Logger, Factory);
				var header = reader1.ReadIntoBusinessObject();
				AssertEquals(testHeader1, header);
				AssertContains("Successfully loaded matching AsycudaManifestHeader.", Logger.Logs);

				var shippy2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "B567",
				};

				shippy2.SetEntryHeaderCollection(() => new List<EntryHeader>(new[] { helper.SetupCountryHeaderEntryHeader("ZA", 1) }));
				shippy2.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO() { Code = "ZAZZZ" }, "OTT1", "ZA", "MA1", "NT1") }));
				var reader2 = new AsycudaManifestHeaderDataObjectReader(shippy2, Logger, Factory);
				header = reader2.ReadIntoBusinessObject();
				AssertContains("No matching AsycudaManifestHeader found, creating new AsycudaManifestHeader", Logger.Logs);
				AssertNotEquals(testHeader2, header);
			}
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_GetHeaderFromShipmentWhenIsHVLV()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var genPivot = Factory.NewWithValidTestData<GenPivot>();
			genPivot.XX_RelationType = "HVL";
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = manifestHeader.PK;
			genPivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;

			Factory.SaveForTesting();

			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_MasterBillNum);
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);

			var shipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipmentDataObject });

			AssertEquals("Data object is HVLV", true, consolDataObject.IsHVLV());

			var newFactory = new UniversalObjectFactory();
			var reader = new AsycudaManifestHeaderDataObjectReader(consolDataObject, Logger, newFactory);
			var existingManifestHeader = reader.TryGetExistingBusinessObject();

			AssertEquals("Reader should find existing manifest header", manifestHeader.PK, existingManifestHeader.PK);
		}

		public void TestUpdateBillsUsingTheActionPurpose()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			existingHeader.AMA_JobReference = "MAN001";
			existingHeader.AMA_ManifestType = "MGI";
			existingHeader.MasterBill.ABL_BillNumber = "MST001";

			var existingBill = existingHeader.Bills.AddNew();
			existingBill.ABL_BillNumber = "HOUSE001";

			var portOfLoading = new UNLOCO() { Code = help.GetAirLocalPort1(Core.Constants.CountryCodes.Australia).RL_Code };
			var portOfDischarge = new UNLOCO() { Code = help.GetAirLocalPort1(Core.Constants.CountryCodes.Singapore).RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();

			var headerDataObject = help.SetupManifestHeader("MST001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "MAN001", "MGI");
			headerDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				ActionPurpose = new CodeDescriptionPair() { Code = AsycudaEventMessageConstants.ActionPurpose.UPD, Description = "ASYCUDA Manifest Update" }
			});
			var billDataObject = help.SetupBill("HOUSE001", portOfLoading, portOfDischarge, 10m, "Many Goods", 0.5m, "REF99987", "STD", "PRE");
			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(billDataObject);
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var header = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			AssertEquals("MAN001", header.AMA_JobReference);
			AssertEquals("MST001", header.MasterBill.ABL_BillNumber);
			AssertEquals(portOfLoading.Code, header.AMA_RL_NKPortOfLoading);
			AssertEquals(portOfDischarge.Code, header.AMA_RL_NKPortOfDischarge);
			AssertEquals(arrivalTime, header.AMA_E_ARV);
			AssertEquals(departureTime, header.AMA_E_DEP);

			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("HOUSE001", bill.ABL_BillNumber);
			AssertEquals(portOfLoading.Code, bill.ABL_RL_NKOrigin);
			AssertEquals(portOfDischarge.Code, bill.ABL_RL_NKFinalDestination);
			AssertEquals(10m, bill.ABL_GrossWeight);
			AssertEquals("Many Goods", bill.ABL_GoodsDescription);
			AssertEquals(0.5m, bill.ABL_Volume);
			AssertEquals("REF99987", bill.ABL_CarrierReference);
			AssertEquals("STD", bill.ABL_BolType);
			AssertEquals("PRE", bill.ABL_PrepaidCollect);

			AssertEquals(Core.Constants.CountryCodes.Singapore, bill.CountryCode);
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_ExistingObjectNotReturnedForActionPurposeAdd()
		{
			using (FreightDataRegistry.Instance.MAWBRecyclePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var helper = new AsycudaManifestDataObjectReaderTestHelper();
				var testHeader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				testHeader1.MasterBill.ABL_BillNumber = "MAWB1234";
				testHeader1.AMA_JobReference = "VV1";
				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					ActionPurpose = new CodeDescriptionPair() { Code = AsycudaEventMessageConstants.ActionPurpose.ADD, Description = "ASYCUDA Manifest Add" }
				});

				var shippy1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MAWB1234",
				};
				shippy1.SetEntryHeaderCollection(() => new List<EntryHeader>(new[] { helper.SetupCountryHeaderEntryHeader("ER", 1) }));
				shippy1.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO() { Code = "ERZZZ" }, "OTT1", "ER", "MA1", "NT1") }));
				var reader1 = new AsycudaManifestHeaderDataObjectReader(shippy1, Logger, Factory);
				AssertNotEquals(testHeader1, reader1.ReadIntoBusinessObject());

				dataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					ActionPurpose = new CodeDescriptionPair() { Code = AsycudaEventMessageConstants.ActionPurpose.UPD, Description = "ASYCUDA Manifest Update" }
				});
				var shippy2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MAWB1234",
				};
				shippy2.SetEntryHeaderCollection(() => new List<EntryHeader>(new[] { helper.SetupCountryHeaderEntryHeader("ER", 1) }));
				shippy2.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO() { Code = "ERZZZ" }, "OTT1", "ER", "MA1", "NT1") }));
				reader1 = new AsycudaManifestHeaderDataObjectReader(shippy2, Logger, Factory);
				AssertEquals(testHeader1, reader1.ReadIntoBusinessObject());
			}
		}

		public void TestGetMainHeaderFromMultipleHeaders()
		{
			using (FreightDataRegistry.Instance.MAWBRecyclePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
				var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

				var header1 = UpdateHeaderAndBill((AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(), consol1, "BOL180301");
				header1.AMA_ManifestType = "ASY";

				var header2 = UpdateHeaderAndBill((AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(), consol2, "BOL180301");
				header2.AMA_ManifestType = "ASY";

				var header3 = UpdateHeaderAndBill((AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(), consol2, "BOL180302");
				header3.AMA_ManifestType = "ASY";

				var header4 = UpdateHeaderAndBill((AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>(), consol2, "BOL180302");
				header4.AMA_ManifestType = "ASY";

				Factory.SaveForTesting();

				var shippy1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "BOL180301",
				};
				shippy1.SetEntryHeaderCollection(() => new List<EntryHeader>
				{
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "SG" } }
				});
				var reader = new AsycudaManifestHeaderDataObjectReader(shippy1, Logger, Factory);

				AssertNull("Should not found as there are two manifest bill data with different consol.", reader.ReadIntoBusinessObject());
				AssertContains("Cannot update data as there are multiple jobs matching (Manifest No: BOL180301, Country: SG).", Logger.GetErrors());

				Logger.ClearLogs();
				var shippy2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "BOL180302",
				};
				shippy2.SetEntryHeaderCollection(() => new List<EntryHeader>
					{
						new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "SG" } },
						new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "ZA" } },
					});
				reader = new AsycudaManifestHeaderDataObjectReader(shippy2, Logger, Factory);

				AssertNull("Should not found as XML has more than one country code.", reader.ReadIntoBusinessObject());
				AssertContains("The entry header collection contains multiple countries.", Logger.GetErrors());

				var shippy3 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "BOL180302",
				};
				shippy3.SetEntryHeaderCollection(() => new List<EntryHeader>
				{
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "ZA" } }
				});
				shippy3.SetEntryInstructionCollection(() => new List<EntryInstruction>
				{
					new EntryInstruction { Style = "ASY" }
				});
				reader = new AsycudaManifestHeaderDataObjectReader(shippy3, Logger, Factory);

				AssertEquals("Should matched the header4.", header4, reader.ReadIntoBusinessObject());
			}
		}

		AsycudaManifestHeader UpdateHeaderAndBill(AsycudaManifestHeader header, ForwardingConsol consol, string masterBill)
		{
			header.SetParent(consol);

			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			bill.ABL_BillNumber = masterBill;

			return header;
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var shippy1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "BOL180301",
			};
			shippy1.SetEntryHeaderCollection(() => new List<EntryHeader>
				{
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "SG" } },
					new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "ZA" } },
				});
			var reader = new AsycudaManifestHeaderDataObjectReader(shippy1, Logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertContains("The entry header collection contains multiple countries.", Logger.GetErrors());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				Logger.ClearLogs();
				reader = new AsycudaManifestHeaderDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "BOL180301"
				}, Logger, Factory);
				reader.ReadIntoBusinessObject();
				AssertContains("Entry Header->Type->Code  is not a valid manifest country.", Logger.GetErrors());
			}
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBOWithManifestType()
		{
			#region setup Org
			var shippingLineAddress = GetNewAddressData_INTHEMSYD(DocAddressType.ShippingLineAddress);
			var orgAddress = new OrganisationDataObjectReader(shippingLineAddress, Logger, Factory).GetMatchedOrNewForTesting();

			var airline = Factory.New<RefAirline>();
			airline.RM_AirlineName1 = "Best Airline Ever";
			airline.RM_TwoCharacterCode = "XD";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";

			var org = orgAddress.Header;
			org.OH_Code = "AIRLINE_WW";

			orgAddress.OA_Address1 = "456 Main Address Road";
			orgAddress.OA_Code = "Test";
			orgAddress.OA_City = "Auckland";
			orgAddress.OA_PostCode = "1234";

			var miscServ = org.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			#endregion

			#region setup dataobject for HVLV
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "");
			headerDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "12345678");
			headerDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "87654321");
			var subShipment = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			subShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			subShipment.SetDateCollection(() => new List<Date>());
			subShipment.DateCollection.AddSafe(new Date() { Type = DateType.Departure, Value = arrivalTime });
			subShipment.DateCollection.AddSafe(new Date() { Type = DateType.Arrival, Value = departureTime });

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(subShipment);

			headerDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			headerDataObject.OrganizationAddressCollection.AddSafe(shippingLineAddress);

			headerDataObject.PortOfFirstArrival = portOfFirstArrival;
			#endregion

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Logger.ClearLogs();
				headerDataObject.MessageType = new CodeDescriptionPair() { Code = "IAM", Description = "IAM" };
				var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
				reader.ReadIntoBusinessObject();
				AssertNotContains("More than one manifest types are found for country", Logger.GetErrors());
			}
		}

		public void TestSupportCustomsPortsEnabledImportManifests()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var sgCustomPortCode = "SGZZZ";
			var portOfAustralia = new UNLOCO() { Code = help.GetAirLocalPort1(Core.Constants.CountryCodes.Australia).RL_Code };
			var portOfSingapore = new UNLOCO() { Code = help.GetAirLocalPort1(Core.Constants.CountryCodes.Singapore).RL_Code };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			var factory = new BusinessObjectFactory();

			#region TestCustomsPortDischarge
			var existingForDischargeHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			existingForDischargeHeader.AMA_ManifestType = "MGI";
			existingForDischargeHeader.AMA_JobReference = "MAN001";
			existingForDischargeHeader.MasterBill.ABL_BillNumber = "MST001";

			Factory.SaveForTesting();
			var headerDataObjectCustomsPortForDischarge = help.SetupManifestHeaderWithCustomsPortsOfManifestHeader("MAN00001", portOfAustralia, portOfSingapore, arrivalTime, departureTime, "MAN001", "", sgCustomPortCode, "MGI");

			var readerCustomsPortForDischarge = new AsycudaManifestHeaderDataObjectReader(headerDataObjectCustomsPortForDischarge, Logger, Factory);
			var readerHeaderCustomsPortForDischarge = readerCustomsPortForDischarge.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var headerCustomsPortForDischarge = factory.Load<AsycudaManifestHeader>(readerHeaderCustomsPortForDischarge.PK);
			AssertEquals(sgCustomPortCode, headerCustomsPortForDischarge.MasterBill.ABL_CustomsDischargePort);
			AssertEquals(sgCustomPortCode, headerCustomsPortForDischarge.AMA_CustomsDischargePort);
			#endregion

			#region TestCustomsPortLoading
			var existingForLoadingHeader = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			existingForLoadingHeader.AMA_ManifestType = "MGI";
			existingForLoadingHeader.AMA_JobReference = "MAN002";
			existingForLoadingHeader.MasterBill.ABL_BillNumber = "MST002";

			Factory.SaveForTesting();
			var headerDataObjectCustomsPortForLoading = help.SetupManifestHeaderWithCustomsPortsOfManifestHeader("MAN00002", portOfSingapore, portOfAustralia, arrivalTime, departureTime, "MAN002", sgCustomPortCode, "", "MGI");

			var readerCustomsPortForLoading = new AsycudaManifestHeaderDataObjectReader(headerDataObjectCustomsPortForLoading, Logger, Factory);
			var readerHeaderCustomsPortForLoading = readerCustomsPortForLoading.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var headerCustomsPortForLoading = factory.Load<AsycudaManifestHeader>(readerHeaderCustomsPortForLoading.PK);
			AssertEquals(sgCustomPortCode, headerCustomsPortForLoading.MasterBill.ABL_CustomsLoadPort);
			AssertEquals(sgCustomPortCode, headerCustomsPortForLoading.AMA_CustomsLoadPort);
			#endregion

		}

		public void TestSGHVLVProperties()
		{
			#region setup Org
			var shippingLineAddress = GetNewAddressData_INTHEMSYD(DocAddressType.ShippingLineAddress);
			var orgAddress = new OrganisationDataObjectReader(shippingLineAddress, Logger, Factory).GetMatchedOrNewForTesting();

			var airline = Factory.New<RefAirline>();
			airline.RM_AirlineName1 = "Best Airline Ever";
			airline.RM_TwoCharacterCode = "XD";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";

			var org = orgAddress.Header;
			org.OH_Code = "AIRLINE_WW";

			orgAddress.OA_Address1 = "456 Main Address Road";
			orgAddress.OA_Code = "Test";
			orgAddress.OA_City = "Auckland";
			orgAddress.OA_PostCode = "1234";

			var miscServ = org.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			#endregion

			#region setup dataobject for HVLV
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "");
			headerDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "12345678");
			headerDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "87654321");
			var subShipment = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			subShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			subShipment.SetDateCollection(() => new List<Date>());
			subShipment.DateCollection.AddSafe(new Date() { Type = DateType.Departure, Value = arrivalTime });
			subShipment.DateCollection.AddSafe(new Date() { Type = DateType.Arrival, Value = departureTime });

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(subShipment);

			headerDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			headerDataObject.OrganizationAddressCollection.AddSafe(shippingLineAddress);

			headerDataObject.PortOfFirstArrival = portOfFirstArrival;

			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>()
			{
				new EntryHeader()
				{
					Type = new EntryType() { Code = Core.Constants.CountryCodes.Singapore },
				}
			});

			#endregion

			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.SaveForTesting();

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);

			AssertNotNull(headerBO);

			AssertEquals("Customs Load Port", portOfLoading.Code, headerBO.AMA_CustomsLoadPort);
			AssertEquals("Customs Discharge Port", portOfDischarge.Code, headerBO.AMA_CustomsDischargePort);
			AssertEquals("Port Of First Arrival", portOfFirstArrival.Code, headerBO.AMA_RL_NKPortOfFirstArrival);
			AssertEquals("SG Manifest Type", "MGE", headerBO.AMA_ManifestType);
			AssertEquals("Carrier Address", orgAddress.PK, headerBO.AMA_OA_Carrier);
			AssertEquals("Carrier Code", airline.RM_TwoCharacterCode, headerBO.AMA_CarrierCode);
			AssertEquals("headerBO.AMA_E_DEP", arrivalTime, headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", departureTime, headerBO.AMA_E_ARV);
		}

		public void TestApplicationCodeCorrectlyDefaulted()
		{
			CombineAssertions(() =>
			{
				var shippy = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shippy.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation> { });
				shippy.SetEntryHeaderCollection(() => new List<EntryHeader>
					{
						new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "GB" } },
					});
				shippy.SetEntryInstructionCollection(() => new List<EntryInstruction>
					{
						new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) { Style = "GVM" },
					});
				AssertEquals("If manifest type/country allows both, then default should be NVC (if not supplied).", ApplicationCodeTypeList.Codes.Consolidator, new AsycudaManifestHeaderDataObjectReader(shippy, Logger, Factory).ReadIntoBusinessObject().AMA_ApplicationCode);

				shippy.MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.TRETrade, Description = ApplicationCodeTypeList.Descriptions.ShippingLine };
				AssertEquals("There are cases where the value is neither VOC/NVC in which case, the appropriate application code should default � e.g. etrade TR", ApplicationCodeTypeList.Codes.TRETrade, new AsycudaManifestHeaderDataObjectReader(shippy, Logger, Factory).ReadIntoBusinessObject().AMA_ApplicationCode);

				shippy.SetEntryHeaderCollection(() => new List<EntryHeader>
					{
						new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance) { Type = new EntryType { Code = "ZA" } },
					});
				shippy.SetEntryInstructionCollection(() => new List<EntryInstruction>
					{
						new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) { Style = "RMA" },
					});
				AssertEquals("If manifest type/country only allows one application code, then that one should default. (even if wrong one is supplied)", ApplicationCodeTypeList.Codes.ShippingLine, new AsycudaManifestHeaderDataObjectReader(shippy, Logger, Factory).ReadIntoBusinessObject().AMA_ApplicationCode);
			});
		}

		public void TestImportCustomsOffice()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM");
			headerDataObject.MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.Consolidator };

			headerDataObject.CustomsOffice = new CodeDescriptionPair10Char { Code = "AT240000" };

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);

			AssertEquals("AT240000", headerBO.AMA_CustomsOffice);
		}

		public void TestImportingAsycudaManifestDataWithSubShipmentPartialContentFromEDIMessage()
		{
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = helper.GetAirLocalPort1("US");
			var airLocalPort2 = helper.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var billIssuerCode = "BIC1";

			var headerDataObject = helper.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM");
			headerDataObject.MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.Consolidator };

			var bill1 = helper.SetupBill("BIL0001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader(Core.Constants.CountryCodes.SouthAfrica, 1, "CLR", "Sender Reference1");
			var billCountryEntryInstruction = helper.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", Core.Constants.CountryCodes.SouthAfrica, billIssuerCode, 200.01, 300.01, "Q", "A", "TEST", ZDateTime.Empty, ZString.Empty);
			_ = bill1.SetEntryHeaderCollection(() => new List<EntryHeader>());
			_ = bill1.EntryHeaderCollection.AddSafe(billCountryEntryHeader);
			_ = bill1.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			_ = bill1.EntryInstructionCollection.AddSafe(billCountryEntryInstruction);

			var bill2 = helper.SetupBill("BIL0002", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			billCountryEntryHeader = helper.SetupCountryBillEntryHeader(Core.Constants.CountryCodes.SouthAfrica, 2, "CLR", "Sender Reference2");
			billCountryEntryInstruction = helper.SetupCountryBillEntryInstruction(1, "Goods Location2", "Location Information2", "IMP", Core.Constants.CountryCodes.SouthAfrica, billIssuerCode, 200.01, 300.01, "Q", "A", "TEST", ZDateTime.Empty, ZString.Empty);
			_ = bill2.SetEntryHeaderCollection(() => new List<EntryHeader>());
			_ = bill2.EntryHeaderCollection.AddSafe(billCountryEntryHeader);
			_ = bill2.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			_ = bill2.EntryInstructionCollection.AddSafe(billCountryEntryInstruction);

			_ = headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.AddRange(new[] { bill1, bill2 });
			Factory.SaveForTesting();

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
			CombineAssertions(() =>
			{
				AssertNotNull(headerBO);
				AssertEquals("headerBO has 2 bill", 2, headerBO.Bills.Count);
				var bill1 = headerBO.Bills[0];
				AssertEquals(nameof(bill1.ABL_BillNumber), "BIL0001", bill1.ABL_BillNumber);

				var bill2 = headerBO.Bills[1];
				AssertEquals(nameof(bill2.ABL_RL_NKOrigin), "USAAF", bill2.ABL_RL_NKOrigin);
				AssertEquals(nameof(bill2.ABL_RL_NKFinalDestination), "USAAZ", bill2.ABL_RL_NKFinalDestination);
				AssertEquals(nameof(bill2.ABL_GrossWeight), 300m, bill2.ABL_GrossWeight);
			});

			var message = GetQueuedUniversalShipmentMessage(embeddedResourceRetriever.GetString(helper.GetTestFilePathFor("UniversalXmlSubShipmentPartialContent.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			_ = manager.Process(message);
			Factory.SaveForTesting();

			headerBO.Bills.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals("headerBO has 3 bill", 3, headerBO.Bills.Count);

				Assert("manager has warnings", manager.Logger.HasWarnings);
				var warningMessage = "Warning - Could not delete Manifest Bill BIL0001 (Type: AsycudaBill | Key: BIL0001) due to the following reason: SubShipmentCollection Content=Partial is not supported";
				AssertNotNull(manager.Logger.Logs.ToList().Where(l => l.Message == warningMessage));

				var bill2 = headerBO.Bills[1];
				AssertEquals(nameof(bill2.ABL_BillNumber), "BIL0002", bill2.ABL_BillNumber);
				AssertEquals(nameof(bill2.ABL_RL_NKOrigin), "BDKHL", bill2.ABL_RL_NKOrigin);
				AssertEquals(nameof(bill2.ABL_RL_NKFinalDestination), "BDMGL", bill2.ABL_RL_NKFinalDestination);
				AssertEquals(nameof(bill2.ABL_GrossWeight), 10000m, bill2.ABL_GrossWeight);

				var bill3 = headerBO.Bills[2];
				AssertEquals(nameof(bill3.ABL_BillNumber), "BIL0003", bill3.ABL_BillNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var us = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, "United States", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(us.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "17.3.29.1");
			var sg = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "17.3.29.1");
			Factory.SaveForTesting();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
