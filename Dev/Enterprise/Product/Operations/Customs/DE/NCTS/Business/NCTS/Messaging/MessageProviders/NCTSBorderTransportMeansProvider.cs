using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSBorderTransportMeansProvider : INCTSBorderTransportMeans
	{
		public static NCTSBorderTransportMeansProvider NewOrNull(NctsCommonMovementHeader movementHeader) => movementHeader != null ? new NCTSBorderTransportMeansProvider(movementHeader) : null;

		public static NCTSBorderTransportMeansProvider NewOrNull(DepartureCusTransportMeans departureCusTransPortMeans) => departureCusTransPortMeans != null ? new NCTSBorderTransportMeansProvider(departureCusTransPortMeans) : null;

		NCTSBorderTransportMeansProvider(NctsCommonMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		NCTSBorderTransportMeansProvider(DepartureCusTransportMeans departureCusTransPortMeans)
		{
			this.departureCusTransPortMeans = Argument.NotNull(departureCusTransPortMeans, nameof(departureCusTransPortMeans));
		}

		public string TypeOfIdentification => movementHeader?.BM_ActiveBorderIdentificationType ?? departureCusTransPortMeans.TPM_TypeOfIdentification;

		public string IdentificationNumber => movementHeader?.BM_TOLCarrierID ?? departureCusTransPortMeans.TPM_IdentificationNumber;

		public string Nationality => movementHeader?.BM_RN_NKTOLCarrierNationality ?? departureCusTransPortMeans.TPM_RN_NKTransportNationality;

		public string ConveyanceReferenceNumber => movementHeader?.BM_ConveyanceNumber ?? departureCusTransPortMeans.TPM_ReferenceNumber;

		public string CustomsOfficeAtBorder => movementHeader?.BM_CustomsOfficeAtBorder ?? departureCusTransPortMeans.TPM_CustomsOffice;

		readonly NctsCommonMovementHeader movementHeader;

		readonly DepartureCusTransportMeans departureCusTransPortMeans;
	}
}
