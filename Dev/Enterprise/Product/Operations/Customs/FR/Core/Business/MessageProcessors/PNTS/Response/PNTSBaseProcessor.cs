using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class PNTSBaseProcessor<T> : ApplicationTypeMessageProcessor
		where T : class
	{
		public PNTSBaseProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override ZQuery MessageFilterCore
		{
			get
			{
				var messageFilter = base.MessageFilterCore;
				messageFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetMessageSubType());
				return messageFilter;
			}
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.STO };

		protected override string MessageFriendlyNameCore => (NoResString)"FR PNTS Base Processor";

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.FRCustomsMessage;

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			if (message is PNTSEDIMessage pNTSResponseMessage
				&& pNTSResponseMessage.MessageDataObject is PNTSMessageDataObject<T> messageDataObject
				&& messageDataObject.ResponseMessage is T responseMessage)
			{
				var (header, errorMessage) = GetLinkedTemporaryStorage(pNTSResponseMessage.Factory, responseMessage, pNTSResponseMessage.GetCountryCodeSafe());
				if (header != null)
				{
					pNTSResponseMessage.EM_Status = MessageStatusCodeList.Codes.OK;
					pNTSResponseMessage.EM_LinkedObject = header;
					UpdateMessageStatus(header);
					UpdateCustomsStatus(header, messageDataObject);
					UpdateCustomsStatusDate(header, responseMessage);
					UpdateReferenceNumber(header, responseMessage);
				}
				else
				{
					pNTSResponseMessage.EM_Status = MessageStatusCodeList.Codes.DCD;
					var note = pNTSResponseMessage.Notes.AddNew();
					note.ST_NoteText = errorMessage;
				}
			}
		}

		#region GetLinkedTemporaryStorage

		(TemporaryStorageHeader Header, ZString ErrorMessage) GetLinkedTemporaryStorage(BusinessObjectFactory factory, T responseMessage, ZString country)
		{
			var (entryNumber, errorMessage) = GetLinkedCusEntryNumber(factory, responseMessage, country);
			return ((TemporaryStorageHeader)entryNumber?.Parent, errorMessage);
		}

		protected abstract (CusEntryNumber EntryNumber, ZString ErrorMessage) GetLinkedCusEntryNumber(BusinessObjectFactory factory, T responseMessage, ZString country);

		protected CusEntryNumber GetCusEntryNumberFromCRN(BusinessObjectFactory factory, ZString crn, ZString country) => crn.IsEmpty ? null : CusEntryNumber.Load(factory, CusEntryNumberTypes.EU.CustomsRegistrationNumber, crn, country).FirstOrDefault();

		protected CusEntryNumber GetCusEntryNumberFromFRN(BusinessObjectFactory factory, ZString frn, ZString country) => frn.IsEmpty ? null : CusEntryNumber.Load(factory, CusEntryNumberTypes.EU.FunctionalReferenceNumber, frn, country).FirstOrDefault();

		protected CusEntryNumber GetCusEntryNumberFromLRN(BusinessObjectFactory factory, ZString lrn, ZString country) => lrn.IsEmpty ? null : CusEntryNumber.Load(factory, CusEntryNumberTypes.Standard.LocalReferenceNumber, lrn, country).FirstOrDefault();

		protected CusEntryNumber GetCusEntryNumberFromMRN(BusinessObjectFactory factory, ZString mrn, ZString country) => mrn.IsEmpty ? null : CusEntryNumber.Load(factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn, country).FirstOrDefault();

		protected CusEntryNumber GetCusEntryNumberFromCorrelationId(BusinessObjectFactory factory, ZString correlationID, ZString country) => correlationID.IsEmpty ? null : CusEntryNumber.Load(factory, CusEntryNumberTypes.EU.CorrelationIdentifier, correlationID, country).FirstOrDefault();

		#endregion

		#region UpdateMessageStatus

		void UpdateMessageStatus(TemporaryStorageHeader header)
		{
			var newMessageStatus = GetNewMessageStatus();
			if (!newMessageStatus.IsEmpty)
			{
				header.AMA_MessageStatus = newMessageStatus;
			}
		}

		protected virtual ZString GetNewMessageStatus() => ZString.Empty;

		#endregion

		#region UpdateCustomsStatus

		void UpdateCustomsStatus(TemporaryStorageHeader header, PNTSMessageDataObject<T> messageDataObject)
		{
			var customsStatus = messageDataObject.CustomsStatus;
			if (!customsStatus.IsEmpty)
			{
				header.CustomsStatus = customsStatus;
			}
		}

		#endregion

		#region UpdateCustomsStatusDate
		void UpdateCustomsStatusDate(TemporaryStorageHeader header, T messageObject)
		{
			var customsStatusDate = GetCustomsStatusDateFromMessage(messageObject);
			if (!customsStatusDate.IsEmpty)
			{
				header.CustomsStatusDate = customsStatusDate;
			}
		}

		protected virtual ZDateTime GetCustomsStatusDateFromMessage(T messageObject) => ZDateTime.Empty;
		#endregion

		#region UpdateReferenceNumber

		protected virtual void UpdateReferenceNumber(TemporaryStorageHeader header, T responseMessage)
		{
		}

		#endregion

		protected abstract ZString GetMessageSubType();
	}
}
