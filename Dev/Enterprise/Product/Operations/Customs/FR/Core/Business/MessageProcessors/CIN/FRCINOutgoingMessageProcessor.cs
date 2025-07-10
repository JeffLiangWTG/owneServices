using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRCINOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public FRCINOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter => fMessageFilter ?? (fMessageFilter = GetNewMessageFilter());
		ZQuery fMessageFilter;

		ZQuery GetNewMessageFilter()
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.FRCustomsMessage);

			var subQuery = new ZQuery(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CIN)
									.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CIN745)
									.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CIN755);

			query.AddToFilter(subQuery, JoinCondition.And);

			return query;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new FRCINInterchangeProvider(readyMessages);
		}
	}
}
