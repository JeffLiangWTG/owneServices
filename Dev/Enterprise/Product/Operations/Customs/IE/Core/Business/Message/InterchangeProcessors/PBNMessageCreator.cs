using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business;

class PBNMessageCreator : IInboundMessageCreator
{
	public PBNMessageCreator(LoggingInformation logger) 
	{
		this.logger = Argument.NotNull(logger, nameof(logger));
	}

	public void CreateMessagesForInterchange(EDIInterchange interchange)
	{
		try
		{
			var incomingMessage = CreateIncomingMessage(interchange);
			logger.Log(string.Format("Created message ({0}, {1}, {2}, {3}).", incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, EDIMessage.Direction.Receive, incomingMessage.EM_MessageNum));
		}
		catch (Exception e)
		{
			logger.LogError(e.Message);
		}
	}

	BaseEDIMessage CreateIncomingMessage(EDIInterchange interchange)
	{
		var incomingMessage = interchange.ContainedMessages.AddNew();
		incomingMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
		incomingMessage.EM_GB = interchange.EI_GB;
		incomingMessage.EM_MessageType = interchange.EI_InterchangeType;
		incomingMessage.EM_Status = EDIMessage.Status.Queued;
		incomingMessage.EM_MessageText = interchange.EI_BodyText;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Status.Received;
		incomingMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
		return incomingMessage;
	}

	readonly LoggingInformation logger;
}
