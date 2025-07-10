using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Testing
{
	[NUnit.Framework.TestedType(typeof(RenameOrgRelatedPartyPR_CSAStatusToPR_CustomsStatus))]
	class RenameOrgRelatedPartyPR_CSAStatusToPR_CustomsStatusTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RenameOrgRelatedPartyPR_CSAStatusToPR_CustomsStatus();
	}
}
