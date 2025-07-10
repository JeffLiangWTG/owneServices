using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE431Processor : DeltaIEBaseProcessor<CC431BType>
	{
		public IE431Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(CC431BType messageObject) => messageObject.ImportOperation?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.TimerExpirySupplementaryDeclaration;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.TimerExpired;
	}
}
