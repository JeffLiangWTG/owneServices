using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business
{
	public class AesERRNCKMessageProcessorProvider : IDEBranchCustomsApplicationTypeMessageProcessorProvider
	{
		public AesERRNCKMessageProcessorProvider(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly LoggingInformation logger;

		public BranchCustomsApplicationTypeMessageProcessor GetProcessor(EDIMessage message)
		{
			switch (message.EM_MessageSubType)
			{
				case ExportMessageSubTypeList.Codes.EXQ:
					return new AesStatusRequestERRNCKMessageProcessor(logger);
				case ExportMessageSubTypeList.Codes.EXP:
					return new ExportERRNCKMessageProcessor(logger);
				default:
					return null;
			}
		}
	}
}
