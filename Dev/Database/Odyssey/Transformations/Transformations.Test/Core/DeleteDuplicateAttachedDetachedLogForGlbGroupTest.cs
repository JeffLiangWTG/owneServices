using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing;

[TestedType(typeof(DeleteDuplicateAttachedDetachedLogForGlbGroup))]
public class DeleteDuplicateAttachedDetachedLogForGlbGroupTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new DeleteDuplicateAttachedDetachedLogForGlbGroup();
	}

	protected override void AssertTransformationResults()
	{
		Assert(true);
	}
}