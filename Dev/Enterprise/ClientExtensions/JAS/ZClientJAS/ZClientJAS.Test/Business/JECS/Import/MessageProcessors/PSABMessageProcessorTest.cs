using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class PSABMessageProcessorTest : AWBMessageProcessorTestCase
	{
		#region TestProcessRecords
		public void TestProcessRecords()
		{
			PSABMessageProcessor processor = (PSABMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, NotificationBuffer));
			ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, "10/0403184");
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
					fRecordsForTest = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;ITVCE;AUCOR;ITMIL;AUSYD"), new HAWBRecord(JXCConstants.LineTypes.PSAB, "n;10/0403184;ITVCE;IT;10/0403184;VCE;000;00000000;JET AIR SERVICE  SPA0192191015;;;;;LA NUOVA ERA SRL;VIA BOSCO 17;;;30030 RIVALE DI P;VE;30030;IT;;N;TE  041/5195444;282701;;MOKADOR CAFFE' AUSTRALIA;UNIT 4, 43-51 COLLEGE STREET;;;2111 GLADESVILLE;;XXXXX;AU;;N;99/99999;303132;;;;;;;;N;99/99999;;;;;;ITMIL;VENICE;171;;;;;;;SYD;;;;;;;EUR;;C;C;NVD;EUR;000020502.72;EUR;SYDNEY;;;;;;MARKS  ADDR.;ATTCD  INVOICE;;;;;;;;;;13;0843.00;K;000001770.30;;000001770.30;;;;;;000000260.85;;000000379.35;;000002410.50;;07/10/2004;VCE;;;;000002410.50;;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "171;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "13;;0843.00;K;Q;ZZ;;;00843.0;00002.10;000001770.30;ESPRESSO COFFEE     MACHINE AND SPARE   PARTS               FREIGHT COLLECT     DMS  79.62.58/6                                                  98.62.58/6                                                  35.39.46/1;ESPRESSO COFFEE     MACHINE AND SPA;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MYC;FS/MY;000000252.90;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MCC;SC;000000126.45;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;F.O.B;000000035.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;SAF;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;X RAY;000000023.40;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;INLAN;000000126.45;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "C;MAA;CUST.;000000051.00;;;;;;"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
				}

				return fRecordsForTest;
			}
		}

		void AssertShipmentAfterProcessingRecordsForTest(JASForwardingShipment shipment)
		{
			AssertEquals("Should not be attached to any consol. This is a pre-shipment", 0, shipment.Consols.Count);
			AssertEquals("Should be imported from PSAB", 843m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from PSAB", "VCE", shipment.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "171", shipment.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 13, shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 7, shipment.AWBHeader.AWBOtherCharges.Count);
			BusinessObjectCreatedOrUpdatedNotification[] importNotifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingConsol), typeof(JASForwardingShipment));
			AssertEquals(1, importNotifications.Length);
			AssertBusinessObjectCreatedOrUpdatedNotification(importNotifications[0], shipment);
		}

		JXCRecord[] fRecordsForTest;
		#endregion
		protected override AWBMessageProcessor GetNewAWBMessageProcessor(JXCRecord[] records)
		{
			return new PSABMessageProcessor(records);
		}
	}
}
