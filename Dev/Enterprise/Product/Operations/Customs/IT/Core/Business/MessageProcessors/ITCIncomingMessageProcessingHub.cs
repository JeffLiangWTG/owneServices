using System.Collections.Generic;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IT.Business;

public class ITCIncomingMessageProcessingHub : BaseMessageProcessor
{
	public static void ProcessMessages(LoggingInformation logger, CancellationToken token)
	{
		using (var processor = new ITCIncomingMessageProcessingHub(logger))
		{
			processor.ExecuteBatch(token);
		}
	}

	protected ITCIncomingMessageProcessingHub(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool MessageShouldBeProcessedInASeparateFactory => true;

	protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
	{
		var result = base.GetMessageProcessors();

		result.Add(new SadIrispMessageProcessor(Logger));
		result.Add(new SadIvistoMessageProcessor(Logger));
		result.Add(new SingleWindowAcknowledgementResponseMessageProcessor(Logger));
		result.Add(new SingleWindowXmlResponseMessageProcessor(Logger));
		result.Add(new SingleWindowPdfResponseMessageProcessor(Logger));
		result.Add(new AcknowledgementResponseMessageProcessor(Logger));
		result.Add(new ResponseMessageProcessor(Logger));
		result.Add(new ElectronicFolderResponseMessageProcessor(Logger));
		result.Add(new XTradeErrorResponseMessageProcessor(Logger));
		result.Add(new IvistoResponseMessageProcessor(Logger));
		result.Add(new CustomsXTradeErrorResponseMessageProcessor(Logger));
		result.Add(new IrildesResponseMessageProcessor(Logger));
		result.Add(new EadTadResponseMessageProcessor(Logger));
		result.Add(new Eur1ResponseMessageProcessor(Logger));
		result.Add(new ReleaseProspectusResponseMessageProcessor(Logger));
		result.Add(new AccountingSummaryDownloadMessageProcessor(Logger));
		result.Add(new AccountingSummaryResponseMessageProcessor(Logger));
		result.Add(new XTradeSignatureErrorResponseMessageProcessor(Logger));
		result.Add(new XtradeSignatureResponseMessageProcessor(Logger));
		result.Add(new SummaryProspectusResponseMessageProcessor(Logger));
		result.Add(new SummaryProspectusDownloadMessageProcessor(Logger));
		return result;
	}
}
