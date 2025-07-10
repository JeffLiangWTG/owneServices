using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CDepartureTransportMeansProvider : IDepartureTransportMeans
	{
		protected readonly CusTransportMeans transportMeans;
		public CC044CDepartureTransportMeansProvider(CusTransportMeans transportMeans)
		{
			this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
		}

		public int SequenceNumber => transportMeans.TPM_SequenceNumber;

		public int? TypeOfIdentification => StatusIsNew ? int.TryParse(transportMeans.TPM_TypeOfIdentification, out int value) ? value : null : null;

		public string IdentificationNumber => StatusIsNew ? transportMeans.TPM_IdentificationNumber : ZString.Empty;

		public string Nationality => StatusIsNew ? transportMeans.TPM_RN_NKTransportNationality : ZString.Empty;

		bool StatusIsNew => transportMeans.TPM_TransportState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
	}
}
