using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(AddCustomsAdditionalCMRNumberDataTransformation))]
	public class AddCustomsAdditionalCMRNumberDataTransformationTest : AddCustomsAdditionalNumberDataTransformationTest
	{
		protected override string Code => "CMR";

		protected override string Description => "Carrier Message Reference";

		protected override bool IsUnique => true;

		protected override bool IsAutomation => false;

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new AddCustomsAdditionalCMRNumberDataTransformation();
		}
	}
}
