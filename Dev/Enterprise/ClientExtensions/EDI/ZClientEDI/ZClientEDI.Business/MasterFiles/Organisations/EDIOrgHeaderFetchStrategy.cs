using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgHeaderFetchStrategy : OrgHeaderFetchStrategy
	{
		public EDIOrgHeaderFetchStrategy(EDIOrgHeader org)
			: base(org)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(LicenceDatabaseSchema.LD_OH_WebAccessOrg, BusinessObject.PK);
		}
	}
}
