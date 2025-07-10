using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

class PBNMessageCreatorTest : TestCaseWithFactory
{
	public void TestMessageResponse()
	{
		var outgoingMessage = CreateOutgoingEDIMessage(Factory, PBNMessageTypes.Codes.CreatePBN);
		var msgBodytext = JsonSerializer.Serialize(GetCPBResponse());
		var branch = GlbBranch.CurrentBranch.PK;
		var outgoing_intchg = InterchangeCreator.CreateOutgoingInterchange(
			Factory, EDIInterchange.ApplicationCodes.IECustomsPBN, "CPB",
			branch, "", msgBodytext);

		outgoing_intchg.ContainedMessages.Add(outgoingMessage);

		var incoming_intchg = InterchangeProcessorTestHelper.CreateIncomingInterchange(
			factory: Factory,
			applicationCode: EDIInterchange.ApplicationCodes.IECustomsPBN,
			interchangeType: PBNMessageTypes.Codes.CreatePBN,
			bodyText: msgBodytext,
			wrapInSOAPEnvelope: false);

		incoming_intchg.EI_SessionGUID = outgoing_intchg.EI_SessionGUID;

		Factory.Save();

		new MessageRetrievingProcessor().ExecuteBatch();
		incoming_intchg.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Incoming Interchange Status", EDIInterchange.Status.Received, incoming_intchg.EI_Status);

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, incoming_intchg.PK));
			AssertEquals("Loaded EDIMessage count", 1, msg1.Length);

			AssertEquals(1, incoming_intchg.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, incoming_intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", incoming_intchg.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsPBN, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", PBNMessageTypes.Codes.CreatePBN, msg1[0].EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			var expectedText = @"{""pbnId"":""AA11GH99"",""status"":""INCOMPLETE"",""issue"":""Import declaration MRNs are missing"",""validationErrors"":null}";
			AssertEquals("EM_MessageText body text", expectedText, msg1[0].EM_MessageText);
		});
	}

	static EDIMessage CreateOutgoingEDIMessage(BusinessObjectFactory factory, string messageType)
	{
		var message = factory.NewWithValidTestData<PBNOutboundEDIMessage>();
		message.EM_ReceiveTransmit = "TRX";
		message.EM_MessageType = messageType;
		return message;
	}

	static CreateAndUpdatePBNMessageDefinition textForCPBResponse;

	static CreateAndUpdatePBNMessageDefinition GetCPBResponse()
	{
		if (textForCPBResponse == null)
		{
			textForCPBResponse = new CreateAndUpdatePBNMessageDefinition();
			textForCPBResponse.PbnID = "AA11GH99";
			textForCPBResponse.Status = "INCOMPLETE";
			textForCPBResponse.Issue = "Import declaration MRNs are missing";
		}
		return textForCPBResponse;
	}
}
