using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoRailSchedulesViewerGroup))]
	public class AddNeoRilSchedulesViewerGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override string GroupCode => "WEBRAILSCHDVIEW";

		protected override string RoleName => "webrailschedulesviewer";

		protected override string GroupDescription => "Web Rail Schedules (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override bool IsSystemGroup => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoRailSchedulesViewerGroup();
	}

	[TestedType(typeof(AddNeoRailSchedulesViewerGroup))]
	public class AddNeoRailScheduleViewerGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBRAILSCHDVIEW";

		protected override string RoleName => "webrailschedulesviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoRailSchedulesViewerGroup();
	}
}
