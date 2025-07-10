using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM483HeaderProvider : IIM483Header, IIM483ImportOperation, IEDocsToAttach
	{
		public IM483HeaderProvider(UploadDocumentsSendingAction uploadDocumentsSendingAction)
		{
			SendingAction = Argument.NotNull(uploadDocumentsSendingAction, nameof(uploadDocumentsSendingAction));
			PreparationDateAndTime = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, removeMillisecond: true);
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

		public IIM483ImportOperation ImportOperation => this;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations;
		readonly List<IAdditionalInformation> additionalInformations = new List<IAdditionalInformation>();

		public IReadOnlyCollection<ISupportingDocumentWithImage> SupportingDocuments => supportingDocuments;
		readonly List<ISupportingDocumentWithImage> supportingDocuments = new List<ISupportingDocumentWithImage>();

		public IFallbackProcedure FallbackProcedure => null;

		public DateTime PreparationDateAndTime { get; private set; }

		#region IIM432Operation

		public string LRN => SendingAction.Bill.LocalReferenceNumber;

		public string MRN => SendingAction.Bill.MovementReferenceNumber;

		public UploadDocumentsSendingAction SendingAction { get; }

		#endregion

		#region IEDocsToAttach

		public IReadOnlyDictionary<ZGuid, string> EDocsToAttach => eDocsToAttach;
		readonly Dictionary<ZGuid, string> eDocsToAttach = new Dictionary<ZGuid, string>();

		#endregion
	}
}
