using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE917Processor : DeltaIEBaseProcessor<CC917BType>
	{
		public IE917Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetLRNFromResponseMessage(CC917BType messageObject) => messageObject.Operation?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.TechnicalRejection;

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override ZString GetNewEntryStatus() => ZString.Empty;

		protected override void UpdateFees(CusEntryHeader entryHeader, CC917BType messageObject)
		{
		}
	}
}
