using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Import;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business
{
	public class ECWINFMessageProcessorProvider : IDEBranchCustomsApplicationTypeMessageProcessorProvider
	{
		public ECWINFMessageProcessorProvider(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly LoggingInformation logger;

		public BranchCustomsApplicationTypeMessageProcessor GetProcessor(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result;

			var importProcessor = new ImportECWINFMessageProcessor(logger);

			if (message is AtlasInboundEDIMessage<IECWINF> ecwinfMessage && importProcessor.ShouldProcessMessage(ecwinfMessage))
			{
				result = importProcessor;
			}
			else
			{
				result = (BranchCustomsApplicationTypeMessageProcessor)Activator.CreateInstance(ObjectFactory.GetType("DENCTSECWINFMessageProcessor"), logger);
			}

			return result;
		}
	}
}
