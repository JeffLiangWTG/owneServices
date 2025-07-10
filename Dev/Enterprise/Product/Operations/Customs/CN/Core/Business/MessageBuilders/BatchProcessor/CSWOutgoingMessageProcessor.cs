using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CSWOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public CSWOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		ZQuery GetMessageFilterQuery()
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CNCustomsSingleWindow);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			return query;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new CSWInterchangeProvider(readyMessages);
	}
}
