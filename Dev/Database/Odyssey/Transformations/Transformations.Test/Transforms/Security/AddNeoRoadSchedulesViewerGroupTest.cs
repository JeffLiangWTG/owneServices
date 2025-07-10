using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoRoadSchedulesViewerGroup))]
	public class AddNeoRoadSchedulesViewerGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override string GroupCode => "WEBROADSCHDVIEW";

		protected override string RoleName => "webroadschedulesviewer";

		protected override string GroupDescription => "Web Road Schedules (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override bool IsSystemGroup => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoRoadSchedulesViewerGroup();
	}

	[TestedType(typeof(AddNeoRoadSchedulesViewerGroup))]
	public class AddNeoRoadSchedulesViewerGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBROADSCHDVIEW";

		protected override string RoleName => "webroadschedulesviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoRoadSchedulesViewerGroup();
	}
}
