using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(COLSMessage))]
	sealed class COLSMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			Env.Registry.AQISMessagingTestMode = false;
			var message = Factory.New<COLSMessage>();
			CombineAssertions("COLSMessage default values", () =>
			{
				AssertEquals(EDIMessage.ApplicationCodes.COLS, message.EM_ApplicationCode);
				AssertEquals(false, message.EM_IsTestMessage);
			});

			Env.Registry.AQISMessagingTestMode = true;
			message = Factory.New<COLSMessage>();
			AssertEquals(true, message.EM_IsTestMessage);

			message.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			message.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.LastdocAttachment;
			message.EM_ReceiveTransmit = "RCV";
			message.ResetToQueuedStatus();
			CombineAssertions("ResetToQueuedStatus", () =>
			{
				AssertEquals("Not reset EM_MessageType", AUCOLSMessageTypeList.Codes.AddNewLodgement, message.EM_MessageType);
				AssertEquals("Not reset EM_MessageSubType", AUCOLSMessageSubTypeList.Codes.LastdocAttachment, message.EM_MessageSubType);
			});
		}

		public void TestMessageReferenceNumber()
		{
			var message = Factory.New<COLSMessage>();
			AssertEquals(ZString.Empty, message.EM_MessageNum);
			Factory.Save();

			AssertNotEquals(ZString.Empty, message.EM_MessageNum);
		}

		public void TestShouldUseNTextOrEvenBetterUseMessageData()
		{
			var message = Factory.New<COLSMessage>();
			message.EM_MessageText = "A Test Message";
			AssertEquals("A Test Message", Messaging.Integration.MessageEncoding.UTF8WithoutBOM.GetString(message.EM_MessageData));
		}

		public void TestFormattedMessageText()
		{
			var message = Factory.New<COLSMessage>();
			var messageText = @"{""entryNumber"":""AAAF7YHX6"",""branchId"":""AA94XK"",""thirdPartyInd"":true,""thirdPartyEmail"":""TEST@MAIL.COM"",""aaRefNum"":""RID00"",""lateLodgementReason"":""TEST MESSAGE CONTENT"",""directionRequests"":[{""direction"":""Release On Document""}],""generalDeclaration"":""True""}";
			message.EM_MessageText = messageText;

			var formattedText = @"{
  ""entryNumber"": ""AAAF7YHX6"",
  ""branchId"": ""AA94XK"",
  ""thirdPartyInd"": true,
  ""thirdPartyEmail"": ""TEST@MAIL.COM"",
  ""aaRefNum"": ""RID00"",
  ""lateLodgementReason"": ""TEST MESSAGE CONTENT"",
  ""directionRequests"": [
    {
      ""direction"": ""Release On Document""
    }
  ],
  ""generalDeclaration"": ""True""
}";

			AssertEquals("EM_FormattedMessageText - Should use the formatted message text.", formattedText, message.EM_FormattedMessageText);
			AssertEquals("EM_MessageInterpretation - Should use the formatted message text.", formattedText, message.EM_MessageInterpretation);
		}
	}
}
