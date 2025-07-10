using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class CHABMessageProcessorTest : AWBMessageProcessorTestCase
	{
		#region TestProcessRecords
		public void TestProcessRecords()
		{
			CHABMessageProcessor processor = (CHABMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "16030336633");
			JASForwardingConsol[] consols = Factory.Load<JASForwardingConsol>(filter);
			AssertEquals("There should only be one consol", 1, consols.Length);
			AssertConsolAfterProcessingRecordsForTest(consols[0]);
		}

		JXCRecord[] RecordsForTest
		{
			get
			{
				if (fRecordsForTest == null)
				{
					fRecordsForTest = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUMEL;ITNAP;AUCOR;ITMIL;AUMEL"), new HAWBRecord(JXCConstants.LineTypes.CHAB, "n;7/0400859;ITNAP;IT;7/0400859;NAP;160;30336633;JET AIR SERVICE  SPA0192191015;20090 SEGRATE/MILANO;;;;AVIO S.P.A.;VIALE IMPERO ANG. VIALE ALFA;;;80038 POMIGLIANO;NA;80038;IT;;N;TE  081/3163111;287844;;ADI LIMITED;FINN STREET 3550;;;BENDIGO VICTOR;;XXXXX;AU;;N;99/99999;255945;;;;;;;;N;99/99999;;;;;38/4/7576/0013;;NAPLES;INV.4010409613+4+5+6;;;;;;;MEL;;CX;;;;;EUR;;P;;NVD;EUR;000050812.00;USD;MELBOURNE;CX1050;23/09/2004;;;;ATTACHED COMM.INVOICE AND P.LIST;;;;;;;;;;;4;0172.00;K;000000000.00;000000000.00;;;;;;;;;;;;;20/09/2004;NAP;;;;;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV.4010409613+4+5+6;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "4;;0172.00;K;Q;ZZ;;;00821.0;ASAGREED;000000000.00;PARTS FOR ENGINE    AIRCRAFT            S/TERMS  DDP        TTL CARTONS NO  4   MARKED P/L 376673+  376674+5+6          FREIGHT PREPAID;PARTS FOR ENGINE    AIRCRAFT;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;P.C.S/C;00001.26;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;ATA;P.C.S/C;00021.26;;;;;;"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest(JASForwardingConsol consol)
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
			AssertEquals("Should be imported from HEAD", "AUMEL", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from CHAB", "CX1050", flight1.JW_VoyageFlight);
			AssertEquals("Should not be imported for ConsolExportAWBHeader", 0, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Should be imported from CHAB", 1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("Should be imported from CHAB", 172m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from CHAB", "NAP", shipment.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "INV.4010409613+4+5+6", shipment.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 4, shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 2, shipment.AWBHeader.AWBOtherCharges.Count);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(2, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], consol);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[1], shipment);
		}

		JXCRecord[] fRecordsForTest;
		#endregion
		#region TestProcessRecords_UpdateExistingConsol
		public void TestProcessRecords_UpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASForwardingConsol consol = CreateExistingConsol();
			CHABMessageProcessor processor = (CHABMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTest(consol);
		}

		JASForwardingConsol CreateExistingConsol()
		{
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "16030336633";
			consol.JK_RL_NKLoadPort = "AUPER";
			consol.JK_RL_NKDischargePort = "";
			Transport transport = consol.Transports[0];
			transport.JW_ATD = ZDateTime.Now;
			Factory.Save();
			return consol;
		}

		#endregion
		#region TestProcessRecords_ShouldNotUpdateExistingConsol
		public void TestProcessRecords_ShouldNotUpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASForwardingConsol consol = CreateExistingConsol();
			CHABMessageProcessor processor = (CHABMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTestButWithoutUpdatingConsol(consol);
		}

		void AssertConsolAfterProcessingRecordsForTestButWithoutUpdatingConsol(JASForwardingConsol consol)
		{
			AssertEquals("Should not update Consol from HEAD", "", consol.FreightDest);
			AssertEquals("Should not update Consol from CHAB", "", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Should import shipment from CHAB if doesn't already exist", 1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("Should be imported from CHAB", 172m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from CHAB", "NAP", shipment.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "INV.4010409613+4+5+6", shipment.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 4, shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 2, shipment.AWBHeader.AWBOtherCharges.Count);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(1, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], shipment);
			AssertContainConsolNotUpdatedWarningNotification(NotificationBuffer, consol);
		}

		#endregion
		protected override AWBMessageProcessor GetNewAWBMessageProcessor(JXCRecord[] records)
		{
			return new CHABMessageProcessor(records);
		}
	}
}
