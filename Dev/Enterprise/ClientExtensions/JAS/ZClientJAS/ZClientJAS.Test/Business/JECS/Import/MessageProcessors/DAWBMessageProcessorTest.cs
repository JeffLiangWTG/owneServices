using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class DAWBMessageProcessorTest : AWBMessageProcessorTestCase
	{
		#region TestProcessRecords
		public void TestProcessRecords()
		{
			DAWBMessageProcessor processor = (DAWBMessageProcessor)GetNewMessageProcessor(RecordsForTest);
			Assert("Should be successful", processor.ProcessRecords(FactoryProvider, new NotificationBuffer()));
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, "17254302684");
			JASForwardingConsol[] consols = (JASForwardingConsol[])Factory.Load(typeof(JASForwardingConsol), filter);
			AssertEquals("There should only be one consol", 1, consols.Length);
			AssertConsolAfterProcessingRecordsForTest(consols[0]);
		}

		JXCRecord[] RecordsForTest
		{
			get
			{
				if (fRecordsForTest == null)
				{
					fRecordsForTest = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUMEL;ITTRN;AUCOR;ITMIL;AUMEL"), new DAWBRecord(JXCConstants.LineTypes.DAWB, "n;0;TRN;172;54302684;SAGITTARIO ITALY SRL;VIA ROCCIAMELONE, 15;10077 CERETTA S.MAURIZIO C. TO;;011/9263444;ITCTR;ISABELLE MAZZA 13 AVONDALE;COURT GLADSTONE PARK 3043 VICT;ORIA MELBOURNE AUSTRALIA;;99/99999;AUMEL;CARGOLUX AIRLINES INT'L;;;;....;JAS SPA MILAN  ITALY;J.A.S. JET AIR SERVICE S.P.A.;20090 SEGRATE - MILANO ITALY;E1J LIN 7576001;38/4/7576/0013;ITMIL;TRN TURIN;0                VIA R.SANZIO,1;NON-CEE TRAFFIC20090 SEGRATE   MI;P.I.06069340153;CCIAA1062891 MIL;TRIB.201878;VOL 5639 FASC 28;;MEL;CV;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;CV7300;20/10/2004;;;;;;;;;;;;X;;0002;0073.00;K;000000725.00;000000725.00;000000000.00;000000000.00;000000000.00;;;000000031.20;000000000.00;000000037.65;000000000.00;000000793.85;;;;;;;;;;;;;;;;;JAS SPA MILAN ITALY;18/10/2004;MILANO;;;;000000000.00;;;;;;IT;JP;VIA ROCCIAMELONE, 15;011/9263444;;;COURT GLADSTONE PARK 3043 VICT;99/99999;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY;00021.90;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;POST;00014.99;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MAA;SAF;00025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;AWB;00006.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;PKC;P.C.S;00000.76;;;;;;"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "0002;;0073.00;K;Q;ZZ;;;0100.00;00007.25;000000725.00;USED PERSONAL EFFECTS;USED PERSONAL EFFECTS;;;"), new HAWBRecord(JXCConstants.LineTypes.HAWB, "n;92/0462839;ITTRN;IT;92/0462839;TRN;172;54302684;JET AIR SERVICE  SPA0192191015;20090 SEGRATE/MILANO;;;;SAGITTARIO ITALY SRL;VIA ROCCIAMELONE, 15;;;10077 CERETTA S.M;TO;10077;IT;;N;TE  011/9263444;268541;;ISABELLE MAZZA 13 AVONDALE;COURT GLADSTONE PARK 3043 VICT;;;ORIA MELBOURNE AU;;XXXXX;JP;;N;99/99999;059080;;;;;;;;N;99/99999;;;;;38/4/7576/0013;;TURIN;0;;;;;;;MEL;;CV;;;;;EUR;;P;P;NVD;EUR;NCV;EUR;MELBOURNE;CV7300;20/10/2004;;;;;;;;;;;;;;;2;0073.00;K;000000725.00;000000725.00;;;;;;000000031.20;;000000037.65;;000000793.85;;;18/10/2004;CTR;;;;;;NoFhl;N;;;;;;;;;;;"), new REFRRecord(JXCConstants.LineTypes.REFR, "0;S"), new FBDNRecord(JXCConstants.LineTypes.FBDN, "2;;0073.00;K;Q;ZZ;;;00100.0;00007.25;000000725.00;USED PERSONAL EFFECTS;USED PERSONAL EFFECTS;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MYC;FS/MY/C;000000021.90;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;SC   /C;000000010.95;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MAA;SAF  /A;000000025.00;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;I.A.T/C;000000001.46;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;AWA;AWB  /A;000000006.20;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;MCC;POST /C;000000002.58;;;;;;"), new OTHRRecord(JXCConstants.LineTypes.OTHR, "P;PKC;P.C.S/C;000000000.76;;;;;;"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
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
			AssertEquals(Core.Constants.AgentType.Direct, consol.JK_AgentType);
			AssertEquals("Should be imported from HEAD", "AUMEL", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("Should be imported from MAWB", "CV7300", flight1.JW_VoyageFlight);
			AssertEquals("Should be imported from MAWB", "TRN", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from OTHR", 5, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Should be imported from FBDN", 725m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("Should be imported from HAWB", 1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("Should be imported from HAWB", 73m, shipment.JS_ActualWeight);
			AssertEquals("Should be imported from HAWB", "TRN", shipment.AWBHeader.EH_AWBOriginCode);
			AssertEquals("Should be imported from REFR", "0", shipment.JS_BookingReference);
			AssertEquals("Should be imported from FBDN", 2, shipment.AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCPAsInt);
			AssertEquals("Should be imported from OTHR", 7, shipment.AWBHeader.AWBOtherCharges.Count);
		}

		JXCRecord[] fRecordsForTest;
		#endregion
		protected override AWBMessageProcessor GetNewAWBMessageProcessor(JXCRecord[] records)
		{
			return new DAWBMessageProcessor(records);
		}
	}
}
