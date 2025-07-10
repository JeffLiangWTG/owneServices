using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class ArrivalTransportMeansProvider : IIdType
	{
		ArrivalTransportMeansProvider(ArrivalTransportMeans arrivalTransportMeans)
		{
			this.arrivalTransportMeans = Argument.NotNull(arrivalTransportMeans, nameof(arrivalTransportMeans));
		}
		readonly ArrivalTransportMeans arrivalTransportMeans;

		public string Type => arrivalTransportMeans.TPM_TypeOfIdentification.ToString();

		public string Id => arrivalTransportMeans.TPM_IdentificationNumber;

		public static ArrivalTransportMeansProvider New(ArrivalTransportMeans arrivalTransportMeans) => arrivalTransportMeans == null ? null : new ArrivalTransportMeansProvider(arrivalTransportMeans);
	}
}
