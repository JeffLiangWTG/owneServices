using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRECSOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public FRECSOutgoingMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override ZQuery MessageFilter => fMessageFilter ?? (fMessageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.FRCustomsMessage)
																							.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ECS)
																							.AddToFilter(EDIMessageSchema.EM_MessageSubType, new string[] { MessageSubTypeList.Codes.ARR, MessageSubTypeList.Codes.DEP }));
		ZQuery fMessageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new FRECSInterchangeProvider(readyMessages);
		}
	}
}
