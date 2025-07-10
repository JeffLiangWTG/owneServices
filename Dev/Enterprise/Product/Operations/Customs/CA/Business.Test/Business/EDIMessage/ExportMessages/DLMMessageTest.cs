using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DLMMessage))]
	sealed class DLMMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<DLMMessage>();
			AssertEquals("EM_MessageType", MessageTypeList.Codes.DataLoadingModule, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.DataLoadingModule, message.EM_MessageSubType);
			AssertEquals("EM_MessageSubTypeDescription", MessageTypeList.Descriptions.DataLoadingModule, message.EM_MessageSubTypeDescription);
		}

		public void TestTransmitMessageInterpretation()
		{
			var message = Factory.New<DLMMessage>();
			message.EM_MessageText = "RINV234323";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("Message Interpretation:\n" + message.EM_MessageInterpretation, true, message.EM_MessageInterpretation.Replace(" ", "").Contains("ReferenceNumber(2-36):INV234323"));

			message = Factory.New<DLMMessage>();
			message.EM_MessageText = "Bad Message Text";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("Message Interpretation", true, message.EM_MessageInterpretation.Contains("Bad Message Text"));
			AssertNotEquals("Message Interpretation", message.EM_MessageText, message.EM_MessageInterpretation);
		}
	}
}
