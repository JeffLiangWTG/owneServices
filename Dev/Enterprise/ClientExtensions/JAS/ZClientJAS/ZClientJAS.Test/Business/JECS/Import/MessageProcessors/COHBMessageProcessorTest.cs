using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class COHBMessageProcessorTest : OceanMessageProcessorTestCase
	{
		#region TestProcessRecords
		public void TestProcessRecords()
		{
			COHBMessageProcessor processor = (COHBMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "101");
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
					fRecordsForTest = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;ESBCN;AUCOR;ESBCN;AUSYD"), new OHBLRecord(JXCConstants.LineTypes.COHB, "n;BARCELONA;ESBCN;BCN039/04;30;ESBCN;ZZZZ;TALLERES CASALS, S.A.;AVGDA. BARCELONA, 20;08970 SANT JOAN DESP;;;101;TALLERES CASALS, S.A.;AVGDA. BARCELONA, 20;08970 SANT JOAN DESP;;.;;08970;ES;xxx@xx.xx;N;.;203452;;MAJOR WOODWORKING;55 GOW STREET PADSTOW;P.O.BOX 165;EX 02211 PODSTOW NSW;.;.;99999;AU;xxx@xx.xx;N;.;304455;;JAS FORWARDING SPAIN, S.A.;BARCELONA;;;;;BARCELONA;MAJOR WOODWORKING;55 GOW STREET PADSTOW;P.O.BOX 165;EX 02211 PODSTOW NSW;;xxx@xx.xx;N;;;;;;xxx@xx.xx;N;;AUSYD;EUR;000000000000;P;SYDNEY;LT GENOVA;012;GT;;23/10/2004;;;;000003;UNT;000000000712;K;00000002.9;;HERRAMIENTAS;;;;;;;;84;;;23/10/2004;BARCELONA.;GMT;JAS FORWARDING WORLDWIDE PTY LTD;BOTANY BAY INDUSTRIAL ESTATE;UNIT 12 ,2-12 , BEAUCHAMP ROAD;BANKSMEADOW , NSW 2019,AUSTRALIA;;;BARCELONA;MAJOR                    ;DESTINATION;;;THREE;AUSTRALIA;;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "42005;S"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest;
			}
		}

		void AssertConsolAfterProcessingRecordsForTest(JASForwardingConsol consol)
		{
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			AssertEquals("Should be imported from HEAD", "AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from HEAD", "AUCOR", consol.DestNettingCode);
			AssertEquals("Should be imported from COHB", 1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("Should be imported from COHB", 712m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from COHB", new ZDateTime(2004, 10, 23), shipment.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "42005", shipment.JS_BookingReference);
			AssertEquals("No packlines were imported from CONT", 0, shipment.OuterPackLines.Count);
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
			COHBMessageProcessor processor = (COHBMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTest(consol);
		}

		JASForwardingConsol CreateExistingConsol()
		{
			JASForwardingConsol consol = Factory.NewWithValidTestData<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "101";
			Factory.Save();
			return consol;
		}

		#endregion
		#region TestProcessRecords_ShouldNotUpdateExistingConsol
		public void TestProcessRecords_ShouldNotUpdateExistingConsol()
		{
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASForwardingConsol consol = CreateExistingConsol();
			COHBMessageProcessor processor = (COHBMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			AssertConsolAfterProcessingRecordsForTestButWithoutUpdatingConsol(consol);
		}

		void AssertConsolAfterProcessingRecordsForTestButWithoutUpdatingConsol(JASForwardingConsol consol)
		{
			AssertEquals("Should not update Consol from HEAD", "", consol.FreightDest);
			AssertEquals("Should not update Consol from HEAD", "", consol.DestNettingCode);
			AssertEquals("Should import shipment from COHB if doesn't already exist", 1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("Should be imported from COHB", 712m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from COHB", new ZDateTime(2004, 10, 23), shipment.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "42005", shipment.JS_BookingReference);
			AssertEquals("No packlines were imported from CONT", 0, shipment.OuterPackLines.Count);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(1, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], shipment);
			AssertContainConsolNotUpdatedWarningNotification(NotificationBuffer, consol);
		}

		#endregion
		protected override OceanMessageProcessor GetNewOceanMessageProcessor(JXCRecord[] records)
		{
			return new COHBMessageProcessor(records);
		}
	}
}
