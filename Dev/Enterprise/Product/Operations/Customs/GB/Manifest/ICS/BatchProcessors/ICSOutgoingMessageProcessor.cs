using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public ICSOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbMessageICSGreatBritain);
			result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland);
			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new ICSInterchangeProvider(Logger, readyMessages);
		}
	}
}
