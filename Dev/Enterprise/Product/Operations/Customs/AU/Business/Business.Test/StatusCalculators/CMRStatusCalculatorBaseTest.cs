using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(StatusCalculatorForTest))]
	sealed class CMRStatusCalculatorBaseTest : StatusCalculatorTest
	{
		[TestDate(2020, 4, 25, 5, 55, 0)]
		public void TestLogStatusChangeInMessageBranchTimeZone_SameEnvironment()
		{
			var incomingMessage = Parent.Messages.AddNew(typeof(CMRCARSTMessage));
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "001";
			Factory.Save();
			AssertEquals("Incoming Message Branch is Current Branch", GlbBranch.CurrentBranch.PK, incomingMessage.EM_GB);
			AssertNull(Parent.Logs.MostRecentLogByEventTime(Events.StatusChange));

			var calc = new TestHelperCARSTStatusCalculator(Parent);
			calc.DeriveStatusForTest();

			var log = Parent.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertEquals("message is logged with current branch timestamp", log.SL_EventTime, ZDateTime.Now);
		}

		public void TestLogStatusChangeInMessageBranchTimeZone_SwitchedEnvironment()
		{
			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "TML";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";
			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "TPR";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			using (Environment.DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var incomingMessage = Parent.Messages.AddNew(typeof(CMRCARSTMessage));
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_MessageNum = "001";
				Factory.Save();
				AssertEquals("Incoming Message Branch is Current Branch", melbourneBranch.PK, incomingMessage.EM_GB);

				var melbourneBranchTime = ZDateTime.Now;
				using (Environment.DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
				{
					AssertLessThan("No longer in East Coast timezone", ZDateTime.Now, melbourneBranchTime);
					AssertNull(Parent.Logs.MostRecentLogByEventTime(Events.StatusChange));

					var calc = new TestHelperCARSTStatusCalculator(Parent);
					calc.DeriveStatusForTest();
				}
				var log = Parent.Logs.MostRecentLogByEventTime(Events.StatusChange);
				AssertGreaterThanOrEqualTo("log should have Melbourne timestamp even though logged in Perth timezone", log.SL_EventTime, melbourneBranchTime);
			}
		}

		public void TestGetRejectedResponseStatusWithIncomingMessage()
		{
			Factory.Save();
			TestHelperStatusCalculatorUsingIncomingMessage testCalculator = new TestHelperStatusCalculatorUsingIncomingMessage(Parent);

			EDIMessage outgoing1 = Factory.New<EDIMessage>();
			outgoing1.EM_ApplicationReference = "AAA";
			outgoing1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			EDIMessage outgoing2 = Factory.New<EDIMessage>();
			outgoing2.EM_ApplicationReference = "BBB";
			outgoing2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			EDIMessage incoming1 = Factory.New<EDIMessage>();
			incoming1.EM_ApplicationReference = "CCC";
			incoming1.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			incoming1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			EDIMessage incoming2 = Factory.New<EDIMessage>();
			incoming2.EM_ApplicationReference = "DDD";
			incoming2.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			incoming2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			testCalculator.SortedMessagesExposed = new EDIMessage[] { incoming2, outgoing2, incoming1, outgoing1 };

			Parent.Z0_VarCharMax = ZString.Empty;
			testCalculator.DeriveStatusForTest();
			AssertEquals("Status should be coming from Incoming2", incoming2.EM_ApplicationReference, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusIfEmptyWithMessages()
		{
			Parent.Z0_VarCharMax = "foo";
			TestHelperCMRStatusBaseCalculator calc = new TestHelperCMRStatusBaseCalculator(Parent);
			((ICalculatedCusStatusCalculator)calc).DeriveStatusIfEmptyWithMessages();
			AssertEquals("Status has not been recalculated", "foo", Parent.Z0_VarCharMax);

			Parent.Z0_VarCharMax = ZString.Empty;
			calc = new TestHelperCMRStatusBaseCalculator(Parent);
			((ICalculatedCusStatusCalculator)calc).DeriveStatusIfEmptyWithMessages();
			AssertEquals("Status has not been recalculated as no messages", "", Parent.Z0_VarCharMax);

			EDIMessage incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
			Parent.Z0_VarCharMax = ZString.Empty;

			calc = new TestHelperCMRStatusBaseCalculator(Parent);
			((ICalculatedCusStatusCalculator)calc).DeriveStatusIfEmptyWithMessages();
			AssertEquals("Status has been recalculated", "default status", Parent.Z0_VarCharMax);
			AssertEquals("HasChanges is true", true, Parent.HasChanges);
		}

		public void TestGetAllCustomsStatusesFromInboundMessage()
		{
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2HF2 5CBC D7BF:1+8'
DTM+9:20051103122504817010:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+QA123++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00001052/SYD1::1'
RFF+MB:1234564'
RFF+AAQ:LCLU99999999'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
			TestHelperCMRStatusBaseCalculator calc = new TestHelperCMRStatusBaseCalculator(Parent);
			ZString expectedResult = @"CONSOLIDATED STATUS : HELD
COMPLETE UNDERBOND SERIES APPROVED : N/A
LCL UNDERBOND SATISFIED : NO
CARGO REPORT ACS EVALUATED : NO
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
			AssertEquals(expectedResult, calc.GetAllCustomsStatusesFromInboundMessage(message));
		}

		[ExpectNoExceptions]
		public void TestDeriveStatusOnDeletedObject()
		{
			Parent.Z0_VarCharMax = "foo";
			TestHelperCMRStatusBaseCalculator calc = new TestHelperCMRStatusBaseCalculator(Parent);
			Parent.Delete();
			calc.DeriveStatusForTest();
		}

		public void TestDeriveStatusDoesNotSetIfCalculatedStatusIsEmpty()
		{
			TestHelperCMRStatusBaseCalculator calculator = CalculatorWithCONTRLRejectedOriginal;
			EDIMessage message = calculator.ParentForTest.Messages[0];
			message.EM_ApplicationReference = "";

			Parent.Z0_VarCharMax = "TTT";
			calculator.DeriveStatusForTest();
			AssertEquals("Status should read TTT", "TTT", Parent.Z0_VarCharMax);
		}

		public void TestResetToOriginal()
		{
			TestHelperCMRStatusBaseCalculator calculator = this.Calculator;
			EDIMessage message = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
			Parent.Z0_VarCharMax = "123";
			calculator.ResetToOriginal();
			AssertEquals("Message Status", EDIMessage.Status.Discarded, message.EM_Status);
			AssertEquals("Status", CMRBaseStatuses.Codes.NotSent, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForCONTRLRejectedOutgoing()
		{
			CalculatorWithCONTRLRejectedOriginal.DeriveStatusForTest();
			AssertEquals("Status", TestStatus.OriginalRejection, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForCONTRLRejectedOutgoingFollowedByValidResponse()
		{
			var calculator = CalculatorWithCONTRLRejectedOriginalFollowedByValidResponse;
			calculator.DeriveStatusForTest();
			AssertEquals("Status", "default status", Parent.Z0_VarCharMax);
			calculator.SetAcceptedWithErrorsSupported(true);
			calculator.DeriveStatusForTest();
			AssertEquals("Status", TestStatus.OriginalWithErrors, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForAmendmentRejectedOutgoing()
		{
			CalculatorWithRejectedAmendment.DeriveStatusForTest();
			AssertEquals("Status", TestStatus.AmendmentRejection, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForAcceptedWithErrorsOriginal()
		{
			TestHelperCMRStatusBaseCalculator calculator = CalculatorWithAcceptedWithErrorsOriginal;
			calculator.DeriveStatusForTest();
			AssertEquals("Status", "default status", Parent.Z0_VarCharMax);
			calculator.SetAcceptedWithErrorsSupported(true);
			calculator.DeriveStatusForTest();
			AssertEquals("Status", TestStatus.OriginalWithErrors, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForNoResponseReceived()
		{
			CalculatorWithNonRespondedToWithdrawal.DeriveStatusForTest();
			AssertEquals("Status", TestStatus.WithdrawNotResponded, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForResponseReceived()
		{
			CalculatorWithResponse.DeriveStatusForTest();
			AssertEquals("Status", CalculatorWithResponse.ExpectedStatus, Parent.Z0_VarCharMax);
		}

		public void TestMessagesWeAreInterestedIn()
		{
			EDIMessage message1 = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
			EDIMessage message2 = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
			EDIMessage message3 = Parent.Messages.AddNew(typeof(CMRSACMessage));
			EDIMessage message4 = Parent.Messages.AddNew(typeof(CMRESMMessage));

			EDIMessage[] view = Calculator.MessagesWeAreInterestedInForTest;
			AssertEquals("Count", 2, view.Length);
			AssertEquals("View.Contains(Message1)", true, ((IList)view).IndexOf(message1) > -1);
			AssertEquals("View.Contains(Message2)", true, ((IList)view).IndexOf(message2) > -1);
			message1.EM_Status = EDIMessage.Status.Discarded;
			view = Calculator.MessagesWeAreInterestedInForTest;
			AssertEquals("Count", 1, view.Length);
		}

		public void TestMessagesWeAreInterestedInForAIROUT()
		{
			var message1 = Parent.Messages.AddNew(typeof(CMRAIROUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = Parent.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = Parent.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var message2 = Parent.Messages.AddNew(typeof(CMRSACMessage));
			var message3 = Parent.Messages.AddNew(typeof(CMRESMMessage));
			var message4 = Parent.Messages.AddNew(typeof(CMRSEAOUTMessage));

			EDIMessage[] view = AirOutStatusCalculator.MessagesWeAreInterestedInForTest;
			AirOutStatusCalculator.DeriveStatusForTest();
			AssertEquals("Count should find all 3 AirOut messages", 3, view.Length);
			AssertEquals("View.Contains(Message1)", true, ((IList)view).IndexOf(message1) > -1);
			AssertEquals("View.Contains(splitMessage2)", true, ((IList)view).IndexOf(splitMessage2) > -1);
			AssertEquals("View.Contains(splitMessage3)", true, ((IList)view).IndexOf(splitMessage3) > -1);
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			view = AirOutStatusCalculator.MessagesWeAreInterestedInForTest;
			AirOutStatusCalculator.DeriveStatusForTest();
			AssertEquals("Count should still be 3 AirOut messages including Pending messages", 3, view.Length);
		}

		public void TestMessagesWeAreInterestedInForSEAOUT()
		{
			var message1 = Parent.Messages.AddNew(typeof(CMRSEAOUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = Parent.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = Parent.Messages.AddNew(typeof(CMRSEAOUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var message2 = Parent.Messages.AddNew(typeof(CMRSACMessage));
			var message3 = Parent.Messages.AddNew(typeof(CMRESMMessage));
			var message4 = Parent.Messages.AddNew(typeof(CMRAIROUTMessage));

			EDIMessage[] view = SeaOutStatusCalculator.MessagesWeAreInterestedInForTest;
			SeaOutStatusCalculator.DeriveStatusForTest();
			AssertEquals("Count should find all 3 SeaOut messages", 3, view.Length);
			AssertEquals("View.Contains(Message1)", true, ((IList)view).IndexOf(message1) > -1);
			AssertEquals("View.Contains(splitMessage2)", true, ((IList)view).IndexOf(splitMessage2) > -1);
			AssertEquals("View.Contains(splitMessage3)", true, ((IList)view).IndexOf(splitMessage3) > -1);
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;
			view = SeaOutStatusCalculator.MessagesWeAreInterestedInForTest;
			SeaOutStatusCalculator.DeriveStatusForTest();
			AssertEquals("Count should still be 3 SeaOut messages including Pending messages", 3, view.Length);
		}

		public void TestWeAreNotInterestedInCancelledMessages()
		{
			EDIMessage message1 = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));

			EDIMessage[] view = Calculator.MessagesWeAreInterestedInForTest;
			AssertEquals("Count", 1, view.Length);
			message1.EM_Status = EDIMessage.Status.Cancelled;
			view = Calculator.MessagesWeAreInterestedInForTest;
			AssertEquals("Count", 0, view.Length);
		}

		public void TestWeAreInterestedInPreProcessedMessages()
		{
			var message = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			AssertEquals("Count", 1, Calculator.MessagesWeAreInterestedInForTest.Length);
		}

		public void TestWeDoNotRecalculateStatusWhenSuspended()
		{
			var calc = new TestHelperStatusCalculatorWithOverridenDeriveStatusCore(Parent);
			AssertEquals("(pre-condition)", false, calc.DeriveStatusWasCalled);

			calc.DeriveStatusForTest();
			AssertEquals(true, calc.DeriveStatusWasCalled);

			try
			{
				CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
				calc.DeriveStatusWasCalled = false;

				calc.DeriveStatusForTest();
				AssertEquals(false, calc.DeriveStatusWasCalled);
			}
			finally
			{
				CMRStatusRecalculationSuspender.ResumeStatusRecalculation(Factory);
			}

			calc.DeriveStatusForTest();
			AssertEquals(true, calc.DeriveStatusWasCalled);
		}

		public void TestProcessingClearHeldClearMessagesReceivedInRandomOrder()
		{
			#region Test Message Texts

			const string AIRCRRTransactionAcceptedMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIRCRR+20J7 18IG J2GF:001+11'
NAD+MR+FGH939C::95'
RFF+ACW:AIRCR'
RFF+AFM:9'
RFF+ABO:321/PRD1::001'
DTM+310:20051109103042:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'
";

			const string CARSTHeldMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";

			const string CARSTClearMessage1 = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+475I B892 5A6F:1+8'
DTM+9:20051110061124519774:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+1886B::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
DOC+1'
PAC+0000001'
UNT+17+000001'
";

			const string CARSTClearMessage2 = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+475I B892 5A6F:1+8'
DTM+9:20051110061124519774:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:YES'
TDT+20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+1886B::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
DOC+1'
PAC+0000001'
UNT+17+000001'
";

			#endregion

			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08145322174";
			mAWB.CM_RL_NKLoadPort = "NZAKL";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "V0014102928";
			hAWB.CS_MessageReference = "321";
			hAWB.CS_RL_NKOrigin = "NZAKL";
			hAWB.CS_RL_NKDestination = "AUSYD";

			Factory.Save();
			CusHAWBAIRCRMessageManager airCargoMessenger = new CusHAWBAIRCRMessageManager(hAWB);
			airCargoMessenger.GenerateOriginalMessages(hAWB);
			Factory.Save();

			CMRCUSRESMessage transactionAcceptedMessage = (CMRCUSRESMessage)CreateMessage(typeof(CMRAIRCRRMessage), 1, new ZDateTime(2005, 1, 1), EDIMessage.Direction.Receive, AIRCRRTransactionAcceptedMessageText.Replace("\r\n", ""));
			CMRCUSRESMessage clearResponseMessage1 = (CMRCUSRESMessage)CreateMessage(typeof(CMRCARSTMessage), 2, new ZDateTime(2005, 2, 2), EDIMessage.Direction.Receive, CARSTClearMessage1.Replace("\r\n", ""));
			CMRCUSRESMessage heldResponseMessage = (CMRCUSRESMessage)CreateMessage(typeof(CMRCARSTMessage), 3, new ZDateTime(2005, 4, 4), EDIMessage.Direction.Receive, CARSTHeldMessage.Replace("\r\n", ""));
			CMRCUSRESMessage clearResponseMessage2 = (CMRCUSRESMessage)CreateMessage(typeof(CMRCARSTMessage), 4, new ZDateTime(2005, 3, 3), EDIMessage.Direction.Receive, CARSTClearMessage2.Replace("\r\n", ""));

			Factory.Save();

			AssertEquals("The last 2 messages must be received out of order", new ZDateTime(2005, 4, 4), heldResponseMessage.EM_SystemCreateTimeUtc);
			AssertEquals("The last 2 messages must be received out of order", new ZDateTime(2005, 3, 3), clearResponseMessage2.EM_SystemCreateTimeUtc);

			transactionAcceptedMessage.SetEM_LinkedObject();
			Factory.Save();
			clearResponseMessage1.SetEM_LinkedObject();
			Factory.Save();
			heldResponseMessage.SetEM_LinkedObject();
			Factory.Save();
			clearResponseMessage2.SetEM_LinkedObject();
			Factory.Save();

			AssertEquals("Status should be clear, because that was the status of the message with the highest message number", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, hAWB.CMRCargoStatus.Code);
		}

		public void TestGetSortedMessages()
		{
			EDIMessage message1 = CreateMessage(1, new ZDateTime(2005, 6, 17, 12, 6, 0), EDIMessage.Direction.Transmit, "Message1");
			EDIMessage message2 = CreateMessage(1, new ZDateTime(2005, 6, 17, 12, 8, 0), EDIMessage.Direction.Receive, "Message2");//3
			EDIMessage message3 = CreateMessage(2, new ZDateTime(2005, 6, 17, 12, 8, 0), EDIMessage.Direction.Transmit, "Message3");//4
			EDIMessage message4 = CreateMessage(3, new ZDateTime(2005, 6, 17, 12, 8, 0), EDIMessage.Direction.Transmit, "Message4");//2
			EDIMessage message5 = CreateMessage(3, new ZDateTime(2005, 6, 17, 12, 10, 0), EDIMessage.Direction.Receive, "Message5");
			EDIMessage message6 = CreateMessage(4, new ZDateTime(2005, 6, 17, 12, 10, 0), EDIMessage.Direction.Receive, "Message6");
			EDIMessage message7 = CreateMessage(4, new ZDateTime(2005, 6, 17, 12, 12, 0), EDIMessage.Direction.Receive, "Message7");
			EDIMessage message8 = CreateMessage(5, new ZDateTime(2005, 6, 17, 12, 12, 0), EDIMessage.Direction.Receive, "Message8");
			EDIMessage message9 = CreateMessage(0, ZDateTime.Empty, EDIMessage.Direction.Transmit, "Message9");

			EDIMessage[] result = Calculator.GetSortedMessagesForTest(GetMessagesInRandomOrder(message1, message2, message3, message4, message5, message6, message7, message8, message9));

			ZString expected = "Message9\r\nMessage8\r\nMessage7\r\nMessage6\r\nMessage5\r\nMessage4\r\nMessage3\r\nMessage2\r\nMessage1\r\n";
			ZString actual = new ZString();
			foreach (EDIMessage message in result)
			{
				actual += message.EM_MessageText + "\r\n";
			}
			AssertEquals("Messages", expected, actual);
		}

		EDIMessage[] GetMessagesInRandomOrder(params EDIMessage[] messages)
		{
			Random ransomNumberGenerator = new Random();
			EDIMessage[] result = new EDIMessage[messages.Length];
			int numberAssigned = 0;
			while (numberAssigned < messages.Length)
			{
				int indexToUse = ransomNumberGenerator.Next(messages.Length);
				if (messages[indexToUse] != null)
				{
					result[numberAssigned] = messages[indexToUse];
					messages[indexToUse] = null;
					numberAssigned++;
				}
			}
			return result;
		}

		EDIMessage CreateMessage(int interchangeNumber, ZDateTime systemCreateTime, ZString direction, ZString messageText)
		{
			return CreateMessage(typeof(EDIMessage), interchangeNumber, systemCreateTime, direction, messageText);
		}

		EDIMessage CreateMessage(Type eDIMessageType, int interchangeNumber, ZDateTime systemCreateTime, ZString direction, ZString messageText)
		{
			EDIMessage result = (EDIMessage)Factory.New(eDIMessageType);
			result.EM_ReceiveTransmit = direction;
			result.EM_MessageText = messageText;
			if (!systemCreateTime.IsEmpty)
			{
				result.EM_SystemCreateTimeUtc = systemCreateTime;
			}
			if (interchangeNumber != 0)
			{
				EDIInterchange interchange = Factory.New<EDIInterchange>();
				interchange.EI_InterchangeNum = interchangeNumber.ToString();
				interchange.ContainedMessages.Add(result);
			}
			return result;
		}

		TestHelperStatusNeedsRecalculationProvider Parent => parent ?? (parent = Factory.New<TestHelperStatusNeedsRecalculationProvider>());
		TestHelperStatusNeedsRecalculationProvider parent;

		TestHelperCMRStatusBaseCalculator Calculator => new TestHelperCMRStatusBaseCalculator(Parent);

		TestHelperAirOUTStatusBaseCalculator AirOutStatusCalculator => new TestHelperAirOUTStatusBaseCalculator(Parent);

		TestHelperSeaOUTStatusBaseCalculator SeaOutStatusCalculator => new TestHelperSeaOUTStatusBaseCalculator(Parent);

		TestHelperCMRStatusBaseCalculator CalculatorWithCONTRLRejectedOriginal
		{
			get
			{
				EDIMessage message = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				message.EM_ApplicationReference = TestStatus.OriginalRejection;
				message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				message.EM_Status = EDIMessage.Status.Rejected;
				return new TestHelperCMRStatusBaseCalculator(Parent);
			}
		}

		TestHelperStatusCalculatorForRejectedOutgoingMessage CalculatorWithCONTRLRejectedOriginalFollowedByValidResponse
		{
			get
			{
				var outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				outgoingMessage.EM_Status = EDIMessage.Status.Rejected;
				Factory.Save();
				System.Threading.Thread.Sleep(1000);
				var incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				incomingMessage.EM_MessageText = TestStatus.OriginalWithErrors;
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Error;
				Factory.Save();
				return new TestHelperStatusCalculatorForRejectedOutgoingMessage(Parent);
			}
		}

		TestHelperCMRStatusBaseCalculator CalculatorWithRejectedAmendment
		{
			get
			{
				EDIMessage outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
				outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				outgoingMessage.EM_ApplicationReference = TestStatus.AmendmentRejection;
				Factory.Save();
				System.Threading.Thread.Sleep(1000);
				EDIMessage incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				incomingMessage.EM_MessageText = TestStatus.AmendmentRejection;
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
				Factory.Save();
				return new TestHelperCMRStatusBaseCalculator(Parent);
			}
		}

		TestHelperCMRStatusBaseCalculator CalculatorWithAcceptedWithErrorsOriginal
		{
			get
			{
				EDIMessage outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				outgoingMessage.EM_ApplicationReference = TestStatus.OriginalWithErrors;
				Factory.Save();
				System.Threading.Thread.Sleep(1000);
				EDIMessage incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				incomingMessage.EM_MessageText = TestStatus.OriginalWithErrors;
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Error;
				Factory.Save();
				return new TestHelperCMRStatusBaseCalculator(Parent);
			}
		}

		TestHelperCMRStatusBaseCalculator CalculatorWithNonRespondedToWithdrawal
		{
			get
			{
				EDIMessage outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_ApplicationReference = TestStatus.WithdrawNotResponded;
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
				return new TestHelperCMRStatusBaseCalculator(Parent);
			}
		}

		TestHelperCMRStatusBaseCalculator CalculatorWithResponse
		{
			get
			{
				_ = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				TestHelperCMRStatusBaseCalculator result = new TestHelperCMRStatusBaseCalculator(Parent);
				result.ExpectedStatus = "123";
				return result;
			}
		}

		sealed class TestStatus
		{
			public const string OriginalRejection = "OriginalRejection";
			public const string AmendmentRejection = "AmendmentRejection";
			public const string WithdrawNotResponded = "WithdrawNotResponded";
			public const string OriginalWithErrors = "OriginalWithErrors";
		}

		class TestHelperCMRStatusBaseCalculator : CMRStatusCalculatorBase<TestHelperStatusNeedsRecalculationProvider>
		{
			public TestHelperCMRStatusBaseCalculator(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			internal ZString ExpectedStatus;

			internal void DeriveStatusForTest() => DeriveStatus();

			internal TestHelperStatusNeedsRecalculationProvider ParentForTest => Parent;

			internal EDIMessage[] MessagesWeAreInterestedInForTest => MessagesWeAreInterestedIn;

			internal EDIMessage[] GetSortedMessagesForTest(EDIMessage[] messages) => base.GetSortedMessages(messages);

			public void SetAcceptedWithErrorsSupported(bool value)
			{
				acceptedWithErrorsSupported = value;
			}

			protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIRCR };

			protected internal override ZPropertyInfo StatusInfo => Parent.Z0_VarCharMaxInfo;

			protected internal override ZString GetStatusFromInboundMessage(EDIMessage message)
			{
				if (!ExpectedStatus.IsEmpty)
				{
					return ExpectedStatus;
				}
				else
				{
					return "default status";
				}
			}

			protected internal override ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage) => outgoingMessage.EM_ApplicationReference;

			protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage) => outgoingMessage.EM_ApplicationReference;

			protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage) => outgoingMessage.EM_ApplicationReference;

			protected override CodeDescriptionPairList StatusList => new CodeDescriptionPairList();

			protected override bool AcceptedWithErrorsSupported => acceptedWithErrorsSupported;

			bool acceptedWithErrorsSupported;

			protected internal override ZString GetAcceptedWithErrorsResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
				=> outgoingMessage.EM_ApplicationReference;
		}

		sealed class TestHelperAirOUTStatusBaseCalculator : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperAirOUTStatusBaseCalculator(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIROUT };

			protected internal override bool InterestedInPendingMessages => true;

			protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage)
			{
				var result = ZString.Empty;
				switch (outgoingMessage.EM_MessageSubType)
				{
					case CMRMessage.MessageSubTypes.Original:
					case CMRMessage.MessageSubTypes.Request:
						result = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
						break;
					case CMRMessage.MessageSubTypes.Amendment:
					case CMRMessage.MessageSubTypes.Change:
					case CMRMessage.MessageSubTypes.ReplaceHeader:
						result = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
						break;
					case CMRMessage.MessageSubTypes.Withdraw:
						result = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
						break;
				}
				return result;
			}
		}

		sealed class TestHelperSeaOUTStatusBaseCalculator : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperSeaOUTStatusBaseCalculator(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEAOUT };

			protected internal override bool InterestedInPendingMessages => true;

			protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage)
			{
				var result = ZString.Empty;
				switch (outgoingMessage.EM_MessageSubType)
				{
					case CMRMessage.MessageSubTypes.Original:
					case CMRMessage.MessageSubTypes.Request:
						result = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
						break;
					case CMRMessage.MessageSubTypes.Amendment:
					case CMRMessage.MessageSubTypes.Change:
					case CMRMessage.MessageSubTypes.ReplaceHeader:
						result = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
						break;
					case CMRMessage.MessageSubTypes.Withdraw:
						result = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
						break;
				}
				return result;
			}
		}

		sealed class TestHelperCARSTStatusCalculator : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperCARSTStatusCalculator(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARST };
		}

		sealed class TestHelperStatusCalculatorUsingIncomingMessage : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperStatusCalculatorUsingIncomingMessage(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			internal EDIMessage[] SortedMessagesExposed;

			protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage) => incomingMessage.EM_ApplicationReference;

			protected override EDIMessage[] GetSortedMessages(EDIMessage[] messages) => SortedMessagesExposed;
		}

		sealed class TestHelperStatusCalculatorForRejectedOutgoingMessage : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperStatusCalculatorForRejectedOutgoingMessage(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage) => TestStatus.OriginalRejection;

			protected internal override ZString GetAcceptedWithErrorsResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage) => TestStatus.OriginalWithErrors;
		}

		sealed class TestHelperStatusCalculatorWithOverridenDeriveStatusCore : TestHelperCMRStatusBaseCalculator
		{
			public TestHelperStatusCalculatorWithOverridenDeriveStatusCore(TestHelperStatusNeedsRecalculationProvider parent) : base(parent)
			{
			}

			internal bool DeriveStatusWasCalled { get; set; }

			protected override void DeriveStatusCore()
			{
				base.DeriveStatusCore();
				DeriveStatusWasCalled = true;
			}
		}
	}
}
