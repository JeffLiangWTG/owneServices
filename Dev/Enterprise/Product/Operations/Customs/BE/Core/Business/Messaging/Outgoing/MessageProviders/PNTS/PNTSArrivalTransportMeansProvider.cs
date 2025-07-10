using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSArrivalTransportMeansProvider : IArrivalTransportMeans
{
	public PNTSArrivalTransportMeansProvider(ArrivalTransportMeans arrivalTransportMeans)
	{
		this.arrivalTransportMeans = Argument.NotNull(arrivalTransportMeans, nameof(arrivalTransportMeans));
	}

	readonly ArrivalTransportMeans arrivalTransportMeans;

	public int TypeOfIdentification => int.TryParse(arrivalTransportMeans.TPM_TypeOfIdentification, out int result) ? result : 0;

	public string IdentificationNumber => arrivalTransportMeans.TPM_IdentificationNumber;
}
