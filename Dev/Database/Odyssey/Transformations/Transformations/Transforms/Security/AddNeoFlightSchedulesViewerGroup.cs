namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoFlightSchedulesViewerGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBAIRSCHDVIEW";

		protected override string GroupDescription => "Web Flight Schedules (View)";

		protected override string RoleName => "webflightschedulesviewer";

		protected override bool IsNeoDefaultRole => true;
	}
}
