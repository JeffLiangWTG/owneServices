namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddDOC_SCL_SCAADocumentType : AddDocumentTypeToNeoGroup
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCAA";

		protected override string DocumentDescription => "Competent Authority Approval";
	}
}
