using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ActiveBorderTransportMeansProvider : IActiveBorderTransportMeans
	{
		public ActiveBorderTransportMeansProvider(NctsDepartureMovementHeader depHeader, int sequenceNumber)
		{
			this.depHeader = Argument.NotNull(depHeader, nameof(depHeader));
			SequenceNumber = sequenceNumber;
		}

		public ActiveBorderTransportMeansProvider(DepartureCusTransportMeans departureCusTransportMeans, int sequenceNumber)
		{
			this.departureCusTransportMeans = Argument.NotNull(departureCusTransportMeans, nameof(departureCusTransportMeans));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; private set; }

		public string CustomsOfficeAtBorderReferenceNumber => depHeader?.BM_CustomsOfficeAtBorder ?? departureCusTransportMeans.TPM_CustomsOffice;

		public int TypeOfIdentification => int.TryParse(depHeader?.BM_ActiveBorderIdentificationType ?? departureCusTransportMeans.TPM_TypeOfIdentification, out int result) ? result : 0;

		public string IdentificationNumber => depHeader?.BM_TOLCarrierID ?? departureCusTransportMeans.TPM_IdentificationNumber;

		public string Nationality => depHeader?.BM_RN_NKTOLCarrierNationality ?? departureCusTransportMeans.TPM_RN_NKTransportNationality;

		public string ConveyanceReferenceNumber => depHeader?.BM_ConveyanceNumber ?? departureCusTransportMeans.TPM_ReferenceNumber;

		readonly NctsCommonMovementHeader depHeader;
		readonly DepartureCusTransportMeans departureCusTransportMeans;
	}
}
