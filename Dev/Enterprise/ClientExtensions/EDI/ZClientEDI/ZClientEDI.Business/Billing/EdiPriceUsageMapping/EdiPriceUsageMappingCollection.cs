using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceUsageMappingCollection : ActiveBusinessObjectCollection<EdiPriceUsageMapping>
	{
		public EdiPriceUsageMappingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiPriceUsageMappingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EdiPriceUsageMappingCollection(ClientLicencePriceHeader master)
			: base(master.Factory, master, null, EdiPriceUsageMappingSchema.PUM_L6)
		{
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed; }
		}

		#endregion
	}
}

