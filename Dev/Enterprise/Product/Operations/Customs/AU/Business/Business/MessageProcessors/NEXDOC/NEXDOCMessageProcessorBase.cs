using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business;

public abstract class NEXDOCMessageProcessor : CustomsMessageProcessor
{
	protected NEXDOCMessageProcessor(LoggingInformation logger)
		: base(logger, EDIMessage.ApplicationCodes.NEXDOCS, "Request for Permit")
	{
	}

	public abstract bool HandlesDocument(string documentType);
}
