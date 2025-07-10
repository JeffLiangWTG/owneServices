namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddNeoEDocUploaderGroup : SystemOrgSecurityGroupTransform
	{
		protected override string GroupCode => "WEBEDOCUPLOAD";

		protected override string GroupDescription => "Web Documents Uploader";

		protected override string RoleName => "webdocumentsuploader";

		protected override bool IsNeoDefaultRole => false;
	}
}
