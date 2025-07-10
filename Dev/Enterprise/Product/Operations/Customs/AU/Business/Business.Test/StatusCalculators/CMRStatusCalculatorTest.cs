using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(StatusCalculatorForTest))]
	sealed class CMRStatusCalculatorTest : StatusCalculatorTest
	{
		public void TestDeriveStatusForCONTRLRejectedOutgoing()
		{
			CalculatorWithCONTRLRejectedOriginal.DeriveStatusForTest();
			AssertEquals("Status", CMRBaseStatuses.Codes.OriginalRejected, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForRejectedOutgoing()
		{
			CalculatorWithRejectedAmendment.DeriveStatusForTest();
			AssertEquals("Status", CMRBaseStatuses.Codes.AmendmentRejected, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForNoResponseReceived()
		{
			CalculatorWithNonRespondedToWithdrawal.DeriveStatusForTest();
			AssertEquals("Status", CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, Parent.Z0_VarCharMax);
		}

		public void TestDeriveStatusForResponseReceived()
		{
			CalculatorWithResponse.DeriveStatusForTest();
			AssertEquals("Status", CalculatorWithResponse.ExpectedStatus, Parent.Z0_VarCharMax);
		}

		public void TestGetAwaitingResponseStatus()
		{
			EDIMessage message = Factory.New<CMRAIRCRMessage>();
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToOriginal, Calculator.GetAwaitingResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Request;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToOriginal, Calculator.GetAwaitingResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToAmendment, Calculator.GetAwaitingResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToAmendment, Calculator.GetAwaitingResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.ReplaceHeader;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToAmendment, Calculator.GetAwaitingResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			AssertEquals("AwaitingResponseStatus", CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, Calculator.GetAwaitingResponseStatusForTest(message));
		}

		public void TestGetRejectedResponseStatus()
		{
			EDIMessage message = Factory.New<CMRAIRCRMessage>();
			EDIMessage incomingMessage = Factory.New<CMRAIRCRRMessage>();

			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.OriginalRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Request;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.OriginalRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.ReplaceHeader;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.WithdrawalRejected, Calculator.GetRejectedResponseStatusForTest(message, incomingMessage));
		}

		public void TestGetAcceptedResponseStatus()
		{
			EDIMessage message = Factory.New<CMRAIRCRMessage>();
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.OriginalAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Request;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.OriginalAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.ReplaceHeader;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.AmendmentAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			AssertEquals("RejectedResponseStatus", CMRBaseStatuses.Codes.WithdrawalAccepted, Calculator.GetAcceptedResponseStatusForTest(message));
		}

		public void TestGetStatusForIncomingCARSTMessage()
		{
			TestGetStatusForIncomingCARSTMessage("CLEAR", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
			TestGetStatusForIncomingCARSTMessage("CONDCLEAR", CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
			TestGetStatusForIncomingCARSTMessage("CLEARHRM", CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
			TestGetStatusForIncomingCARSTMessage("DCLALLOWED", CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo);
			TestGetStatusForIncomingCARSTMessage("SUBUBMOV", CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed);
			TestGetStatusForIncomingCARSTMessage("TRANSHIP", CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus);
			TestGetStatusForIncomingCARSTMessage("TRANSIT", CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction);
			TestGetStatusForIncomingCARSTMessage("TRANSHPHRM", CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement);
			TestGetStatusForIncomingCARSTMessage("HELD", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl);
			TestGetStatusForIncomingCARSTMessage("ACSSEIZED", CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms);
			TestGetStatusForIncomingCARSTMessage("AQISSEIZED", CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine);
			TestGetStatusForIncomingCARSTMessage("WITHDRAWN", CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn);
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForOrgOnly()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Original, "Send", "", "");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForAmdOnly()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Amendment, "Send", "", "");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForWdrOnly()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Withdraw, "Withdraw", "", "");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForOrgAcc()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Original, "Send", CMRMessage.ManifestResponseSubTypes.Clear, "Accepted");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForOrgRej()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Original, "Send", CMRMessage.ManifestResponseSubTypes.Rejected, "Rejected");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForAmdAcc()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Amendment, "Send", CMRMessage.ManifestResponseSubTypes.Clear, "Accepted");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForAmdRej()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Amendment, "Send", CMRMessage.ManifestResponseSubTypes.Rejected, "Rejected");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForWdrAcc()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Withdraw, "Withdraw", CMRMessage.ManifestResponseSubTypes.Clear, "Accepted");
		}

		[TestDate(2008, 1, 1)]
		public void TestAddingCargoReportEventForWdrRej()
		{
			RunOneCargoReportEventTest(CMRMessage.MessageSubTypes.Withdraw, "Withdraw", CMRMessage.ManifestResponseSubTypes.Rejected, "Rejected");
		}

		void RunOneCargoReportEventTest(ZString sendMessageSubType, ZString expectedSendEvent1, ZString receiveMessageSubType, ZString expectedReceiveEvent1)
		{
			var house = Factory.New<CusSCAHouse>();
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00000001";
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00000002";
			var mawb = Factory.New<CusMAWB>();
			CusHAWB hawb = mawb.ChildBills.AddNew();
			Event expectedSendEvent = null;
			var sendStatus = ZString.Empty;
			var receiveStatus = ZString.Empty;
			switch (expectedSendEvent1)
			{
				case "Send":
					{
						expectedSendEvent = ((IParentForCargoReporter)shipment1).CargoReportSentEvent;
						sendStatus = sendMessageSubType == CMRMessage.MessageSubTypes.Original ? "WTO" : "WTA";
						receiveStatus = sendMessageSubType == CMRMessage.MessageSubTypes.Original ? "O" : "A";
						break;
					}
				case "Withdraw":
					{
						expectedSendEvent = ((IParentForCargoReporter)shipment1).CargoReportWithdrawEvent;
						sendStatus = "WTW";
						receiveStatus = "W";
						break;
					}
			}

			Event expectedReceiveEvent = null;
			switch (expectedReceiveEvent1)
			{
				case "Accepted":
					{
						expectedReceiveEvent = ((IParentForCargoReporter)shipment1).CargoReportAcceptedEvent;
						receiveStatus = "AC" + receiveStatus;
						break;
					}
				case "Rejected":
					{
						expectedReceiveEvent = ((IParentForCargoReporter)shipment1).CargoReportRejectedEvent;
						receiveStatus = "RJ" + receiveStatus;
						break;
					}
			}

			house.CA_JS = shipment1.PK;
			hawb.CS_JS = shipment2.PK;
			Factory.Save();

			EDIMessage outgoingMessage1 = house.Messages.AddNew(typeof(CMRSEACRMessage));
			outgoingMessage1.EM_MessageSubType = sendMessageSubType;
			outgoingMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			LogsForNominatedEvent eventLogs = new LogsForNominatedEvent(shipment1.Logs, expectedSendEvent);
			AssertEquals("send log exists", 1, eventLogs.Count);
			AssertEquals("send log is correct date", new ZDateTime(2008, 01, 01), eventLogs[0].SL_EventTime);
			AssertEquals("send log has correct reference", sendStatus + " Shipment: S00000001", eventLogs[0].SL_Reference);
			if (expectedReceiveEvent1 != "")
			{
				System.Threading.Thread.Sleep(1000);
				EDIMessage incomingMessage1 = house.Messages.AddNew(typeof(CMRSEACRRMessage));
				incomingMessage1.EM_MessageSubType = receiveMessageSubType;
				AssertEquals("log exists", 0, new LogsForNominatedEvent(shipment1.Logs, expectedReceiveEvent).Count);
				Factory.Save();
				eventLogs = new LogsForNominatedEvent(shipment1.Logs, expectedReceiveEvent);
				AssertEquals("log exists " + sendMessageSubType + " " + receiveMessageSubType, 1, eventLogs.Count);
				AssertEquals("correct date " + sendMessageSubType + " " + receiveMessageSubType, new ZDateTime(2008, 01, 01), eventLogs[0].SL_EventTime);
				AssertEquals("correct reference", receiveStatus + " Shipment: S00000001", eventLogs[0].SL_Reference);
			}

			EDIMessage outgoingMessage2 = hawb.Messages.AddNew(typeof(CMRAIRCRMessage));
			outgoingMessage2.EM_MessageSubType = sendMessageSubType;
			outgoingMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			eventLogs = new LogsForNominatedEvent(shipment2.Logs, expectedSendEvent);
			AssertEquals("send log exists", 1, eventLogs.Count);
			AssertEquals("send log is correct date", new ZDateTime(2008, 01, 01), eventLogs[0].SL_EventTime);
			AssertEquals("send log has correct reference", sendStatus + " Shipment: S00000002", eventLogs[0].SL_Reference);
			if (expectedReceiveEvent1 != "")
			{
				System.Threading.Thread.Sleep(1000);
				EDIMessage incomingMessage2 = hawb.Messages.AddNew(typeof(CMRAIRCRRMessage));
				incomingMessage2.EM_MessageSubType = receiveMessageSubType;
				AssertEquals("log exists", 0, new LogsForNominatedEvent(shipment2.Logs, expectedReceiveEvent).Count);
				Factory.Save();
				eventLogs = new LogsForNominatedEvent(shipment2.Logs, expectedReceiveEvent);
				AssertEquals("log exists " + sendMessageSubType + " " + receiveMessageSubType, 1, eventLogs.Count);
				AssertEquals("correct date " + sendMessageSubType + " " + receiveMessageSubType, new ZDateTime(2008, 01, 01), eventLogs[0].SL_EventTime);
				AssertEquals("correct reference", receiveStatus + " Shipment: S00000002", eventLogs[0].SL_Reference);
			}
		}

		void TestGetStatusForIncomingCARSTMessage(ZString status, ZString expectedCode)
		{
			var carstMessage = Factory.New<CMRCARSTMessage>();
			var messageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+IFBG 0J19 3AF:1+8'
FTX+AHN+++CONSOLIDATED STATUS:" + status + @"'
TDT+20+8313++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
RFF+MB:123'
RFF+BH:1'
RFF+AAQ:1'
DOC+1'
PAC+++FCL:67:95'
UNT+12+000001'";
			carstMessage.EM_MessageText = messageText.Replace("\r\n", "");

			var result = Calculator.GetStatusFromInboundMessageForTest(carstMessage);
			AssertEquals("Status Code", expectedCode, result);
		}

		TestHelperCMRStatusCalculator CalculatorWithCONTRLRejectedOriginal
		{
			get
			{
				EDIMessage message = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				message.EM_Status = EDIMessage.Status.Rejected;
				return new TestHelperCMRStatusCalculator(Parent);
			}
		}

		TestHelperCMRStatusCalculator CalculatorWithRejectedAmendment
		{
			get
			{
				EDIMessage outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
				outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				Factory.Save();
				System.Threading.Thread.Sleep(1000);
				EDIMessage incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				incomingMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
				Factory.Save();
				return new TestHelperCMRStatusCalculator(Parent);
			}
		}

		TestHelperCMRStatusCalculator CalculatorWithNonRespondedToWithdrawal
		{
			get
			{
				EDIMessage outgoingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
				return new TestHelperCMRStatusCalculator(Parent);
			}
		}

		TestHelperCMRStatusCalculator CalculatorWithResponse
		{
			get
			{
				EDIMessage incomingMessage = Parent.Messages.AddNew(typeof(CMRAIRCRRMessage));
				TestHelperCMRStatusCalculator result = new TestHelperCMRStatusCalculator(Parent);
				result.ExpectedStatus = "123";
				return result;
			}
		}

		TestHelperCMRStatusCalculator Calculator => new TestHelperCMRStatusCalculator(Parent);

		TestHelperStatusNeedsRecalculationProvider parent;
		TestHelperStatusNeedsRecalculationProvider Parent => parent ?? (parent = Factory.New<TestHelperStatusNeedsRecalculationProvider>());

		sealed class TestHelperCMRStatusCalculator : CMRStatusCalculator<TestHelperStatusNeedsRecalculationProvider>
		{
			public TestHelperCMRStatusCalculator(TestHelperStatusNeedsRecalculationProvider parent)
				: base(parent)
			{
			}

			internal ZString ExpectedStatus;

			internal void DeriveStatusForTest() => DeriveStatus();

			internal ZString GetAwaitingResponseStatusForTest(EDIMessage message) => GetAwaitingResponseStatus(message);

			internal ZString GetRejectedResponseStatusForTest(EDIMessage outgoingMessage, EDIMessage incomingMessage)
				=> GetRejectedResponseStatus(outgoingMessage, incomingMessage);

			internal ZString GetAcceptedResponseStatusForTest(EDIMessage outgoingMessage) => GetAcceptedResponseStatus(outgoingMessage);

			internal ZString GetStatusFromInboundMessageForTest(EDIMessage message) => base.GetStatusFromInboundMessage(message);

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
					return base.GetStatusFromInboundMessage(message);
				}
			}

			new TestHelperStatusNeedsRecalculationProvider Parent => base.Parent;
		}
	}
}
