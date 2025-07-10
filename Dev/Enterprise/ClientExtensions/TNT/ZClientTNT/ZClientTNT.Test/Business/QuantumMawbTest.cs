using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(QuantumMawb))]
	public class QuantumMawbTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRelatedShipmentsInFile()
		{
			QuantumConsolRecord consol = new QuantumConsolRecord("Consol");
			QuantumShipmentRecord shipment1 = QuantumShipmentRecord.New("1", "1", "Shipment1");
			QuantumShipmentRecord shipment2 = QuantumShipmentRecord.New("2", "2", "Shipment2");
			QuantumShipmentNotesRecord[] shipmentNotes = Array.Empty<QuantumShipmentNotesRecord>();
			QuantumSegment segment = new QuantumSegment(consol, new QuantumShipmentRecord[] { shipment1, shipment2 }, shipmentNotes);
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			AssertEquals("RelatedShipmentsInFile", 2, mawb.RelatedShipmentsInFile);
			segment = new QuantumSegment(consol, new QuantumShipmentRecord[] { shipment2 }, shipmentNotes);
			mawb = new QuantumMawb(Factory, segment);
			AssertEquals("RelatedShipmentsInFile", 1, mawb.RelatedShipmentsInFile);
		}

		public void TestLinkMawbShipmentsToConsolCreatingNonExisting()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			NotificationBuffer buffer = new NotificationBuffer();
			QuantumConsolRecord quantumConsol = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord quantumShipment1 = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			QuantumShipmentRecord quantumShipment2 = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			quantumShipment2.HouseBill = "HWBDUMMY02";
			QuantumShipmentNotesRecord shipmentNote1 = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumShipmentNotesRecord shipmentNote2 = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			shipmentNote2.HouseBill = "HWBDUMMY02";
			QuantumSegment segment = new QuantumSegment(quantumConsol, new QuantumShipmentRecord[] { quantumShipment1 }, new QuantumShipmentNotesRecord[] { shipmentNote1 });
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2006, 1, 11);
			transport.JW_ETA = new ZDateTime(2006, 1, 18);
			transport.JW_ETD = new ZDateTime(2006, 1, 12);
			transport.JW_ETA = new ZDateTime(2006, 1, 19);
			consol.JK_MasterBillNum = "MAWBDUMMY125";
			transport.JW_VoyageFlight = "QF0151";
			consol.JK_UniqueConsignRef = "C00001000";
			Factory.Save();
			int shipmentCount = Factory.GetDatabaseCount(typeof(CommonShipment));
			TNTProgressEventHandler progressHandler = new TNTProgressEventHandler(Mawb_OnProgress);
			HousebillsInInterfaceFileList.Instance.Clear();
			ProgessMessags.Clear();
			try
			{
				mawb.OnProgress += progressHandler;
				mawb.LinkMawbShipmentsToConsolCreatingNonExisting(buffer, false);
			}
			finally
			{
				mawb.OnProgress -= progressHandler;
			}

			AssertEquals("No new shipment created", shipmentCount, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("ProgessMessags.Count", 0, ProgessMessags.Count);
			AssertEquals("Mawb.HousebillsInInterfaceFile.Count", 0, HousebillsInInterfaceFileList.Instance.NoOfShipmentLinkedToConsol(mawb.LinkedConsolUniqueConsignRef));
			mawb.LinkedConsol = consol;
			try
			{
				HousebillsInInterfaceFileList.Instance.Clear();
				mawb.OnProgress += progressHandler;
				mawb.LinkMawbShipmentsToConsolCreatingNonExisting(buffer, false);
			}
			finally
			{
				mawb.OnProgress -= progressHandler;
			}

			Factory.Save();
			AssertEquals("One more new shipment created", shipmentCount + 1, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("ProgessMessags", true, ProgessMessags.Contains("Processing HouseBill: HWBDUMMY01"));
			AssertEquals("Consol.Shipments.Count", 1, consol.Shipments.Count);
			CommonShipment shipment1 = consol.Shipments[0];
			AssertEquals("Housebill", "HWBDUMMY01", shipment1.JS_HouseBill);
			AssertEquals("Shipment1.JS_E_DEP", consol.JK_JX_JA_E_DEP, shipment1.JS_E_DEP);
			AssertEquals("Shipment1.JS_E_ARV", consol.JK_JX_JB_E_ARV, shipment1.JS_E_ARV);
			AssertEquals("Mawb.HousebillsInInterfaceFile should contain Shipment1.PK", true, HousebillsInInterfaceFileList.Instance.Contains(mawb.LinkedConsolUniqueConsignRef, shipment1.PK));
			AssertEquals("Detailed Goods Description", true, shipment1.DetailedGoodsDescriptionNoteText.Contains("DOCUMENTS AND DOCS.IN FOLDER"));
			segment = new QuantumSegment(quantumConsol, new QuantumShipmentRecord[] { quantumShipment2 }, new QuantumShipmentNotesRecord[] { shipmentNote2 });
			mawb = new QuantumMawb(Factory, segment);
			mawb.LinkedConsol = consol;
			shipmentCount = Factory.GetDatabaseCount(typeof(CommonShipment));
			HousebillsInInterfaceFileList.Instance.Clear();
			ProgessMessags.Clear();
			try
			{
				mawb.OnProgress += progressHandler;
				mawb.LinkMawbShipmentsToConsolCreatingNonExisting(buffer, false);
			}
			finally
			{
				mawb.OnProgress -= progressHandler;
			}

			AssertEquals("One more new shipment created", shipmentCount + 1, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("ProgessMessags", true, ProgessMessags.Contains("Processing HouseBill: HWBDUMMY02"));
			AssertEquals("Consol.Shipments.Count", 1, consol.Shipments.Count);
			CommonShipment shipment2 = consol.Shipments[0];
			AssertEquals("Housebill", "HWBDUMMY02", shipment2.JS_HouseBill);
			AssertEquals("Shipment1 has been removed from Consol", true, shipment2.PK != shipment1.PK);
			AssertEquals("Shipment2.JS_E_DEP", consol.JK_JX_JA_E_DEP, shipment2.JS_E_DEP);
			AssertEquals("Shipment2.JS_E_ARV", consol.JK_JX_JB_E_ARV, shipment2.JS_E_ARV);
			AssertEquals("Mawb.HousebillsInInterfaceFile should contain Shipment2.PK", true, HousebillsInInterfaceFileList.Instance.Contains(mawb.LinkedConsolUniqueConsignRef, shipment2.PK));
			AssertEquals("Detailed Goods Description", true, shipment2.DetailedGoodsDescriptionNoteText.Contains("DOCUMENTS AND DOCS.IN FOLDER"));
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declaration = JobDeclaration.New(newFactory);
			declaration.JE_JS = shipment1.PK;
			newFactory.Save();
			segment = new QuantumSegment(quantumConsol, new QuantumShipmentRecord[] { quantumShipment1 }, new QuantumShipmentNotesRecord[] { shipmentNote1 });
			mawb = new QuantumMawb(Factory, segment);
			mawb.LinkedConsol = consol;
			HousebillsInInterfaceFileList.Instance.Clear();
			ProgessMessags.Clear();
			try
			{
				mawb.OnProgress += progressHandler;
				mawb.LinkMawbShipmentsToConsolCreatingNonExisting(buffer, false);
			}
			finally
			{
				mawb.OnProgress -= progressHandler;
			}

			AssertEquals("No new shipment created", shipmentCount + 1, Factory.GetDatabaseCount(typeof(CommonShipment)));
			AssertEquals("ProgessMessags", true, ProgessMessags.Contains("Processing HouseBill: HWBDUMMY01"));
			AssertEquals("Consol.Shipments.Count", 1, consol.Shipments.Count);
			CommonShipment shipment3 = consol.Shipments[0];
			AssertEquals("Shipment3 is Shipment1", shipment1.PK, shipment3.PK);
			AssertEquals("Housebill", "HWBDUMMY01", shipment3.JS_HouseBill);
			AssertEquals("Shipment3.JS_E_DEP", consol.JK_JX_JA_E_DEP, shipment3.JS_E_DEP);
			AssertEquals("Shipment3.JS_E_ARV", consol.JK_JX_JB_E_ARV, shipment3.JS_E_ARV);
			AssertEquals("Mawb.HousebillsInInterfaceFile should contain Shipment3.PK", true, HousebillsInInterfaceFileList.Instance.Contains(mawb.LinkedConsolUniqueConsignRef, shipment3.PK));
			AssertEquals("Detailed Goods Description", true, shipment3.DetailedGoodsDescriptionNoteText.Contains("DOCUMENTS AND DOCS.IN FOLDER"));
			AssertEquals("Declaration.JE_DateAtOrigin", consol.JK_JX_JA_E_DEP, declaration.JE_DateAtOrigin);
			AssertEquals("Declaration.JE_DateOfArrival", consol.JK_JX_JB_E_ARV, declaration.JE_DateOfArrival);
			AssertEquals("Declaration.JE_MasterBill", consol.JK_MasterBillNum, declaration.JE_MasterBill);
			AssertEquals("Declaration.JE_VoyageFlightNo", consol.JK_JX_JV_VoyageFlight, declaration.JE_VoyageFlightNo);
		}

		public void TestNoDeclarationIsCreated()
		{
			TestCaseHelper.ClearTable(JobDeclarationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobShipmentSchema.Constants.TableName);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			QuantumConsolRecord consolRecord = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord shipment = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			QuantumShipmentNotesRecord shipmentNote = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumSegment segment = new QuantumSegment(consolRecord, new QuantumShipmentRecord[] { shipment }, new QuantumShipmentNotesRecord[] { shipmentNote });
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			int declarationsinDataBase = Factory.GetDatabaseCount(typeof(JobDeclaration));
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2006, 5, 10);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2006, 5, 12);
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "QF0151";
			consol.JK_MasterBillNum = "MAWBDUMMY125";
			Factory.Save();
			mawb.LinkedConsol = consol;
			mawb.LinkMawbShipmentsToConsolCreatingNonExisting(new NotificationBuffer(), true);
			Factory.Save();
			AssertEquals("One Shipment should be created", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("Declaration should NOT be Created", 0, Factory.GetDatabaseCount(typeof(JobDeclaration)));
		}

		[ExpectNoExceptions("Syntax exception near 'WHERE' clause, make sure JobSailing is join on something")]
		public void TestSearchMatchingConsols()
		{
			QuantumConsolRecord consolRecord = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord shipmentRecord = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			QuantumShipmentNotesRecord shipmentNote = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumSegment segment = new QuantumSegment(consolRecord, new QuantumShipmentRecord[] { shipmentRecord }, new QuantumShipmentNotesRecord[] { shipmentNote });
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			mawb.SearchFlightNumber = "QF0001";
		}

		public void TestSearchMatchingConsolsIsLinkedFalse()
		{
			Transport transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_ParentGUID = Factory.NewWithValidTestData<CommonConsol>().PK;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "QF101";
			Factory.Save();
			QuantumConsolRecord consolRecord = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord shipmentRecord = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			QuantumShipmentNotesRecord shipmentNote = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumSegment segment = new QuantumSegment(consolRecord, new QuantumShipmentRecord[] { shipmentRecord }, new QuantumShipmentNotesRecord[] { shipmentNote });
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			mawb.SearchFlightNumber = "QF101";
			MainFormConsolCollection consolCollection = mawb.SearchMatchingConsols;
			AssertEquals("Console Collection", 1, consolCollection.Count);
		}

		public void TestLoadNaturalUsingSailingFilter()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "BA0151";
			voyage.JV_AirSeaRoad = Core.Constants.TransportCodes.Air;
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_E_DEP = new ZDateTime(2005, 7, 15);
			origin.JA_RL_NKPortOfLoading = "SIN";
			origin.JA_JV = voyage.PK;
			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "SYD";
			destination.JB_JV = voyage.PK;
			JobSailing sailing = consol.JobSailingList.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			consol.Transports.MostInterestingTransport.JW_JX = sailing.PK;
			Factory.Save();
			AssertLoadNaturalConsolWithEmptyMasterbillNum();
		}

		public void TestLoadNaturalUsingTransportFilter()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "BA0151";
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2005, 7, 15);
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "SYD";
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "SIN";
			Factory.Save();
			AssertLoadNaturalConsolWithEmptyMasterbillNum();
		}

		void AssertLoadNaturalConsolWithEmptyMasterbillNum()
		{
			ForwardingConsol miscConsol = Factory.New<ForwardingConsol>();
			miscConsol.Transports.MostInterestingTransport.JW_VoyageFlight = "NZ005";
			miscConsol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2006, 7, 14);
			miscConsol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
			miscConsol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "NZAKL";
			Factory.Save();
			QuantumConsolRecord consolRecord = new QuantumConsolRecord(ConsolRecordString);
			QuantumShipmentRecord shipmentRecord = QuantumShipmentRecord.New("SYD", "", ShipmentRecordString);
			QuantumShipmentNotesRecord shipmentNote = new QuantumShipmentNotesRecord(ShipmentRecordNoteString);
			QuantumSegment segment = new QuantumSegment(consolRecord, new QuantumShipmentRecord[] { shipmentRecord }, new QuantumShipmentNotesRecord[] { shipmentNote });
			segment.Consol.MasterBill = ZString.Empty;
			QuantumMawb mawb = new QuantumMawb(Factory, segment);
			AssertEquals("Should match to 1 consol", 1, mawb.NaturalMatchingConsols.Count);
			Assert("Should not find MiscConsol", mawb.NaturalMatchingConsols[0].PK != miscConsol.PK);
		}

#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			QuantumFile testQuantumFile = QuantumFile.GetFile(TestUtils.TinyFileFullName, new NotificationBuffer(null));
			QuantumSegment segment = testQuantumFile.QuantumSegments[0];
			return new QuantumMawb(Factory, segment);
		}

#region ProgessMessags
		ArrayList ProgessMessags
		{
			get
			{
				if (fProgessMessags == null)
				{
					fProgessMessags = new ArrayList();
				}

				return fProgessMessags;
			}
		}

		ArrayList fProgessMessags;
#endregion
		void Mawb_OnProgress(object sender, TNTProgressEventArgs e)
		{
			ProgessMessags.Add(e.Message);
		}

		const string ConsolRecordString = "01BA0151SINSYD150705AMAWBDUMMY123M0000151  SINSYDTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string ShipmentRecordString = "03HWBDUMMY01ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ShipmentRecordNoteString = "04HWBDUMMY0101123456789012345DOCUMENTS AND DOCS.IN FOLDER                                                  " + "DEscription 2                                                                 " + "Description 3                                                                 " + "MELIAHEX2456789012                                                                                                                                                                                                                .";
		TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestUtils.Dispose();
		}
		#endregion
	}
}
