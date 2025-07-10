using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddWebMessageViewer))]
	public class AddWebMessageViewerTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => false;

		protected override string GroupCode => "WEBMSGVIEW";

		protected override string RoleName => "webmessageviewer";

		protected override string GroupDescription => "Web Message (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddWebMessageViewer();
	}

	[TestedType(typeof(AddWebMessageViewer))]
	public class AddWebMessageViewerSystemGroupTest : AddWebMessageViewerTest
	{
		protected override bool IsSystemGroup => true;
	}

	[TestedType(typeof(AddWebMessageViewer))]
	public class AddWebMessageViewerTestEDI : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBMSGVIEW";

		protected override string RoleName => "webmessageviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddWebMessageViewer();
	}
}

