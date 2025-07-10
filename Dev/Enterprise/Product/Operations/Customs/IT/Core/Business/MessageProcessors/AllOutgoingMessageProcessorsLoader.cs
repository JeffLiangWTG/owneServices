using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.IT.Business;

public static class AllOutgoingMessageProcessorsLoader
{
	public static IEnumerable<OutgoingMessageProcessor> GetAll(LoggingInformation logger)
	{
		Argument.NotNull(logger, nameof(logger));

		return new OutgoingMessageProcessor[]
		{
			new SadOutgoingMessageProcessor(logger),
			new SingleWindowOutgoingMessageProcessor(logger),
			new XTradeOutgoingMessageProcessor(logger),
		};
	}
}
