using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonDepartureAndNotifConsignmentWrapper : NCTS5CommonDepartureConsignmentWrapper, INCTSCommonDepartureAndNotifConsignment
	{
		public NCTS5CommonDepartureAndNotifConsignmentWrapper(NctsHeader header) : base(header)
		{
			isEXISecurityType = departureMovement.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI;
		}
		protected readonly ZBool isEXISecurityType;

		public INCTSCommonLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = (departureMovement.GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty).IsEmpty ? null : new NCTS5CommonLocationOfGoodsWrapper(departureMovement.GoodsLocation));
		NCTS5CommonLocationOfGoodsWrapper locationOfGoods;

		public IReadOnlyCollection<INCTSCommonActiveBorderTransportMeansWithOffice> ActiveBorderTransportMeans
		{
			get
			{
				if (activeBorderTransportMeans == null)
				{
					var activeBorderTransportMeansList = new List<NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper>();

					var office = departureMovement.BM_CustomsOfficeAtBorder;
					var mode = departureMovement.BM_ActiveBorderIdentificationType;
					var id = departureMovement.BM_TOLCarrierID;
					var nationality = departureMovement.BM_RN_NKTOLCarrierNationality;
					var conveyance = departureMovement.BM_ConveyanceNumber;

					if (departureMovement.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment
						&& (!office.IsEmpty || !mode.IsEmpty || !id.IsEmpty || !nationality.IsEmpty || !conveyance.IsEmpty))
					{
						ZShort seqNum = 1;
						activeBorderTransportMeansList.Add(new NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper(mode, id, nationality, office, conveyance, seqNum));
					}
					activeBorderTransportMeans = activeBorderTransportMeansList.AsReadOnly();
				}
				return activeBorderTransportMeans;
			}
		}
		IReadOnlyCollection<NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper> activeBorderTransportMeans;

		public INCTSCommonPlace PlaceOfLoading => placeOfLoading ?? (placeOfLoading = (!nctsHeader.IsInPhase5TransitionPeriod && !departureMovement.BM_PlaceOfLoading.IsEmpty) || (nctsHeader.IsInPhase5TransitionPeriod && isEXISecurityType) ? new NCTS5CommonPlaceWrapper(departureMovement.BM_PlaceOfLoading, nctsHeader.Factory) : null);
		NCTS5CommonPlaceWrapper placeOfLoading;
	}
}
