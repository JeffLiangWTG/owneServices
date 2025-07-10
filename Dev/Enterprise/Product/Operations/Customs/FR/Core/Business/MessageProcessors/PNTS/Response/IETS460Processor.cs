using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS460;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS460Processor : PNTSBaseProcessor<Iets460>
	{
		public IETS460Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets460 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS460 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets460 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromCRN(factory, responseMessage.Crn, country)
				?? GetCusEntryNumberFromMRN(factory, responseMessage.Mrn, country);

			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided MRN# or CRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override ZString GetMessageSubType() => "460";
	}
}
