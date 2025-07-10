namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoRoadSchedulesViewerGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBROADSCHDVIEW";

		protected override string GroupDescription => "Web Road Schedules (View)";

		protected override string RoleName => "webroadschedulesviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
