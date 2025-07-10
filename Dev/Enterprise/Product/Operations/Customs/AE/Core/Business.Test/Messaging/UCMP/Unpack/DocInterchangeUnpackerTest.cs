using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class DocInterchangeUnpackerTest : TestCaseWithFactory
{
	public void TestValidDOCSUC()
	{
		AssertSuccessUnpack(AEConstants.Messaging.MessageTypes.DOCSUC);
	}

	public void TestValidDOCERR()
	{
		AssertSuccessUnpack(AEConstants.Messaging.MessageTypes.DOCERR);
	}

	void AssertSuccessUnpack(string messageType) => CombineAssertions(() =>
	{
		var unpacker = new DocInterchangeUnpacker();
		var interchange = CreateIncomingInterchange(messageType);
		var outgoingInterchange = CreateOutgoingInterchange(messageType, interchange.EI_SessionGUID);
		var outgoingMessage = CreateOutgoingMessage(outgoingInterchange);

		var unpackResult = unpacker.Unpack(interchange, outgoingMessage);

		Assert("Success", unpackResult.IsSuccess);
		var message = unpackResult.EdiMessages.Single();
		AssertEquals("Application Code", ApplicationCodeList.Codes.UAECustoms, message.EM_ApplicationCode);
		AssertEquals("Message Type", messageType, message.EM_MessageType);
		AssertEquals("Message SubType", Common.Shared.MessageSubTypeCodes.Codes.Undefined, message.EM_MessageSubType);
		AssertEquals("Direction", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals("Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		AssertEquals("Message Text", interchange.EI_BodyText, message.EM_MessageText);
		AssertEquals("Is Active", true, message.EM_IsActive);
		AssertEquals("Message Number", "IN001", message.EM_MessageNum);
		AssertSame("Interchange", interchange, message.Interchange);
	});

	public void TestOutgoingMessageNotFound() => CombineAssertions(() =>
	{
		var unpacker = new DocInterchangeUnpacker();
		var interchange = CreateIncomingInterchange(AEConstants.Messaging.MessageTypes.DOCSUC);

		var unpackResult = unpacker.Unpack(interchange, null);

		AssertEquals("Error", "[ERROR] Outbound Document Submission message not found.", unpackResult.ErrorReason);
		AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
		AssertEquals("No messages connected to the interchange", 0, interchange.ContainedMessages.Count);
	});

	EDIInterchange CreateIncomingInterchange(string messageType)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchange.EI_InterchangeType = messageType;
		interchange.EI_InterchangeNum = "IN001";
		interchange.EI_BodyText = "{\"key\":\"value\"}";
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		return interchange;
	}

	EDIInterchange CreateOutgoingInterchange(string messageType, ZGuid sessionGUID)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		interchange.EI_InterchangeType = messageType;
		interchange.EI_InterchangeNum = "OUT001";
		interchange.EI_BodyText = "{\"key\":\"value\"}";
		interchange.EI_SessionGUID = sessionGUID;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
		return interchange;
	}

	EDIMessage CreateOutgoingMessage(EDIInterchange outgoingInterchange)
	{
		var message = outgoingInterchange.ContainedMessages.AddNew();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		message.EM_LinkedObject = Factory.New<LinkedObjectForTest>();
		return message;
	}
}
