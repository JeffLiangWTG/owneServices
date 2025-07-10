using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Workflow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Workflow
{
	[TestedType(typeof(RemoveProcessHeaderWithG2ParentTableCode))]
	public class RemoveProcessHeaderWithG2ParentTableCodeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			// WI00890102: Assertion removed as we emptied the transform method body
		}

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new RemoveProcessHeaderWithG2ParentTableCode();
	}
}
