using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business
{
	public class NFFTAXMessageProcessorProvider : IDEBranchCustomsApplicationTypeMessageProcessorProvider
	{
		public NFFTAXMessageProcessorProvider(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly LoggingInformation logger;

		public BranchCustomsApplicationTypeMessageProcessor GetProcessor(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result = null;

			var factory = message.Factory;
			var messageSubType = message.EM_MessageSubType;
			if (factory.GetCachedValue<MonthlyClosingMessageSubTypeList>().ContainsCode(messageSubType))
			{
				result = new MonthlyClosingNFFTAXMessageProcessor(logger);
			}
			else if (factory.GetCachedValue<ImportMessageSubTypeList>().ContainsCode(messageSubType))
			{
				result = new ImportNFFTAXMessageProcessor(logger);
			}

			return result;
		}
	}
}
