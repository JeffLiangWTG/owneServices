using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class JXCMessageImporterTest : JXCImportTestCase
	{
		public void TestImportData_EmptyFile()
		{
			string fileName = CreateTestFile("");
			bool importResult = Importer.ImportData(fileName, NotificationBuffer, SourceInfo.EmptySourceInfo);
			Assert("Empty file should not be valid", !importResult);
			Assert("Should have error", NotificationBuffer.HasErrors);
			AssertEquals(JXCConstants.MessageCategories.Unknown, Importer.LastMessageCategoryImported);
		}

		public void TestImportData_InvalidRecord()
		{
			string fileName = CreateTestFile(FileContent_TestImportData_InvalidRecord);
			bool importResult = Importer.ImportData(fileName, NotificationBuffer, SourceInfo.EmptySourceInfo);
			Assert("not a valid file", !importResult);
			Assert("Should have error", NotificationBuffer.HasErrors);
			AssertEquals(JXCConstants.MessageCategories.Unknown, Importer.LastMessageCategoryImported);
		}

		public void TestImportData()
		{
			string fileName = CreateTestFile(FileContent_TestImportData);
			bool importResult = Importer.ImportData(fileName, NotificationBuffer, SourceInfo.EmptySourceInfo);
			Assert("Should return true", importResult);
			Assert("Should not have error", !NotificationBuffer.HasErrors);
			AssertEquals("ProcessRecordsCalled", ((INotificationSubscriberNotification)NotificationBuffer.Events[7]).AdditionalInfo);
			AssertEquals(JXCConstants.MessageCategories.Unknown, Importer.LastMessageCategoryImported);
		}

		public void TestGetNewJXCMessageProcessorFactory()
		{
			AssertEquals(typeof(JXCMessageProcessorFactory), Importer.BaseGetNewJXCMessageProcessorFactory().GetType());
		}

		public void TestImportDataEndToEndTest()
		{
			JXCMessageImporter importer = new JXCMessageImporter();
			string testFileName = CreateTestFile(FileContent_TestImportDataBackToBackTest);
			JASOrgHeader aUCOROrg = Factory.NewWithValidTestData<JASOrgHeader>();
			aUCOROrg.NettingCode = "AUCOR";
			aUCOROrg.OfficeCode = "AUSYD";
			aUCOROrg.OH_Code = "AUCORTST";
			Factory.Save();
			Assert("Should be successful", importer.ImportData(testFileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo));
			AssertImportedData();
			AssertEquals(JXCConstants.MessageCategories.Air, importer.LastMessageCategoryImported);
		}

		public void TestLastMessageCategoryImported()
		{
			JXCMessageImporter importer = new JXCMessageImporter();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			AssertEquals("If nothing has been imported, should default to Unknown", JXCConstants.MessageCategories.Unknown, importer.LastMessageCategoryImported);
			string testFileName = CreateTestFile(FileContent_TestImportData_AirMessage);
			importer.ImportData(testFileName, notificationBuffer, SourceInfo.EmptySourceInfo);
			AssertEquals(JXCConstants.MessageCategories.Air, importer.LastMessageCategoryImported);
			testFileName = CreateTestFile(FileContent_TestImportData_OceanMessage);
			importer.ImportData(testFileName, notificationBuffer, SourceInfo.EmptySourceInfo);
			AssertEquals(JXCConstants.MessageCategories.Ocean, importer.LastMessageCategoryImported);
			// TODO: when financial and misc messages are implemented
			//
			//			TestFileName = CreateTestFile(FileContent_TestImportData_FinancialMessage);
			//			Importer.ImportData(TestFileName, NotificationBuffer);
			//			AssertEquals(JXCConstants.MessageCategories.Financial, Importer.LastMessageCategoryImported);
			//
			//			TestFileName = CreateTestFile(FileContent_TestImportData_MiscMessage);
			//			Importer.ImportData(TestFileName, NotificationBuffer);
			//			AssertEquals(JXCConstants.MessageCategories.Miscellaneous, Importer.LastMessageCategoryImported);
		}

		#region TestJobsAreDisposedWhenFactorySavingFails
		public void TestJobsAreDisposedWhenFactorySavingFails()
		{
			string testFilePath = CreateTestFile("");
			JXCMessageImporterForTestJobsAreDisposedWhenFactorySavingFails importer = new JXCMessageImporterForTestJobsAreDisposedWhenFactorySavingFails();
			importer.ImportData(testFilePath, NotificationBuffer, SourceInfo.EmptySourceInfo);
			Assert("Should be able to create jobs successfully, mutexes should be released OnAfterImportData", importer.CreateShipmentJobs(Factory));
		}

		class JXCMessageImporterForTestJobsAreDisposedWhenFactorySavingFails : JXCMessageImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				additionalTransactionActions = Array.Empty<ITransactionParticipant>();
				CreateShipmentJobs(FactoryProvider.Current);
				return false;
			}

			public bool CreateShipmentJobs(BusinessObjectFactory factory)
			{
				bool result = true;
				if (ShipmentPKs == null)
				{
					CreateShipmentsForTest(factory);
				}

				foreach (ZGuid shipmentPK in ShipmentPKs)
				{
					JASForwardingShipment shipment = factory.Load<JASForwardingShipment>(shipmentPK);
					Job job = new Job.Loader(factory, shipment).TryCreate();
					result &= (job != null);
				}

				return result;
			}

			void CreateShipmentsForTest(BusinessObjectFactory factory)
			{
				if (ShipmentPKs == null)
				{
					ShipmentPKs = new ZGuid[10];
					for (int i = 0; i < 10; i++)
					{
						ShipmentPKs[i] = factory.NewWithValidTestData(typeof(JASForwardingShipment)).PK;
					}

					factory.Save();
				}
			}

			ZGuid[] ShipmentPKs;
		}

		#endregion
		#region Implementation
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

		JXCMessageImporterForTest Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = new JXCMessageImporterForTest();
				}

				return fImporter;
			}
		}

		void AssertImportedData()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobConsolSchema.JK_MasterBillNum, "12937465514");
			JASForwardingConsol consol = Factory.LoadTop1<JASForwardingConsol>(filter);
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("FRPAR", consol.SendingForwarder.NettingCode);
			AssertEquals("FRLYS", consol.SendingForwarder.OfficeCode);
			AssertEquals("AUCOR", consol.ReceivingForwarder.NettingCode);
			AssertEquals("AUSYD", consol.ReceivingForwarder.OfficeCode);
			AssertEquals("AUCORTST", consol.ReceivingForwarder.OH_Code);
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			Assert(consol.JK_OverrideWaybillDefaults);
			AssertEquals(5, consol.AWBHeader.AWBOtherCharges.Count);
			AssertEquals(556.32m, consol.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals(1, consol.Shipments.Count);
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[0];
			AssertEquals("698252", shipment.JS_HouseBill);
			AssertEquals("HAWB 698252", shipment.JS_BookingReference);
			Assert(shipment.JS_OverrideWaybillDefaults);
			AssertEquals(80m, shipment.AWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals(6, shipment.AWBHeader.AWBOtherCharges.Count);
		}

		NotificationBuffer fNotificationBuffer;
		JXCMessageImporterForTest fImporter;
		#region File Contents
		const string FileContent_TestImportData = "HEAD3100\r\nTEST3100\r\nHAWB3100\r\nOTHR3100\r\nREFR3100\r\nTRLR3100";
		const string FileContent_TestImportData_InvalidRecord = "HEAD3100\r\nasldjalskfjlkasjdf\r\nTRLR3100";
		const string FileContent_TestImportDataBackToBackTest = "HEAD3100;AUSYD;FRLYS;AUCOR;FRPAR;AUSYD\r\n" + "MAWB3100;n;1;LYS;129;37465514;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;69125 LYON ST EXUPERY, FRANCE;JAS (JET AIR SERV;FRPAR;JAS FORWARDING WORLDWIDE (PTY);UNIT 12, BUILDING C;BOTANY BAY INDUSTRIAL ESTATE;2-12 BEAUCHAMP RD, BANKSMEADOW NSW;JAS FORWARDING WO;AUCOR;MARTINAIR HOLLAND;1118 ZG SCHIPHOL AIRPORT;AMSTERDAM / THE NETHERLANDS;;MARTINAIR HOLLAND;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;69125 LYON ST EXU;20-4/7164/6915;FRPAR;LYON;;;;;;;;SYD;MP;;;;;EUR;PP;P;P;NVD;EUR;NCV;EUR;SYDNEY;MP9177;19/09/2004;;;;DR NO. 698252;ONE POUCH ATTACHED;FRED;;;;;;X;ZZ;0001;0011.00;K;000000556.32;000000556.32;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000009.45;000000000.00;000000029.15;000000000.00;000000594.92;000000000.00;0011.00;K;000000556.32;000000556.32;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000009.45;000000000.00;000000029.15;000000000.00;000000594.92;000000000.00;JAS FRANCE;17/09/2004;LYON;;;;;;;;;;FR;AU;BP 519 - BAT. SFS;JAS (JET AIR SERV;;;UNIT 12, BUILDING C;JAS FORWARDING WO;;;;\r\n" + "OTHR3100;P;AWA;AWA;000000009.45;;;;;;\r\n" + "OTHR3100;P;CHC;CHC;000000013.95;;;;;;\r\n" + "OTHR3100;P;SCC;SCC;000000010.80;;;;;;\r\n" + "OTHR3100;P;IRC;IRC;000000001.65;;;;;;\r\n" + "OTHR3100;P;MYC;MYC;000000002.75;;;;;;\r\n" + "FBDN3100;0001;;0011.00;K;Q;;;;0000019;00029.28;000000556.32; CONSOL SHIPMENT; CONSOL SHIPMENT;;;\r\n" + "HAWB3100;n;2004698252;FRLYS;FR;698252;LYS;129;37465514;MARTINAIR HOLLAND;1118 ZG SCHIPHOL AIRPORT;AMSTERDAM / THE NETHERLANDS;;;THALES ELECTRON DEVICES;ZI DE VONGY;BP 84;74202 THONON LES BAINS;THONON LES BAINS;74;74202;FR;;N;;F016705;;ENGINEERING DESIGN & SYSTEMS;KARTEL HOLDINGS PTY LTD;3 RACHAEL CLOSE SLOUGH ESTATE;SILVERWATER NSW 2141;SILVERWATER NSW 2;AU;STRALIA;AU;;N;;E044691;;;;;;;;N;;JAS (JET AIR SERVICE) FRANCE;BP 519 - BAT. SFS;ZONE DE FRET;JAS (JET AIR SERV;20-4/7164/6915;FRPAR;LYON;;;;MAWB 129 3746 5514;;;;SYD;;MP;;;;;EUR;CC;C;C;NVD;EUR;NCV;EUR;SYDNEY;MP9177;19/09/2004;;;;HAWB 698252;INV. 450035802;FRED;;;;;;;;;0001;0011.00;K;000000080.00;000000000.00;000000080.00;000000000.00;000000000.00;000000000.00;000000000.00;000000000.00;000000101.80;000000000.00;000000000.00;000000000.00;000000181.80;JAS FRANCE;17/09/2004;LYON;;;;;;NoFhl;N;;;;;;;;;;;\r\n" + "REFR3100;HAWB 698252;S\r\n" + "FBDN3100;0001;;0011.00;K;Q;;;;0000019;ASAGREED;000000080.00; ELECTRON TUBE; ELECTRON TUBE;;;\r\n" + "OTHR3100;C;LTA;TAXE LTA / AWB FEE;000000023.40;;;;;;\r\n" + "OTHR3100;C;FOB;FRAIS DE FOB/FOB CHARGES;000000018.00;;;;;;\r\n" + "OTHR3100;C;EXP;DOU EXP/EXP CUST CLEARANC;000000042.00;;;;;;\r\n" + "OTHR3100;C;SCC;SAFETY COLLECT CHARGE;000000012.00;;;;;;\r\n" + "OTHR3100;C;IRC;RISK SURCHARGE;000000001.65;;;;;;\r\n" + "OTHR3100;C;MYC;FUEL SURCHARGE;000000004.75;;;;;;\r\n" + "TRLR3100\r\n";
		const string FileContent_TestImportData_AirMessage = "HEAD3100;AUMEL;ITMIL;AUCOR;ITMIL;AUMEL\r\n" + "PSAB3100;n;1/0411246;ITMIL;IT;1/0411246;MIL;000;00000000;JET AIR SERVICE  SPA0192191015;;;;;FGV DIV.FORMENTI & GIOVENZANA;S.P.A.;;;VIA CONCORDIA 16;MI;20050;IT;;N;TE  03629471;299019;;CORNALL PTY LTD;145 BAMFIELD RD-PO BOX 112;;;03081 WEST HEIDEL;;XXXXX;IT;;N;TE  02/216921;028068;;;;;;;;N;99/99999;;;;;;ITMIL;MILAN/ITALY;INVOICE N.7826;;;;;;;MEL;;;;;;;EUR;;P;;NVD;EUR;000000860.00;EUR;MELBOURNE;;;;;;;ATTACHED DOCS;;;;;;;;;;1;0184.00;K;000000349.60;000000349.60;;;;;;;;;;000000349.60;;;12/10/2004;MIL;;;;;;;N\r\n" + "REFR3100;INVOICE N.7826;S\r\n" + "FBDN3100;1;;0184.00;K;Q;ZZ;;;00184.0;ASAGREED;000000349.60;FURNITURE ACCESORIESORIGIN ITALY        C&F AIR MELBOURNE   FREIGHT PREPAID     MEAS 120X80X40;FURNITURE ACCESORIESORIGIN ITALY;;;\r\n" + "TRLR3100\r\n";
		const string FileContent_TestImportData_OceanMessage = "HEAD3100;AUSYD;CNSHA;AUCOR;CNSHA;AUSYD\r\n" + "OMAN3100;n;S6411357;YANG JIANG HE;353S;SHANGHAI;SYDNEY;CNSHA;CNSHA;AUSYD;AUSYD;16/11/2004;12/11/2004;27/11/2004\r\n" + "OHBL3100;n;MSHA0411315;CNSHA;MSHA0411315;30;CNSHA;EURA;EURASIA;;;;;EURFL04N02660S;1)WENZHOU LIGHT INDUSTRIAL PRODUCTS;& CRAFTS IMPORT & EXPORT CO.,LTD.;2)ZHENJIANG WANXI OPTICAL GLASSES C;3)DANYANG EAST LAKE OPTICAL CASE CO;CHINA;N/A;;CN;;n;;000000;;BONASTAR PTY LTD.PO BOX 438 SPIT;JUNCTION,NSW 2088,AUSTRALIA;TEL:00612-92811988;FAX:00612-92127228;SYD;N/A;;AU;;n;;000000;;SHANGHAI JAS INT'L CARGO TRANS CO.,LTD.;ROOM 405, KUN YANG INT'L BUSINESS ;PLAZA NO.798 ZHAO JIA BANG ROAD;200030 SHANGHAI, P.R.C.;;;SHANGHAI;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUSYD;CNY;0.00;C;SYDNEY;YANG JIANG HE;153S;PA;;12/11/2004;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;93;CTN;1313.000;K;5.603;;CTNS         CR-39 LENS;SW/M-GOGGLES;LENSES;OPTICAL GASE;   ;   ;  ; FREIGHT COLLECT ;ZZ;SAY NINETY THREE CARTONS ONLY.;;12/11/2004;SHANG HAI;CCT;JAS FORWARDING WORLDWIDE PTY LTD;UNIT 12,BUILDING C 2-12, BEAUCHAMP ;BANKSMEADOW, NSW 2019;P.O.BOX 645;BOTANY, NSW 1455;;SHANGHAI;;;;;0;SYDNEY;0;;N\r\n" + "REFR3100;S6411357;S\r\n" + "REFR3100;S6411357;C\r\n" + "TRLR3100\r\n";
		#endregion
		#region JXCMessageImporterForTest
		class JXCMessageImporterForTest : JXCMessageImporter
		{
			protected override JXCMessageProcessorFactory GetNewJXCMessageProcessorFactory()
			{
				return new JXCMessageProcessorFactoryForTest();
			}

			public JXCMessageProcessorFactory BaseGetNewJXCMessageProcessorFactory()
			{
				return base.GetNewJXCMessageProcessorFactory();
			}

			class JXCMessageProcessorFactoryForTest : JXCMessageProcessorFactory
			{
				public override JXCMessageProcessor NewProcessor(JXCRecord[] records, INotifications notificationSubscriber)
				{
					return new JXCMessageProcessorForTest(records);
				}
			}

			class JXCMessageProcessorForTest : JXCMessageProcessor
			{
				public JXCMessageProcessorForTest(JXCRecord[] records) : base(records)
				{
				}

				protected override Type FirstLineType
				{
					get
					{
						return typeof(HAWBRecord);
					}
				}

				protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
				{
					notificationSubscriber.Notify(new InfoNotification("ProcessRecordsCalled"));
					return true;
				}
			}
		}
		#endregion
		#endregion
	}
}
