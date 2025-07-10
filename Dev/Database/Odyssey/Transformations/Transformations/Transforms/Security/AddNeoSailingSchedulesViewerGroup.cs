namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoSailingSchedulesViewerGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBSEASCHDVIEW";

		protected override string GroupDescription => "Web Sailing Schedules (View)";

		protected override string RoleName => "websailingschedulesviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
