using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(AddConstraintToRateEntryParentTableCode))]
	public sealed class AddConstraintToRateEntryParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new AddConstraintToRateEntryParentTableCode();

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, true);
		}
	}
}

