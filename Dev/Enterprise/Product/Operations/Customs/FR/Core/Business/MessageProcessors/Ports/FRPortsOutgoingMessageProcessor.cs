using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRPortsOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public FRPortsOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery MessageFilter => fMessageFilter ?? (fMessageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, FREDIMessage.ApplicationCodes.FRPortMessage));
		ZQuery fMessageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new FRPortsInterchangeProvider(readyMessages);
		}
	}
}
