using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRA101Processor : DeltaIEBaseProcessor<FRA101AType>
	{
		public FRA101Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(FRA101AType messageObject) => messageObject.ImportOrExportOperation?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ToNotifyAPaymentOrAnInsufficientCredit;

		protected override ZString GetNewEntryStatus() => ZString.Empty;

		protected override void SetEvent(CusEntryHeader entryHeader, FRA101AType messageObject)
		{
			var stateDateStr = messageObject.DeclarationStatus?.StateDateTime ?? ZString.Empty;
			var stateDate = ParseUtcDateTimeString(stateDateStr);
			ZString reference = "FRA101";
			entryHeader.Logs.AddNew(Events.CustomsUpdate, reference, stateDate.ToOffset());
		}

		protected override void UpdateFees(CusEntryHeader entryHeader, FRA101AType messageObject)
		{
		}
	}
}
