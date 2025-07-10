namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddWebMessageViewer : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBMSGVIEW";

		protected override string GroupDescription => "Web Message (View)";

		protected override string RoleName => "webmessageviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
