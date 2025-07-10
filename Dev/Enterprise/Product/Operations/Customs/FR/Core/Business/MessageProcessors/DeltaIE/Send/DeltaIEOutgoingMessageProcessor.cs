using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public DeltaIEOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
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
							MessageTypeList.Codes.DEC
						});
				}
				return fMessageFilter;
			}
		}

		ZQuery fMessageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new DeltaIEOutgoingInterchangeProvider(readyMessages);
		}
	}
}
