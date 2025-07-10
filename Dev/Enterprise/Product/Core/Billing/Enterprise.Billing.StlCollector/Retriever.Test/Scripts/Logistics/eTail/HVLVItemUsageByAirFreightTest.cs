using CargoWise.Billing.Collectors.Logistics;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemUsageByAirFreight))]
	sealed class HVLVItemUsageByAirFreightTest : HVLVItemUsageByTransportModeTest
	{
		protected override bool ShouldIncludeSeaShipments => false;

		protected override bool ShouldIncludeAirShipments => true;

		protected override bool ShouldIncludeRoadShipments => false;

		protected override bool ShouldIncludeOtherTransportModeShipments => false;
	}
}
