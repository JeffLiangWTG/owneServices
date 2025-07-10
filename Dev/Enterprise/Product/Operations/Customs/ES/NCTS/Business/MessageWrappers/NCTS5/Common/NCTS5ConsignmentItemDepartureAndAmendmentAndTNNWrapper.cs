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
	public class NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper : NCTS5CommonConsignmentItemWrapper, INCTSCommonConsignmentItemDepartureAndAmendmentAndTNN
	{
		public NCTS5ConsignmentItemDepartureAndAmendmentAndTNNWrapper(NctsDepartureCargoDesc item, ZBool shouldDeclareDeclarationTypeInItem, ZBool shouldDeclareCountryOfDestinationInItem, ZBool shouldDeclareReferenceNumberUCRInItem) : base(item)
		{
			this.shouldDeclareDeclarationTypeInItem = shouldDeclareDeclarationTypeInItem;
			this.shouldDeclareCountryOfDestinationInItem = shouldDeclareCountryOfDestinationInItem;
			this.shouldDeclareReferenceNumberUCRInItem = shouldDeclareReferenceNumberUCRInItem;
			itemDeparture = item;
		}
		readonly ZBool shouldDeclareDeclarationTypeInItem;
		readonly ZBool shouldDeclareCountryOfDestinationInItem;
		readonly ZBool shouldDeclareReferenceNumberUCRInItem;
		protected readonly NctsDepartureCargoDesc itemDeparture;

		protected override IReadOnlyCollection<NCTS5CommonPackagingWrapper> GetPackagingCore() => NCTS5CommonPackagingWrapper.GetPackagesListDeparture(itemDeparture);

		public ZString DeclarationType => shouldDeclareDeclarationTypeInItem ? item.BY_Type : ZString.Empty;

		public ZString CountryOfDestination => shouldDeclareCountryOfDestinationInItem ? GetCountryOfDestinationFromGoodsItemOrParents() : ZString.Empty;
		ZString GetCountryOfDestinationFromGoodsItemOrParents() => !item.BY_RN_NKCountryOfDestination.IsEmpty
																	? item.BY_RN_NKCountryOfDestination
																	: !item.Bill.B0_RN_NKCountryOfDestination.IsEmpty
																			? item.Bill.B0_RN_NKCountryOfDestination
																			: item.Header.MovementHeader.BM_RL_NKDestinationPort;

		public ZString ReferenceNumberUCR => shouldDeclareReferenceNumberUCRInItem ? GetReferenceNumberUCRFromGoodsItemOrParents() : ZString.Empty;
		ZString GetReferenceNumberUCRFromGoodsItemOrParents() => !item.BY_CommercialReferenceNumber.IsEmpty
																	? item.BY_CommercialReferenceNumber
																	: !item.Bill.B0_ReferenceID.IsEmpty
																			? item.Bill.B0_ReferenceID
																			: item.Header.MovementHeader.BM_UniqueConsignmentReference;

		public IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation
		{
			get
			{
				if (additionalInformation == null)
				{
					var addDocs = item.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
					if (item.IsInPhase5TransitionPeriod)
					{
						addDocs.AddRange(item.Bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList());
						addDocs.AddRange(item.Header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList());
					}

					additionalInformation = CommonWrappersHelper.GetDocumentSequenceNumberWrapperListWithLineNoAsSeq(addDocs, shouldSendReferenceNumber: false, shouldSendDescription: true);
				}
				return additionalInformation;
			}
		}
		IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> additionalInformation;
	}
}
