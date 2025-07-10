using System;
using System.Reflection;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class GatePassMovementEventLogManagerTest : TestCaseWithFactory
	{
		[TestDate(2024, 12, 04, 14, 33, 3)]
		public void TestDetermineEventAndLog_MessageAccepted_WhenLastEventIsMSNAndGatePassStatusIs2()
		{
			(var response, var shipment) = PrepareTestEntities(false, 2, null);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MSN and GatePassStatus = 2",
				() =>
				{
					AssertEquals(Events.MessageAccepted, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageAccepted);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Gatepass Movement|RFN=1005", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 04, 14, 33, 3), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 04, 14, 33, 4)]
		public void TestDetermineEventAndLog_MessageRejectedByCustoms_WhenLastEventIsMSNAndGatePassStatusIs1AndGatePassReturnCodeIs6()
		{
			(var response, var shipment) = PrepareTestEntities(false, 1, 6);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MSN, GatePassStatus = 1 and GatePassReturnCode = 6",
				() =>
				{
					AssertEquals(Events.MessageRejected, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageRejected);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Gatepass Movement|RES=Rejected By Customs|RFN=1005", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 04, 14, 33, 4), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 04, 14, 33, 5)]
		public void TestDetermineEventAndLog_MessageRejectedByOriginSite_WhenLastEventIsMSNAndGatePassStatusIs1AndGatePassReturnCodeIs7()
		{
			(var response, var shipment) = PrepareTestEntities(false, 1, 7);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MSN, GatePassStatus = 1 and GatePassReturnCode = 7",
				() =>
				{
					AssertEquals(Events.MessageRejected, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageRejected);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Gatepass Movement|RES=Rejected By Origin Site|RFN=1005", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 04, 14, 33, 5), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 04, 14, 33, 6)]
		public void TestDetermineEventAndLog_MessageWithdrawCancelAccepted_WhenLastEventIsMWRAndAndGatePassReturnCodeIs5()
		{
			(var response, var shipment) = PrepareTestEntities(true, 2, 5);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MWR and GatepassReturnCode = 5",
				() =>
				{
					AssertEquals(Events.MessageWithdrawCancelAccepted, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelAccepted);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Gatepass Movement|RFN=1005", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 04, 14, 33, 6), theExpectedEventLog.SL_EventTime);
				});
		}

		public void TestDetermineEventAndLog_NoEventRaised_WhenLastEventIsMSNAndGatePassStatusIsNeither1Nor2()
		{
			(var response, var shipment) = PrepareTestEntities(false, 3, null);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MSN and GatePassStatus not in (1,2)",
				() =>
				{
					AssertNull(@event);
				});
		}

		public void TestDetermineEventAndLog_NoEventRaised_WhenLastEventIsMWRAndGatepassReturnCodeIsNot5()
		{
			(var response, var shipment) = PrepareTestEntities(true, 1, 9);

			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event = MWR and GatepassReturnCode is not 5",
				() =>
				{
					AssertNull(@event);
				});
		}

		public void TestDetermineEventType_ResponseWithEmptyGatePassFeedbackMessage_ReturnsNull()
		{
			var manager = new GatePassMovementEventLogManager();
			var response = new GpNg1035Msg2GatepassFeedbackMessage
			{
				GatepassFeedbackMessage = new System.Collections.ObjectModel.Collection<GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessage>()
			};
			var enterpriseBusinessObject = Factory.New<ForwardingShipment>();

			var methodInfo = typeof(GatePassMovementEventLogManager).GetMethod("DetermineEventType", BindingFlags.NonPublic | BindingFlags.Instance);

			var result = methodInfo.Invoke(manager, new object[] { response, enterpriseBusinessObject });

			AssertNull("Response With Empty GatePassFeedbackMessage Returns null", result);
		}

		public void TestDetermineEventType_ResponseWithoutGatePassFeedbackMessage_ReturnsNull()
		{
			var manager = new GatePassMovementEventLogManager();
			var response = new GpNg1035Msg2GatepassFeedbackMessage();
			var enterpriseBusinessObject = Factory.New<ForwardingShipment>();

			var methodInfo = typeof(GatePassMovementEventLogManager).GetMethod("DetermineEventType", BindingFlags.NonPublic | BindingFlags.Instance);

			var result = methodInfo.Invoke(manager, new object[] { response, enterpriseBusinessObject });

			AssertNull("Response Without GatePassFeedbackMessage Returns null", result);
		}

		[TestDate(2024, 12, 03, 14, 33, 6)]
		public void TestLogEventWithParameters_ShouldLogEventWithCorrectParameters()
		{
			(var response, var shipment) = PrepareTestEntities(true, 2, 5);
			var deliveryOrderEventLogManager = new GatePassMovementEventLogManager();

			deliveryOrderEventLogManager.LogEventWithParameters(response, shipment, Events.MessageAccepted);

			var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageAccepted);
			AssertNotNull(theExpectedEventLog);
			AssertEquals("|DEP=Customs|MST=Gatepass Movement|RFN=1005", theExpectedEventLog.SL_Reference);
			AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 6), theExpectedEventLog.SL_EventTime);
		}

		(GpNg1035Msg2GatepassFeedbackMessage response, ForwardingShipment shipment) PrepareTestEntities(bool withdrawCancelRequestIsTheLastMessage, int responseStatus, int? gatePassReturnCode)
		{
			var response = new GpNg1035Msg2GatepassFeedbackMessage()
			{
				GatepassFeedbackMessage = new System.Collections.ObjectModel.Collection<GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessage>()
				{
					new GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessage
					{
						GatepassStatus = responseStatus,
						GatepassReturnCode = gatePassReturnCode
					}
				},
				ResponseContentHeader = new CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.Customs.ResponseContentHeader()
				{
					TransmitionDateTime = new DateTime(2021, 1, 1)
				}
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GMN = "1005";
			shipment.Logs.AddNew(Events.MessageSent, "MST=Gatepass Movement", ZDateTimeOffset.Now.AddHours(withdrawCancelRequestIsTheLastMessage ? -2 : -1));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, "MST=Gatepass Movement", ZDateTimeOffset.Now.AddHours(withdrawCancelRequestIsTheLastMessage ? -1 : -2));

			return (response, shipment);
		}
	}
}
