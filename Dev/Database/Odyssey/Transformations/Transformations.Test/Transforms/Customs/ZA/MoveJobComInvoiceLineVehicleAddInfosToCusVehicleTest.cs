using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.ZA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.ZA;

[TestedType(typeof(MoveJobComInvoiceLineVehicleAddInfosToCusVehicle))]
public sealed class MoveJobComInvoiceLineVehicleAddInfosToCusVehicleTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new MoveJobComInvoiceLineVehicleAddInfosToCusVehicle();

	protected override void AssertTransformationResults()
	{
		AssertEquals(true, true);
	}
}
