using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class TP5OutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public TP5OutgoingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ??= new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.FRCustomsMessage)
			.AddToFilter(EDIMessageSchema.EM_MessageType, new string[] { Business.MessageTypeList.Codes.TP5 });
		ZQuery messageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new TP5OutgoingInterchangeProvider(readyMessages);
	}
}
