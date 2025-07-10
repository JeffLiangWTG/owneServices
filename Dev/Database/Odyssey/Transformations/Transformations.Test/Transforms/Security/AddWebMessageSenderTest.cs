using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddWebMessageSender))]
	public class AddWebMessageSenderTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => false;

		protected override string GroupCode => "WEBMSGSEND";

		protected override string RoleName => "webmessagesender";

		protected override string GroupDescription => "Web Message (Send)";

		protected override bool IsNeoDefaultRole => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddWebMessageSender();
	}

	[TestedType(typeof(AddWebMessageSender))]
	public class AddWebMessageSenderSystemGroupTest : AddWebMessageSenderTest
	{
		protected override bool IsSystemGroup => true;
	}

	[TestedType(typeof(AddWebMessageSender))]
	public class AddWebMessageSenderTestEDI : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBMSGSEND";

		protected override string RoleName => "webmessagesender";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddWebMessageSender();
	}
}
