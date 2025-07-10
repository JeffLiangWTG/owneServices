using CargoWise.IO;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.xTMessaging.Business.Test
{
	sealed class UniversalEventWrapperTest : TestCase
	{
		public void TestGenericUniversalEvent()
		{
			foreach (var eventType in new[] { AutoEvents.InterchangeAcknowledgedCode, AutoEvents.InterchangeRejectedCode }) {
				var messageText = CreateUniversalEventXml(eventType: eventType, messageType: "TST", responseType: "Response Type", reason: "Test Reason", responseMessage: "无人爱苦，亦无人寻之欲之，乃因其苦...");
				var universalEvent = new UniversalEventWrapper(messageText);
				CombineAssertions(() =>
				{
					Assert($"Should be true when EventType = {eventType}", eventType == AutoEvents.InterchangeAcknowledgedCode ? universalEvent.IsAcknowledgement : universalEvent.IsRejection);
					AssertEquals("MessageType should be TST", "TST", universalEvent.MessageType);
					AssertEquals("Response Type should equal Response Type", "Response Type", universalEvent.ResponseType);
					AssertEquals("Reason should equal Test Reason", "Test Reason", universalEvent.Reason);
					AssertEquals("Test Context Value contains complex characters", "无人爱苦，亦无人寻之欲之，乃因其苦...", universalEvent.GetContextValueByType("ResponseMessage"));
				});
			}
		}

		public void TestErrorNotificationMessageProcesessingComponentError_Pre()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Processing step execution error FAIL", originalMessage: "test failed message", originalAttributes: "fromobj : xt-application:/Framework/Direct Endpoints/WTLDPL");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a pre processing component step error", universalEvent.HasPreProcessingError);
			});
		}

		public void TestErrorNotificationMessagePreProcesessingError_WithType()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "PreProcessingError", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc", typeInEventParameters: "PreProcessingError");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a pre processing step error", universalEvent.HasPreProcessingError);
			});
		}

		public void TestErrorNotificationMessageProcesessingComponentError_Post()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Processing step execution error FAIL", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a post processing component step error", universalEvent.HasPostProcessingError);
			});
		}

		public void TestErrorNotificationMessageProcesessingError_withType()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "PostProcessingError", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc", typeInEventParameters: "PostProcessingError");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a post processing step error", universalEvent.HasPostProcessingError);
			});
		}

		public void TestErrorNotificationMessageTransmissionError_404()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description: Message transmission to http://XXX rejected by peer: result code 404 not accepted", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Transmission error", universalEvent.HasMessageTransmissionFailure);
				Assert("not a Message Unauthorized error", !universalEvent.HasUnauthorizedFailure);
			});
		}

		public void TestErrorNotificationMessageTransmissionError_NoStatusCode()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description: Message transmission to http://XXX rejected by peer:\r\nReference Object: xt-httpclientaddress:{31562e01-8ad9-4701-b841-fd3dc4e49f1d}", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Transmission error", universalEvent.HasMessageTransmissionFailure);
				Assert("not a Message Unauthorized error", !universalEvent.HasUnauthorizedFailure);
			});
		}

		public void TestErrorNotificationMessageTransmissionError_WithType()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description is Transmission error", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc", typeInEventParameters: "TransmissionError");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Transmission error", universalEvent.HasMessageTransmissionFailure);
			});
		}

		public void TestErrorNotificationBusinessError()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description from Customs", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc", typeInEventParameters: "BusinessError");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Business error", universalEvent.HasBusinessError);
			});
		}

		public void TestErrorNotificationUnauthorized_NoType()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description: Message transmission to http://XXX rejected by peer: result code 401 not accepted", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Unauthorized error", universalEvent.HasUnauthorizedFailure);
				Assert("401 is not a Message Transmission error", !universalEvent.HasMessageTransmissionFailure);
			});
		}

		public void TestErrorNotificationUnauthorized_WithType()
		{
			var messageText = CreateErrorNotificationXml(messageType: "TST", reason: "Error description: HasUnauthorizedFailure", originalMessage: "test failed message", originalAttributes: "custom.blah:blah etc", typeInEventParameters: "Unauthorized");
			var universalEvent = new UniversalEventWrapper(messageText);
			CombineAssertions(() =>
			{
				Assert("universalEvent should have a Message Unauthorized error", universalEvent.HasUnauthorizedFailure);
			});
		}

		public void TestUniversalEventGetResponseMessage()
		{
			var messageText = CreateUniversalEventXml(eventType: AutoEvents.InterchangeAcknowledgedCode, messageType: "TST", responseType: "Response Type", reason: "Test Reason", responseMessage: "this is the response!");
			var universalEvent = new UniversalEventWrapper(messageText);
			AssertEquals("GetResponseMessage function should return the responseMessage variable contents", universalEvent.GetResponseMessage(), "this is the response!");
		}

		public static string CreateErrorNotificationXml(string eventType = AutoEvents.InterchangeAcknowledgedCode, string messageType = null, string reason = null, string originalMessage = null, string originalAttributes = null, string typeInEventParameters = null)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var data = resourceRetriever.GetString("Enterprise.xTMessaging.Business.Test.TestFiles.FailureResponseTemplate.xml");
				return string.Format(data, eventType, reason, messageType, originalMessage, originalAttributes, typeInEventParameters);
			}
		}

		public static string CreateUniversalEventXml(string eventType = AutoEvents.InterchangeAcknowledgedCode, string messageType = null, string responseType = null, string reason = null, string responseMessage = null)
		{
			return $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
	<EventTime>2022-11-08T07:08:36</EventTime>
	<EventType>{eventType}</EventType>
	<EventParameters>
	  <MessageType>{messageType}</MessageType>
	  <Type>{responseType}</Type>
	  <Reason>{reason}</Reason>
	</EventParameters>
	<ContextCollection>
	  <Context>
		<Type>ResponseMessage</Type>
		<Value><![CDATA[{responseMessage}]]></Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>";
		}
	}
}
