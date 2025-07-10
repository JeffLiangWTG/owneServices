using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NCTSBillDepartureTransportMeansProvider : IDepartureTransportMeans
	{
		readonly DepartureCusTransportMeans departureTransportMeans;
		readonly string transportMode;

		public NCTSBillDepartureTransportMeansProvider(DepartureCusTransportMeans departureTransportMeans, string transportMode)
		{
			this.departureTransportMeans = Argument.NotNull(departureTransportMeans, nameof(departureTransportMeans));
			this.transportMode = transportMode;
		}

		public int SequenceNumber => departureTransportMeans.TPM_SequenceNumber;

		public int? TypeOfIdentification
		{
			get
			{
				switch (transportMode)
				{
					case ModeOfTransportList.Codes._1_SeaTransport:
						return 10;
					case ModeOfTransportList.Codes._2_RailTransport:
						return departureTransportMeans.TPM_SequenceNumber == 1 ? 20 : 21;
					case ModeOfTransportList.Codes._3_RoadTransport:
						return departureTransportMeans.TPM_SequenceNumber == 1 ? 30 : 31;
					case ModeOfTransportList.Codes._4_AirTransport:
						return 40;
					case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
						return 81;
					case ModeOfTransportList.Codes._9_OwnPropulsion:
						return int.TryParse(departureTransportMeans.TPM_TypeOfIdentification, out var type) ? type : null;
					default:
						return null;
				}
			}
		}

		public string IdentificationNumber => departureTransportMeans.TPM_IdentificationNumber;

		public string Nationality => departureTransportMeans.TPM_RN_NKTransportNationality;
	}
}
