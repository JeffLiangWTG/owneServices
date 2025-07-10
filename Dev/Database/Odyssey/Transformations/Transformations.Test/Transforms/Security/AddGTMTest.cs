using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoGTMGroup))]
	public class AddGTMUserGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => false;

		protected override string GroupCode => "WEBGTMVIEW";

		protected override string RoleName => "webgtmviewer";

		protected override string GroupDescription => "Web GTM (View)";

		protected override bool IsNeoDefaultRole => false;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoGTMGroup();
	}

	[TestedType(typeof(AddNeoGTMGroup))]
	public class AddGTMSystemGroupTest : AddGTMUserGroupTest
	{
		protected override bool IsSystemGroup => true;
	}

	[TestedType(typeof(AddNeoGTMGroup))]
	public class AddGTMTestEDI : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBGTMVIEW";

		protected override string RoleName => "webgtmviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoGTMGroup();
	}
}
