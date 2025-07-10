using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE460;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE460Processor : DeltaIEBaseProcessor<CC460BType>
	{
		public IE460Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetLRNFromResponseMessage(CC460BType messageObject) => messageObject.ImportOperation?.LRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.UnderControlNotification;

		protected override ZString GetNewCRN(CC460BType messageObject) => messageObject.ImportOperation?.CustomsRegistrationNumber;

		protected override ZString GetNewMRN(CC460BType messageObject) => messageObject.ImportOperation?.MRN;

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.UnderControl;

		protected override ZString GetEntryStatusChangedTimeString(CC460BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;
	}
}
