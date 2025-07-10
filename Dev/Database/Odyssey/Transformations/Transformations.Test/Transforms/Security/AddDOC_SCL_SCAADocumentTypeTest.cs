using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddDOC_SCL_SCAADocumentType))]
	public class AddDOC_SCL_SCAADocumentTypeTest : AddDocumentTypeToNeoGroupTest
	{
		protected override string DocumentGroup => "SCL";

		protected override string DocumentType => "SCAA";

		protected override string DocumentDescription => "Competent Authority Approval";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddDOC_SCL_SCAADocumentType();
	}
}
