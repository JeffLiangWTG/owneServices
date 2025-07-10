using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities.Testing;
using Enterprise.Client.JAS.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class OHBLRecordTest : JXCRecordTestCase
	{
		#region TestLoadOrCreateConsol
		[ExpectNoExceptions]
		public void TestLoadOrCreateConsol_NullParam()
		{
			OHBLRecord record = new OHBLRecord("", "");
			record.LoadOrCreateConsol(null);
		}

		public void TestLoadOrCreateConsol()
		{
			CreateConsolsForTest();
			OHBLRecord record = (OHBLRecord)RecordFactory.NewRecord("OHBL3100;n;NV05050449;HKHKG;NV05050449;30;HKHKG;OOCL;OOCL;;;;;;POLK AUDIO;C/O ORIENTAL LOGISTICS CO., LTD;1-11 KA TING ROAD,;KWAI CHUNG, N.T.,;HKHKG;N/A;;HK;;n;;93-18117;;ASSOCIATED MARKETING GROUP;88 ENTERPRISE AVENUE;BERWICK (MELBOURNE), 3806;AUSTRALIA;MEL;N/A;;AU;;n;;10985;(10985);JAS FORWARDING (HK) LIMITED;UNIT B, 5/F., MTL WARHOUSE BULIDING;PHASE 1 BERTH ONE, KWAI CHUNG;CONTAINER TERMINALS, KWAI CHUNG NT;;;HONG KONG;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUMEL;USD;184.80;C;MELBOURNE;CSCL KELANG;068S;HK;;16/05/2005;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;2;CTN;702.770;K;4.620;;SAID TO CONTAIN;;;PLTS(55 CTNS);HI-FI LOUDSPEAKER   ;   ;  ; -FREIGHT COLLECT- ;ZZ;TWO (2) PALLETS ONLY;;16/05/2005;HONG KONG;CCT;JAS FORWARDING WORLDWIDE PTY LTD;GROUND FLOOR, THE MILLS;200 ARDEN STREET, NORTH MELBOURNE,;VICTORIA 3051, AUSTRALIA;MELBOURNE;;HKHKG;;;;;3;MELBOURNE;0;;N");
			JASForwardingConsol loadedConsol = record.LoadOrCreateConsol(Factory);
			Assert("Consol not found, should be creating new record", !loadedConsol.IsInDatabase);
			AssertConsolDefaultValues(loadedConsol);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.BillOfLadingNo, "102");
			record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			loadedConsol = record.LoadOrCreateConsol(Factory);
			Assert("Consol should be matched", loadedConsol.IsInDatabase);
			AssertEquals("FROMOHBL", loadedConsol.JK_UniqueConsignRef);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.BillOfLadingNo, "103");
			record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			OMANRecord oMANRecord = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;101;CSCL KELANG;068S;HKHKG;MELBOURNE;HKHKG;HKHKG;AUMEL;AUMEL;19/05/2005;16/05/2005;25/05/2005");
			loadedConsol = record.LoadOrCreateConsol(Factory, oMANRecord);
			Assert("Consol should be matched", loadedConsol.IsInDatabase);
			AssertEquals("Should be matching it from the OMANRecord if cannot match from OHBLRecord available", "FROMOMAN", loadedConsol.JK_UniqueConsignRef);
		}

		void AssertConsolDefaultValues(JASForwardingConsol consol)
		{
			AssertEquals(Core.Constants.AgentType.Agent, consol.JK_AgentType);
			AssertEquals(Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.PaymentType.Prepaid, consol.JK_PrepaidCollect);
		}

		#endregion
		#region TestUpdateConsol
		public void TestUpdateConsol()
		{
			SetFieldValuesForTestUpdateConsol();
			HEADRecord hEADRecord = new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
			OHBLRecord record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(Factory);
			Transport transport = consol.Transports.ArrivalTransport;
			transport.JW_RL_NKLoadPort = "";
			DataImportFlagChanger.LastBizO = null;
			record.UpdateConsol(hEADRecord, consol, NotificationBuffer);
			AssertConsolPropertiesAfterUpdateConsol(consol, record);
			AssertEquals("Should use DataImportFlagChanger", consol, DataImportFlagChanger.LastBizO);
		}

		public void TestUpdateConsol_MatchVesselFromLloydsCode()
		{
			SetFieldValuesForTestUpdateConsol_MatchVesselFromLloydsCode();
			HEADRecord hEADRecord = new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
			OHBLRecord record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(Factory);
			Transport transport = consol.Transports.ArrivalTransport;
			transport.JW_RL_NKLoadPort = "";
			record.UpdateConsol(hEADRecord, consol, NotificationBuffer);
			AssertConsolPropertiesAfterUpdateConsol(consol, record);
		}

		public void TestUpdateConsol_DoNotReplaceIfSomeMandatoryFieldsHaveAlreadyBeenPopulated()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			SetFieldValuesForTestUpdateConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_Vessel = "VESSEL2";
			transport.JW_VoyageFlight = "VOY 002";
			transport.JW_RL_NKLoadPort = "USATL";
			transport.JW_RL_NKDiscPort = "HKHKG";
			JASOrgHeader shippingLine = Factory.New<JASOrgHeader>();
			consol.SetDefaultShippingLineAddress(shippingLine);
			JASOrgHeader receivingForwarder = Factory.New<JASOrgHeader>();
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			HEADRecord hEADRecord = new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
			OHBLRecord record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			record.UpdateConsol(hEADRecord, consol, NotificationBuffer);
			AssertEquals("VESSEL2", transport.JW_Vessel);
			AssertEquals("VOY 002", transport.JW_VoyageFlight);
			AssertEquals("USATL", transport.JW_RL_NKLoadPort);
			AssertEquals("HKHKG", transport.JW_RL_NKDiscPort);
			AssertEquals(shippingLine.PK, consol.ShippingLinePK);
		}

		public void TestUpdateConsol_MatchShippingLineFromSSLCode()
		{
			try
			{
				JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
				JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
				SetFieldValuesForTestUpdateConsol();
				JASOrgHeader shippingLine = Factory.New<JASOrgHeader>();
				OrgPatternMatchOverride @override = shippingLine.CreatePatternMatchOverrideForTest();
				@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
				@override.OO_ForeignCode = "SSL-1";
				HEADRecord hEADRecord = new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
				OHBLRecord record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
				record.UpdateConsol(hEADRecord, consol, NotificationBuffer);
				AssertEquals("Should be matching it from the SSL code if available", shippingLine.PK, consol.ShippingLinePK);
				AssertNull("Should not need to try to match it using OrganisationMatching", record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CARRIER"));
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed();
			HEADRecord hEADRecord = new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
			OHBLRecord record = new OHBLRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			record.UpdateConsol(hEADRecord, consol, NotificationBuffer);
		}

		void SetFieldValuesForTestUpdateConsol()
		{
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.BillOfLadingNo, "BILL 023");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.VesselName, "VESSEL101");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.LloydsCode, "V12345");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.VoyageSSELNumber, "VOY 001");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, "ITMIL");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, "AUSYD");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierName, "CARRIER");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress1, "CARR ADD1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress2, "CARR ADD2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierSSLCode, "SSL-1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierCity, "BOTANY BAY");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentName, "RECEIVER");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentAddress, "RCV Address");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentCity, "PADDINGTON");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USSEA");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUADL");
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL101";
		}

		void SetFieldValuesForTestUpdateConsol_MatchVesselFromLloydsCode()
		{
			SetFieldValuesForTestUpdateConsol();
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.VesselName, "");
			RefVessel vessel = RefVessel.LookupVesselByCode("VESSEL101", Factory);
			vessel.RV_LloydsNumber = "V12345";
		}

		void SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('A', 1000);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.BillOfLadingNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.VesselName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.LloydsCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.VoyageSSELNumber, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierSSLCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.CarrierCity, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentAddress, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ReceivingAgentCity, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, excessivelyLongString);
		}

		void AssertConsolPropertiesAfterUpdateConsol(JASForwardingConsol consol, OHBLRecord record)
		{
			AssertEquals("BILL 023", consol.JK_MasterBillNum);
			AssertEquals("VESSEL101", consol.JK_JX_JV_NKVessel);
			AssertEquals("VOY 001", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("ITMIL", consol.Transports.DepartureTransport.JW_RL_NKLoadPort);
			AssertEquals("AUSYD", consol.Transports.ArrivalTransport.JW_RL_NKDiscPort);
			JXCRecord.FindOrCreateTempOrganisationParams carrierParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CARRIER");
			AssertCarrierOrgValueObjectForTestUpdateConsol(carrierParams.Organisation);
			AssertEquals(OrganisationTypes.Carrier, carrierParams.OrganisationType);
			AssertEquals(consol, carrierParams.SourceObject);
			AssertEquals(NotificationBuffer, carrierParams.NotificationSubscriber);
			Assert("Should be assigned", !consol.ShippingLinePK.IsEmpty);
			JXCRecord.FindOrCreateTempOrganisationParams receivingAgentParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("RECEIVER");
			AssertReceivingAgentOrgValueObjectForTestUpdateConsol(receivingAgentParams.Organisation);
			AssertEquals(OrganisationTypes.Forwarder, receivingAgentParams.OrganisationType);
			AssertEquals(consol, receivingAgentParams.SourceObject);
			AssertEquals(NotificationBuffer, receivingAgentParams.NotificationSubscriber);
			Assert("Should be assigned", !consol.ReceivingForwarderPK.IsEmpty);
		}

		void AssertCarrierOrgValueObjectForTestUpdateConsol(Xsd.Organisation organisation)
		{
			AssertEquals("CARRIER", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("CARR ADD1", mainAddress.AddressLine1);
			AssertEquals("CARR ADD2", mainAddress.AddressLine2);
			AssertEquals("BOTANY BAY", mainAddress.CityOrSuburb);
		}

		void AssertReceivingAgentOrgValueObjectForTestUpdateConsol(Xsd.Organisation organisation)
		{
			AssertEquals("RECEIVER", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("RCV Address", mainAddress.AddressLine1);
			AssertEquals("PADDINGTON", mainAddress.CityOrSuburb);
			Xsd.RegistrationNumber nettingCode = organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UNC, "");
			AssertEquals("AUCOR", nettingCode.Number);
			Xsd.RegistrationNumber officeCode = organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UOC, "");
			AssertEquals("AUADL", officeCode.Number);
		}

		#endregion
		#region TestLoadOrCreateShipment
		public void TestLoadOrCreateShipment_FromFactory()
		{
			SetupTestDataForTestLoadOrCreateShipment();
			SetFieldValuesForTestLoadOrCreateShipment("HB101", "AUSYD", "AUMEL");
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			JASForwardingShipment shipment = record.LoadOrCreateShipment(Factory);
			Assert("Should not match, should create a new one", shipment.JS_BookingReference != "MATCHED");
			AssertDefaultShipmentValues(shipment);
			SetFieldValuesForTestLoadOrCreateShipment("HB101", "HKHKG", "AUMEL");
			record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			shipment = record.LoadOrCreateShipment(Factory);
			Assert("Should not match, should create a new one", shipment.JS_BookingReference != "MATCHED");
			AssertDefaultShipmentValues(shipment);
			SetFieldValuesForTestLoadOrCreateShipment("HB101", "ITMIL", "HKHKG");
			record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			shipment = record.LoadOrCreateShipment(Factory);
			AssertEquals("Should match", "MATCHED", shipment.JS_BookingReference);
		}

		public void TestLoadOrCreateShipment_FromConsol()
		{
			SetupTestDataForTestLoadOrCreateShipment();
			JASForwardingConsol consol = new BusinessObjectFactory().New<JASForwardingConsol>();
			SetFieldValuesForTestLoadOrCreateShipment("HB101", "ITMIL", "AUMEL");
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			JASForwardingShipment resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			Assert("Should not match, Consol is in a different factory.", resultShipment.JS_BookingReference != "MATCHED");
			AssertDefaultShipmentValues(resultShipment);
			AssertContainAttachingNewShipmentNotification(NotificationBuffer, consol);
			consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment1 = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB102";
			JASForwardingShipment shipment2 = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB103";
			SetFieldValuesForTestLoadOrCreateShipment("HB102", "AUSYD", "IDJKT");
			record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals("Should match the housebill regardless of other details if attached to the same consol", shipment1.PK, resultShipment.PK);
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreateShipment_NullParam()
		{
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			BusinessObjectFactory factory = null;
			AssertNull(record.LoadOrCreateShipment(factory));
			JASForwardingConsol consol = null;
			AssertNull(record.LoadOrCreateShipment(consol, NotificationBuffer));
			consol = new BusinessObjectFactory().New<JASForwardingConsol>();
			AssertNotNull("Should not be null if consol not null", record.LoadOrCreateShipment(consol, null));
		}

		public void TestLoadOrCreateShipment_ShouldBeAttachedToTheConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_HouseBill = "HB102";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			SetFieldValuesForTestLoadOrCreateShipment("HB102", "", "");
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			JASForwardingShipment resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals("Should load shipment", shipment.PK, resultShipment.PK);
			Assert("Should be attached to the consol", consol.Shipments.Contains(resultShipment.PK));
			AssertContainAttachingExistingShipmentNotification(NotificationBuffer, consol, shipment);
		}

		void SetFieldValuesForTestLoadOrCreateShipment(ZString houseBillNumber, ZString portOfLoading, ZString portOfDischarge)
		{
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HouseBillOfLadingNo, houseBillNumber);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, portOfLoading);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, portOfDischarge);
		}

		void SetupTestDataForTestLoadOrCreateShipment()
		{
			InsertShipment("HB101", "ITMIL", "HKHKG");
			InsertShipment("HB101", "ITMIL", "AUSYD");
			InsertShipment("HB102", "AUSYD", "IDJKT");
			InsertShipment("HB102", "AUSYD", "SGSIN");
		}

		void InsertShipment(ZString houseBillNumber, ZString origin, ZString destination)
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_BookingReference = "MATCHED";
			shipment.JS_HouseBill = houseBillNumber;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
		}

		void AssertDefaultShipmentValues(JASForwardingShipment shipment)
		{
			AssertEquals(Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertEquals(Core.Constants.ShipmentReleaseTypes.OriginalReq, shipment.JS_ReleaseType);
		}

		#endregion
		#region TestUpdateShipment
		public void TestUpdateShipment()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			SetFieldValuesForTestUpdateShipment();
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			DataImportFlagChanger.LastBizO = null;
			record.UpdateShipment(shipment, NotificationBuffer);
			AssertShipmentPropertiesAfterUpdateShipment(shipment, record);
			AssertEquals("Should use DataImportFlagChanger", shipment, DataImportFlagChanger.LastBizO);
		}

		[ExpectNoExceptions]
		public void TestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			SetFieldValuesForTestUpdateShipment_ExcessivelyLongStringIsTrimmed();
			OHBLRecord record = (OHBLRecord)GetNewRecord(JXCConstants.LineTypes.OHBL, Fields.ConvertToJXCLine());
			record.UpdateShipment(shipment, NotificationBuffer);
		}

		void SetFieldValuesForTestUpdateShipment()
		{
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HouseBillOfLadingNo, "HB1293");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, "ITMIL");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, "BRSAO");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperName, "SHIPPER");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress1, "SHP Address1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress2, "SHP Address2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCity, "SEATTLE");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperState, "WA");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPostCode, "90102");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCountryCode, "US");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAccountNo, "SHP ACC");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPhone, "SHPPHONE");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperEmail, "shpemail@email.com");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeName, "CONSIGNEE");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress1, "CNE Address1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress2, "CNE Address2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCity, "SYDNEY");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeState, "NSW");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePostCode, "2000");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCountryCode, "AU");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAccountNo, "CNE ACC");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePhone, "CNEPHONE");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeEmail, "cneemail@email.com");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentName, "DELIVERY");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress1, "DLV Address1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress2, "DLV Address2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentCity, "RANDWICK");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.OnBoardDate, "20/12/2005");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions1, "special1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions2, "special2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions3, "special3\r\n  \n   ");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PrepaidOrCollect, "c");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.Currency, "IDR");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.TotalNoOfPackages, "200");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PieceTypeCode, "PAL");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.KilosOrPounds, "L");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.GrossWeight, "200.56");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.MeasurementInCBM, "111.23");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.Rate, "45.4");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods1, "DESC1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods2, "DESC2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods3, "DESC3");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods4, "DESC4");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods5, "DESC5");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods6, "DESC6");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods7, "DESC7");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods8, "DESC8");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions1, "handling1");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions2, "handling2");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions3, "handling3\r\n  \n   ");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.MarksAndNumbers, "MARKSANDNumberS");
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.NoOfOriginalBillOfLadings, "200");
		}

		void AssertShipmentPropertiesAfterUpdateShipment(JASForwardingShipment shipment, OHBLRecord record)
		{
			AssertEquals("HB1293", shipment.JS_HouseBill);
			AssertEquals("ITMIL", shipment.JS_RL_NKOrigin);
			AssertEquals("BRSAO", shipment.JS_RL_NKDestination);
			JXCRecord.FindOrCreateTempOrganisationParams shipperParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER");
			AssertShipperOrgValueObjectForTestUpdateShipment(shipperParams.Organisation);
			AssertEquals(OrganisationTypes.Consignor, shipperParams.OrganisationType);
			AssertEquals(shipment, shipperParams.SourceObject);
			AssertEquals(NotificationBuffer, shipperParams.NotificationSubscriber);
			Assert("Should be assigned", !shipment.ConsignorPK.IsEmpty);
			JXCRecord.FindOrCreateTempOrganisationParams consigneeParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE");
			AssertConsigneeOrgValueObjectForTestUpdateShipment(consigneeParams.Organisation);
			AssertEquals(OrganisationTypes.Consignee, consigneeParams.OrganisationType);
			AssertEquals(shipment, consigneeParams.SourceObject);
			AssertEquals(NotificationBuffer, consigneeParams.NotificationSubscriber);
			Assert("Should be assigned", !shipment.ConsigneePK.IsEmpty);
			JXCRecord.FindOrCreateTempOrganisationParams deliveryAgentParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("DELIVERY");
			AssertDeliveryAgentOrgValueObjectForTestUpdateShipment(deliveryAgentParams.Organisation);
			AssertEquals(OrganisationTypes.Forwarder, deliveryAgentParams.OrganisationType);
			AssertEquals(shipment, deliveryAgentParams.SourceObject);
			AssertEquals(NotificationBuffer, deliveryAgentParams.NotificationSubscriber);
			Assert("Should be assigned", !shipment.JS_OH_DeliveryAgent.IsEmpty);
			AssertEquals(new ZDateTime(2005, 12, 20), shipment.JS_ShippedOnBoardDate);
			AssertEquals("special1\r\nspecial2\r\nspecial3", shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description)[0].ST_NoteText);
			AssertEquals(Core.Constants.IncoTerms.ExWorks, shipment.JS_INCO);
			AssertEquals(200, shipment.JS_OuterPacks);
			AssertEquals(Core.Constants.PkgUnit.Pail, shipment.JS_F3_NKPackType);
			AssertEquals(Core.Constants.Weight.Pounds, shipment.JS_UnitOfWeight);
			AssertEquals(200.56m, shipment.JS_ActualWeight);
			AssertEquals(Core.Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(111.23m, shipment.JS_ActualVolume);
			AssertEquals("IDR", shipment.FrtRateCurrency.RX_Code);
			AssertEquals(45.4m, shipment.JS_UnitFreightRate);
			AssertEquals("DESC1\r\nDESC2\r\nDESC3\r\nDESC4\r\nDESC5\r\nDESC6\r\nDESC7\r\nDESC8", shipment.DetailedGoodsDescriptionNoteText);
			AssertEquals("handling1\r\nhandling2\r\nhandling3", shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)[0].ST_NoteText);
			AssertEquals("MARKSANDNumberS", shipment.JS_MarksAndNumbers);
			AssertEquals((ZByte)200, shipment.JS_NoOriginalBills);
		}

		void AssertShipperOrgValueObjectForTestUpdateShipment(Xsd.Organisation organisation)
		{
			AssertEquals("SHIPPER", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("SHP Address1", mainAddress.AddressLine1);
			AssertEquals("SHP Address2", mainAddress.AddressLine2);
			AssertEquals("SEATTLE", mainAddress.CityOrSuburb);
			AssertEquals("WA", mainAddress.StateOrProvince);
			AssertEquals("90102", mainAddress.PostCode);
			AssertEquals("US", mainAddress.Location.Country);
			Xsd.TelephoneNumber phoneNumber = mainAddress.TelephoneNumbers[0];
			AssertEquals(Xsd.TelephoneNumberNumberType.Business, phoneNumber.NumberType);
			AssertEquals("SHPPHONE", phoneNumber.Value);
			AssertEquals("shpemail@email.com", mainAddress.Email);
		}

		void AssertConsigneeOrgValueObjectForTestUpdateShipment(Xsd.Organisation organisation)
		{
			AssertEquals("CONSIGNEE", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("CNE Address1", mainAddress.AddressLine1);
			AssertEquals("CNE Address2", mainAddress.AddressLine2);
			AssertEquals("SYDNEY", mainAddress.CityOrSuburb);
			AssertEquals("NSW", mainAddress.StateOrProvince);
			AssertEquals("2000", mainAddress.PostCode);
			AssertEquals("AU", mainAddress.Location.Country);
			Xsd.TelephoneNumber phoneNumber = mainAddress.TelephoneNumbers[0];
			AssertEquals(Xsd.TelephoneNumberNumberType.Business, phoneNumber.NumberType);
			AssertEquals("CNEPHONE", phoneNumber.Value);
			AssertEquals("cneemail@email.com", mainAddress.Email);
		}

		void AssertDeliveryAgentOrgValueObjectForTestUpdateShipment(Xsd.Organisation organisation)
		{
			AssertEquals("DELIVERY", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("DLV Address1", mainAddress.AddressLine1);
			AssertEquals("DLV Address2", mainAddress.AddressLine2);
			AssertEquals("RANDWICK", mainAddress.CityOrSuburb);
		}

		void SetFieldValuesForTestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('0', 1000);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HouseBillOfLadingNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfLoadingCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PortOfDischargeCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCity, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperState, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPostCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperCountryCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperAccountNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperPhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ShipperEmail, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCity, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeState, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePostCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeCountryCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeAccountNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneePhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.ConsigneeEmail, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DeliveryAgentCity, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.OnBoardDate, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.SpecialInstructions3, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PrepaidOrCollect, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.Currency, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.TotalNoOfPackages, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.PieceTypeCode, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.KilosOrPounds, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.GrossWeight, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.MeasurementInCBM, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.Rate, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods3, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods4, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods5, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods6, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods7, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.DescriptionOfPackagesAndGoods8, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.HandlingInstructions3, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.MarksAndNumbers, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.OHBLFieldPositions.NoOfOriginalBillOfLadings, excessivelyLongString);
		}

		#endregion
		#region Implementation
		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new OHBLRecord(lineType, lineContent);
		}

		JASCsvLineForTest Fields
		{
			get
			{
				if (fFields == null)
				{
					fFields = new JASCsvLineForTest(JXCConstants.OHBLFieldCount);
				}

				return fFields;
			}
		}

		JASCsvLineForTest HEADRecordFields
		{
			get
			{
				if (fHEADRecordFields == null)
				{
					fHEADRecordFields = new JASCsvLineForTest(JXCConstants.HEADFieldCount);
				}

				return fHEADRecordFields;
			}
		}

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		void CreateConsolsForTest()
		{
			RefVessel vesselToUse = Factory.LoadTop1<RefVessel>(new ZQuery());
			JASForwardingConsol consolFromOMAN = Factory.NewWithValidTestData<JASForwardingConsol>();
			consolFromOMAN.JK_UniqueConsignRef = "FROMOMAN";
			consolFromOMAN.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolFromOMAN.JK_AgentsReference = "101";
			Transport transportFromOMAN = consolFromOMAN.Transports[0];
			transportFromOMAN.JW_Vessel = vesselToUse.RV_Code;
			transportFromOMAN.JW_VoyageFlight = "X2123";
			transportFromOMAN.JW_RL_NKLoadPort = "ITMIL";
			transportFromOMAN.JW_RL_NKDiscPort = "AUSYD";
			transportFromOMAN.JW_ATD = new ZDateTime(2005, 12, 12);
			transportFromOMAN.JW_ETA = new ZDateTime(2005, 12, 14);
			JASForwardingConsol consolFromOHBL = Factory.NewWithValidTestData<JASForwardingConsol>();
			consolFromOHBL.JK_UniqueConsignRef = "FROMOHBL";
			consolFromOHBL.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolFromOHBL.JK_MasterBillNum = "102";
			Transport transportFromOHBL = consolFromOHBL.Transports[0];
			transportFromOHBL.JW_Vessel = vesselToUse.RV_Code;
			transportFromOHBL.JW_VoyageFlight = "X885";
			transportFromOHBL.JW_RL_NKLoadPort = "ITMIL";
			transportFromOHBL.JW_RL_NKDiscPort = "AUSYD";
			transportFromOHBL.JW_ETD = new ZDateTime(2005, 12, 12);
			transportFromOHBL.JW_ETA = new ZDateTime(2005, 12, 14);
			Factory.Save();
		}

		NotificationBuffer fNotificationBuffer;
		JASCsvLineForTest fFields;
		JASCsvLineForTest fHEADRecordFields;
		#endregion
	}
}
