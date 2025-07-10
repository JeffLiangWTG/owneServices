using CargoWise.Billing.Collectors.Logistics;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemUsageByRoadFreight))]
	sealed class HVLVItemUsageByRoadFreightTest : HVLVItemUsageByTransportModeTest
	{
		protected override bool ShouldIncludeSeaShipments => false;

		protected override bool ShouldIncludeAirShipments => false;

		protected override bool ShouldIncludeRoadShipments => true;

		protected override bool ShouldIncludeOtherTransportModeShipments => false;
	}
}
