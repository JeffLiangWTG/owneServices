using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS095Processor : PNTSBaseProcessor<Iets095>
	{
		public IETS095Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets095 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS095 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets095 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromMRN(factory, responseMessage.Mrn, country)
				?? GetCusEntryNumberFromLRN(factory, responseMessage.Lrn, country);

			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided MRN# or LRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override void UpdateReferenceNumber(TemporaryStorageHeader header, Iets095 responseMessage)
		{
			if (responseMessage.Mrn != null && header.MRN.IsEmpty)
			{
				header.MRN = responseMessage.Mrn;
			}

			if (responseMessage.Crn != null && header.CRN.IsEmpty)
			{
				header.CRN = responseMessage.Crn;
			}
		}

		protected override ZString GetMessageSubType() => "095";
	}
}
