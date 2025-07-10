using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESREMTransportMeansProvider : NCTSTransportMeansProvider, IDESREMTransportMeans
	{
		public static DESREMTransportMeansProvider NewOrNull(ArrivalCusTransportMeans transportMeans) => transportMeans != null ? new DESREMTransportMeansProvider(transportMeans) : null;

		DESREMTransportMeansProvider(ArrivalCusTransportMeans transportMeans)
			: base(transportMeans.TPM_TypeOfIdentification, transportMeans.TPM_IdentificationNumber, transportMeans.TPM_RN_NKTransportNationality)
		{
			this.transportMeans = transportMeans;
		}
		readonly ArrivalCusTransportMeans transportMeans;

		public int SequenceNumber => transportMeans.TPM_SequenceNumber;

		string INCTSTransportMeans.TypeOfIdentification => TransportStateIsNewOrDif ? TypeOfIdentification : null;

		string INCTSTransportMeans.IdentificationNumber => TransportStateIsNewOrDif ? IdentificationNumber : null;

		string INCTSTransportMeans.Nationality => TransportStateIsNewOrDif ? Nationality : null;

		bool TransportStateIsNewOrDif => CachedValueHelper.GetValue(ref transportStateIsNewOrDif, () => transportMeans.TPM_TransportState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF }));
		CachedValue<bool> transportStateIsNewOrDif;
	}
}
