using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSPED;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class TCPResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public TCPResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new TCPStatusCalculator(), MessageTypeList.Codes.TradeChainPartner, MessageTypeList.Descriptions.TradeChainPartner)
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage message)
		{
			var tcpMessage = message as TCPMessage;
			var originalMessage = tcpMessage.OriginalMessage;
			CUSPEDMessage cuspedMessage;
			OrgHeader master = null;
			OrgHeaderTCPMessageWrapper wrapper = null;
			TradeChainPartner tradeChainPartner = null;
			if (tcpMessage != null)
			{
				cuspedMessage = originalMessage?.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet()) as CUSPEDMessage;
				if (cuspedMessage != null)
				{
					master = message.Factory.Load<OrgHeader>(originalMessage.EM_LinkUniqueID);
					wrapper = OrgHeaderTCPMessageWrapper.New(master);
					linkedObjectReference = cuspedMessage.Group7[0].RFF[0].Reference.ReferenceIdentifier;
					tradeChainPartner = OrgImpAddInfo.Get(master)?.TradeChainPartners.GetTCPByCSAID(linkedObjectReference);
				}
			}
			else
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			var resultStatus = ZString.Empty;
			var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet()) as CUSRESMessage;
			if (tradeChainPartner != null && cusresMessage != null && wrapper != null && cusresMessage.GIS.Count > 0)
			{
				if (cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode
					== ProcessingIndicatorDescriptionCodeList.ErrorMessage)
				{
					DoProcessErrorTypeMessage(cusresMessage, tcpMessage, tradeChainPartner, master);
					resultStatus = EDIMessage.Status.Received;
					message.EM_Status = resultStatus;
					wrapper.Messages.Add(message);
				}
				else if (cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode
					== ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
				{
					DoProcessAcceptedMessage(cusresMessage, tcpMessage, tradeChainPartner, master);
					resultStatus = EDIMessage.Status.Received;
					message.EM_Status = resultStatus;
					wrapper.Messages.Add(message);
				}
				else
				{
					throw new UnableToInterpretMessageException(message, this);
				}
			}
			else
			{
				throw new UnableToInterpretMessageException(message, this);
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			return resultStatus;
		}

		void DoProcessErrorTypeMessage(CUSRESMessage cusresMessage, TCPMessage message, TradeChainPartner tcp, OrgHeader master)
		{
			tcp.CA_CSAStatus = TCPStatusCalculator.CalculateTCPStatus(tcp.CA_CSAStatus, ProcessingIndicatorDescriptionCodeList.ErrorMessage);
			SendErrorReport(master, GetErrorResponseEmailAndSetOnMessage(cusresMessage, message));
		}

		void DoProcessAcceptedMessage(CUSRESMessage cusresMessage, TCPMessage message, TradeChainPartner tcp, OrgHeader master)
		{
			tcp.CA_CSAStatus = TCPStatusCalculator.CalculateTCPStatus(tcp.CA_CSAStatus, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted);
			SendAcknowledgementReport(master, GetAcceptedResponseEmailAndSetOnMessage(cusresMessage, message));
		}
	}
}
