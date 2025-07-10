using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddDOC_SCL_SCTKDocumentType))]
	public class AddDOC_SCL_SCTKDocumentTypeTest : AddDocumentTypeToNeoGroupTest
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCTK";

		protected override string DocumentDescription => "Cargo Tracking Note";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddDOC_SCL_SCTKDocumentType();
	}
}
