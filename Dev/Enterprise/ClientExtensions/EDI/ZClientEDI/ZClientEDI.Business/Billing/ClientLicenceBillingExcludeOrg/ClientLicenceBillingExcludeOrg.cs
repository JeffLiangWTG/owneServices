using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingExcludeOrg : AutoClientLicenceBillingExcludeOrg
	{
		public ClientLicenceBillingExcludeOrg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.SystemCodes")]
		public override ZString CEX_BillingSystem
		{
			get { return base.CEX_BillingSystem; }
			set { base.CEX_BillingSystem = value; }
		}
	}
}
