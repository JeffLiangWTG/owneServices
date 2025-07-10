using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.ServiceTasks
{
	public class DEAOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public DEAOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		ZQuery messageFilter;
		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());

		ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.DECustomsAtlasSystem);

			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new DEOutboundInterchangeProvider(readyMessages);
	}
}
