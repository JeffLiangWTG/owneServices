using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS410Processor : PNTSBaseProcessor<Iets410>
	{
		public IETS410Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets410 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS410 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets410 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromCRN(factory, responseMessage.Crn, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided CRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override ZString GetMessageSubType() => "410";
	}
}
