using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class ArrivalCusTransportMeans : EU.NCTS.Business.ArrivalCusTransportMeans
	{
		public ArrivalCusTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ArrivalCusTransportMeansLookups Lookups => (ArrivalCusTransportMeansLookups)base.Lookups;

		protected override Customs.Business.CusTransportMeansLookups GetNewLookups() => new ArrivalCusTransportMeansLookups(this);

		public override ZString TPM_TransportState
		{
			get => base.TPM_TransportState;
			set
			{
				base.TPM_TransportState = value;
				if (base.TPM_TransportState == NctsUnloadedStateList.Codes.DIF)
				{
					TPM_TypeOfIdentification = ZString.Empty;
					TPM_IdentificationNumber = ZString.Empty;
					TPM_RN_NKTransportNationality = ZString.Empty;
				}
			}
		}
	}
}
