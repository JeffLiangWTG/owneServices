using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddDOC_SCL_SCLNDocumentType))]
	public class AddDOC_SCL_SCLNDocumentTypeTest : AddDocumentTypeToNeoGroupTest
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCLN";

		protected override string DocumentDescription => "Classification Notes";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddDOC_SCL_SCLNDocumentType();
	}
}
