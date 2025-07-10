using System;
using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeliveryOrderEventLogManagerTest : TestCaseWithFactory
	{
		[TestDate(2024, 12, 03, 14, 33, 3)]
		public void TestDetermineEventAndLog_MessageAccepted_WhenLastEventIsMSNAndResponseStatusIs1Or2()
		{
			(var response, var shipment) = PrepareTestEntities(1, false);

			var deliveryOrderEventLogManager = new DeliveryOrderEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event Is MSN and ResponseStatus in (1,2)",
				() =>
			{
				AssertEquals(Events.MessageAccepted, @event);
				var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageAccepted);
				AssertNotNull(theExpectedEventLog);
				AssertEquals("|DEP=Customs|MST=Delivery Order|RFN=1003", theExpectedEventLog.SL_Reference);
				AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 3), theExpectedEventLog.SL_EventTime);
			});
		}

		[TestDate(2024, 12, 03, 14, 33, 4)]
		public void TestDetermineEventAndLog_MessageRejected_WhenLastEventIsMSNAndResponseStatusIs3()
		{
			(var response, var shipment) = PrepareTestEntities(3, false);

			var deliveryOrderEventLogManager = new DeliveryOrderEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event Is MSN and ResponseStatus is 3",
				() =>
				{
					AssertEquals(Events.MessageRejected, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageRejected);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Delivery Order|RFN=1003", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 4), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 03, 14, 33, 5)]
		public void TestDetermineEventAndLog_MessageWithdrawCancelAccepted_WhenLastEventIsMWRAndResponseStatusIs1Or2()
		{
			(var response, var shipment) = PrepareTestEntities(2, true);

			var deliveryOrderEventLogManager = new DeliveryOrderEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event Is MWR and ResponseStatus In (1,2)",
				() =>
				{
					AssertEquals(Events.MessageWithdrawCancelAccepted, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelAccepted);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Delivery Order|RFN=1003", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 5), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 03, 14, 33, 6)]
		public void TestDetermineEventAndLog_MessageRejected_WhenLastEventIsMWRAndResponseStatusIs3()
		{
			(var response, var shipment) = PrepareTestEntities(3, true);

			var deliveryOrderEventLogManager = new DeliveryOrderEventLogManager();
			var @event = deliveryOrderEventLogManager.DetermineEventAndLog(response, shipment);

			CombineAssertions("When Last Event Is MWR and ResponseStatus In (1,2)",
				() =>
				{
					AssertEquals(Events.MessageRejected, @event);
					var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageRejected);
					AssertNotNull(theExpectedEventLog);
					AssertEquals("|DEP=Customs|MST=Delivery Order|RFN=1003", theExpectedEventLog.SL_Reference);
					AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 6), theExpectedEventLog.SL_EventTime);
				});
		}

		[TestDate(2024, 12, 03, 14, 33, 6)]
		public void TestLogEventWithParameters_ShouldLogEventWithCorrectParameters()
		{
			(var response, var shipment) = PrepareTestEntities(3, true);
			var deliveryOrderEventLogManager = new DeliveryOrderEventLogManager();

			deliveryOrderEventLogManager.LogEventWithParameters(response, shipment, Events.MessageAccepted);

			var theExpectedEventLog = shipment.Logs.MostRecentLogByEventTime(Events.MessageAccepted);
			AssertNotNull(theExpectedEventLog);
			AssertEquals("|DEP=Customs|MST=Delivery Order|RFN=1003", theExpectedEventLog.SL_Reference);
			AssertEquals(new ZDateTime(2024, 12, 03, 14, 33, 6), theExpectedEventLog.SL_EventTime);
		}

		(MnNg1220Msg22DeliveryOrderFeedBackMessage response, ForwardingShipment shipment) PrepareTestEntities(int responseStatus, bool withdrawCancelRequestIsTheLastMessage)
		{
			var response = new MnNg1220Msg22DeliveryOrderFeedBackMessage()
			{
				DeliveryOrderResponse = new System.Collections.ObjectModel.Collection<MnNg1220Msg22DeliveryOrderFeedBackMessageDeliveryOrderResponse>
				 {
					 new MnNg1220Msg22DeliveryOrderFeedBackMessageDeliveryOrderResponse()
					 {
						 ResponseStatus = responseStatus,
					 }
				 },
				ResponseContentHeader = new CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.NS1.ResponseContentHeader
				{
					TransmitionDateTime = new DateTime(2021, 1, 1)
				}
			};
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_DLO = "1003";
			shipment.Logs.AddNew(Events.MessageSent, "MST=Delivery Order", ZDateTimeOffset.Now.AddHours(withdrawCancelRequestIsTheLastMessage ? -2 : -1));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, "MST=Delivery Order", ZDateTimeOffset.Now.AddHours(withdrawCancelRequestIsTheLastMessage ? -1 : -2));

			return (response, shipment);
		}
	}
}
