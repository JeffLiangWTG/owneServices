using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(UpdateCarrierShipmentCargoRoutingLegCRG_TransportModeValue))]
sealed class UpdateCarrierShipmentCargoRoutingLegCRG_TransportModeValueTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCarrierShipmentCargoRoutingLegCRG_TransportModeValue();
}
