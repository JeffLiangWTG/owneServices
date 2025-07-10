using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddDOC_SCL_SEHBDocumentType))]
	public class AddDOC_SCL_SEHBDocumentTypeTest : AddDocumentTypeToNeoGroupTest
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SEHB";

		protected override string DocumentDescription => "Electronic House Bill";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddDOC_SCL_SEHBDocumentType();
	}
}
