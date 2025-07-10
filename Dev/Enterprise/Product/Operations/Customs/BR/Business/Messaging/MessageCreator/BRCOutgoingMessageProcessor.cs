using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class BRCOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public BRCOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new BRInterchangeProvider(readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get { return messageFilter ?? (messageFilter = GetMessageFilterQuery()); }
		}
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
			result.OrderBy = EDIMessage.Schema.EM_MessageNum;
			return result;
		}
	}
}
