using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UBMREQRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestStatusEventsContainer()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9122P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = EventTestLCLConsol;
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestContainerUnderbondMessage);
			AssertEquals("Received", EDIMessage.Status.Received, TestContainerUnderbondMessage.EM_Status);
			StmALog lastStatusAdviceEvent = consol.Containers[0].Logs.MostRecentLogByEventTime(Events.UnderbondCustomsApproval);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
			Assert("Event should not be", !lastStatusAdviceEvent.SL_IsCancelled);

			GetMessageProcessor().ProcessMessage(TestContainerUnderbondRescindMessage);
			Assert("Event should be cancelled after rescind", lastStatusAdviceEvent.SL_IsCancelled);
		}

		public void TestStatusEventsContainer_ExpectedArrival()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = EventTestLCLConsol;
			Factory.Save();
			GetMessageProcessor().ProcessMessage(TestContainerExpectedCargoArrivalMessage);
			AssertEquals("Received", EDIMessage.Status.Received, TestContainerExpectedCargoArrivalMessage.EM_Status);
			StmALog lastStatusAdviceEvent = consol.Containers[0].Logs.MostRecentLogByEventTime(Events.UnderbondRequest);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
			Assert("Event should not be cancelled", !lastStatusAdviceEvent.SL_IsCancelled);
			GetMessageProcessor().ProcessMessage(TestContainerExpectedCargoArrivalRescindMessage);
			Assert("Event should be cancelled after rescind", lastStatusAdviceEvent.SL_IsCancelled);
			AssertNotNull("BP Failed to add underbond approval event to container", lastStatusAdviceEvent);
		}

		public void TestStatusEventsAirConsol()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9922W";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			var consol = TestAirConsol;

			var mawb = fTestAirConsol.Factory.New<CusMAWB>();
			mawb.CM_JK = fTestAirConsol.PK;
			mawb.SynchroniseData();
			mawb.CM_MAWB = "00102062015";

			var underbond = mawb.AllUnderbonds.AddNew();
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_MAWB = "00102062015";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			underbond.C4_SendersMessageReference = "U00003297";
			Factory.Save();

			GetMessageProcessor().ProcessMessage(TestConsolUnderbondClearedMessage);
			AssertEquals("Received", EDIMessage.Status.Received, TestConsolUnderbondClearedMessage.EM_Status);
			Factory.Save();
			var mawbLogCollection = new StmALogDependentCollection(mawb);
			mawbLogCollection.Load(new ZQuery(StmALogSchema.SL_IsCancelled, "N"));
			Assert("Process failed to add an underbond submitted event to the Consol/Mawb", CollectionContainsExpectedEvent(mawbLogCollection, Events.UnderbondCustomsApproval));
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

		public void TestExpectedArrivalWithNoMatchingUnderbondStillSendsReportEmail()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CMRMessageResponseProcessor processor = GetMessageProcessor();
			AssertEquals("Precondition: AcknowledgementEmailSendCount is 0", 0, processor.AcknowledgementEmailSendCount);

			processor.ProcessMessage(TestContainerExpectedCargoArrivalMessage);
			AssertEquals("Received", EDIMessage.Status.Received, TestContainerExpectedCargoArrivalMessage.EM_Status);
			AssertEquals("Acknowledgement email has been sent", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestUnknownExpectedArrivalSetsDiscardedStatus() => AssertUnknownExpectedArrivalSetsDiscardedStatus(processor => processor.ProcessMessage(TestContainerExpectedCargoArrivalMessage));

		public void TestUnknownExpectedArrivalSetsDiscardedStatus_PreProcessMessage() => AssertUnknownExpectedArrivalSetsDiscardedStatus(processor => processor.PreProcessMessage(TestContainerExpectedCargoArrivalMessage));

		public void TestUnknownUnderbondApprovalDoesNotThrowDiscardedMessage() => AssertUnknownUnderbondApprovalDoesNotThrowDiscardedMessage(processor => processor.ProcessMessage(TestContainerUnderbondMessage));

		public void TestUnknownUnderbondApprovalDoesNotThrowDiscardedMessage_PreProcessMessage() => AssertUnknownUnderbondApprovalDoesNotThrowDiscardedMessage(processor => processor.PreProcessMessage(TestContainerUnderbondMessage));

		public void TestUnknownApprovalSetsErrorStatus()
		{
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "?????";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
			CMRMessageResponseProcessor processor = GetMessageProcessor();
			processor.ProcessMessage(TestContainerUnderbondMessage);
			AssertEquals("Errored", EDIMessage.Status.Error, TestContainerUnderbondMessage.EM_Status);
		}

		public void TestExpectedArrivalWithMatchingUnderbondStillSendsReportEmail()
		{
			const string TestVesselName = "SHIP OF FOO";

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = TestDestinationPremiseID;
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader header = tranHead.OceanBills.AddNew();
			CusSeaManOBLDetail detail = header.Details.AddNew();

			tranHead.BT_VoyageNum = TestVoyageNumber;
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_Code = TestVesselName;
			vessel.RV_LloydsNumber = TestVesselLloyds;
			tranHead.BT_VesselName = vessel.RV_Code;

			header.BO_OceanBill = TestMasterBillNum;

			detail.BD_ContainerNumber = TestContainerNumber;

			CusUnderbond underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)detail).Underbonds.AddNew();
			underbond.C4_OriginPremiseID = TestOriginPremiseID;
			underbond.C4_DestinationPremiseID = TestDestinationPremiseID;
			underbond.C4_SendersMessageReference = TestMessageReference;
			EDIMessage message = underbond.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_Status = "SNT";
			EDIInterchange interchange = EDIInterchange.New(Factory);
			message.EM_EI = interchange.PK;

			CMRMessageResponseProcessor processor = GetMessageProcessor();
			AssertEquals("Precondition: AcknowledgementEmailSendCount is 0", 0, processor.AcknowledgementEmailSendCount);

			processor.ProcessMessage(TestContainerUnderbondMessage);
			AssertEquals("Acknowledgement email has been sent", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.UBMREQR;

		protected override ZString GetExpectedMessageName() => "Underbond Movement Request Response - (UBMREQR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new UBMREQRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRUBMREQRMessage);

		CFSLoadListConsol EventTestLCLConsol
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

		ForwardingConsol TestAirConsol
		{
			get
			{
				if (fTestAirConsol == null)
				{
					fTestAirConsol = Factory.New<ForwardingConsol>();
					fTestAirConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
					fTestAirConsol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.AIR;
					fTestAirConsol.JK_MasterBillNum = "00102062015";
					var transport = fTestAirConsol.Transports[0];
				}

				return fTestAirConsol;
			}
		}
		ForwardingConsol fTestAirConsol;

		bool CollectionContainsExpectedEvent(StmALogDependentCollection logCollection, Event cargoEvent)
		{
			bool result = false;
			foreach (StmALog aLog in logCollection)
			{
				if (aLog.SL_SE_NKEvent == cargoEvent.Code)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		void AssertUnknownExpectedArrivalSetsDiscardedStatus(Action<TestHelperUBMREQRMessageProcessor> processFunc)
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "?????";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
			var processor = new TestHelperUBMREQRMessageProcessor();
			processFunc(processor);
			AssertEquals("Discarded", EDIMessage.Status.Discarded, TestContainerExpectedCargoArrivalMessage.EM_Status);
			AssertEquals("Report Email Sent", 1, processor.SentReportEmails.Count);
			var email = processor.SentReportEmails[0] as EmailDef;
			AssertEquals("Mail Subject", "Expected arrival notice discarded.", email.Subject);
			AssertContains("Mail Body", @"An inbound expected arrival, or expected arrival rescind, notice has been detected, but could not be processed and so
has been discarded. These notices are processed by the CargoWise One CFS Module. You either are not using this Module or the
destination premise code (9914N) is unknown to CargoWise One.  For more information about the CargoWise One CFS Module please
contact the CargoWise support desk.", email.Body);
			AssertEquals("No attachement", 2, email.Attachments.Count);
		}

		void AssertUnknownUnderbondApprovalDoesNotThrowDiscardedMessage(Action<TestHelperUBMREQRMessageProcessor> processFunc)
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "?????";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
			var processor = new TestHelperUBMREQRMessageProcessor();
			processFunc(processor);
			AssertEquals("Error", EDIMessage.Status.Error, TestContainerUnderbondMessage.EM_Status);
			var email = processor.SentReportEmails[0] as EmailDef;
			AssertEquals("Mail Subject", "Underbond Movement Request Response - (UBMREQR) Message Processor Error Report", email.Subject);
		}

		const string TestVesselLloyds = "8811924";
		const string TestVoyageNumber = "936";
		const string TestContainerNumber = "TRCU3382910";
		const string TestMasterBillNum = "OBL250805003";
		const string TestHouseBill = "OBL250805003H1";
		const string TestDestinationPremiseID = "9914N";
		const string TestOriginPremiseID = "9122P";
		const string TestMessageReference = "L5040044C0001";
		const string TestContainerUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
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
		const string TestContainerUnderbondRescindText = @"UNH+000001+CUSRES:D:99B:UN'
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

		const string TestContainerExpectedCargoArrivalText = @"UNH+000001+CUSRES:D:99B:UN'
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

		const string TestContainerExpectedCargoArrivalRescindText = @"UNH+000001+CUSRES:D:99B:UN'
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

		const string TestShipmentUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
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

		const string TestUBMRApprovalText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+212C B9FI GDFF:1+32'
DTM+9:20150602110709581314:ZZZ'
DTM+132:20150602:102'
FTX+AAH+++AAA374MU00003297/CMT1'
TDT+20+0206++6+PW::3'
TDT+1++ROA'
LOC+5+9922W::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00003297/CMT1::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++AIR:67:95'
PAC+0000021'
RFF+MWB:00102062015'
UNT+19+000001'
";

		const string TestBreakBulkShipmentUnderbondText = @"UNH+000001+CUSRES:D:99B:UN'
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

		CMRUBMREQRMessage TestContainerUnderbondMessage
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

		CMRUBMREQRMessage TestContainerUnderbondRescindMessage
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

		CMRUBMREQRMessage TestContainerExpectedCargoArrivalMessage
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

		CMRUBMREQRMessage TestContainerExpectedCargoArrivalRescindMessage
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

		CMRUBMREQRMessage TestBreakBulkShipmentUnderbondMessage
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

		CMRUBMREQRMessage TestShipmentUnderbondMessage
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

		CMRUBMREQRMessage TestConsolUnderbondClearedMessage
		{
			get
			{
				if (fTestConsolUnderbondCleared == null)
				{
					fTestConsolUnderbondCleared = Factory.New<CMRUBMREQRMessage>();
					fTestConsolUnderbondCleared.EM_MessageText = TestUBMRApprovalText.Replace("\r\n", "");
				}
				return fTestConsolUnderbondCleared;
			}
		}
		CMRUBMREQRMessage fTestConsolUnderbondCleared;

		sealed class TestHelperUBMREQRMessageProcessor : UBMREQRMessageProcessor
		{
			public TestHelperUBMREQRMessageProcessor()
				: base(new LoggingInformation())
			{
			}

			protected override void SendReport(EmailDef email)
			{
				base.SendReport(email);
				SentReportEmails.Add(email);
			}
			public ArrayList SentReportEmails = new ArrayList();
		}
	}
}
