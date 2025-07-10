using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM446MessageProvider : EntryHeaderMessageProvider, IIM446Header, IIM446ImportOperation, IUploadingDocumentMapper
	{
		public IM446MessageProvider(UploadDocumentsSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
		}

		readonly UploadDocumentsSendingAction sendingAction;
		public IIM446ImportOperation ImportOperation => this;

		public string CustomsOfficeLodgement => entryHeader.Declaration?.JE_CustomsOffice ?? null;

		public IReadOnlyCollection<IRequestedDocument> RequestedDocuments => requestedDocuments;
		readonly List<IRequestedDocument> requestedDocuments = new List<IRequestedDocument>();

		public IFallbackProcedure FallbackProcedure => CachedValueHelper.GetValue(ref fallbackProcedureCached, () => FallbackProcedureProvider.New(sendingAction));
		CachedValue<IFallbackProcedure> fallbackProcedureCached;

		public bool DocumentsAvailable => RequestedDocuments.Count > 0;

		public string MRN => entryHeader.MovementReferenceNumber;

		public void AddData(AdditionalInfoSendingObject additionalInfoSendingObject, IEnumerable<IeDoc> documents)
		{
			requestedDocuments.Add(new RequestedDocumentProvider(additionalInfoSendingObject, documents));
		}
	}
}
