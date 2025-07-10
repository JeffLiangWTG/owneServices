using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoBookingSailingScheduleGroup))]
	public class AddNeoBookingSailingScheduleGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => true;
		protected override string GroupCode => "WEBBKGSCHEDULE";
		protected override string GroupDescription => "Web Booking (Select Schedules)";
		protected override string RoleName => "WebBookingScheduleSelector";
		protected override bool IsNeoDefaultRole => false;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoBookingSailingScheduleGroup();
	}

	[TestedType(typeof(AddNeoBookingSailingScheduleGroup))]
	public class AddNeoBookingSailingScheduleGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBBKGSCHEDULE";

		protected override string RoleName => "WebBookingScheduleSelector";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoBookingSailingScheduleGroup();
	}
}
