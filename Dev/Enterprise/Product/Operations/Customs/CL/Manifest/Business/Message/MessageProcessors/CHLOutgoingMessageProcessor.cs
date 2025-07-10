using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CHLOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public CHLOutgoingMessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		ZQuery messageFilter;
		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());

		ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CLCustoms);
			result.OrderBy = EDIMessage.Schema.EM_MessageNum;
			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new CLInterchangeProvider(Logger, readyMessages);
	}
}
