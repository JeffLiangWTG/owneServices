using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class NAICInterchangeUnpackerTest : TestCaseWithFactory
{
	public void TestValidCONTROL() => CombineAssertions(() =>
	{
		var unpacker = new NAICInterchangeUnpacker();
		var interchange = CreateTestInterchange(MessageTypeList.SyntaxAndServiceReportMessage);
		var unpackResult = unpacker.Unpack(interchange);

		Assert("Success", unpackResult.IsSuccess);
		var message = unpackResult.EdiMessages.Single();
		AssertEquals("Application Code", ApplicationCodeList.Codes.UAECustoms, message.EM_ApplicationCode);
		AssertEquals("Direction", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals("Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		AssertEquals("Message Number", "123H456", message.EM_MessageNum);
		AssertEquals("Message Type", AEConstants.Messaging.MessageTypes.CONTRL, message.EM_MessageType);
		AssertEquals("Message Text", $"UNH+123H456+CONTRL'", message.EM_MessageText);
		var interchangeMessage = interchange.ContainedMessages.Single();
		AssertSame("Message linked to Interchange", interchangeMessage, message);
	});

	public void TestValidCUSRES() => CombineAssertions(() =>
	{
		var unpacker = new NAICInterchangeUnpacker();
		var interchange = CreateTestInterchange(MessageTypeList.CustomsResponseMessage);
		var unpackResult = unpacker.Unpack(interchange);

		Assert("Success", unpackResult.IsSuccess);
		var message = unpackResult.EdiMessages.Single();
		AssertEquals("Message Type", AEConstants.Messaging.MessageTypes.CUSRES, message.EM_MessageType);
		AssertEquals("Message Text", $"UNH+123H456+CUSRES'", message.EM_MessageText);
	});

	public void TestNoMessageType() => CombineAssertions(() =>
	{
		var unpacker = new NAICInterchangeUnpacker();
		var interchange = CreateTestInterchange("UNKNWN");
		var unpackResult = unpacker.Unpack(interchange);

		AssertEquals("Unknown message type error", "[ERROR] Message Type extracted from the interchange is empty.",
			unpackResult.ErrorReason);
		AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
		AssertEquals("No messages connected to the interchange", 0, interchange.ContainedMessages.Count);
	});

	public void TestRetrieveInterchangeHeader() => CombineAssertions(() =>
	{
		var unpacker = new NAICInterchangeUnpacker();
		var interchange = CreateTestInterchange(MessageTypeList.CustomsResponseMessage);
		var uNBSegment = unpacker.RetrieveInterchangeHeader(interchange);

		AssertEquals("UNOB", uNBSegment.SyntaxIdentifier.SyntaxIdentifier);
		AssertEquals("4", uNBSegment.SyntaxIdentifier.SyntaxVersionNumber);
		AssertEquals("2", uNBSegment.SyntaxIdentifier.CharacterEncoding);
		AssertEquals("02", uNBSegment.SyntaxIdentifier.SyntaxReleaseNumber);
		AssertEquals("UAENAIC", uNBSegment.InterchangeSender.SenderIdentification);
		AssertEquals("SERPRID", uNBSegment.InterchangeRecipient.RecipientIdentification);
		AssertEquals("FORFFID", uNBSegment.InterchangeRecipient.InterchangeRecipientInternalIdentification);
		AssertEquals("LOCFFID", uNBSegment.InterchangeRecipient.InterchangeRecipientInternalSubIdentification);
		AssertEquals("20240516", uNBSegment.DateTimeOfPreparation.Date);
		AssertEquals("0840", uNBSegment.DateTimeOfPreparation.Time);
		AssertEquals("084059B3333333", uNBSegment.InterchangeControlReference);
	});

	EDIInterchange CreateTestInterchange(string messageType)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchange.EI_InterchangeNum = "INT001";
		interchange.EI_BodyText = @$"UNB+UNOB:4::2:02+UAENAIC+SERPRID::FORFFID:LOCFFID+20240516:0840+084059B3333333'
UNH+123H456+{messageType}'
UNZ+1+084059B3333333'
";
		return interchange;
	}
}
