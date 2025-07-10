using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateInspectionCountryCodeForUKShipments))]
	public class UpdateInspectionCountryCodeForUKShipmentsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert("This transformation has been removed", true);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateInspectionCountryCodeForUKShipments();
		}
	}
}
