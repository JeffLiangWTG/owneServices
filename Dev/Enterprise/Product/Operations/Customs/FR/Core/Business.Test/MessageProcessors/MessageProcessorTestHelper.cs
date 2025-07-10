using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public static class MessageProcessorTestHelper
	{
		public static TestEDIMessage CreateEDIMessageForTesting(BusinessObjectFactory factory, string applicationCode, string receiveTransmit, string status, ZGuid branchPK, string messageText, string messageType, string messageSubType)
		{
			var message = factory.NewWithValidTestData<TestEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_GB = branchPK;
			message.EM_MessageText = messageText;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			return message;
		}

		public static void AssertEDIInterchangeProperties(EDIInterchange interchange, ZString expectedApplicationCode, ZString expectedInterchangeType, ZString expectedReceiveTransmit, ZString expectedStatus, ZString expectedFrom, ZString expectedTo, ZString expectedBodyText, ZString expectedHeaderText, string message = "")
		{
			Assertion.CombineAssertions(message, () =>
			{
				Assertion.AssertEquals("EI_ApplicationCode", expectedApplicationCode, interchange.EI_ApplicationCode);
				Assertion.AssertEquals("EI_InterchangeType", expectedInterchangeType, interchange.EI_InterchangeType);
				Assertion.AssertEquals("EI_ReceiveTransmit", expectedReceiveTransmit, interchange.EI_ReceiveTransmit);
				Assertion.AssertEquals("EI_Status", expectedStatus, interchange.EI_Status);
				Assertion.AssertEquals("Sender", expectedFrom, interchange.EI_From);
				Assertion.AssertEquals("Recepient", expectedTo, interchange.EI_To);
				Assertion.AssertEquals("EI_BodyText", expectedBodyText, interchange.EI_BodyText);
				Assertion.AssertContains("EI_HeaderText", expectedHeaderText, interchange.EI_HeaderText);
			});
		}

		public static void AssertEDIInterchangePropertiesWithJson(EDIInterchange interchange, ZString expectedApplicationCode, ZString expectedInterchangeType, ZString expectedReceiveTransmit, ZString expectedStatus, ZString expectedFrom, ZString expectedTo, ZString expectedBodyText, ZString expectedHeaderText, string message = "")
		{
			Assertion.CombineAssertions(message, () =>
			{
				Assertion.AssertEquals("EI_ApplicationCode", expectedApplicationCode, interchange.EI_ApplicationCode);
				Assertion.AssertEquals("EI_InterchangeType", expectedInterchangeType, interchange.EI_InterchangeType);
				Assertion.AssertEquals("EI_ReceiveTransmit", expectedReceiveTransmit, interchange.EI_ReceiveTransmit);
				Assertion.AssertEquals("EI_Status", expectedStatus, interchange.EI_Status);
				Assertion.AssertEquals("Sender", expectedFrom, interchange.EI_From);
				Assertion.AssertEquals("Recepient", expectedTo, interchange.EI_To);
				var expected = JsonSerializer.Serialize(JsonDocument.Parse(expectedBodyText).RootElement);
				var actual = JsonSerializer.Serialize(JsonDocument.Parse(interchange.EI_BodyText).RootElement);
				Assertion.AssertEquals("EI_BodyText", expected, actual);
				Assertion.AssertContains("EI_HeaderText", expectedHeaderText, interchange.EI_HeaderText);
			});
		}
	}
}
