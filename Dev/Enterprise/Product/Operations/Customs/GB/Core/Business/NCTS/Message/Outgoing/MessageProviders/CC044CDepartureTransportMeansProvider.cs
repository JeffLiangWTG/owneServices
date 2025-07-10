using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC044CDepartureTransportMeansProvider : IDepartureTransportMeans
	{
		public CC044CDepartureTransportMeansProvider(CusTransportMeans transportMeans)
		{
			this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
		}

		public int SequenceNumber => transportMeans.TPM_SequenceNumber;

		public int TypeOfIdentification => StatusIsNew && int.TryParse(transportMeans.TPM_TypeOfIdentification, out int value) ? value : 0;

		public string IdentificationNumber => StatusIsNew ? transportMeans.TPM_IdentificationNumber : ZString.Empty;

		public string Nationality => StatusIsNew ? transportMeans.TPM_RN_NKTransportNationality : ZString.Empty;

		bool StatusIsNew => transportMeans.TPM_TransportState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

		protected readonly CusTransportMeans transportMeans;
	}
}
