using System.Threading;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public static class OutboundMessageProcessor
{
	public static void ProcessMessages(LoggingInformation logger, CancellationToken token)
	{
		new CHCOutboundMessageProcessor(logger).ProcessMessage(token);

		if (GlbCompanyWrapper.CurrentCompanyTokenCredentialsEnabled)
		{
			new CHPOutboundMessageProcessor(logger).ProcessMessage(token);
			new CHOOutboundMessageProcessor(logger).ProcessMessage(token);
		}
	}
}
