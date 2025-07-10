using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED801MessageProcessor : IDEBranchCustomsApplicationTypeMessageProcessorProvider
	{
		public ED801MessageProcessor(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly LoggingInformation logger;

		public BranchCustomsApplicationTypeMessageProcessor GetProcessor(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result = null;
			if (message.EM_MessageSubType == Messaging.EmcsMessageSubTypeList.Codes.Eme)
			{
				result = new ED801ConsignorMessageProcessor(logger);
			}
			else if (message.EM_MessageSubType == Messaging.EmcsMessageSubTypeList.Codes.Emb)
			{
				result = new ED801ConsigneeMessageProcessor(logger);
			}
			return result;
		}
	}
}
