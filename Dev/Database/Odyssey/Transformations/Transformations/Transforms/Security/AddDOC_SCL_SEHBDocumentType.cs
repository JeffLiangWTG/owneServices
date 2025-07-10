namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddDOC_SCL_SEHBDocumentType : AddDocumentTypeToNeoGroup
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SEHB";

		protected override string DocumentDescription => "Electronic House Bill";
	}
}
