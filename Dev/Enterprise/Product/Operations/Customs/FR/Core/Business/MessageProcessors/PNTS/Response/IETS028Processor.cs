using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IETS028Processor : PNTSBaseProcessor<Iets028>
	{
		public IETS028Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetNewMessageStatus() => PNTSMessageStatusList.Codes.Acknowledged;

		protected override ZDateTime GetCustomsStatusDateFromMessage(Iets028 messageObject) => messageObject.NotificationDate;

		protected override string MessageFriendlyNameCore => (NoResString)"IETS028 Processor";

		protected override (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, Iets028 responseMessage, ZString country)
		{
			var entryNumber = GetCusEntryNumberFromLRN(factory, responseMessage.Lrn, country);
			if (entryNumber == null)
			{
				entryNumber = GetCusEntryNumberFromCRN(factory, responseMessage.Crn, country);
				if (entryNumber == null)
				{
					entryNumber = GetCusEntryNumberFromMRN(factory, responseMessage.Mrn, country);
				}
			}

			var errorMessage = entryNumber == null ? new ZString((NoResString)"Couldn't locate Job using provided LRN# or CRN# or MRN#") : ZString.Empty;
			return (entryNumber, errorMessage);
		}

		protected override void UpdateReferenceNumber(TemporaryStorageHeader header, Iets028 responseMessage)
		{
			if (responseMessage.Mrn != null && header.MRN.IsEmpty)
			{
				header.MRN = responseMessage.Mrn;
			}
			else if (responseMessage.Crn != null && header.CRN.IsEmpty)
			{
				header.CRN = responseMessage.Crn;
				header.PreLodgedDate = ZDateTime.Today;
			}
			else if (responseMessage.Frn != null && header.FRN.IsEmpty)
			{
				header.FRN = responseMessage.Frn;
			}
		}

		protected override ZString GetMessageSubType() => "028";
	}
}
