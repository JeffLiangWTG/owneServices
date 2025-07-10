using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

class UpdateCarrierShipmentCargoRoutingLegCRG_TransportModeValue : DataTransformation
{
	// the table CarrierShipmentCargoRoutingLeg has been removed, therefor there is nothing more to fix
	// the table CarrierShipmentRouteLeg is a replacement for it (not a rename) and will start with empty data and correct constraint, therefor there is nothing more to fix
	// we keep the transition to avoid side effects, it will be cleaned up when it's old enough
	public override string UserDescription => "Convert CRG_TransportMode values of 'ITW' to 'IWT' (Obsolete)";
}
