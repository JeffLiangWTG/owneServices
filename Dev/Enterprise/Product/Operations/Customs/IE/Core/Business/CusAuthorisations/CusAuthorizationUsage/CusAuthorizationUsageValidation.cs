namespace Enterprise.Customs.IE.Business
{
	public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
	{
		public CusAuthorizationUsageValidation(CusAuthorizationUsage parent)
			: base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;
	}
}
