using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrgAddress : OrgAddress
	{
		public WoolworthsOrgAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultUsageComment()
		{
		}
	}
}
