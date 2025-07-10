using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5ConsignmentWrapper : NCTS5CommonDepartureConsignmentWrapper, ITNNNCTSConsignment
	{
		public TNNNCTS5ConsignmentWrapper(NctsHeader header) : base(header)
		{
			isTIRDeclaration = departureMovement.BM_InBondEntryType == NctsDeclarationTypeList.Codes.TIR;
			isEXISecurityType = departureMovement.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI;

			shouldDeclareCountryOfDestinationInConsignment = ShouldDeclareCountryOfDestinationInConsignment();
		}
		readonly ZBool isTIRDeclaration;
		readonly ZBool isEXISecurityType;
		readonly ZBool shouldDeclareCountryOfDestinationInConsignment;

		public INCTSCommonConsignmentDepartureAndAmendmentAndTNN CommonConsignmentData => commonConsignmentData ??
																							(commonConsignmentData = new NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper(nctsHeader,
																																													isTIRDeclaration,
																																													shouldDeclareCountryOfDispatchInConsignment: true,
																																													shouldDeclareCountryOfDestinationInConsignment,
																																													shouldDeclareUCRInConsignment: true,
																																													shouldDeclareConsigneeInConsignment: true,
																																													any30600AdditionalInfoInItems: false,
																																													isEXISecurityType));
		NCTS5CommonConsignmentDepartureAndAmendmentAndTNNWrapper commonConsignmentData;

		public INCTSPartyNameProviderWithAddress Consignor => consignor ?? (consignor = !isEXISecurityType && (nctsHeader?.Principal?.Address?.Header != nctsHeader.Consignor?.Address?.Header) ? NCTS5PartyNameProviderWithAddressWrapper.New(nctsHeader.Consignor, isInPhase5TransitionPeriod: nctsHeader.IsInPhase5TransitionPeriod) : null);
		NCTS5PartyNameProviderWithAddressWrapper consignor;

		public IReadOnlyCollection<INCTSCommonActiveBorderTransportMeans> ActiveBorderTransportMeans
		{
			get
			{
				if (activeBorderTransportMeans == null)
				{
					var activeBorderTransportMeansList = new List<NCTS5CommonActiveBorderTransportMeansWrapper>();

					var mode = departureMovement.BM_ActiveBorderIdentificationType;
					var id = departureMovement.BM_TOLCarrierID;
					var nationality = departureMovement.BM_RN_NKTOLCarrierNationality;
					var conveyance = departureMovement.BM_ConveyanceNumber;

					if (!mode.IsEmpty || !id.IsEmpty || !nationality.IsEmpty || !conveyance.IsEmpty)
					{
						ZShort seqNum = 1;
						activeBorderTransportMeansList.Add(new NCTS5CommonActiveBorderTransportMeansWrapper(mode, id, nationality, conveyance, seqNum));
					}
					activeBorderTransportMeans = activeBorderTransportMeansList.AsReadOnly();
				}
				return activeBorderTransportMeans;
			}
		}
		IReadOnlyCollection<NCTS5CommonActiveBorderTransportMeansWrapper> activeBorderTransportMeans;

		public IReadOnlyCollection<ITNNNCTSHouseConsignment> HouseConsignment =>
			houseConsignment ?? (houseConsignment = nctsHeader.Bills.Cast<NctsBill>().Select(x => new TNNNCTS5HouseConsignmentWrapper(x,
																											shouldDeclareCountryOfDestinationInItem: !shouldDeclareCountryOfDestinationInConsignment
																											)).ToList().AsReadOnly());
		IReadOnlyCollection<TNNNCTS5HouseConsignmentWrapper> houseConsignment;

		ZBool ShouldDeclareCountryOfDestinationInConsignment()
		{
			var countryOfDestinationInItems = nctsHeader.Bills.SelectMany(b => b.GoodsItems.Select(y => y.BY_RN_NKCountryOfDestination)).Distinct();
			return countryOfDestinationInItems.Count() <= 1;
		}
	}
}
