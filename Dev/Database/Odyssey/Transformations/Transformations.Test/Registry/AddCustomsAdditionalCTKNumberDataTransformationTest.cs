using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(AddCustomsAdditionalCTKNumberDataTransformation))]
	public class AddCustomsAdditionalCTKNumberDataTransformationTest : AddCustomsAdditionalNumberDataTransformationTest
	{
		protected override string Code => "CTK";

		protected override string Description => "Cargo Tracking Note";

		protected override bool IsUnique => true;

		protected override bool IsAutomation => false;

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new AddCustomsAdditionalCTKNumberDataTransformation();
		}
	}
}
