using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.Security.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security;

[TestedType(typeof(DenyAccessUnpublishedCustomizedDocumentsAndReportsByDefault))]
class DenyAccessUnpublishedCustomizedDocumentsAndReportsByDefaultTest : DenyRootSecurityRightsByDefaultForGroupsTest
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new DenyAccessUnpublishedCustomizedDocumentsAndReportsByDefault();
	}
}