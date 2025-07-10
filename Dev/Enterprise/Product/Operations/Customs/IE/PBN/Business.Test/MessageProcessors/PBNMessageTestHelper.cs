using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

public static class PBNMessageTestHelper
{
	public static CreateAndUpdatePBNMessageDefinition GetCreateOrUpdatePBNResponse()
	{
		if (textForCPBResponse == null)
		{
			textForCPBResponse = new CreateAndUpdatePBNMessageDefinition();
			textForCPBResponse.PbnID = "AA11GH99";
			textForCPBResponse.Status = "PROCEED";
		}
		return textForCPBResponse;
	}
	static CreateAndUpdatePBNMessageDefinition textForCPBResponse;

	public static (AsycudaManifestHeader relatedJobAndAttachee, PBNOutboundEDIMessage outgoingMessage, PBNInboundEDIMessage incomingMessage) SetupMessages(BusinessObjectFactory factory, string messageText, string messageType, bool needSaving = false)
	{
		var manifestHeader = factory.New<AsycudaManifestHeader>();
		var staff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "!1@"));
		staff ??= MessageProcessorNotificationTestHelper.CreateStaff(factory, "!1@", "Staff 1", "staff1@where.com");
		var outgoingMessage = CreateOutgoingEDIMessage(factory, messageType);
		var incomingMessage = CreateIncomingEDIMessage(factory, messageText, messageType);

		var interchanges = CreateInterchanges(factory, messageText, messageType);

		outgoingMessage.EM_EI = interchanges.outgoingInterchange.PK;
		outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
		outgoingMessage.EM_LinkedObject = manifestHeader;

		incomingMessage.EM_EI = interchanges.incomingInterchange.PK;

		if (needSaving)
		{
			factory.Save();
		}

		return (manifestHeader, outgoingMessage, incomingMessage);
	}

	static PBNOutboundEDIMessage CreateOutgoingEDIMessage(BusinessObjectFactory factory, string messageType)
	{
		var message = factory.New<PBNOutboundEDIMessage>();
		message.EM_ReceiveTransmit = "TRX";
		message.EM_MessageType = messageType;
		message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
		return message;
	}

	static PBNInboundEDIMessage CreateIncomingEDIMessage(BusinessObjectFactory factory, string messageText, string messageType)
	{
		var message = factory.New<PBNInboundEDIMessage>();
		message.EM_ReceiveTransmit = "RCV";
		message.EM_MessageType = messageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageText = messageText;
		message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
		return message;
	}

	static (EDIInterchange incomingInterchange, EDIInterchange outgoingInterchange) CreateInterchanges(BusinessObjectFactory factory, string msgBodyText, string messageType)
	{
		var utcNow = ZDateTime.UtcNow;

		var outgoing_intchg = InterchangeCreator.CreateOutgoingInterchange(
			factory: factory,
			applicationCode: EDIInterchange.ApplicationCodes.IECustomsPBN,
			interchangeType: messageType,
			branchPK: MasterFiles.Business.GlbBranch.CurrentBranch.PK,
			webServiceEndPoint: "",
			bodyText: msgBodyText);
		outgoing_intchg.EI_SystemCreateTimeUtc = utcNow.AddMinutes(-1);

		var incoming_intchg = InterchangeProcessorTestHelper.CreateIncomingInterchange(
			factory: factory,
			applicationCode: EDIInterchange.ApplicationCodes.IECustomsPBN,
			interchangeType: messageType,
			bodyText: msgBodyText,
			wrapInSOAPEnvelope: false);
		incoming_intchg.EI_SystemCreateTimeUtc = utcNow;
		incoming_intchg.EI_SessionGUID = outgoing_intchg.EI_SessionGUID;

		return (incoming_intchg, outgoing_intchg);
	}

	public static string universalInterchangeEventText => @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>WTLSGC</SenderID>
    <RecipientID>IECustomsTest</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Event>
        <EventTime>2025-05-23 20:41:03.014</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <MessageType>XER</MessageType>
          <Type>TransmissionError</Type>
          <Reason>{""status"":""REJECTED"",""validationErrors"":[{""code"":""RORO-0005"",""path"":""declarations[0].declarationId"",""description"":""Invalid Format MRN 21IEDUB1E8AC14A6R9""}]}</Reason>
        </EventParameters>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";
}
