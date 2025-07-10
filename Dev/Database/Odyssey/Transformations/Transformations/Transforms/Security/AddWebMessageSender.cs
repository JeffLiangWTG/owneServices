namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddWebMessageSender : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBMSGSEND";

		protected override string GroupDescription => "Web Message (Send)";

		protected override string RoleName => "webmessagesender";

		protected override bool IsNeoDefaultRole => true;
	}
}
