using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM483HeaderProvider : IIM483Header, IIM483DeclarationType, IEDocsToAttach
	{
		public IM483HeaderProvider(UploadDocumentsSendingAction uploadDocumentsSendingAction)
		{
			SendingAction = Argument.NotNull(uploadDocumentsSendingAction, nameof(uploadDocumentsSendingAction));
			InitializeCollections();
		}

		void InitializeCollections()
		{
			foreach (var addInfoObject in SendingAction.AddInfoCollection.Cast<AdditionalInfoSendingObject>())
			{
				additionalInformations.Add(new AdditionalInformationProvider(addInfoObject));

				foreach (var suppDocObj in addInfoObject.EDocsCollection.Cast<DocumentSendingObject>())
				{
					supportingDocuments.Add(new SupportingDocumentWithImageProvider(suppDocObj));
					if (suppDocObj.Document is IeDoc ieDoc && !eDocsToAttach.ContainsKey(ieDoc.UniqueKey))
					{
						eDocsToAttach.Add(ieDoc.UniqueKey, ieDoc.FileName);
					}
				}
			}
		}

		public IIM483DeclarationType Declaration => this;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations;
		readonly List<IAdditionalInformation> additionalInformations = new List<IAdditionalInformation>();

		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocuments;
		readonly List<ISupportingDocumentWithImage> supportingDocuments = new List<ISupportingDocumentWithImage>();

		public UploadDocumentsSendingAction SendingAction { get; }

		#region IIM483DeclarationType

		public string LRN => SendingAction.Bill.LocalReferenceNumber;

		public string MRN => SendingAction.Bill.MovementReferenceNumber;

		#endregion

		#region IEDocsToAttach

		public IReadOnlyDictionary<ZGuid, string> EDocsToAttach => eDocsToAttach;

		readonly Dictionary<ZGuid, string> eDocsToAttach = new Dictionary<ZGuid, string>();

		#endregion
	}
}
