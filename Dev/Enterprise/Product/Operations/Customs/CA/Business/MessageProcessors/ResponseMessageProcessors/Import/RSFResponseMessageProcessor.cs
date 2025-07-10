using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class RSFResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public RSFResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new RSFStatusCalculator(), MessageTypeList.Codes.CSARevenueSummaryForm, MessageTypeList.Descriptions.CSARevenueSummaryForm)
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage message)
		{
			var rsfMessage = message as RSFMessage;
			var cusresMessage = rsfMessage?.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet()) as CUSRESMessage;
			var statementNumber = cusresMessage?.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
			CusStatementHeader header = null;
			if (rsfMessage != null && cusresMessage != null && statementNumber != null)
			{
				header = new CusStatementHeaderLoader(message.Factory).Load(CusStatementHeaderTypes.Codes.RSF, statementNumber);
				linkedObjectReference = header?.B2_StatementNumber ?? ZString.Empty;
			}
			else
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			var resultStatus = ZString.Empty;
			if (header != null && cusresMessage != null && cusresMessage.GIS.Count > 0)
			{
				rsfMessage.EM_LinkedObject = header;
				if (cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode
					== ProcessingIndicatorDescriptionCodeList.ErrorMessage)
				{
					header.B2_Status = RSFStatusCalculator.CalculateRSFStatus(header.B2_Status, ProcessingIndicatorDescriptionCodeList.ErrorMessage);
					SendErrorReport(header, GetErrorResponseEmailAndSetOnMessage(cusresMessage, rsfMessage));
					resultStatus = EDIMessage.Status.Received;
					message.EM_Status = resultStatus;
				}
				else if (cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode
					== ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
				{
					header.B2_Status = RSFStatusCalculator.CalculateRSFStatus(header.B2_Status, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted);
					SendAcknowledgementReport(header, GetAcceptedResponseEmailAndSetOnMessage(cusresMessage, rsfMessage));
					resultStatus = EDIMessage.Status.Received;
					message.EM_Status = resultStatus;
					foreach (DTMSegment dtm in cusresMessage.DTM)
					{
						if (dtm.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == "9")
						{
							ZDateTime dateTime;
							if (ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriodValue, out dateTime, "yyyyMMddHHmm"))
							{
								header.B2_ProcessDate = dateTime;
							}
						}
					}
				}
				else
				{
					throw new UnableToInterpretMessageException(message, this);
				}
			}
			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(message, this);
			}
			return resultStatus;
		}
	}
}
