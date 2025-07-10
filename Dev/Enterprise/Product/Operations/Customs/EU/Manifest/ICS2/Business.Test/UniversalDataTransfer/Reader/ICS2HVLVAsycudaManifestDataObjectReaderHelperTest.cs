using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2HVLVAsycudaManifestDataObjectReaderHelperTest : DataObjectReaderTest
	{
		public void TestImportAsycudaManifestHeaderFromHVLVShipment()
		{
			var helper = new AsycudaManifestDataObjectReaderTestHelper();

			var loadingPort = new UNLOCO { Code = helper.GetAirLocalPort1("AU").RL_Code };
			var dischargePort = new UNLOCO { Code = helper.GetAirLocalPort1("FR").RL_Code };

			var estimatedArrival = new Date
			{
				Type = DateType.Arrival,
				Value = new ZDateTime(2018, 2, 10)
			};
			var estimatedDeparture = new Date
			{
				Type = DateType.Departure,
				Value = new ZDateTime(2018, 2, 1)
			};
			var actualArrivalTime = new Date
			{
				Type = DateType.ActualArrival,
				Value = new ZDateTime(2018, 2, 11)
			};

			var shipment = helper.SetupManifestHeader("EUICS0001", loadingPort, dischargePort, (ZDateTime)estimatedArrival.Value, (ZDateTime)estimatedDeparture.Value, "", EUICS2ManifestTypes.Codes.ENS);
			var entryHeader = helper.SetupCountryHeaderEntryHeader(Core.Constants.CountryCodes.France, 1);

			var headerEntryInstruction = helper.SetupCountryHeaderEntryInstruction(1, loadingPort, "OTT1", Core.Constants.CountryCodes.France, EUICS2ManifestTypes.Codes.ENS, "IMP");
			shipment.SetEntryHeaderCollection(() => new List<EntryHeader>());
			shipment.EntryHeaderCollection.Add(entryHeader);
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			shipment.EntryInstructionCollection.Add(headerEntryInstruction);

			shipment.PortOfFirstArrival = loadingPort;
			shipment.VoyageFlightNo = "Voyage";
			shipment.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			shipment.WayBillNumber = "MasterBillMAWB";
			shipment.PortOfLoading = loadingPort;
			shipment.PortOfDischarge = dischargePort;

			shipment.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeader.Schema.AddressedMemberState, Value = "FR" } );

			var trasportLeg1 = new TransportLeg { ActualArrival = new ZDateTime(2018, 2, 4) };
			var trasportLeg2 = new TransportLeg { ActualArrival = actualArrivalTime.Value };

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				trasportLeg1,
				trasportLeg2
			});

			var carrierAddress = SetUpOrganisationAddress("I'm Carrying Stuff", "FRNAK", "Unit 100", "55 Why Lane", "Conficious Ave", "ABC", "9000", "Carrier", nameof(DocAddressType.Carrier), dischargePort);
			var sendingForwarderAddress = SetUpOrganisationAddress("I'm Sending Stuff", "FRNAK", "Unit 200", "55 Yes Lane", "Conficious Ln", "XYZ", "10000", "Shipper", nameof(DocAddressType.SendingForwarderAddress), dischargePort);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				carrierAddress,
				sendingForwarderAddress
			});

			SetUpHVLVSubShipment(shipment, loadingPort, dischargePort, helper);
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var headerBO = ProcessAndFindHeader(shipment);
			var carrierAddressBO = new OrganisationDataObjectReader(carrierAddress, logger, Factory).GetMatched();
			var sendingForwarderAddressBO = new OrganisationDataObjectReader(sendingForwarderAddress, logger, Factory).GetMatched();

			AssertNotNull(headerBO);

			AssertEquals("ICS2 Member State should map to HVLV Transport", "FR", headerBO.AddressedMemberState);
			AssertEquals("ICS2 Load Port should map to HVLV Load Port", loadingPort.Code, headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("ICS2 Discharge Port should map to HVLV Discharge Port", dischargePort.Code, headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("ICS2 Act. Arrival should map to HVLV ATA", actualArrivalTime.Value, headerBO.AMA_A_ARV);
			AssertEquals("ICS2 MAWB should map to HVLV MAWB", "MasterBillMAWB", headerBO.AMA_MasterBill);
		}

		OrganizationAddress SetUpOrganisationAddress(string fullName, string closestPort, string address1, string address2, string city, string state, string postcode, string companyName, string addressType, UNLOCO dischargePort)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.Address2 = address2;
			org.MainAddress.City = city;
			org.MainAddress.State = state;
			org.MainAddress.Postcode = postcode;
			org.MainAddress.OA_RN_NKCountryCode = ((ZString)dischargePort.Code).Left(2);
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressShortCode = companyName.ToUpper().Substring(0, 3),
				AddressOverride = false,
				OrganizationCode = companyName.ToUpper(),
				CompanyName = companyName,
				Address1 = org.MainAddress.Address1,
				Address2 = org.MainAddress.Address2,
				City = org.MainAddress.City,
				State = org.MainAddress.State,
				Postcode = org.MainAddress.Postcode
			};

			return orgAddress;
		}

		void SetUpHVLVSubShipment(Shipment shipment, UNLOCO loadingPort, UNLOCO dischargePort, AsycudaManifestDataObjectReaderTestHelper helper)
		{
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var subShipment = helper.SetupBill("BIL00001", loadingPort, dischargePort, 300m, "Goods Desc", 3m, "Carrier Reference", "HVL", "PRE");
			shipment.SubShipmentCollection.Add(subShipment);

			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("FR", 1, "CLR", "BIL00001");
			subShipment.SetEntryHeaderCollection(() => new List<EntryHeader> { billCountryEntryHeader });

			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "12345678");
			shipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "87654321");

			var consignmentAsSubshipment = new Shipment();
			consignmentAsSubshipment.DataContext = DataContextFactory.New();
			consignmentAsSubshipment.DataContext.AddDataSource(DataContextType.HVLVConsignment, "HVC001");

			subShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentAsSubshipment });
			subShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
		}

		AsycudaManifestHeader ProcessAndFindHeader(Shipment shipment)
		{
			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			var reader = new AsycudaManifestHeaderDataObjectReader(shipment, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			return Factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);
		}
	}
}
