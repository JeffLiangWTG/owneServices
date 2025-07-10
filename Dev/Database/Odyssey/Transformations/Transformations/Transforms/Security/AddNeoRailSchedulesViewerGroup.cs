namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoRailSchedulesViewerGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBRAILSCHDVIEW";

		protected override string GroupDescription => "Web Rail Schedules (View)";

		protected override string RoleName => "webrailschedulesviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
