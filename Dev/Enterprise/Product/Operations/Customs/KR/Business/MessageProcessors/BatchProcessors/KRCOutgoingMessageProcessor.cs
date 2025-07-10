using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KRCOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public KRCOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms));
		ZQuery messageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new KRCInterchangeProvider(readyMessages);
	}
}
