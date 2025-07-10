using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts
{
	public class NctsPhase5OutgoingMessageProcessor : CTCOutgoingMessageProcessor
	{
		public NctsPhase5OutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsNCTS);
			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new CTCGB5InterchangeProvider(Logger, readyMessages);
		}
	}
}
