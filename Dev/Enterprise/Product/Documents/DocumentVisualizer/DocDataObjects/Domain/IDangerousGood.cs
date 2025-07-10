using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IDangerousGood
	{
		ZInt Quantity { get; set; }
		ZString Prefix { get; set; }
		ZString Code { get; set; }
		ZString Unno { get; set; }
		ZString Variant { get; set; }
		ZString ProperShippingName { get; set; }
		ZString TechnicalName { get; set; }
		ZString IMOClass { get; set; }
		ZString PackingGroup { get; set; }
		ZString SubLabel1 { get; set; }
		ZString SubLabel2 { get; set; }
		ZString State { get; set; }
		ZString MaterialFormDescription { get; set; }
		ZString RadionuclideElementSuffix { get; set; }
		ZString SpecialPermitNumber { get; set; }
		ZString HazardousWasteCode { get; set; }
		ZString PoisonInhalationHazard { get; set; }
		ZString PSAGroup { get; set; }

		ZBool PackedInLimitedQuantity { get; set; }
		ZBool IsFissileExcepted { get; set; }
		ZBool IsExclusiveUse { get; set; }
		ZBool IsHighwayRouteControlledQuantity { get; set; }
		ZBool IsResidueLastContained { get; set; }
		ZBool RequiresTemperatureControl { get; set; }
		ZBool IsSalvagePackaging { get; set; }

		ZDecimal RadioactiveTransportIndex { get; set; }

		IMeasurement Weight { get; }
		IMeasurement NetExplosiveWeight { get; }
		IMeasurement Volume { get; }
		IMeasurement FlashPoint { get; set; }
		IMeasurement ReportableQuantity { get; set; }
		IMeasurement RequiredTemperatureMaximum { get; set; }
		IMeasurement RadioactiveMaximumActivity { get; set; }

		ICodeDescription PackageType { get; }
		ICodeDescription MarinePollutant { get; }
		ICodeDescription TransportMode { get; set; }
		ICodeDescription EmergencyScheduleFire { get; set; }
		ICodeDescription EmergencyScheduleSpillage { get; set; }
		ICodeDescription ExceptedQuantityCode { get; set; }
		ICodeDescription RadioactiveLabelCategory { get; set; }
		ICodeDescription RadionuclideElement { get; set; }

		IContact Contact { get; }

		ZString Standard { get; set; }
		ZString SecondaryClass { get; set; }
		ZString TertiaryClass { get; set; }
	}
}
