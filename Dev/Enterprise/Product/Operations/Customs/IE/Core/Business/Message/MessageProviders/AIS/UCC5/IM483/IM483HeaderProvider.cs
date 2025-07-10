using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM483HeaderProvider : EntryHeaderMessageProvider, IIM483Header, IDocumentSendingMapper
	{
		public IM483HeaderProvider(UploadDocumentsSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
		}

		public IIM483DeclarationType Declaration => CachedValueHelper.GetValue(ref declarationCached, () => new IM483DeclarationTypeProvider(entryHeader));
		CachedValue<IIM483DeclarationType> declarationCached;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations;

		readonly List<IAdditionalInformation> additionalInformations = new List<IAdditionalInformation>();
		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocuments;
		readonly List<ISupportingDocumentWithImage> supportingDocuments = new List<ISupportingDocumentWithImage>();

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
