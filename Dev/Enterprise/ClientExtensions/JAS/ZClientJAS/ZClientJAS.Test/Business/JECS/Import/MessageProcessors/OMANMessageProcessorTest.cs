using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class OMANMessageProcessorTest : AirOceanMessageProcessorTestCase
	{
		#region TestProcessRecords1
		public void TestProcessRecords1()
		{
			OMANMessageProcessor processor = (OMANMessageProcessor)GetNewMessageProcessor(RecordsForTest1);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_AgentsReference, "01406005003509");
			JASForwardingConsol[] consols = Factory.Load<JASForwardingConsol>(filter);
			AssertEquals("There should only be one consol", 1, consols.Length);
			AssertConsolAfterProcessingRecordsForTest1(consols[0]);
		}

		JXCRecord[] RecordsForTest1
		{
			get
			{
				if (fRecordsForTest1 == null)
				{
					fRecordsForTest1 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;USSEA;AUCOR;USCOR;AUSYD"), new OMANRecord(JXCConstants.LineTypes.OMAN, "n;01406005003509;KAPITAN MASLOV;615SB;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;23/05/2005;13/06/2005"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;01406005003508;USSEA;406005003508;30;USYVR;FANE;FESCO AUSTRALIA NORTH AMERICA LINE;1087 DOWNTOWER BLVD;SUITE 100;MOBILE, AL 36609;;NA0059288;CONTINENTAL FOOD SALES INC;600 WINSLOW WAY E;SUITE 231;BAINBRIDGE ISLAND, WA 98110;Not Known;wa;;CA;;Y;2068427440;COFO00;;ATYS AUSTRALIA;200 GEORGE DOWNES DR.;CENTRAL MANGROVE;NSW AUSTRALIA 2250;SYDNEY;00;00000;AU;;Y;;COFO00ATYS AUS;;JAS FORWARDING (USA), INC. SEA;11521 EAST MARGINAL WAY, SUITE 120;SEATTLE, WA 98168;;;2122R/055731;VANCOUVER, BC, CANAD;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUSYD;USD;0;P;SYDNEY, AUS.;KAPITAN MASLOV;Voy:615SB;CA;;23/05/2005;;;;1;PCS;51200.000;L;0;4362.00;40FT REEFER CONTAINER SAID TO CONTA;1600-30# CASES FROZEN IQF;BLUEBERRIES;(VENTS CLOSED-TEMP.@-18 DEGREES C);CAED#02A200BC080820050500023;EXPRESS RELEASE;;;08;Optional Field;;23/05/2005;SEATTLE;UTC;JAS FORWARDING WORLDWIDE PTY LTD;UNIT 12, BLDG C, 2-12 BEAUCHAMP RD;BANKSMEADOW NSW 2019;AUSTRALIA;Tel:01161283362888;Fax:01161283362800;NOT KNOWN;LADEN ON BOARD/05.23.05/;ORIGIN;;;One;;1;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "SM6180 / PO #9716;S"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461608;NOT KNOWN;;4;51200.000;1.00;1;BLUEBERRIES, CULTIVATED, FROZE;;;0;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "690;SAFe (Security Admin. Fee;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "621;NVOCC HBL FREIGHT PREPAID;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "639;NVOCC BILL OF LADING FEE;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "649;NVOCC ISPS PORT FEE;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "625;NVOCC DRAYAGE;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "637;NVOCC HANDLING FEES;1.00;P;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "649;NVOCC CARRIER B/L FEE;1.00;P;USD"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;NV05050449;HKHKG;NV05050449;30;HKHKG;OOCL;OOCL;;;;;;POLK AUDIO;C/O ORIENTAL LOGISTICS CO., LTD;1-11 KA TING ROAD,;KWAI CHUNG, N.T.,;HKHKG;N/A;;HK;;n;;93-18117;;ASSOCIATED MARKETING GROUP;88 ENTERPRISE AVENUE;BERWICK (MELBOURNE), 3806;AUSTRALIA;MEL;N/A;;AU;;n;;10985;(10985);JAS FORWARDING (HK) LIMITED;UNIT B, 5/F., MTL WARHOUSE BULIDING;PHASE 1 BERTH ONE, KWAI CHUNG;CONTAINER TERMINALS, KWAI CHUNG NT;;;HONG KONG;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUMEL;USD;184.80;C;MELBOURNE;CSCL KELANG;068S;HK;;16/05/2005;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;2;CTN;702.770;K;4.620;;SAID TO CONTAIN;;;PLTS(55 CTNS);HI-FI LOUDSPEAKER   ;   ;  ; -FREIGHT COLLECT- ;ZZ;TWO (2) PALLETS ONLY;;16/05/2005;HONG KONG;CCT;JAS FORWARDING WORLDWIDE PTY LTD;GROUND FLOOR, THE MILLS;200 ARDEN STREET, NORTH MELBOURNE,;VICTORIA 3051, AUSTRALIA;MELBOURNE;;HKHKG;;;;;3;MELBOURNE;0;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050178;S"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050178;C"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest1;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest1(JASForwardingConsol consol)
		{
			AssertEquals("Should be imported from HEAD", "AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("Should be imported from HEAD", "AUSYD", consol.Transports.ArrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from OMAN", "615SB", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Should be imported from OMAN", new ZDateTime(2005, 6, 13), consol.Transports.ArrivalTransport.JW_ETA);
			AssertEquals("Should be imported from OHBLs", 2, consol.Shipments.Count);
			JASForwardingShipment shipment1 = FindShipmentByHouseBillNumber(consol, "406005003508");
			AssertEquals("Should be imported from OHBL", 51200m, shipment1.JS_ActualWeight);
			AssertEquals("Should be imported from OHBL", new ZDateTime(2005, 05, 23), shipment1.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "SM6180 / PO #9716", shipment1.JS_BookingReference);
			AssertEquals("Should be imported from CONT", "TRIU8461608", consol.Containers[0].JC_ContainerNum);
			AssertEquals("Should be imported from CONT", "BLUEBERRIES, CULTIVATED, FROZE", shipment1.OuterPackLines[0].JL_Description);
			JASForwardingShipment shipment2 = FindShipmentByHouseBillNumber(consol, "NV05050449");
			AssertEquals("Should be imported from OHBL", 702.77m, shipment2.JS_ActualWeight);
			AssertEquals("Should be imported from OHBL", new ZDateTime(2005, 05, 16), shipment2.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "65050178", shipment2.JS_BookingReference);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(3, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment1);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[2], shipment2);
		}

		JXCRecord[] fRecordsForTest1;
		#endregion
		#region TestProcessRecords2
		public void TestProcessRecords2()
		{
			OMANMessageProcessor processor = (OMANMessageProcessor)GetNewMessageProcessor(RecordsForTest2);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_AgentsReference, "S6411357");
			JASForwardingConsol[] consols = Factory.Load<JASForwardingConsol>(filter);
			AssertEquals("There should only be one consol", 1, consols.Length);
			AssertConsolAfterProcessingRecordsForTest2(consols[0]);
		}

		JXCRecord[] RecordsForTest2
		{
			get
			{
				if (fRecordsForTest2 == null)
				{
					fRecordsForTest2 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;CNSHA;AUCOR;CNSHA;AUSYD"), new OMANRecord(JXCConstants.LineTypes.OMAN, "n;S6411357;YANG JIANG HE;353S;SHANGHAI;SYDNEY;CNSHA;CNSHA;AUSYD;AUSYD;16/11/2004;12/11/2004;27/11/2004"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;MSHA0411315;CNSHA;MSHA0411315;30;CNSHA;EURA;EURASIA;;;;;EURFL04N02660S;1)WENZHOU LIGHT INDUSTRIAL PRODUCTS;& CRAFTS IMPORT & EXPORT CO.,LTD.;2)ZHENJIANG WANXI OPTICAL GLASSES C;3)DANYANG EAST LAKE OPTICAL CASE CO;CHINA;N/A;;CN;;n;;000000;;BONASTAR PTY LTD.PO BOX 438 SPIT;JUNCTION,NSW 2088,AUSTRALIA;TEL:00612-92811988;FAX:00612-92127228;SYD;N/A;;AU;;n;;000000;;SHANGHAI JAS INT'L CARGO TRANS CO.,LTD.;ROOM 405, KUN YANG INT'L BUSINESS ;PLAZA NO.798 ZHAO JIA BANG ROAD;200030 SHANGHAI, P.R.C.;;;SHANGHAI;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUSYD;CNY;0.00;C;SYDNEY;YANG JIANG HE;153S;PA;;12/11/2004;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;93;CTN;1313.000;K;5.603;;CTNS         CR-39 LENS;SW/M-GOGGLES;LENSES;OPTICAL GASE;   ;   ;  ; FREIGHT COLLECT ;ZZ;SAY NINETY THREE CARTONS ONLY.;;12/11/2004;SHANG HAI;CCT;JAS FORWARDING WORLDWIDE PTY LTD;UNIT 12,BUILDING C 2-12, BEAUCHAMP ;BANKSMEADOW, NSW 2019;P.O.BOX 645;BOTANY, NSW 1455;;SHANGHAI;;;;;0;SYDNEY;0;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "S6411357;S"), new REFRRecord(JXCConstants.LineTypes.REFR, "S6411357;C"), new DummyHouseRecord(JXCConstants.LineTypes.DOHB, "N;TRAFFICNO#123;AUCOR;HB103"), new DummyHouseRecord(JXCConstants.LineTypes.DOHB, "N;TRAFFICNO#123;AUCOR;HB104"), new DummyHouseRecord(JXCConstants.LineTypes.DOHB, "N;TRAFFICNO#123;AUCOR;HB105"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest2;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest2(JASForwardingConsol consol)
		{
			AssertEquals("Should be imported from HEAD", "AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("Should be imported from HEAD", "AUSYD", consol.Transports.ArrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from OMAN", "353S", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Should be imported from OMAN", new ZDateTime(2004, 11, 27), consol.Transports.ArrivalTransport.JW_ETA);
			AssertEquals("Should be imported from DOHBs and OHBL", 4, consol.Shipments.Count);
			JASForwardingShipment shipment1 = FindShipmentByHouseBillNumber(consol, "MSHA0411315");
			AssertEquals("Should be imported from OHBL", 1313m, shipment1.JS_ActualWeight);
			AssertEquals("Should be imported from OHBL", new ZDateTime(2004, 11, 12), shipment1.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "S6411357", shipment1.JS_BookingReference);
			JASForwardingShipment shipment2 = FindShipmentByHouseBillNumber(consol, "HB103");
			AssertNotNull(shipment2);
			JASForwardingShipment shipment3 = FindShipmentByHouseBillNumber(consol, "HB104");
			AssertNotNull(shipment3);
			JASForwardingShipment shipment4 = FindShipmentByHouseBillNumber(consol, "HB105");
			AssertNotNull(shipment4);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(5, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment1);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[2], shipment2);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[3], shipment3);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[4], shipment4);
		}

		JXCRecord[] fRecordsForTest2;
		#endregion
		#region TestProcessRecords_NoOHBLRecord
		public void TestProcessRecords_NoOHBLRecord()
		{
			OMANMessageProcessor processor = (OMANMessageProcessor)GetNewMessageProcessor(RecordsForNoOHBLRecord);
			Assert("Should be unsuccessful, no house bill of lading record", !processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_AgentsReference, "S6411357");
			JASForwardingConsol[] consols = Factory.Load<JASForwardingConsol>(filter);
			AssertEquals("There should be no consol created", 0, consols.Length);
			AssertContainNotification("Should contain OHBL error notification", NotificationBuffer, ErrorType.InvalidFileFormat, "Consolidation has to have at least one House Bill of Lading record");
		}

		JXCRecord[] RecordsForNoOHBLRecord
		{
			get
			{
				if (fRecordsForTest2 == null)
				{
					fRecordsForTest2 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;CNSHA;AUCOR;CNSHA;AUSYD"), new OMANRecord(JXCConstants.LineTypes.OMAN, "n;S6411357;YANG JIANG HE;353S;SHANGHAI;SYDNEY;CNSHA;CNSHA;AUSYD;AUSYD;16/11/2004;12/11/2004;27/11/2004"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest2;
			}
		}

		#endregion
		#region TestProcessRecords_UpdateExistingConsol
		public void TestProcessRecords_UpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASForwardingConsol consol = CreateExistingConsol();
			OMANMessageProcessor processor = (OMANMessageProcessor)GetNewMessageProcessor(RecordsForTest2);
			AssertEquals("Pre-condition", 1, consol.Containers.Count);
			ForwardingContainer container = consol.Containers[0];
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertEquals("Should be removed from collection", 0, consol.Containers.Count);
			Assert("Container should be deleted", container.IsDeleted);
			AssertConsolAfterProcessingRecordsForTest2(consol);
		}

		JASForwardingConsol CreateExistingConsol()
		{
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentsReference = "S6411357";
			ForwardingContainer container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			Factory.Save();
			return consol;
		}

		#endregion
		#region TestProcessRecords_ShouldNotUpdateExistingConsol
		public void TestProcessRecords_ShouldNotUpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASForwardingConsol consol = CreateExistingConsol();
			OMANMessageProcessor processor = (OMANMessageProcessor)GetNewMessageProcessor(RecordsForTest2);
			AssertEquals("Pre-condition", 1, consol.Containers.Count);
			ForwardingContainer container = consol.Containers[0];
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertEquals("Should be removed from collection", 0, consol.Containers.Count);
			Assert("Container should be deleted", container.IsDeleted);
			AssertConsolAfterProcessingRecordsForTest2ButWithoutUpdatingConsol(consol);
		}

		void AssertConsolAfterProcessingRecordsForTest2ButWithoutUpdatingConsol(JASForwardingConsol consol)
		{
			AssertNull("Should not update Consol from HEAD", consol.ReceivingForwarder);
			AssertEquals("Should not update Consol from HEAD", "", consol.FreightDest);
			AssertEquals("Should not update Consol from OMAN", "", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Should not update Consol from OMAN", ZDateTime.Empty, consol.JK_JX_JB_E_ARV);
			AssertEquals("Should import shipment from DOHBs and OHBLs if they don't already exist", 4, consol.Shipments.Count);
			JASForwardingShipment shipment1 = FindShipmentByHouseBillNumber(consol, "MSHA0411315");
			AssertEquals("Should be imported from OHBL", 1313m, shipment1.JS_ActualWeight);
			AssertEquals("Should be imported from OHBL", new ZDateTime(2004, 11, 12), shipment1.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "S6411357", shipment1.JS_BookingReference);
			JASForwardingShipment shipment2 = FindShipmentByHouseBillNumber(consol, "HB103");
			AssertNotNull(shipment2);
			JASForwardingShipment shipment3 = FindShipmentByHouseBillNumber(consol, "HB104");
			AssertNotNull(shipment3);
			JASForwardingShipment shipment4 = FindShipmentByHouseBillNumber(consol, "HB105");
			AssertNotNull(shipment4);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(4, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], shipment1);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment2);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[2], shipment3);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[3], shipment4);
			AssertContainConsolNotUpdatedWarningNotification(NotificationBuffer, consol);
		}

		#endregion
		protected override AirOceanMessageProcessor GetNewAirOceanMessageProcessor(JXCRecord[] records)
		{
			return new OMANMessageProcessor(records);
		}
	}
}
