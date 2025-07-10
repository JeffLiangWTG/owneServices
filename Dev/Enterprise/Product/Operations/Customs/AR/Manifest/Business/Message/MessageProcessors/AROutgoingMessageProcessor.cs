using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AROutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public AROutgoingMessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		ZQuery messageFilter;
		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());

		ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.ARCustoms);
			result.OrderBy = EDIMessage.Schema.EM_MessageNum;
			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new ARInterchangeProvider(Logger, readyMessages);
	}
}
