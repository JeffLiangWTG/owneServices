using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5ConsignmentItemDepartureAndAmendmentWrapper : NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper, INCTSConsignmentItemDepartureAndAmendment
	{
		public NCTS5ConsignmentItemDepartureAndAmendmentWrapper(NctsDepartureCargoDesc item, ZBool shouldDeclareDeclarationTypeInItem, ZBool shouldDeclareCountryOfDispatchInItem, ZBool shouldDeclareCountryOfDestinationInItem, ZBool shouldDeclareReferenceNumberUCRInItem, ZBool shouldDeclareConsigneeInItem) : base(item, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem)
		{
			this.shouldDeclareCountryOfDispatchInItem = shouldDeclareCountryOfDispatchInItem;
			this.shouldDeclareConsigneeInItem = shouldDeclareConsigneeInItem;
		}
		readonly ZBool shouldDeclareCountryOfDispatchInItem;
		readonly ZBool shouldDeclareConsigneeInItem;

		public ZString CountryOfDispatch => shouldDeclareCountryOfDispatchInItem ? GetCountryOfDispatchFromGoodsItemOrParents() : ZString.Empty;
		ZString GetCountryOfDispatchFromGoodsItemOrParents() => !item.BY_RN_NKCountryOfDispatch.IsEmpty
																	? item.BY_RN_NKCountryOfDispatch
																	: !item.Bill.B0_RN_NKCountryOfExport.IsEmpty
																			? item.Bill.B0_RN_NKCountryOfExport
																			: item.Header.MovementHeader.BM_RN_NKCountryOfDispatch;

		public INCTSPartyNameProviderWithAddress Consignee => consignee ?? (consignee = item.IsInPhase5TransitionPeriod && shouldDeclareConsigneeInItem ? NCTS5PartyNameProviderWithAddressWrapper.New(GetConsigneeFromGoodsItemOrParents(), true, true) : null);
		NCTS5PartyNameProviderWithAddressWrapper consignee;
		JobDocAddress GetConsigneeFromGoodsItemOrParents() => !itemDeparture.Consignee.IsEmpty
																	? itemDeparture.Consignee
																	: !item.Bill.Consignee.IsEmpty
																			? item.Bill.Consignee
																			: item.Header.Consignee;

		public IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor
		{
			get
			{
				if (additionalSupplyChainActor == null)
				{
					var additionalSupplyChainActorList = new List<CommonAdditionalSupplyChainActorSeqNumWrapper>();

					ZShort seqNum = 1;
					foreach (var chainActor in itemDeparture.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>())
					{
						additionalSupplyChainActorList.Add(new CommonAdditionalSupplyChainActorSeqNumWrapper(chainActor, seqNum));
						seqNum++;
					}
					additionalSupplyChainActor = additionalSupplyChainActorList.AsReadOnly();
				}
				return additionalSupplyChainActor;
			}
		}
		IReadOnlyCollection<CommonAdditionalSupplyChainActorSeqNumWrapper> additionalSupplyChainActor;

		public INCTSCommodityDepartureAndAmendment Commodity => commodity ?? (commodity = new NCTS5CommodityDepartureAndAmendmentWrapper(itemDeparture));
		NCTS5CommodityDepartureAndAmendmentWrapper commodity;

		public IReadOnlyCollection<INCTSCommonPreviousDocument> PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					var previousDocumentList = new List<NCTS5CommonPreviousDocumentWrapper>();

					var doc = item.IsInPhase5TransitionPeriod ? itemDeparture.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault()
								?? item.Bill.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault()
								?? item.Header.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault()
								: itemDeparture.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault()
									?? item.Header.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault();

					if (doc != null)
					{
						var seqNum = (ZInt)1;
						GetUOMAndQuantityForPreviousDocument(out var uom, out var quantity);
						previousDocumentList.Add(new NCTS5CommonPreviousDocumentWrapper(doc, seqNum, uom, quantity));
					}
					previousDocument = previousDocumentList.AsReadOnly();
				}
				return previousDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonPreviousDocumentWrapper> previousDocument;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> SupportingDocument
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

					supportingDocument = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs);
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> supportingDocument;

		void GetUOMAndQuantityForPreviousDocument(out ZString uom, out ZDecimal quantity)
		{
			uom = ZString.Empty;
			quantity = ZDecimal.Zero;
			var vehiclesQuantity = itemDeparture.IsVehicles ? itemDeparture.Packages.Sum(g => 1) : ZDecimal.Zero;
			if (!vehiclesQuantity.IsEmpty)
			{
				uom = CustomsUq.Number.NumberOfItems;
				quantity = vehiclesQuantity;
			}
		}
	}
}
