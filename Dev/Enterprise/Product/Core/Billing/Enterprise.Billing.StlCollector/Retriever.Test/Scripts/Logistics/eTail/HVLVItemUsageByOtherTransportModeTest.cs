using CargoWise.Billing.Collectors.Logistics;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemUsageByOtherTransportMode))]
	sealed class HVLVItemUsageByOtherTransportModeTest : HVLVItemUsageByTransportModeTest
	{
		protected override bool ShouldIncludeSeaShipments => false;

		protected override bool ShouldIncludeAirShipments => false;

		protected override bool ShouldIncludeRoadShipments => false;

		protected override bool ShouldIncludeOtherTransportModeShipments => true;
	}
}
