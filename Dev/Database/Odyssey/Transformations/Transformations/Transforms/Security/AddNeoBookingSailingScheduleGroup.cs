namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoBookingSailingScheduleGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBBKGSCHEDULE";
		protected override string GroupDescription => "Web Booking (Select Schedules)";
		protected override string RoleName => "WebBookingScheduleSelector";
		protected override bool IsNeoDefaultRole => false;
	}
}
