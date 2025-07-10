namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddMarketIntelligenceGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBMARKETINTEL";

		protected override string GroupDescription => "Web Market Intelligence (View)";

		protected override string RoleName => "webmarketintelligenceviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
