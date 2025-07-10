using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class PSBLMessageProcessorTest : OceanMessageProcessorTestCase
	{
		#region TestProcessRecords
		public void TestProcessRecords()
		{
			PSBLMessageProcessor processor = (PSBLMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, "13510153/0");
			JASForwardingShipment[] shipments = (JASForwardingShipment[])Factory.Load(typeof(JASForwardingShipment), filter);
			AssertEquals("There should only be one shipment", 1, shipments.Length);
			AssertShipmentAfterProcessingRecordsForTest(shipments[0]);
		}

		JXCRecord[] RecordsForTest
		{
			get
			{
				if (fRecordsForTest == null)
				{
					fRecordsForTest = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUBNE;ITMIL;AUCOR;ITMIL;AUBNE"), new OHBLRecord(JXCConstants.LineTypes.PSBL, "9;13510153/0;ITPRM;13510153/0;30;ITSPE;MISC;NO DESCR;;;;;;ZIFER DI FERCODINI AND C S.N.C;VIALE RISORGIMENTO, 23;46017 RIVAROLO MANTOVANO (MN);ITALY;RIVAROLO MANTOVAN;;46017;IT;;N;0376/99590;239332;(239332);TO THE ORDER OF SHIPPER;........;........;;ADELAIDE;;00000;AU;;N;99/99999;250137;;CUSTOM AGENCIES SERV.PTY LTD;COWANDILLA   ADELAIDE;;;;;LA SPEZIA;OUTDOOR WORLD PTY LTD,;239 LEACH RD,;TAMBORINE QLD 4270;AUSTRALIA;;;N;;;;;;;N;;AUBNE;USD;00001650.00;C;BRISBANE;BUNGA PELANGI;47E20;;;18/05/2005;NON-CEE TRAFFIC;;;000051;;0002113.00;K;0025.000;;.;SHIPPER'S LOAD STOW AND COUNT;.;+ROOF TOP CAMPERS AS PER PROFORMA;INVOICE 58;.;EXW / RIVAROLO, MANTOVANO (MN), ITALY;.;ZZ;;;18/05/2005;PRM;CET;000000.00;;;;;;BRI;;;;;ONE;;00;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "INV. 144;S"), new CONTRecord(JXCConstants.LineTypes.CONT, "CAXU 639462/5;33662;;1;0002113.00;0025.000;00051;Some Description;;;000.00;EUR"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S001;FREIGHT;00001650.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S004;FREIGHT ADDITIONALS;00000158.40;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S005;CONGESTION;00000171.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S006;CLEARANCE AT ORIGIN;00000085.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S007;B/L FEES;00000050.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S013;T.H.C. ORIGIN;00000185.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S016;PRE-CARRIAGE;00000480.00;C;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "S197;I.S.P.S.;00000025.00;C;USD"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest;
			}
		}

		void AssertShipmentAfterProcessingRecordsForTest(JASForwardingShipment shipment)
		{
			AssertEquals("Should not be attached to any consol. This is a pre-shipment", 0, shipment.Consols.Count);
			AssertEquals("Should be imported from PSBL", 2113m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from PSBL", new ZDateTime(2005, 05, 18), shipment.JS_ShippedOnBoardDate);
			AssertEquals("Should be imported from REFR", "INV. 144", shipment.JS_BookingReference);
			AssertEquals("Should be imported from CONT", "Some Description", shipment.OuterPackLines[0].JL_Description);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(1, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], shipment);
		}

		JXCRecord[] fRecordsForTest;
		#endregion
		protected override OceanMessageProcessor GetNewOceanMessageProcessor(JXCRecord[] records)
		{
			return new PSBLMessageProcessor(records);
		}
	}
}
