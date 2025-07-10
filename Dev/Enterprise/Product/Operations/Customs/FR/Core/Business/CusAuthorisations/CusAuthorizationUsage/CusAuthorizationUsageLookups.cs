namespace Enterprise.Customs.FR.Business
{
	public abstract class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
	{
		public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		protected new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;
	}
}
