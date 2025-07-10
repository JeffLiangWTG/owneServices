using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Messaging
{
	public class CustomsAndExciseReportMessageProcessor : OutgoingMessageProcessor
	{
		public CustomsAndExciseReportMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override bool IsBranchFilter => false;

		protected override ZQuery MessageFilter => messageFilter ??= new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IECustomsAndExcise);
		ZQuery messageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new CustomsAndExciseReportInterchangeProvider(readyMessages);
	}
}
