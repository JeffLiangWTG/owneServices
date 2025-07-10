using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS030;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class IETS030Processor : PNTSBaseProcessor<Iets030>
	{
		public IETS030Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets030 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS030 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets030 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromFRN(factory, responseMessage.Frn ?? ZString.Empty, country) ?? GetCusEntryNumberFromCRN(factory, responseMessage.RelatedTsd?.Crn ?? ZString.Empty, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn’t locate Job using provided FRN# or CRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override void UpdateReferenceNumber(TemporaryStorageHeader header, Iets030 responseMessage)
		{
			if (responseMessage.RelatedTsd?.Mrn != null && header.MRN.IsEmpty)
			{
				header.MRN = responseMessage.RelatedTsd.Mrn;
			}

			if (responseMessage.Frn != null && header.FRN.IsEmpty)
			{
				header.FRN = responseMessage.Frn;
			}
		}

		protected override ZString GetMessageSubType() => "030";
	}
}
