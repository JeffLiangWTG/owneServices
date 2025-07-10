using CargoWise.Billing.Collectors.Logistics;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemUsageBySeaFreight))]
	sealed class HVLVItemUsageBySeaFreightTest : HVLVItemUsageByTransportModeTest
	{
		protected override bool ShouldIncludeSeaShipments => true;

		protected override bool ShouldIncludeAirShipments => false;

		protected override bool ShouldIncludeRoadShipments => false;

		protected override bool ShouldIncludeOtherTransportModeShipments => false;
	}
}
