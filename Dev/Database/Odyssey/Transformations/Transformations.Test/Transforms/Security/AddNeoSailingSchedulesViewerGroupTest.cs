using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoSailingSchedulesViewerGroup))]
	public class AddNeoSailingSchedulesViewerGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override string GroupCode => "WEBSEASCHDVIEW";

		protected override string RoleName => "websailingschedulesviewer";

		protected override string GroupDescription => "Web Sailing Schedules (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override bool IsSystemGroup => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoSailingSchedulesViewerGroup();
	}

	[TestedType(typeof(AddNeoSailingSchedulesViewerGroup))]
	public class AddNeoSailingSchedulesViewerGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBSEASCHDVIEW";

		protected override string RoleName => "websailingschedulesviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoSailingSchedulesViewerGroup();
	}
}
