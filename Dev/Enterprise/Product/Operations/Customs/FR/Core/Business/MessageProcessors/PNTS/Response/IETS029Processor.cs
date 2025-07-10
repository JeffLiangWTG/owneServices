using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS029;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class IETS029Processor : PNTSBaseProcessor<Iets029>
	{
		public IETS029Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets029 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS029 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets029 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromFRN(factory, responseMessage.RelatedPn?.Frn ?? ZString.Empty, country) ?? GetCusEntryNumberFromCRN(factory, responseMessage.Crn, country);
			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn’t locate Job using provided FRN# or CRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override void UpdateReferenceNumber(TemporaryStorageHeader header, Iets029 responseMessage)
		{
			if (responseMessage.Mrn != null && header.MRN.IsEmpty)
			{
				header.MRN = responseMessage.Mrn;
			}

			if (responseMessage.RelatedPn?.Frn != null && header.FRN.IsEmpty)
			{
				header.FRN = responseMessage.RelatedPn.Frn;
			}
		}

		protected override ZString GetMessageSubType() => "029";
	}
}
