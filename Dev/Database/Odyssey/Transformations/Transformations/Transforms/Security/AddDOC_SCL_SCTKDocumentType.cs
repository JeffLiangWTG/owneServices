namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddDOC_SCL_SCTKDocumentType : AddDocumentTypeToNeoGroup
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCTK";

		protected override string DocumentDescription => "Cargo Tracking Note";
	}
}
