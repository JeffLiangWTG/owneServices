using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class PassiveBorderTransportMeansProvider : IPassiveBorderTransportMeans
	{
		public PassiveBorderTransportMeansProvider(AsycudaTransportMeans asycudaTransportMeans)
		{
			this.asycudaTransportMeans = Argument.NotNull(asycudaTransportMeans, nameof(asycudaTransportMeans));
		}

		readonly AsycudaTransportMeans asycudaTransportMeans;

		public string IdentificationNumber => asycudaTransportMeans.TPM_IdentificationNumber;

		public string TypeOfIdentification => asycudaTransportMeans.TPM_TypeOfIdentification;

		public string TypeOfMeansOfTransport => asycudaTransportMeans.TPM_TypeOfTransportMeans;

		public string Nationality => asycudaTransportMeans.TPM_RN_NKTransportNationality;
	}
}
