using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5ConsignmentItemWrapper : NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper, ITNNNCTSConsignmentItem
	{
		public TNNNCTS5ConsignmentItemWrapper(NctsDepartureCargoDesc item, ZBool shouldDeclareDeclarationTypeInItem, ZBool shouldDeclareCountryOfDestinationInItem, ZBool shouldDeclareReferenceNumberUCRInItem) : base(item, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem)
		{
		}

		public ITNNNCTSCommodity Commodity => commodity ?? (commodity = new TNNNCTS5CommodityWrapper(itemDeparture));
		TNNNCTS5CommodityWrapper commodity;

		public IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					var addDocs = item.SupportingDocuments.Cast<CusSupportingInfo>().ToList();
					if (item.IsInPhase5TransitionPeriod)
					{
						addDocs.AddRange(item.Bill.SupportingDocuments.Cast<CusSupportingInfo>().ToList());
						addDocs.AddRange(item.Header.MovementHeader.SupportingDocuments.Cast<CusSupportingInfo>().ToList());
					}
					var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

					var docList = new List<NCTS5CommonDocumentWithInfoWrapper>();
					foreach (var document in orderedDocs)
					{
						docList.Add(new NCTS5CommonDocumentWithInfoWrapper(document, document.CSI_LineNo));
					}

					supportingDocument = docList.AsReadOnly();
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonDocumentWithInfoWrapper> supportingDocument;
	}
}
