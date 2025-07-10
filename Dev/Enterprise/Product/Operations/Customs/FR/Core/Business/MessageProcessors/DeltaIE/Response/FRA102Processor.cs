using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRA102Processor : DeltaIEBaseProcessor<FRA102AType>
	{
		public FRA102Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(FRA102AType messageObject) => messageObject.Operation?.FirstOrDefault()?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.RegistrationOfTheInvalidationOrAmendment;

		protected override ZString GetNewEntryStatus() => ZString.Empty;

		protected override void SetEvent(CusEntryHeader entryHeader, FRA102AType messageObject)
		{
			var request = messageObject.Request;
			var registrationDateStr = request?.RequestRegistrationDateTime ?? ZString.Empty;
			var registrationDate = ParseUtcDateTimeString(registrationDateStr);
			ZString reference = "FRA102";
			entryHeader.Logs.AddNew(Events.CustomsUpdate, reference, registrationDate.ToOffset());
		}

		protected override void UpdateFees(CusEntryHeader entryHeader, FRA102AType messageObject)
		{
		}
	}
}
