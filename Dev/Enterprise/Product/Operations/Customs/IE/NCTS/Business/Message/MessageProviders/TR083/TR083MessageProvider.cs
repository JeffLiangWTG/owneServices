using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR083MessageProvider : NctsDepartureHeaderMessageProvider, ITR083Header, ITR083Declaration, IDocumentSendingMapper
	{
		public TR083MessageProvider(DocumentSendingAction sendingAction) : base(sendingAction.nctsHeader)
		{
		}

		public ITR083Declaration Declaration => this;

		public string MRN => NctsHeader.MovementReferenceNumber;

		public string LRN => NctsHeader.MovementHeader?.BM_PaperlessInbondNum ?? string.Empty;

		readonly List<IAdditionalInformation> additionalInformationList = new List<IAdditionalInformation>();
		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformationsCache ?? (additionalInformationsCache = additionalInformationList.ToArray());
		IReadOnlyCollection<IAdditionalInformation> additionalInformationsCache;
		readonly List<ISupportingDocumentWithImage> supportingDocumentList = new List<ISupportingDocumentWithImage>();
		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocumentsCache ?? (supportingDocumentsCache = supportingDocumentList.ToArray());

		public DateTime RequestDate => ZDateTime.Today.ToDateTime();

		IReadOnlyCollection<ISupportingDocumentWithImage> supportingDocumentsCache;

		void IDocumentSendingMapper.AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject)
		{
			additionalInformationList.Add(new AdditionalInfoSendingObjectProvider(sendingObject));
		}

		void IDocumentSendingMapper.AddSupportingDocument(DocumentSendingObject sendingObject)
		{
			supportingDocumentList.Add(new TR083SupportingDocumentProvider(sendingObject));
		}
	}
}
