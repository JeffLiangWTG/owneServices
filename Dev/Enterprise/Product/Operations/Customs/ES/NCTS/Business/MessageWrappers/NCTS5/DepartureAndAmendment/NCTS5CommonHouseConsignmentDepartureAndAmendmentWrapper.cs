using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper : NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper, INCTSCommonHouseConsignmentDepartureAndAmendment
	{
		public NCTS5CommonHouseConsignmentDepartureAndAmendmentWrapper(NctsBill houseConsignment, ZBool shouldDeclareCountryOfDispatchInHouseOrItem, ZBool shouldDeclareCountryOfDestinationInHouseOrItem, ZBool shouldDeclareReferenceNumberUCRInHouseOrItem, ZBool shouldDeclareConsigneeInHouseOrItem, ZBool shouldDeclareConsignorInHouse, ZBool shouldDeclareDepartureTransportMeansInHouse, ZString messageType) : base(houseConsignment, shouldDeclareReferenceNumberUCRInHouseOrItem)
		{
			this.shouldDeclareCountryOfDispatchInHouseOrItem = shouldDeclareCountryOfDispatchInHouseOrItem;
			this.shouldDeclareCountryOfDestinationInHouseOrItem = shouldDeclareCountryOfDestinationInHouseOrItem;
			this.shouldDeclareConsigneeInHouseOrItem = shouldDeclareConsigneeInHouseOrItem;
			this.shouldDeclareConsignorInHouse = shouldDeclareConsignorInHouse;
			this.shouldDeclareDepartureTransportMeansInHouse = shouldDeclareDepartureTransportMeansInHouse;
			shouldDeclareDeclarationTypeInItem = ShouldDeclareDeclarationTypeInItem();
			this.departureMovement = houseConsignment.Header.MovementHeader;
			this.messageType = messageType;
		}
		readonly ZBool shouldDeclareCountryOfDispatchInHouseOrItem;
		readonly ZBool shouldDeclareCountryOfDestinationInHouseOrItem;
		readonly ZBool shouldDeclareConsigneeInHouseOrItem;
		readonly ZBool shouldDeclareConsignorInHouse;
		readonly ZBool shouldDeclareDepartureTransportMeansInHouse;
		readonly ZBool shouldDeclareDeclarationTypeInItem;
		readonly ZString messageType;

		readonly NctsDepartureMovementHeader departureMovement;

		public IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor
		{
			get
			{
				if (additionalSupplyChainActor == null)
				{
					var additionalSupplyChainActorList = new List<CommonAdditionalSupplyChainActorSeqNumWrapper>();

					ZShort seqNum = 1;
					foreach (var chainActor in houseConsignment.CusSupplyChainActorReferences.Cast<EU.NCTS.Business.CusSupplyChainActorReference>())
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

		public IReadOnlyCollection<INCTSConsignmentItemDepartureAndAmendment> ConsignmentItem => consignmentItem ?? (consignmentItem = houseConsignment.GoodsItems.Cast<NctsDepartureCargoDesc>().Select(x => new NCTS5ConsignmentItemDepartureAndAmendmentWrapper(x, shouldDeclareDeclarationTypeInItem, ShouldDeclareCountryOfDispatchInItem(), ShouldDeclareCountryOfDestinationInItem(), ShouldDeclareReferenceNumberUCRInItem(), ShouldDeclareConsigneeInItem())).ToList().AsReadOnly());
		IReadOnlyCollection<NCTS5ConsignmentItemDepartureAndAmendmentWrapper> consignmentItem;

		public ZString CountryOfDispatch => !houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareCountryOfDispatchInHouseOrItem && !HasDifferentCountryOfDispatchInItem() ? GetCountryOfDispatchFromHouseOrParent() : ZString.Empty;
		ZString GetCountryOfDispatchFromHouseOrParent() => !houseConsignment.B0_RN_NKCountryOfExport.IsEmpty
																? houseConsignment.B0_RN_NKCountryOfExport
																: houseConsignment.Header.MovementHeader.BM_RN_NKCountryOfDispatch;

		public ZString CountryOfDestination => !houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareCountryOfDestinationInHouseOrItem && !HasDifferentCountryOfDestinationInItem() ? GetCountryOfDestinationFromHouseOrParent() : ZString.Empty;
		ZString GetCountryOfDestinationFromHouseOrParent() => !houseConsignment.B0_RN_NKCountryOfDestination.IsEmpty
																? houseConsignment.B0_RN_NKCountryOfDestination
																: houseConsignment.Header.MovementHeader.BM_RL_NKDestinationPort;

		public INCTSCommonConsignor Consignor => consignor ?? (consignor = !houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareConsignorInHouse ? NCTS5CommonConsignorWrapper.New(GetConsignorFromHouseOrParent(), houseConsignment.IsInPhase5TransitionPeriod) : null);
		NCTS5CommonConsignorWrapper consignor;
		JobDocAddress GetConsignorFromHouseOrParent() => !houseConsignment.Consignor.IsEmpty
																	? houseConsignment.Consignor
																	: houseConsignment.Header.Consignor;

		public INCTSPartyNameProviderWithAddress Consignee => consignee ?? (consignee = !houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareConsigneeInHouseOrItem && !HasDifferentConsigneeInItem() ? NCTS5PartyNameProviderWithAddressWrapper.New(GetConsigneeFromHouseOrParent(), true, false) : null);
		NCTS5PartyNameProviderWithAddressWrapper consignee;
		JobDocAddress GetConsigneeFromHouseOrParent() => !houseConsignment.Consignee.IsEmpty
																	? houseConsignment.Consignee
																	: houseConsignment.Header.Consignee;

		public IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = (!houseConsignment.IsInPhase5TransitionPeriod && shouldDeclareDepartureTransportMeansInHouse ? GetDepartureTransportMeansFromHouseOrParent() : new List<CommonDepartureTransportMeansWrapper>().AsReadOnly()));
		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> departureTransportMeans;

		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> GetDepartureTransportMeansFromHouseOrParent()
		{
			var houseDepartureTransportMeans = NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(houseConsignment.InlandTransportModeAtDeparture, houseConsignment.TransportTypeAtDeparture,
					houseConsignment.TransportAtDeparture, houseConsignment.TransportCountryAtDeparture, houseConsignment.VesselNameAtDeparture, houseConsignment.VesselCountryAtDeparture,
					houseConsignment.Trailer1IDAtDeparture, houseConsignment.Trailer1NationalityAtDeparture, houseConsignment.Trailer2IDAtDeparture, houseConsignment.Trailer2NationalityAtDeparture);

			return houseDepartureTransportMeans.Count != ZInt.Zero
				? houseDepartureTransportMeans
				: NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(departureMovement.InlandTransportModeAtDeparture, departureMovement.TransportTypeAtDeparture,
					departureMovement.TransportAtDeparture, departureMovement.TransportCountryAtDeparture, departureMovement.VesselNameAtDeparture, departureMovement.VesselCountryAtDeparture,
					departureMovement.Trailer1IDAtDeparture, departureMovement.Trailer1NationalityAtDeparture, departureMovement.Trailer2IDAtDeparture, departureMovement.Trailer2NationalityAtDeparture);
		}

		public IReadOnlyCollection<INCTSCommonDocumentWithInfo> PreviousDocument
		{
			get
			{
				if (previousDocument == null)
				{
					previousDocument = new List<NCTS5CommonDocumentWithInfoWrapper>();
					var isDPD = messageType == DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration;

					var doc = !houseConsignment.IsInPhase5TransitionPeriod && !isDPD ? houseConsignment.PreviousDocuments.Cast<CusSupportingInfo>().FirstOrDefault() : null;

					if (doc != null)
					{
						var seqNum = (ZInt)1;
						previousDocument.Add(new NCTS5CommonDocumentWithInfoWrapper(doc, seqNum));
					}
				}
				return previousDocument;
			}
		}
		List<NCTS5CommonDocumentWithInfoWrapper> previousDocument;

		ZBool ShouldDeclareCountryOfDispatchInItem() => shouldDeclareCountryOfDispatchInHouseOrItem && HasDifferentCountryOfDispatchInItem();

		ZBool HasDifferentCountryOfDispatchInItem() => houseConsignment.GoodsItems.Any(y => !y.BY_RN_NKCountryOfDispatch.IsEmpty && y.BY_RN_NKCountryOfDispatch != houseConsignment.B0_RN_NKCountryOfExport);

		ZBool ShouldDeclareCountryOfDestinationInItem() => shouldDeclareCountryOfDestinationInHouseOrItem && HasDifferentCountryOfDestinationInItem();

		ZBool HasDifferentCountryOfDestinationInItem() => houseConsignment.GoodsItems.Any(y => !y.BY_RN_NKCountryOfDestination.IsEmpty && y.BY_RN_NKCountryOfDestination != houseConsignment.B0_RN_NKCountryOfDestination);

		ZBool ShouldDeclareReferenceNumberUCRInItem() => shouldDeclareReferenceNumberUCRInHouseOrItem && HasDifferentReferenceNumberUCRInItem();

		ZBool ShouldDeclareConsigneeInItem() => shouldDeclareConsigneeInHouseOrItem && HasDifferentConsigneeInItem();

		ZBool HasDifferentConsigneeInItem() => houseConsignment.GoodsItems.Any(y => y.Consignee?.Address?.Header != null && y.Consignee?.Address?.Header != houseConsignment.Consignee?.Address?.Header);

		ZBool ShouldDeclareDeclarationTypeInItem()
		{
			var declarationTypeInItems = houseConsignment.GoodsItems.Select(y => y.BY_Type).Distinct();

			return houseConsignment.Header.MovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T && declarationTypeInItems.Count() > 1;
		}
	}
}
