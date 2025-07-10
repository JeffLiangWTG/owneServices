using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using UNBSegment = Enterprise.Edifact.Generic.V4.UNBSegment;
using UNHSegment = Enterprise.Edifact.D23A.Segments.UNHSegment;

namespace Enterprise.Customs.AE.Business;

sealed class NAICInterchangeUnpacker
{
	public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange)
	{
		var messageSegments = GetMessageSegmentsFromInterchange(interchange);
		var headerSegment = GetMessageHeader(messageSegments);
		var messageType = GetMessageType(headerSegment);
		if (!messageType.IsEmpty)
		{
			return new EDIInterchangeUnpackerResult(CreateRecievedEDIMessage(interchange, headerSegment.MessageReferenceNumber, messageType, messageSegments));
		}
		else
		{
			return new EDIInterchangeUnpackerResult("[ERROR] Message Type extracted from the interchange is empty.");
		}
	}

	public UNBSegment RetrieveInterchangeHeader(EDIInterchange interchange) => interchange == null ? new() : Utils.RetrieveUNBSegment(interchange.EI_BodyText);

	ZString[] GetMessageSegmentsFromInterchange(EDIInterchange interchange)
	{
		var interchangeBodySegments = Utils.SplitSegment(interchange.EI_BodyText, CharacterSet);
		return interchangeBodySegments.Where(x => !IsInterchangeSegment(x)).ToArray();

		bool IsInterchangeSegment(ZString segment)
		{
			return segment.StartsWith("UNB") || segment.StartsWith("UNZ");
		}
	}

	UNHSegment GetMessageHeader(ZString[] messageSegments)
	{
		var headerSegment = new UNHSegment();
		try
		{
			var uNH = messageSegments.First(x => x.StartsWith("UNH"));
			headerSegment.Parse(CharacterSet, uNH.Substring(0, uNH.Length - 1));
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
		}
		return headerSegment;
	}

	ZString GetMessageType(UNHSegment headerSegment)
	{
		var ediFactMessageType = headerSegment.MessageIdentifier.MessageType;
		if (ediFactMessageType == MessageTypeList.SyntaxAndServiceReportMessage)
		{
			return AEConstants.Messaging.MessageTypes.CONTRL;
		}
		if (ediFactMessageType == MessageTypeList.CustomsResponseMessage)
		{
			return AEConstants.Messaging.MessageTypes.CUSRES;
		}
		return ZString.Empty;
	}

	EDIMessage[] CreateRecievedEDIMessage(EDIInterchange interchange, string messageReferenceNumber, string messageType, ZString[] messageSegments)
	{
		var message = interchange.Factory.New<EDIMessage>();
		message.EM_ApplicationCode = interchange.EI_ApplicationCode;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_MessageNum = messageReferenceNumber;
		message.EM_MessageType = messageType;
		message.EM_MessageText = new ZStringBuilder(messageSegments).ToString();
		interchange.ContainedMessages.Add(message);
		return new[] { message };
	}

	AECharacterSet CharacterSet => characterSet ??= AECharacterSet.New();
	AECharacterSet characterSet;
}
