using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRA103Processor : DeltaIEBaseProcessor<FRA103AType>
	{
		public FRA103Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetLRNFromResponseMessage(FRA103AType messageObject) => messageObject.Operation?.FirstOrDefault()?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ValidationRequestExtension;

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetNewEntryStatus() => ZString.Empty;

		protected override void SetEvent(CusEntryHeader entryHeader, FRA103AType messageObject)
		{
			var newTimerRequestInstructionExpiryDateStr = messageObject.ExtendedTimerForRequestInstruction?.NewTimerRequestInstructionExpiryDate ?? ZString.Empty;
			var registrationDate = ParseUtcDateTimeString(newTimerRequestInstructionExpiryDateStr);
			entryHeader.Logs.AddNew(AutoEvents.CustomsUpdate, "FRA103", registrationDate.ToOffset());
		}

		protected override void UpdateFees(CusEntryHeader entryHeader, FRA103AType messageObject)
		{
		}
	}
}
