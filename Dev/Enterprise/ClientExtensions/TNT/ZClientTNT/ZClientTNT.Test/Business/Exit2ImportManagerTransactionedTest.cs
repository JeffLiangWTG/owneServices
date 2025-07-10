using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	public class Exit2ImportManagerTransactionedTest : TestCaseWithFactory
	{
		public void TestLinkingAndUnlinkingOfConsignmentsInInterfaceFile()
		{
			///
			///	This is a specific business case scenario with actual data from TNT that caused the problem - where a shipment 
			/// was not attached to a chosen consol even though it was in the interface file & the output report stated it was attached.
			///
			///	Test Required:
			///	create a consol
			///	create two shipments - 1 that is a match for a shipment contained in the X2 file
			///	attach these two shipments to the consol
			///	run the match process with the given file containing 6 shipments (including 1 of the two previously attached consignments)
			///
			///	assert consol only has 6 linked shipments
			///	assert consol.shipments does not include the shipment that was not in file orginally attached
			///	assert consol.shipments still includes the original shipment attached that is also included in the X2 file
			///
			ForwardingConsol testConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			testConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			testConsol.JK_ConsolMode = "LSE";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_RL_NKDischargePort = "THBKK";
			Transport transport = testConsol.Transports[0];
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now;
			testConsol.JK_IsNeutralMaster = false;
			transport.JW_VoyageFlight = "QF123";
			testConsol.RunPreSaveValidation();
			AssertNoErrors(testConsol);
			NotificationBuffer dummyNotifier = new NotificationBuffer(null);
			QuantumShipmentRecord testRecord = QuantumShipmentRecord.New(DemoBranchCode, "", TestShipmentRecord);
			ForwardingShipment testShipment1 = testRecord.CreateShipment(Factory, false, dummyNotifier);
			QuantumShipmentRecord testRecord2 = QuantumShipmentRecord.New(DemoBranchCode, "", TestShipmentRecordIncludedInInterfaceFile);
			ForwardingShipment testShipment2 = testRecord2.CreateShipment(Factory, false, dummyNotifier);
			testShipment2.JS_CartageWaybill = "ADL-DMG";
			testConsol.Shipments.Add(testShipment1);
			testConsol.Shipments.Add(testShipment2);
			Factory.Save();
			AssertEquals("Consol should now have two shipments attached", 2, testConsol.Shipments.Count);
			Exit2ImportManager testManager = new Exit2ImportManager(Factory, TestUtils.X2TestAttachDetach);
			QuantumMawb selectedMawb = testManager.Exit2Mawbs[0];
			selectedMawb.LinkedConsol = testConsol;
			AssertEquals(selectedMawb.LinkedConsol, testConsol);
			Assert(selectedMawb.IsLinkedToConsol);
			Assert(testManager.IsAnyMawbLinkedToAnEnterpriseConsol);
			testManager.ProcessAllMatches(false);
			AssertEquals("Consol should now have six shipments attached", 6, testConsol.Shipments.Count);
			Assert("Consol should have detached TestShipment1", !testConsol.Shipments.Contains(testShipment1.PK));
			Assert("Consol should still have attached TestShipment2", testConsol.Shipments.Contains(testShipment2.PK));
		}

		public void TestShipmentsFromDifferentQuantumMawbsLinkedToTheSameConsol()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch.GB_Code = "AKL";
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.GB_BranchName = "Aukland";
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Other;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_IsNeutralMaster = false;
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2006, 3, 28);
			transport.JW_VoyageFlight = "QF0400";
			consol.JK_MasterBillNum = "08143779794";
			consol.MasterBillAirlinePrefix = "081";
			Factory.Save();
			NotificationBuffer notify = new NotificationBuffer();
			Exit2ImportManager manager = new Exit2ImportManager(Factory, TestUtils.TwoMawbsWithSameMasterbillFromOneFile);
			AssertEquals("Should be 2 Quantum Mawbs", 2, manager.Exit2Mawbs.Count);
			manager.Exit2Mawbs[0].LinkedConsol = consol;
			manager.Exit2Mawbs[1].LinkedConsol = consol;
			manager.ProcessAllMatches(false);
			AssertEquals("Consol should have 2 Shipments attached", 2, consol.Shipments.Count);
		}

		#region Implementation
		const string TestShipmentRecord = "03930941449 SYDJXO20525452SYDNEY BOUTIQUE WINE           11B REDAN ST                                                  MOSMAN                         NEW SOUTH WALES                AU 2088     0299693601  0299693601  John Smith                                                                                                                                                                                                                                 VILLAGE CELLARS                6-5 UENO , UWADA               HIMI                           HIMI                           TOYAMA                         JP 9350056  0766728680  0766728680  YOSHIKO NAKAMURA                                                                                                                                                                                                                           NS    292115.08JPY     1   522.000ECN                                                          T                         .";
		const string TestShipmentRecordIncludedInInterfaceFile = "03940255153 ADLDMG20003242CLIPSAL INTEGRATED SYSTEMS     PLANT 9 FIFTH ST                                              BOWDEN                         SOUTH AUSTRALIA                AU 5007     0884400570  0884400500  DANSIE                                                                                                                                                                                                                                     CLIPSAL THAILAND CO LTD        VH COMMERCIAL BUILDING 4F      23/1 SOI 9 NGAMWONGWAN ROAD    NONTHABURI                                                    TH 11130    6629527660  6629527660  V ORAWAN                                                                                                                                                                                                                                   NR      9330.89AUD     4    45.620AAAATKT97                                                                              .";
		const string DemoBranchCode = "DEM";
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
