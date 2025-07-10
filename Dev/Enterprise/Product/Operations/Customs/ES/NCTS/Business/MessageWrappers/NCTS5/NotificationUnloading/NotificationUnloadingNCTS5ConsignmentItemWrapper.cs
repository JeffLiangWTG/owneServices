using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5ConsignmentItemWrapper : NCTS5CommonConsignmentItemWrapper, INotifUnloadingNCTSConsignmentItem
	{
		public NotificationUnloadingNCTS5ConsignmentItemWrapper(NctsArrivalCargoDesc item) : base(item)
		{
			itemArrival = item;
			isUnloadedStateNEWorDIF = itemArrival.BY_UnloadedState.IsUnloadingStateNEWorDIF();
		}
		readonly NctsArrivalCargoDesc itemArrival;
		readonly ZBool isUnloadedStateNEWorDIF;

		protected override IReadOnlyCollection<NCTS5CommonPackagingWrapper> GetPackagingCore() => isUnloadedStateNEWorDIF
																										? NCTS5CommonPackagingWrapper.GetPackagesListArrival(itemArrival)
																										: base.GetPackagingCore();

		public INotifUnloadingCommodity Commodity => commodity ?? (commodity = IsUnloadingStateNEWOrCommodityHasDifferences ? new NotificationUnloadingNCTS5CommodityWrapper(itemArrival) : null);
		NotificationUnloadingNCTS5CommodityWrapper commodity;

		bool IsUnloadingStateNEWOrCommodityHasDifferences => itemArrival.BY_UnloadedState.IsUnloadingStateNEW()
			|| itemArrival.HasNctsArrivalCommodityDifferences();

		public IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					if (isUnloadedStateNEWorDIF)
					{
						var addDocs = itemArrival.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF()).ToList();
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
					else
					{
						supportingDocument = new List<NCTS5CommonDocumentWithInfoWrapper>().AsReadOnly();
					}
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonDocumentWithInfoWrapper> supportingDocument;

		protected override List<CusSupportingInfo> GetAdditionalInfos(ZString subType)
		{
			if (isUnloadedStateNEWorDIF)
			{
				return itemArrival.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF() && doc.CSI_SubType == subType).ToList();
			}
			else
			{
				return new List<CusSupportingInfo>();
			}
		}

		protected override IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(List<CusSupportingInfo> documents)
		{
			var docs = new List<CommonDocumentSequenceNumberWrapper>();
			var orderedDocs = documents.OrderBy(doc => doc.CSI_LineNo);
			foreach (var doc in orderedDocs)
			{
				docs.Add(doc.CSI_Status.IsUnloadingStateNEWorDIF()
												? new CommonDocumentSequenceNumberWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber, doc.CSI_LineNo)
												: new CommonDocumentSequenceNumberWrapper(ZString.Empty, ZString.Empty, doc.CSI_LineNo));
			}
			return docs.AsReadOnly();
		}

		protected override bool ShouldSendTransportDocuments => true;
	}
}
