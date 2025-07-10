using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class EX583MessageProvider : ExitReportMessageProvider, IEX583Header, IEX583Declaration, IDocumentSendingMapper
	{
		public EX583MessageProvider(DocumentsSendingAction sendingAction)
			: base(sendingAction.MessagingObject)
		{
			this.sendingAction = sendingAction;
		}
		readonly DocumentsSendingAction sendingAction;

		#region IEX583Declaration

		public IEX583Declaration Declaration => this;
		public string LRN => sendingAction.LocalReference;
		public string MRN => sendingAction.MovementReference;

		#endregion

		readonly List<IAdditionalInformation> additionalInformationList = new List<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformationsCache;
		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformationsCache ?? (additionalInformationsCache = additionalInformationList.ToArray());

		readonly List<ISupportingDocumentWithImage> supportingDocumentList = new List<ISupportingDocumentWithImage>();
		IReadOnlyCollection<ISupportingDocumentWithImage> supportingDocumentsCache;
		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocumentsCache ?? (supportingDocumentsCache = supportingDocumentList.ToArray());

		#region IDocumentSendingMapper

		void IDocumentSendingMapper.AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject)
		{
			additionalInformationList.Add(new EX583AdditionalInformationProvider(sendingObject));
		}

		void IDocumentSendingMapper.AddSupportingDocument(DocumentSendingObject sendingObject)
		{
			supportingDocumentList.Add(new EX583SupportingDocumentProvider(sendingObject));
		}

		#endregion
	}
}
