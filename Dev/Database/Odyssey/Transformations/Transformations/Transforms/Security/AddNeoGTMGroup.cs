namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoGTMGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBGTMVIEW";

		protected override string GroupDescription => "Web GTM (View)";

		protected override string RoleName => "webgtmviewer";

		protected override bool IsNeoDefaultRole => false;
	}
}
