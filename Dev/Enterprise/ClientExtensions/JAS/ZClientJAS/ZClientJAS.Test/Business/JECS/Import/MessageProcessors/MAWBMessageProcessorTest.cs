using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class MAWBMessageProcessorTest : AWBMessageProcessorTestCase
	{
		#region TestProcessRecords1
		public void TestProcessRecords1()
		{
			MAWBMessageProcessor processor = (MAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest1);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "17672809063");
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
					fRecordsForTest1 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUADL;ITROM;AUADL;ITMIL;AUBNE"), new MAWBRecord(JXCConstants.LineTypes.MAWB, "n;0;ROM;176;72809063;J.A.S. JET AIR SERVICE S.P.A.;VIA FALZAREGO, 60;00100 ROMA                  RM;;06-6583341;ITROM;JAS FORWARDING WWDE PTY LTD;C/O SMITH LEWIS & STAFF PTY L.;UNIT 17,MIAC BLDG INT'L DRIVE;3043      TULLAMARINE VIC AUST;99/99999;AUMEL;EMIRATES;;;;...;JAS SPA ROMA  ITALY;J.A.S. JET AIR SERVICE S.P.A.;00100 ROMA;E1J LIN 7576006;38/4/7576/0061;ITMIL;ROM ROME;DOG RDN MG       VIA TOSCANA 11;NON-CEE TRAFFIC00187 ROMA;;P.I.04281291007;CCIAA 748036;TRIB.3434/92;;MEL;EK;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;EK4040;09/10/2004;;;;ATTACHED ONE ENVELOPE WITH DOCUMENTS;;;***************************************;********  O N E  P A L L E T  *********;***************************************;********** ( 1 8 2  P C S ) ***********;;X;;0182;1768.90;K;000008349.68;000008349.68;000000000.00;000000000.00;000000000.00;;;000000006.20;000000000.00;000000746.07;000000000.00;000009101.95;000000000.00;;;;;;;;;;;;;;;;JAS SPA ROMA ITALY;06/10/2004;ROMA;JAS SPA ROM;;;;;;;;;IT;AU;VIA FALZAREGO, 60;06-6583341;;;C/O SMITH LEWIS & STAFF PTY L.;99/99999;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY/C;00442.25;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;POST /C;00303.31;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;AWB  /A;00006.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;PKC;P.C.S/C;00000.51;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "0182;;1768.90;K;Q;ZZ;;;01769.0;00004.72;000008349.68;CONSOLIDATION AS PERATTACHED CARGO      MANIFEST            *** AMF 5516 EK *** 182 PCS;CONSOLIDATION AS PERATTACHED CARGO;;;"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;3/0405855;ITVIC;IT;3/0405855;MIL;176;72809063;JET AIR SERVICE  SPA0192191015;00054 FIUMICINO ROMA;;;;MARZOTTO SPA DIV. TESSUTI GMF;LARGO SANTA MARGHERITA 1;;;36078 VALDAGNO (V;VI;36078;IT;;N;TE  0445/429678;157041;;PINK ROSE CLOTHING PTY LTD;93-95 MORELAND ST.;;;03011 FOOTSCRAY V;;03011;AU;;N;99/99999;266918;;;;;;;;N;99/99999;;;;;38/4/7576/0061;;MILAN/ITALY;INV. 5449289;;;;;;;MEL;;EK;;;;;EUR;;P;P;NVD;EUR;000014546.59;USD;MELBOURNE;EK4040;09/10/2004;;;;ATTACHED INVOICE AND NO-WOOD PACKING DECLARATION;;;;;;;;;;;17;0303.00;K;000000696.90;000000696.90;;;;;;000000030.60;;000000136.35;;000000863.85;;;05/10/2004;VIC;;;;;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV. 5449289;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "17;;0303.00;K;Q;ZZ;;;00303.0;00002.30;000000696.90;FABRICS             CUSTOM CODE 51123010FREIGHT PREPAID     MARKED ADD. 1/17    17 ROLLS            DIM.CM 168X15X15/17;FABRICS             CUSTOM CODE 511;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY;000000090.90;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;SC;000000045.45;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MAA;X RAY;000000030.60;;;;;;"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;3/0405861;ITVIC;IT;3/0405861;MIL;176;72809063;JET AIR SERVICE  SPA0192191015;00054 FIUMICINO ROMA;;;;VALENTINO S.P.A.;VIA TURATI, 16/18;;;20121 MILANO ITAL;MI;20121;IT;;N;TE  02-624921;263885;;GOODS CONSIGNED TO;WESTPAC BANKING CORPORATION;;;ACCOUNT FINEYIELD;;XXXXX;AU;;N;99/99999;290300;;* SHOP 601/123 COLLINS;STREET MELB VICTORIA 3000;AUSTRALIA;;;;N;99/99999;;;;;38/4/7576/0061;;MILAN/ITALY;INV. 4408252;IRREVOCABLE DOCUMENTARY CREDIT NO;SD3BM842277 LOADING IN CHARGE FROM;AIRPORT IN ITALY FOR TRANSPORT TO;MELBOURNE, AUSTRALIA;;;MEL;;EK;;;;;EUR;;C;C;NVD;EUR;000017156.00;EUR;MELBOURNE;EK4040;09/10/2004;;;;THE GOODS HAVE BEEN ACCEPTED FOR CARRIAGE;;;;;;;;;;;18;0030.00;K;000000761.10;;000000761.10;;;;;;000000217.64;;000000166.38;;000001145.12;;05/10/2004;VIC;;;;000001145.12;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV. 4408252;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "18;;0030.00;K;Q;ZZ;;;00354.0;00002.15;000000761.10;PROFORMA INVOICE    0400181 55 PIECES   SHOES/BAGS          EXW;PROFORMA INVOICE    0400181 55 PIEC;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MYC;FS/MY;000000106.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;SC;000000053.10;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000043.90;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;X RAY;000000032.40;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;I.A.T;000000007.08;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;HDL;000000014.16;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;AWA;AWB;000000005.16;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;PIKUP;000000046.02;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;CUST.;000000051.00;;;;;;"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;5/0409971;ITFLR;IT;5/0409971;FLR;176;72809063;JET AIR SERVICE  SPA0192191015;00054 FIUMICINO ROMA;;;;J.A.S. JET AIR SERVICE S.P.A.;VIUZZO DI PORTO 4/B;;;50010 BADIA A SET;FI;50010;IT;;N;TE  055/737311;008616;;GALLERIA TESSUTI;8 CUBITT STR;;;3121 RICHMOND VIC;;XXXXX;AU;;N;99/99999;290235;;;;;;;;N;99/99999;;;;;38/4/7576/0061;;FLORENCE;VARIOUS;MARKS  FULL ADDRESS 1../;ORIGINAL DOCS ATTACHED;;;;;MEL;;EK;;;;;EUR;;C;C;NVD;EUR;000030407.63;EUR;MELBOURNE;EK4040;09/10/2004;;;;ATTENTION !!!   THIS SHIPMENT IS ON A C.O.D. BASIS.DOCUMENTS;MUST BE RELEASED ONLY AGAINST RECEIPT OF A BANK DRAFT TO THE;ORDER OF SHIPPER AS BENEFICIARY             EUR0000027862.03;;;;;;;;;143;1431.90;K;000003078.80;;000003078.80;;;;;;000000232.11;;000000663.94;;000003974.85;;05/10/2004;FLR;;;;000003974.85;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "VARIOUS;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "143;;1431.90;K;Q;ZZ;;;01432.0;00002.15;000003078.80;FABRICS             ORIGIN EUROPE       FREIGHT COLLECT     --------------------PGILLI COD   866,58COCCI COD    559,80DG TEXT COD 1174,95INWOOL COD  2285,29AVIEM COD  15472,00PRATESI COD 1127,98FABRIC COD  6205,43CAFISSI COD  170,00;FABRICS             ORIGIN EUROPE;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;CODF;000000015.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MYC;FS/MY;000000429.60;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;SC;000000214.80;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000036.15;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000015.49;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000015.49;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000015.49;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000015.49;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000010.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000010.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000010.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000010.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000010.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;I.A.T;000000019.54;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;CUST.;000000069.00;;;;;;"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;10/0403155;ITVCE;IT;10/0403155;VCE;176;72809063;JET AIR SERVICE  SPA0192191015;00054 FIUMICINO ROMA;;;;MAG.PETERSANT PERENZIN & C SRL;VIALE XXIV MAGGIO, 56;;;31015 CONEGLIANO;TV;31015;IT;;N;TE  0438/410450;275866;;HUGO BOSS AUSTRALIA;8-14 ALBERT STREET;;;03072     PRESTON;;03072;AU;;N;99/99999;147829;;;;;;;;N;99/99999;;;;;38/4/7576/0061;;VENICE;1333;;;;;;;MEL;;EK;;;;;EUR;;C;C;NVD;EUR;000000010.00;EUR;MELBOURNE;EK4040;09/10/2004;;;;ATTACHED INVOICE;;;;;;;;;;;4;0004.00;K;000000054.00;;000000054.00;;;;;;000000121.20;;000000015.97;;000000191.17;;05/10/2004;VCE;;;;000000191.17;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "1333;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "4;;0004.00;K;Q;ZZ;;;00024.0;00002.25;000000054.00;KNITWEAR            39X30X30/4;KNITWEAR            39X30X30/4;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MYC;FS/MY;000000007.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;SC;000000003.60;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000030.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;X RAY;000000007.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;I.A.T;000000005.17;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;INLAN;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;CUST.;000000034.00;;;;;;"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest1;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest1(JASForwardingConsol consol)
		{
			Transport flight1 = null;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					flight1 = transport;
					break;
				}
			}

			AssertNotNull(flight1);
			AssertEquals("Should be imported from HEAD", "AUADL", consol.ReceivingForwarder.NettingCode);
			AssertEquals("Should be imported from HEAD", "AUBNE", consol.Transports.ArrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from MAWB", "EK4040", flight1.JW_VoyageFlight);
			AssertEquals("Should be imported from MAWB", "ROM", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from OTHR", 4, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Should be imported from FBDN", 8349.68m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("Should be imported from HAWBs", 4, consol.Shipments.Count);
			JASForwardingShipment shipment1 = FindShipmentByHouseBillNumber(consol, "3/0405855");
			AssertEquals("Should be imported from HAWB", 303m, shipment1.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "MIL", shipment1.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "INV. 5449289", shipment1.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 17, shipment1.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 3, shipment1.AWBHeader.AWBOtherCharges.Count);
			JASForwardingShipment shipment2 = FindShipmentByHouseBillNumber(consol, "3/0405861");
			AssertEquals("Should be imported from HAWB", 30m, shipment2.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "MIL", shipment2.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "INV. 4408252", shipment2.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 18, shipment2.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 10, shipment2.AWBHeader.AWBOtherCharges.Count);
			JASForwardingShipment shipment3 = FindShipmentByHouseBillNumber(consol, "5/0409971");
			AssertEquals("Should be imported from HAWB", 1431.9m, shipment3.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "FLR", shipment3.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "VARIOUS", shipment3.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 143, shipment3.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 15, shipment3.AWBHeader.AWBOtherCharges.Count);
			JASForwardingShipment shipment4 = FindShipmentByHouseBillNumber(consol, "10/0403155");
			AssertEquals("Should be imported from HAWB", 4m, shipment4.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "VCE", shipment4.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "1333", shipment4.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 4, shipment4.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 8, shipment4.AWBHeader.AWBOtherCharges.Count);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(5, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment1);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[2], shipment2);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[3], shipment3);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[4], shipment4);
		}

		JXCRecord[] fRecordsForTest1;
		#endregion
		#region TestProcessRecords2
		public void TestProcessRecords2()
		{
			MAWBMessageProcessor processor = (MAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest2);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "16030336633");
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
					fRecordsForTest2 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUMEL;ITNAP;AUCOR;ITMIL;AUMEL"), new MAWBRecord(JXCConstants.LineTypes.MAWB, "n;0;MIL;160;30336633;J.A.S. JET AIR SERVICE S.P.A.;VIA S. MARIA DEL PIANTO N 84;80100 NAPOLI;;081-5841650-;ITROM;JAS FORWARDING WORLWIDE PTY;C/O SMITH LEWIS & STAFF PTY;UNIT 17, MIAC BLDNG INTL DRIVE;03043 TULLAMARINE,VICTORIA;99/99999;AUMEL;CATHAY PACIFIC AIRWAYS;;;;...;JAS SPA MILAN  ITALY;J.A.S. JET AIR SERVICE S.P.A.;20090 SEGRATE - MILANO ITALY;E1J LIN 7576001;38/4/7576/0013;ITMIL;ROM ROME;DOG.FATTA        VIA BARBERINI 3;NON-CEE TRAFFIC00187 ROMA;P.I. 00862211000;C.F. 07277450156;CCIAA 606898;I.TRIB.ROMA1099/86;;MEL;CX;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;CX1050;23/09/2004;;;;TTL PIECES NO  4 -;TO THIS AWB IS ATTACHED CARGO MANIFEST AND ENVELOPE FOR CNEE;;;;;;;T1;;0004;0172.00;K;000002931.12;000002931.12;000000000.00;000000000.00;000000000.00;;;000000006.20;000000000.00;000000067.48;000000000.00;000003004.80;000000000.00;;;;;;;;;;;;;;;;JAS SPA MILAN ITALY;20/09/2004;MILANO;JAS SPA MIL;;;;;;;;;IT;AU;VIA S. MARIA DEL PIANTO N 84;081-5841650-;;;C/O SMITH LEWIS & STAFF PTY;99/99999;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY/C;00034.40;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;POST /C;00031.82;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;AWB  /A;00006.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;PKC;P.C.S/C;00001.26;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "0004;;0172.00;K;Q;ZZ;;;00621.0;00004.72;000002931.12;CONSOLIDATE SHIPMENTAS PER ATTACHED     CARGO MANIFEST      TTL CARTONS NO  4   TTL VOLUME CBM 3,726FREIGHT PREPAID;CONSOLIDATE SHIPMENTAS PER ATTACHED;;;"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB101"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB102"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;7/0400859;ITNAP;IT;7/0400859;NAP;160;30336633;JET AIR SERVICE  SPA0192191015;20090 SEGRATE/MILANO;;;;AVIO S.P.A.;VIALE IMPERO ANG. VIALE ALFA;;;80038 POMIGLIANO;NA;80038;IT;;N;TE  081/3163111;287844;;ADI LIMITED;FINN STREET 3550;;;BENDIGO VICTOR;;XXXXX;AU;;N;99/99999;255945;;;;;;;;N;99/99999;;;;;38/4/7576/0013;;NAPLES;INV.4010409613+4+5+6;;;;;;;MEL;;CX;;;;;EUR;;P;;NVD;EUR;000050812.00;USD;MELBOURNE;CX1050;23/09/2004;;;;ATTACHED COMM.INVOICE AND P.LIST;;;;;;;;;;;4;0172.00;K;000000000.00;000000000.00;;;;;;;;;;;;;20/09/2004;NAP;;;;;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV.4010409613+4+5+6;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "4;;0172.00;K;Q;ZZ;;;00821.0;ASAGREED;000000000.00;PARTS FOR ENGINE    AIRCRAFT            S/TERMS  DDP        TTL CARTONS NO  4   MARKED P/L 376673+  376674+5+6          FREIGHT PREPAID;PARTS FOR ENGINE    AIRCRAFT;;;"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB103"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB104"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB105"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest2;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest2(JASForwardingConsol consol)
		{
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			Transport flight1 = null;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					flight1 = transport;
					break;
				}
			}

			AssertNotNull(flight1);
			AssertEquals("Should be imported from HEAD", "AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("Should be imported from HEAD", "AUMEL", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from MAWB", "CX1050", flight1.JW_VoyageFlight);
			AssertEquals("Should be imported from MAWB", "MIL", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from OTHR", 4, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Should be imported from FBDN", 2931.12m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("Should be imported from DHABs and HAWB", 6, consol.Shipments.Count);
			JASForwardingShipment shipment1 = FindShipmentByHouseBillNumber(consol, "HB101");
			AssertNotNull(shipment1);
			JASForwardingShipment shipment2 = FindShipmentByHouseBillNumber(consol, "HB102");
			AssertNotNull(shipment2);
			JASForwardingShipment shipment3 = FindShipmentByHouseBillNumber(consol, "7/0400859");
			AssertEquals("Should be imported from HAWB", 172m, shipment3.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "NAP", shipment3.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "INV.4010409613+4+5+6", shipment3.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 4, shipment3.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			JASForwardingShipment shipment4 = FindShipmentByHouseBillNumber(consol, "HB103");
			AssertNotNull(shipment4);
			JASForwardingShipment shipment5 = FindShipmentByHouseBillNumber(consol, "HB104");
			AssertNotNull(shipment5);
			JASForwardingShipment shipment6 = FindShipmentByHouseBillNumber(consol, "HB105");
			AssertNotNull(shipment6);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(7, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment1);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[2], shipment2);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[3], shipment3);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[4], shipment4);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[5], shipment5);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[6], shipment6);
		}

		JXCRecord[] fRecordsForTest2;
		#endregion
		#region TestProcessRecords3
		public void TestProcessRecords3()
		{
			MAWBMessageProcessor processor = (MAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest3);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "16030336639");
			JASForwardingConsol[] consols = Factory.Load<JASForwardingConsol>(filter);
			AssertEquals("There should only be one consol", 1, consols.Length);
			AssertConsolAfterProcessingRecordsForTest3(consols[0]);
		}

		JXCRecord[] RecordsForTest3
		{
			get
			{
				if (fRecordsForTest3 == null)
				{
					fRecordsForTest3 = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUMEL;ITNAP;AUCOR;ITMIL;AUMEL"), new MAWBRecord(JXCConstants.LineTypes.MAWB, "n;0;MIL;160;30336639;J.A.S. JET AIR SERVICE S.P.A.;VIA S. MARIA DEL PIANTO N 84;80100 NAPOLI;;081-5841650-;ITROM;JAS FORWARDING WORLWIDE PTY;C/O SMITH LEWIS & STAFF PTY;UNIT 17, MIAC BLDNG INTL DRIVE;03043 TULLAMARINE,VICTORIA;99/99999;AUMEL;CATHAY PACIFIC AIRWAYS;;;;...;JAS SPA MILAN  ITALY;J.A.S. JET AIR SERVICE S.P.A.;20090 SEGRATE - MILANO ITALY;E1J LIN 7576001;38/4/7576/0013;ITMIL;ROM ROME;DOG.FATTA        VIA BARBERINI 3;NON-CEE TRAFFIC00187 ROMA;P.I. 00862211000;C.F. 07277450156;CCIAA 606898;I.TRIB.ROMA1099/86;;MEL;CX;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;CX1050;23/09/2004;;;;TTL PIECES NO  4 -;TO THIS AWB IS ATTACHED CARGO MANIFEST AND ENVELOPE FOR CNEE;;;;;;;T1;;0004;0172.00;K;000002931.12;000002931.12;000000000.00;000000000.00;000000000.00;;;000000006.20;000000000.00;000000067.48;000000000.00;000003004.80;000000000.00;;;;;;;;;;;;;;;;JAS SPA MILAN ITALY;20/09/2004;MILANO;JAS SPA MIL;;;;;;;;;IT;AU;VIA S. MARIA DEL PIANTO N 84;081-5841650-;;;C/O SMITH LEWIS & STAFF PTY;99/99999;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "0004;;0172.00;K;Q;ZZ;;;00621.0;00004.72;000002931.12;CONSOLIDATE SHIPMENTAS PER ATTACHED     CARGO MANIFEST      TTL CARTONS NO  4   TTL VOLUME CBM 3,726FREIGHT PREPAID;CONSOLIDATE SHIPMENTAS PER ATTACHED;;;"), new DummyHouseRecord(JXCConstants.LineTypes.DHAB, "N;TRAFFICNO#123;AUCOR;HB101"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest3;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest3(JASForwardingConsol consol)
		{
			Transport flight1 = null;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					flight1 = transport;
					break;
				}
			}

			AssertNotNull("Should have found flight1", flight1);
			AssertEquals("Should be imported from HEAD", "AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("Should be imported from HEAD", "AUMEL", consol.Transports.ArrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from MAWB", "CX1050", flight1.JW_VoyageFlight);
			Assert("Should be set to true", consol.JK_OverrideWaybillDefaults);
			AssertEquals("Should be imported from MAWB", "MIL", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from FBDN", 2931.12m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("Should be imported from DHAB", 1, consol.Shipments.Count);
			AssertEquals("Should be imported from DHAB", "HB101", consol.Shipments[0].JS_HouseBill);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(2, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], consol.Shipments[0]);
		}

		JXCRecord[] fRecordsForTest3;
		#endregion
		#region TestProcessRecords_UpdateExistingConsol
		public void TestProcessRecords_UpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASForwardingConsol consol = CreateExistingConsol();
			MAWBMessageProcessor processor = (MAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest3);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTest3(consol);
		}

		JASForwardingConsol CreateExistingConsol()
		{
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "16030336639";
			consol.JK_RL_NKLoadPort = "AUPER";
			Factory.Save();
			return consol;
		}

		#endregion
		#region TestProcessRecords_ShouldNotUpdateExistingConsol
		public void TestProcessRecords_ShouldNotUpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASForwardingConsol consol = CreateExistingConsol();
			MAWBMessageProcessor processor = (MAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest3);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTest3ButWithoutUpdatingConsol(consol);
		}

		void AssertConsolAfterProcessingRecordsForTest3ButWithoutUpdatingConsol(JASForwardingConsol consol)
		{
			AssertNull("Should not update consol from HEAD", consol.ReceivingForwarder);
			AssertEquals("Should not update consol from HEAD", "", consol.FreightDest);
			AssertEquals("Should not update consol from MAWB", "", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Should not update consol from MAWB", "PER", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should not update consol from FBDN", 0m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("Should import shipment from DHAB if doesn't already exist", 1, consol.Shipments.Count);
			AssertEquals("Should import shipment from DHAB if doesn't already exist", "HB101", consol.Shipments[0].JS_HouseBill);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(1, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol.Shipments[0]);
			AssertContainConsolNotUpdatedWarningNotification(NotificationBuffer, consol);
		}

		#endregion
		protected override AWBMessageProcessor GetNewAWBMessageProcessor(JXCRecord[] records)
		{
			return new MAWBMessageProcessor(records);
		}
	}
}
