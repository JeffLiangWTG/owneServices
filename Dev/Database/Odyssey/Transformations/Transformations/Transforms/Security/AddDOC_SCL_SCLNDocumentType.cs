namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddDOC_SCL_SCLNDocumentType : AddDocumentTypeToNeoGroup
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCLN";

		protected override string DocumentDescription => "Classification Notes";
	}
}
