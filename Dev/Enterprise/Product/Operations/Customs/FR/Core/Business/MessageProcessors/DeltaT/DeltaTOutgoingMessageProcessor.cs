using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaTOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public DeltaTOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery MessageFilter
		{
			get
			{
				if (fMessageFilter == null)
				{
					fMessageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.FRCustomsMessage)
						.AddToFilter(EDIMessageSchema.EM_MessageType, new string[]
						{
							MessageTypeList.Codes.DT007,
							MessageTypeList.Codes.DT013,
							MessageTypeList.Codes.DT014,
							MessageTypeList.Codes.DT015,
							MessageTypeList.Codes.DT044,
							MessageTypeList.Codes.DT141,
							MessageTypeList.Codes.DTF15,
						})
						.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageSubTypeList.Codes.DT);
				}
				return fMessageFilter;
			}
		}
		ZQuery fMessageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new DeltaTInterchangeProvider(readyMessages);
		}
	}
}
