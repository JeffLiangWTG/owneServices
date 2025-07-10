using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044DepartureTransportMeansProvider : ITransportMeans
	{
		protected readonly CusTransportMeans transportMeans;
		public IE044DepartureTransportMeansProvider(CusTransportMeans transportMeans)
		{
			this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
		}

		public string TypeOfIdentification => IsAsDeclared ? ZString.Empty : transportMeans.TPM_TypeOfIdentification;

		public string IdentificationNumber => IsAsDeclared ? ZString.Empty : transportMeans.TPM_IdentificationNumber;

		public string Nationality => IsAsDeclared ? ZString.Empty : transportMeans.TPM_RN_NKTransportNationality;

		bool IsAsDeclared => transportMeans.TPM_TransportState.EqualsIgnoringCase(EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC);
	}
}
