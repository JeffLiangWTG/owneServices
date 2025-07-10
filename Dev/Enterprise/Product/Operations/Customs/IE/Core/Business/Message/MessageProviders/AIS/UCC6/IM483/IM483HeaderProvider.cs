using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM483HeaderProvider : EntryHeaderMessageProvider, IIM483Header, IIM483ImportOperation, IDocumentSendingMapper
	{
		readonly UploadDocumentsSendingAction sendingAction;
		public IM483HeaderProvider(UploadDocumentsSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			this.sendingAction = sendingAction;
		}

		public IIM483ImportOperation ImportOperation => this;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations;

		readonly List<IAdditionalInformation> additionalInformations = new List<IAdditionalInformation>();
		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocuments;
		readonly List<ISupportingDocumentWithImage> supportingDocuments = new List<ISupportingDocumentWithImage>();

		public IFallbackProcedure FallbackProcedure => fallbackProcedureCached ?? (fallbackProcedureCached = FallbackProcedureProvider.New(sendingAction));
		IFallbackProcedure fallbackProcedureCached;

		#region IIM483ImportOperation

		public string LRN => entryHeader.CH_BGMReference;

		public string MRN => entryHeader.MovementReferenceNumber;

		#endregion

		#region IDocumentSendingMapper

		public void AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject)
		{
			additionalInformations.Add(new AdditionalInfoSendingObjectProvider(sendingObject));
		}

		public void AddSupportingDocument(DocumentSendingObject sendingObject)
		{
			supportingDocuments.Add(new SupportingDocumentWithImageProvider(sendingObject));
		}

		#endregion
	}
}
