using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddNeoEDocUploaderGroup))]
	public class AddNeoEDocUploaderGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => true;
		protected override string GroupCode => "WEBEDOCUPLOAD";
		protected override string GroupDescription => "Web Documents Uploader";
		protected override string RoleName => "webdocumentsuploader";
		protected override bool IsNeoDefaultRole => false;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoEDocUploaderGroup();
	}

	[TestedType(typeof(AddNeoEDocUploaderGroup))]
	public class AddNeoEDocUploaderGroupEDITest : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBEDOCUPLOAD";

		protected override string RoleName => "webdocumentsuploader";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddNeoEDocUploaderGroup();
	}
}
