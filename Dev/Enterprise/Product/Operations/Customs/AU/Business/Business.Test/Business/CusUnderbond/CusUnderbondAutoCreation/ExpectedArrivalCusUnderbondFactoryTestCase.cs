using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ExpectedArrivalCusUnderbondFactoryTestCase : TestCaseWithFactory
	{
		public void TestCreateOrLoadUnderbond()
		{
			AssertNotNull(UnderbondFactory.CreateOrLoadUnderbondInternal(ExpectedArrivalMessage, 1));
		}

		public void TestLoadOrCreateUnderbondForNewUnderbond()
		{
			DummyBizoWithUnderbondCollection dummy = Factory.New<DummyBizoWithUnderbondCollection>();

			CusUnderbond underbond = UnderbondFactory.LoadOrCreateUnderbondInternal(dummy, ExpectedArrivalMessage, 1);
			AssertNotNull("LinkedObject", underbond);
			AssertEquals("PopulateCusUnderbondInfoCalled", true, UnderbondFactory.PopulateCusUnderbondInfoCalled);
			AssertEquals("No outturns should exist for normal Underbond Factorys Has outturn", 0, underbond.Outturns.Count);
		}

		public void TestPopulateCusUnderbondInfo()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			UnderbondFactory.PopulateCusUnderbondInfoInternal(underbond, ExpectedArrivalMessage);
			AssertEquals("C4_DestinationPremiseID", "9913C", underbond.C4_DestinationPremiseID);
			AssertEquals("C4_OriginPremiseID", "9914N", underbond.C4_OriginPremiseID);
			AssertEquals("C4_ModeOfMovement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals("C4_MovementReason", CMRUnderbondRequestCodes.Codes.Quarantine, underbond.C4_MovementReason);
			AssertEquals("C4_UnderbondBySeaVessel", "ADMIRALENGRACHT", underbond.C4_UnderbondBySeaVessel);
			AssertEquals("C4_UnderbondBySeaVoyage", "123", underbond.C4_UnderbondBySeaVoyage);
		}

		public void TestPopulateCusUnderbondInfoLinksToOrgAddressesIfItCan()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Addresses.MainAddress.LocalControlledPremisesID = "9913C";

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			UnderbondFactory.PopulateCusUnderbondInfoInternal(underbond, ExpectedArrivalMessage);
			AssertEquals("C4_OA_DestinationAddress", header.Addresses.MainAddress.PK, underbond.C4_OA_DestinationAddress);
		}

		public void TestPopulateCusUnderbondInfoOriginLinksToOrgAddressesIfItCan()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Addresses.MainAddress.LocalControlledPremisesID = "9914N";

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			UnderbondFactory.PopulateCusUnderbondInfoInternal(underbond, ExpectedArrivalMessage);
			AssertEquals("C4_OA_OriginAddress", header.Addresses.MainAddress.PK, underbond.C4_OA_OriginAddress);
		}

		public void TestLinkMessageOrCloneOfMessageToUnderbondIfMessageNotLinked()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.LinkOrCloneMessage(underbond);
			AssertEquals(underbond, message.EM_LinkedObject);
		}

		public void TestLinkMessageOrCloneOfMessageToUnderbondIfMessageAlreadyLinkedToUnderbond()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_LinkedObject = underbond;
			message.LinkOrCloneMessage(underbond);
			AssertEquals(underbond, message.EM_LinkedObject);
		}

		public void TestLinkMessageOrCloneOfMessageToUnderbondIfMessageAlreadyLinkedToDifferentEntity()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusUnderbond secondUnderbond = Factory.New<CusUnderbond>();

			CMRUBMREQRMessage message = Factory.New<CMRUBMREQRMessage>();
			message.EM_LinkedObject = secondUnderbond;
			message.LinkOrCloneMessage(underbond);
			AssertEquals(secondUnderbond, message.EM_LinkedObject);
			AssertEquals("MessageCount", 1, underbond.Messages.Count);
			AssertEquals("EM_Status", EDIMessage.Status.Received, underbond.Messages[0].EM_Status);
		}

		public void TestProcessIncomingUBMREQDoesNothingIfWereNotInterested()
		{
			UnderbondFactory.IsInterestedInUBMREQRResult = false;
			UnderbondFactory.ProcessIncomingUBMREQR(ExpectedArrivalMessage);
			AssertEquals(null, ExpectedArrivalMessage.EM_LinkedObject);
		}

		public void TestProcessIncomingUBMREQDoesNothingIfWeCantFindTheUnderbond()
		{
			UnderbondFactory.IsInterestedInUBMREQRResult = true;
			UnderbondFactory.CanFindParent = false;
			UnderbondFactory.ProcessIncomingUBMREQR(ExpectedArrivalMessage);
			AssertEquals(null, ExpectedArrivalMessage.EM_LinkedObject);
		}

		public void TestProcessIncomingUBMREQLinksToAllUnderbonds()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9913C";
			UnderbondFactory.IsInterestedInUBMREQRResult = true;
			UnderbondFactory.CanFindParent = true;
			UnderbondFactory.ProcessIncomingUBMREQR(ExpectedArrivalMessage);
			AssertNotNull("EM_LinkedObject", ExpectedArrivalMessage.EM_LinkedObject);
		}

		public void TestStatusEventsContainer()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9122P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = EventTestLCLConsol;
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestContainerUnderbondMessage);
			StmALog lastStatusAdviceEvent = consol.Containers[0].Logs.MostRecentLogByEventTime(Events.UnderbondCustomsApproval);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
			Assert("Event should not be", !lastStatusAdviceEvent.SL_IsCancelled);

			GetMessageProcessor().ProcessMessage(TestContainerUnderbondRescindMessage);
			Assert("Event should be cancelled after rescind", lastStatusAdviceEvent.SL_IsCancelled);
		}

		protected CFSLoadListConsol EventTestLCLConsol
		{
			get
			{
				if (fEventTestLCLConsol == null)
				{
					fEventTestLCLConsol = Factory.New<CFSLoadListConsol>();
					fEventTestLCLConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
					fEventTestLCLConsol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;

					Transport transport = fEventTestLCLConsol.Transports[0];
					transport.JW_JX = CreateSailing(TestVesselLloyds, TestVoyageNumber).PK;
					CFSContainer container = fEventTestLCLConsol.Containers.AddNew();
					container.JC_ContainerNum = TestContainerNumber;
					CFSShipment shipment = fEventTestLCLConsol.Shipments.AddNew();
				}
				return fEventTestLCLConsol;
			}
		}
		CFSLoadListConsol fEventTestLCLConsol;

		public void TestStatusEventsContainer_ExpectedArrival()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = EventTestLCLConsol;
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestContainerExpectedCargoArrivalMessage);
			StmALog lastStatusAdviceEvent = consol.Containers[0].Logs.MostRecentLogByEventTime(Events.UnderbondRequest);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
			Assert("Event should not be cancelled", !lastStatusAdviceEvent.SL_IsCancelled);
			GetMessageProcessor().ProcessMessage(TestContainerExpectedCargoArrivalRescindMessage);
			Assert("Event should be cancelled after rescind", lastStatusAdviceEvent.SL_IsCancelled);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
		}

		public void TestStatusEventsShipment()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestVesselLloyds, TestVoyageNumber).PK;

			consol.JK_MasterBillNum = TestMasterBillNum;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = TestContainerNumber;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = TestHouseBill;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "BOX";
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestShipmentUnderbondMessage);
			StmALog lastStatusAdviceEvent = shipment.Logs.MostRecentLogByEventTime(Events.UnderbondCustomsApproval);
			AssertNotNull("BP Failed to add underbond approval event to Shipment", lastStatusAdviceEvent);
		}

		public void TestStatusEventsBreakBulkShipment()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9122P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestVesselLloyds, TestVoyageNumber).PK;

			consol.JK_MasterBillNum = TestMasterBillNum;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "BREAK BULK HB";
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestBreakBulkShipmentUnderbondMessage);
			StmALog lastStatusAdviceEvent = shipment.Logs.MostRecentLogByEventTime(Events.UnderbondCustomsApproval);
			AssertNotNull("BP Failed to add underbond approval event to Shipment", lastStatusAdviceEvent);
		}

		#region Implementation

		TestHelperExpectedArrivalCusUnderbondFactory fUnderbondFactory;
		TestHelperExpectedArrivalCusUnderbondFactory UnderbondFactory
		{
			get
			{
				if (fUnderbondFactory == null)
				{
					fUnderbondFactory = new TestHelperExpectedArrivalCusUnderbondFactory();
				}
				return fUnderbondFactory;
			}
		}

		#region Test Helper Class

		class TestHelperExpectedArrivalCusUnderbondFactory : ExpectedArrivalCusUnderbondFactory
		{
			public TestHelperExpectedArrivalCusUnderbondFactory()
			{
			}

			protected internal void PopulateCusUnderbondInfoInternal(CusUnderbond underbond, CMRUBMREQRMessage message) => PopulateCusUnderbondInfo(underbond, message);
			protected override void PopulateCusUnderbondInfo(CusUnderbond underbond, CMRUBMREQRMessage message)
			{
				PopulateCusUnderbondInfoCalled = true;
				base.PopulateCusUnderbondInfo(underbond, message);
			}

			protected override ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber)
			{
				return CanFindParent ? (ICusUnderbondDependentCollectionParent)message.Factory.New(typeof(DummyBizoWithUnderbondCollection)) : null;
			}

			protected override bool IsInterestedInUBMREQR(CMRUBMREQRMessage message)
			{
				return IsInterestedInUBMREQRResult;
			}
			public bool IsInterestedInUBMREQRResult;

			public bool PopulateCusUnderbondInfoCalled;
			public bool CanFindParent = true;
		}

		#endregion

		protected CMRMessageResponseProcessor GetMessageProcessor()
		{
			return new UBMREQRMessageProcessor(new LoggingInformation());
		}

		#region Constants

		protected const string TestVesselLloyds = "8811924";
		protected const string TestVoyageNumber = "936";
		protected const string TestContainerNumber = "TRCU3382910";
		protected const string TestMasterBillNum = "OBL250805003";
		protected const string TestHouseBill = "OBL250805003H1";
		protected const string TestContainerUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3I94 BI23 IAG5:1+32'
DTM+9:20050826120936544751:ZZZ'
FTX+AAH+++AAA447YL5040044C0001/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044C0001/TRCU3382910::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000600++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'";
		protected const string TestContainerUnderbondRescindText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+A0IB EHH8 B65:1+32'
DTM+9:20050915081338578585:ZZZ'
FTX+AAH+++AAA447YL5040044C0001/2/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044C0001/2/TRCU3382910::1'
RFF+ANX:UNDERBOND APPROVAL RESCIND NOTICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000600++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'";

		protected const string TestContainerExpectedCargoArrivalText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+21CB 53AD G0G5:1+32'
DTM+9:20050826102622273080:ZZZ'
FTX+AAH+++AAA447YL5040041C0001/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040041C0001/TRCU3382910::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'";

		protected const string TestContainerExpectedCargoArrivalRescindText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+21CB 53AD G0G5:1+32'
DTM+9:20050826102622273080:ZZZ'
FTX+AAH+++AAA447YL5040041C0001/2/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:TRCU3382910/GGHU4483920::1'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'";

		protected const string TestShipmentUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+34E2 GF25 9FG5:1+32'
DTM+9:20050826172746006718:ZZZ'
FTX+AAH+++AAA447YL5040044H0001'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0001::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DEL'
DOC+1'
PAC+++LCL:67:95'
RFF+AAQ:TRCU3382910'
RFF+MB:OBL250805003'
RFF+BH:OBL250805003H1'
UNT+19+000001'";

		protected const string TestBreakBulkShipmentUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3H23 25B6 J115:1+32'
DTM+9:20050830113617386418:ZZZ'
FTX+AAH+++AAA447YL5040042H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9912J::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040042H0001/3::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
RFF+MB:OBL250805003'
FTX+AAA+++BIG PIPE'
UNT+18+000001'";

		#endregion

		#region Test Messages

		protected CMRUBMREQRMessage TestContainerUnderbondMessage
		{
			get
			{
				if (fTestContainerUnderbondMessage == null)
				{
					fTestContainerUnderbondMessage = Factory.New<CMRUBMREQRMessage>();
					fTestContainerUnderbondMessage.EM_MessageText = TestContainerUnderbondText.Replace("\r\n", "");
				}
				return fTestContainerUnderbondMessage;
			}
		}
		CMRUBMREQRMessage fTestContainerUnderbondMessage;

		protected CMRUBMREQRMessage TestContainerUnderbondRescindMessage
		{
			get
			{
				if (fTestContainerUnderbondRescindMessage == null)
				{
					fTestContainerUnderbondRescindMessage = Factory.New<CMRUBMREQRMessage>();
					fTestContainerUnderbondRescindMessage.EM_MessageText = TestContainerUnderbondRescindText.Replace("\r\n", "");
				}
				return fTestContainerUnderbondRescindMessage;
			}
		}
		CMRUBMREQRMessage fTestContainerUnderbondRescindMessage;

		protected CMRUBMREQRMessage TestContainerExpectedCargoArrivalMessage
		{
			get
			{
				if (fTestContainerExpectedCargoArrivalMessage == null)
				{
					fTestContainerExpectedCargoArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fTestContainerExpectedCargoArrivalMessage.EM_MessageText = TestContainerExpectedCargoArrivalText.Replace("\r\n", "");
				}
				return fTestContainerExpectedCargoArrivalMessage;
			}
		}
		CMRUBMREQRMessage fTestContainerExpectedCargoArrivalMessage;

		protected CMRUBMREQRMessage TestContainerExpectedCargoArrivalRescindMessage
		{
			get
			{
				if (fTestContainerExpectedCargoArrivalRescindMessage == null)
				{
					fTestContainerExpectedCargoArrivalRescindMessage = Factory.New<CMRUBMREQRMessage>();
					fTestContainerExpectedCargoArrivalRescindMessage.EM_MessageText = TestContainerExpectedCargoArrivalRescindText.Replace("\r\n", "");
				}
				return fTestContainerExpectedCargoArrivalRescindMessage;
			}
		}
		CMRUBMREQRMessage fTestContainerExpectedCargoArrivalRescindMessage;

		protected CMRUBMREQRMessage TestBreakBulkShipmentUnderbondMessage
		{
			get
			{
				if (fTestBreakBulkShipmentUnderbondMessage == null)
				{
					fTestBreakBulkShipmentUnderbondMessage = Factory.New<CMRUBMREQRMessage>();
					fTestBreakBulkShipmentUnderbondMessage.EM_MessageText = TestBreakBulkShipmentUnderbondText.Replace("\r\n", "");
				}
				return fTestBreakBulkShipmentUnderbondMessage;
			}
		}
		CMRUBMREQRMessage fTestBreakBulkShipmentUnderbondMessage;

		protected CMRUBMREQRMessage TestShipmentUnderbondMessage
		{
			get
			{
				if (fTestShipmentUnderbondMessage == null)
				{
					fTestShipmentUnderbondMessage = Factory.New<CMRUBMREQRMessage>();
					fTestShipmentUnderbondMessage.EM_MessageText = TestShipmentUnderbondText.Replace("\r\n", "");
				}
				return fTestShipmentUnderbondMessage;
			}
		}
		CMRUBMREQRMessage fTestShipmentUnderbondMessage;

		CMRUBMREQRMessage fExpectedArrivalMessage;
		protected virtual CMRUBMREQRMessage ExpectedArrivalMessage
		{
			get
			{
				if (fExpectedArrivalMessage == null)
				{
					fExpectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
					fExpectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+A477 FA1B 3G5:1+32'
DTM+9:20050916142251736787:ZZZ'
FTX+AAH+++AAA374MU00000221/SYD1'
TDT+20+932++11++++7104673::11'
TDT+1++ROA'
TDT+1+123++11++++8811924::11'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000221/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:AQS'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++PK:185:95'
RFF+AAQ:IFTU7823492'
RFF+MB:CSCD002'
RFF+BH:L20059039'
PCI+28+NILMARKS'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fExpectedArrivalMessage;
			}
		}

		#endregion

		protected JobSailing CreateSailing(ZString lloydsNumber, ZString voyageNumber)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNumber;
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion
	}
}
