using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IN.Business;

public static class MessageProcessorFactory
{
	public static BaseMessageProcessor GetMessageProcessor(EDIMessage message, LoggingInformation logger)
	{
		return message.EM_MessageType.ToString() switch
		{
			Constants.MessageType.XtErrorResponse => new XtErrorMessageProcessor(message, logger),
			_ => new EmailMessageProcessor(message, logger)
		};
	}
}
