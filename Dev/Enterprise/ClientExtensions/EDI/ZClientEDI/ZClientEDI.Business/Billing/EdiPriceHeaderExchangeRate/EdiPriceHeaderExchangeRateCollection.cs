using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderExchangeRateCollection : ActiveBusinessObjectCollection<EdiPriceHeaderExchangeRate>
	{
		public EdiPriceHeaderExchangeRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiPriceHeaderExchangeRateCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiPriceHeaderExchangeRateCollection(ClientLicencePriceHeader master)
			: base(master.Factory, master, null, EdiPriceHeaderExchangeRateSchema.PHE_L6)
		{
		}

		public EdiPriceHeaderExchangeRate FindByGroupCodeAndCurrency(ZString groupCode, ZString currency)
		{
			return this.FirstOrDefault(x => x.PHE_GroupCode == groupCode && x.PHE_RX_NKCurrency == currency);
		}

		#region Implementation

		protected override bool AllowNew => EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed;

		#endregion Implementation
	}
}


