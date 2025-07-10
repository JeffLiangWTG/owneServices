using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC414BCciOperationWrapper : ICC414BCciOperation
	{
		CC414BCciOperationWrapper(DeltaIEJobDeclarationMessageSendingObject messageObject)
		{
			this.messageObject = Argument.NotNull(messageObject, nameof(messageObject));
			this.entryHeader = Argument.NotNull(messageObject.Header, nameof(entryHeader));
		}
		readonly DeltaIEJobDeclarationMessageSendingObject messageObject;
		readonly CusEntryHeader entryHeader;

		public static CC414BCciOperationWrapper New(DeltaIEJobDeclarationMessageSendingObject messageObject) => messageObject == null ? null : new CC414BCciOperationWrapper(messageObject);

		public string CustomsRegistrationNumber => customsRegistrationNumber ?? (customsRegistrationNumber = entryHeader.CRN);
		string customsRegistrationNumber;

		public string InvalidationMotivation => invalidationMotivation ?? (invalidationMotivation = messageObject.ChangeAcknowledgementIndicator);
		string invalidationMotivation;

		public string InvalidationReason => invalidationReason ?? (invalidationReason = messageObject.VOCReason);
		string invalidationReason;

		public string InvalidationRequestDateAndTime => invalidationRequestDateAndTime ?? (invalidationRequestDateAndTime = ZDateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
		string invalidationRequestDateAndTime;

		public string LRN => lrn ?? (lrn = entryHeader.CorrelationID);
		string lrn;

		public string MRN => mrn ?? (mrn = entryHeader.MovementReferenceNumber);
		string mrn;

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
		ICollection<ISupportingDocument> supportingDocument;

		ICollection<ISupportingDocument> GetSupportingDocument()
		{
			var result = new Collection<ISupportingDocument>();

			entryHeader.SupportingDocuments.Cast<SupportingDocument>().ForEach(doc => result.Add(SupportingDocumentWrapper.New(doc, entryHeader.Declaration?.JE_CustomsOffice ?? string.Empty)));
			entryHeader.MergedLines.Cast<CusEntryLine>().ForEach(line => line.SupportingDocuments.ForEach(doc => result.Add(SupportingDocumentWrapper.New(doc, entryHeader.Declaration?.JE_CustomsOffice ?? string.Empty))));
			return result;
		}
	}
}
