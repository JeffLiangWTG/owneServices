using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS016Processor : PNTSBaseProcessor<Iets016>
	{
		public IETS016Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.FunctionalRejection;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS016 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets016 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromLRN(factory, responseMessage.Lrn, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided LRN #") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override ZString GetMessageSubType() => "016";
	}
}
