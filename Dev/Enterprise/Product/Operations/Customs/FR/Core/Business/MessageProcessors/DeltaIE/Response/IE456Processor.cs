using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE456Processor : DeltaIEBaseProcessor<CC456BType>
	{
		public IE456Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override ZString GetLRNFromResponseMessage(CC456BType messageObject) => messageObject.ImportOperation.FirstOrDefault()?.LRN;

		protected override ZString GetNewCRN(CC456BType messageObject) => messageObject.ImportOperation.FirstOrDefault()?.CustomsRegistrationNumber;

		protected override ZString GetNewMRN(CC456BType messageObject) => messageObject.ImportOperation.FirstOrDefault()?.MRN;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.FunctionalRejection;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationRejected;

		protected override ZString GetEntryStatusChangedTimeString(CC456BType messageObject) => messageObject.DeclarationStatus?.StateDateTime;
	}
}
