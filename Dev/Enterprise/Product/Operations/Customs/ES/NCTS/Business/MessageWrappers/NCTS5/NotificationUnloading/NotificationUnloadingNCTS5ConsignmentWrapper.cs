using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5ConsignmentWrapper : NCTS5CommonConsignmentWrapper, INotifUnloadingConsignment
	{
		public NotificationUnloadingNCTS5ConsignmentWrapper(NctsHeader header) : base(header, isArrivalDeclaration: true)
		{
		}

		public ZDecimal GrossMass => arrivalMovement.BM_GrossWeightUnloaded;

		public ZBool GrossMassSpecified => arrivalMovement.BM_GrossWeightUnloaded != arrivalMovement.TotalGrossMassInKilograms;

		public IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					var addDocs = arrivalMovement.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF()).ToList();
					var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

					var docList = new List<NCTS5CommonDocumentWithInfoWrapper>();
					foreach (var document in orderedDocs)
					{
						docList.Add(document.CSI_Status.IsUnloadingStateNEWorDIF()
												? new NCTS5CommonDocumentWithInfoWrapper(document, document.CSI_LineNo)
												: new NCTS5CommonDocumentWithInfoWrapper(ZString.Empty, ZString.Empty, ZString.Empty, document.CSI_LineNo));
					}

					supportingDocument = docList.AsReadOnly();
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonDocumentWithInfoWrapper> supportingDocument;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument
		{
			get
			{
				if (transportDocument == null)
				{
					transportDocument = GetAdditionalDocumentsWrapperListForSubType(AdditionalInfoSubTypeList.Codes.TransportDocument);
				}
				return transportDocument;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> transportDocument;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference
		{
			get
			{
				if (additionalReference == null)
				{
					additionalReference = GetAdditionalDocumentsWrapperListForSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				}
				return additionalReference;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalReference;

		public IReadOnlyCollection<INotifUnloadingNCTSHouseConsignment> HouseConsignment =>
			houseConsignment ?? (houseConsignment = nctsHeader.Bills.Cast<NctsBill>()
																	.Where(x => x.MovementDetail.B9_UnloadedState.IsUnloadingStateMISorNEW() || x.HasNctsBillDifferences())
																	.Select(x => new NotificationUnloadingNCTS5HouseConsignmentWrapper(x))
																	.ToList().AsReadOnly());
		IReadOnlyCollection<NotificationUnloadingNCTS5HouseConsignmentWrapper> houseConsignment;

		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetAdditionalDocumentsWrapperListForSubType(ZString subType)
		{
			var docs = new List<CommonDocumentSequenceNumberWrapper>();
			var addDocs = arrivalMovement.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF() && doc.CSI_SubType == subType).ToList();
			var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

			foreach (var doc in orderedDocs)
			{
				docs.Add(doc.CSI_Status.IsUnloadingStateNEWorDIF()
												? new CommonDocumentSequenceNumberWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber, doc.CSI_LineNo)
												: new CommonDocumentSequenceNumberWrapper(ZString.Empty, ZString.Empty, doc.CSI_LineNo));
			}
			return docs.AsReadOnly();
		}
	}
}
