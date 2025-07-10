using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business.Testing;

public static class MessageProcessorTestHelper
{
	public static void SetupEDIInterchangeWithLink(CusEntryHeader entry, EDIMessage incomingMessage, bool setupForFailure = false)
	{
		var factory = entry.Factory;
		var sessionGUID = ZGuid.BrettsGuid;
		var originalOutgoingEDIMessage = factory.New<BEMessage>();
		originalOutgoingEDIMessage.EM_Status = EDIMessage.Status.Sent;

		if (!setupForFailure)
		{
			originalOutgoingEDIMessage.EM_LinkTable = entry.TableName;
			originalOutgoingEDIMessage.EM_LinkUniqueID = entry.PK;
		}

		var outgoingInterchange = factory.New<BECInterchange>();
		outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
		outgoingInterchange.EI_SessionGUID = sessionGUID;
		outgoingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingInterchange.EI_From = "DEJOS";
		outgoingInterchange.EI_To = "DEFRANS";
		originalOutgoingEDIMessage.EM_EI = outgoingInterchange.PK;

		var interchangeIncoming = factory.New<BECInterchange>();
		interchangeIncoming.EI_SessionGUID = sessionGUID;
		interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		interchangeIncoming.EI_From = "DEJOS";
		interchangeIncoming.EI_To = "DEFRANS";

		incomingMessage.EM_EI = interchangeIncoming.PK;
		factory.Save();
	}
}
