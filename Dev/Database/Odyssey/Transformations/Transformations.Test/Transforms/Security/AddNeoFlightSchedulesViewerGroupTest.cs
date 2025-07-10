using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoFlightSchedulesViewerGroup))]
	public class AddNeoFlightSchedulesViewerGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override string GroupCode => "WEBAIRSCHDVIEW";

		protected override string RoleName => "webflightschedulesviewer";

		protected override string GroupDescription => "Web Flight Schedules (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override bool IsSystemGroup => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoFlightSchedulesViewerGroup();
	}

	[TestedType(typeof(AddNeoFlightSchedulesViewerGroup))]
	public class AddNeoFlightSchedulesViewerGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBAIRSCHDVIEW";

		protected override string RoleName => "webflightschedulesviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoFlightSchedulesViewerGroup();
	}
}
