using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsMessageProcessorsProvider : ICHNctsMessageProcessorsProvider
{
	public IEnumerable<ApplicationTypeMessageProcessor> GetMessageProcessors(LoggingInformation logger)
	{
		yield return new PassarNctsMessageProcessor(logger);
		yield return new NC124ResponseMessageProcessor(logger);
		yield return new NC909ResponseMessageProcessor(logger);
		yield return new NT008ResponseMessageProcessor(logger);
		yield return new NT009ResponseMessageProcessor(logger);
		yield return new NT019ResponseMessageProcessor(logger);
		yield return new NT021ResponseMessageProcessor(logger);
		yield return new NT025ResponseMessageProcessor(logger);
		yield return new NT029ResponseMessageProcessor(logger);
		yield return new NT035ResponseMessageProcessor(logger);
		yield return new NT043ResponseMessageProcessor(logger);
		yield return new NT045ResponseMessageProcessor(logger);
		yield return new NT055ResponseMessageProcessor(logger);
		yield return new NT057ResponseMessageProcessor(logger);
		yield return new NT060ResponseMessageProcessor(logger);
		yield return new NT061ResponseMessageProcessor(logger);
		yield return new NT140ResponseMessageProcessor(logger);
		yield return new NT146ResponseMessageProcessor(logger);
		yield return new NT182ResponseMessageProcessor(logger);
		yield return new NTx04ResponseMessageProcessor(logger);
		yield return new NTx28ResponseMessageProcessor(logger);
	}
}
