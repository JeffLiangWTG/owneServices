using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonConsignmentDepartureAndAmendmentWrapper : NCTS5CommonDepartureAndNotifConsignmentWrapper, INCTSCommonConsignmentDepartureAndAmendment
	{
		public NCTS5CommonConsignmentDepartureAndAmendmentWrapper(NctsHeader header, ZString messageType) : base(header)
		{
			isTIRDeclaration = departureMovement.BM_InBondEntryType == NctsDeclarationTypeList.Codes.TIR;

			shouldDeclareCountryOfDispatchInConsignment = ShouldDeclareCountryOfDispatchInConsignment();
			shouldDeclareCountryOfDestinationInConsignment = ShouldDeclareCountryOfDestinationInConsignment();
			shouldDeclareUCRInConsignment = ShouldDeclareUCRInConsignment();
			shouldDeclareConsigneeInConsignment = ShouldDeclareConsigneeInConsignment();
			shouldDeclareConsignorInConsignment = ShouldDeclareConsignorInConsignment();
			shouldDeclareDepartureTransportMeansInConsignment = ShouldDeclareDepartureTransportMeansInConsignment();
			any30600AdditionalInfoInItems = Any30600AdditionalInfoInItems();
			this.messageType = messageType;
		}
		readonly ZBool isTIRDeclaration;
		readonly ZBool shouldDeclareCountryOfDispatchInConsignment;
		readonly ZBool shouldDeclareCountryOfDestinationInConsignment;
		readonly ZBool shouldDeclareUCRInConsignment;
		readonly ZBool shouldDeclareConsigneeInConsignment;
		readonly ZBool shouldDeclareConsignorInConsignment;
		readonly ZBool shouldDeclareDepartureTransportMeansInConsignment;
		readonly ZBool any30600AdditionalInfoInItems;
		readonly ZString messageType;

		public INCTSCommonConsignmentDepartureAndAmendmentAndTNN CommonConsignmentData => commonConsignmentData ??
																							(commonConsignmentData = new NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper(nctsHeader,
																																													isTIRDeclaration,
																																													shouldDeclareCountryOfDispatchInConsignment,
																																													shouldDeclareCountryOfDestinationInConsignment,
																																													shouldDeclareUCRInConsignment,
																																													shouldDeclareConsigneeInConsignment,
																																													any30600AdditionalInfoInItems,
																																													isEXISecurityType));
		NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper commonConsignmentData;

		public INCTSCommonCarrier Carrier => carrier ?? (carrier = nctsHeader?.Principal?.Address?.Header != departureMovement.Carrier?.Address?.Header ? NCTS5CommonCarrierWrapper.New(departureMovement.Carrier) : null);
		NCTS5CommonCarrierWrapper carrier;

		public INCTSCommonConsignor Consignor => consignor ?? (consignor = (nctsHeader.IsInPhase5TransitionPeriod || shouldDeclareConsignorInConsignment) && isEXISecurityType && (nctsHeader?.Principal?.Address?.Header != nctsHeader.Consignor?.Address?.Header) ? NCTS5CommonConsignorWrapper.New(nctsHeader.Consignor, nctsHeader.IsInPhase5TransitionPeriod) : null);
		NCTS5CommonConsignorWrapper consignor;

		public INCTSCommonPlace PlaceOfUnloading => placeOfUnloading ?? (placeOfUnloading = (!nctsHeader.IsInPhase5TransitionPeriod || departureMovement.BM_SpecificCircumstance != NctsSpecificCircumstanceIndicatorList.Codes.XXX)
																							&& isEXISecurityType ? new NCTS5CommonPlaceWrapper(departureMovement.BM_PlaceOfUnloading, nctsHeader.Factory) : null);
		NCTS5CommonPlaceWrapper placeOfUnloading;

		public IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor
		{
			get
			{
				if (additionalSupplyChainActor == null)
				{
					var additionalSupplyChainActorList = new List<CommonAdditionalSupplyChainActorSeqNumWrapper>();

					ZShort seqNum = 1;
					foreach (var chainActor in departureMovement.CusSupplyChainActors.Cast<CusSupplyChainActorReference>())
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

		public IReadOnlyCollection<INCTSCommonHouseConsignmentDepartureAndAmendment> HouseConsignment =>
			houseConsignment ?? (houseConsignment = nctsHeader.Bills.Cast<NctsBill>().Select(x => new NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper(x,
																											shouldDeclareCountryOfDispatchInHouseOrItem: (isTIRDeclaration || !nctsHeader.IsInPhase5TransitionPeriod) && !shouldDeclareCountryOfDispatchInConsignment,
																											shouldDeclareCountryOfDestinationInHouseOrItem: !shouldDeclareCountryOfDestinationInConsignment,
																											shouldDeclareReferenceNumberUCRInHouseOrItem: !shouldDeclareUCRInConsignment,
																											shouldDeclareConsigneeInHouseOrItem: !shouldDeclareConsigneeInConsignment,
																											shouldDeclareConsignorInHouse: !shouldDeclareConsignorInConsignment,
																											shouldDeclareDepartureTransportMeansInHouse: !shouldDeclareDepartureTransportMeansInConsignment, messageType)).ToList().AsReadOnly());
		IReadOnlyCollection<NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper> houseConsignment;

		ZBool ShouldDeclareCountryOfDispatchInConsignment()
		{
			var countryOfDispatchConsignment = departureMovement.BM_RN_NKCountryOfDispatch;
			var countryOfDispatchDifferentInHousesOrGoodsItems = nctsHeader.Bills.Any(b =>
																					(!b.B0_RN_NKCountryOfExport.IsEmpty
																						&& b.B0_RN_NKCountryOfExport != countryOfDispatchConsignment)
																					|| b.GoodsItems.Any(y =>
																										!y.BY_RN_NKCountryOfDispatch.IsEmpty
																										&& y.BY_RN_NKCountryOfDispatch != countryOfDispatchConsignment));
			return !countryOfDispatchDifferentInHousesOrGoodsItems;
		}

		ZBool ShouldDeclareCountryOfDestinationInConsignment()
		{
			var countryOfDestinationConsignment = departureMovement.BM_RL_NKDestinationPort;
			var countryOfDestinationDifferentInHousesOrGoodsItems = nctsHeader.Bills.Any(b =>
																					(!b.B0_RN_NKCountryOfDestination.IsEmpty
																						&& b.B0_RN_NKCountryOfDestination != countryOfDestinationConsignment)
																					|| b.GoodsItems.Any(y =>
																										!y.BY_RN_NKCountryOfDestination.IsEmpty
																										&& y.BY_RN_NKCountryOfDestination != countryOfDestinationConsignment));
			return !countryOfDestinationDifferentInHousesOrGoodsItems;
		}

		ZBool ShouldDeclareUCRInConsignment()
		{
			var ucrConsignment = departureMovement.BM_UniqueConsignmentReference;
			var ucrDifferentInHousesOrGoodsItems = nctsHeader.Bills.Any(b =>
																	(!b.B0_ReferenceID.IsEmpty
																		&& b.B0_ReferenceID != ucrConsignment)
																	|| b.GoodsItems.Any(y =>
																						!y.BY_CommercialReferenceNumber.IsEmpty
																						&& y.BY_CommercialReferenceNumber != ucrConsignment));
			return !ucrDifferentInHousesOrGoodsItems;
		}

		ZBool ShouldDeclareConsigneeInConsignment()
		{
			var consigneeConsignment = nctsHeader.Consignee?.Address?.Header;
			var consigneeDifferentInHousesOrGoodsItems = nctsHeader.Bills.Any(b =>
																			(b.Consignee?.Address?.Header != null
																				&& b.Consignee?.Address?.Header != consigneeConsignment)
																			|| b.GoodsItems.Any(y =>
																								y.Consignee?.Address?.Header != null
																								&& y.Consignee?.Address?.Header != consigneeConsignment));
			return !consigneeDifferentInHousesOrGoodsItems;
		}

		ZBool ShouldDeclareConsignorInConsignment()
		{
			var consignorConsignment = nctsHeader.Consignor?.Address?.Header;
			var consignorDifferentInHouses = nctsHeader.Bills.Any(b =>
																(b.Consignor?.Address?.Header != null
																	&& b.Consignor?.Address?.Header != consignorConsignment));
			return !consignorDifferentInHouses;
		}

		ZBool Any30600AdditionalInfoInItems()
		{
			const string additionalInfoCode = "30600";
			var any30600addInfoInItems = nctsHeader.Bills.Any(b => b.GoodsItems.Any(i => i.AdditionalInfos.Any(a => a.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && a.CSI_Code == additionalInfoCode)));
			return any30600addInfoInItems;
		}

		protected override bool ShouldDeclareDepartureTransportMeansInHouse => !nctsHeader.IsInPhase5TransitionPeriod;
	}
}
