using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class HRCQComponentTest : TestCaseWithFactory
	{
		public void TestHRCQComponentContainsHRCQ()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_IsHighwayRouteControlledQuantity = true;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new HRCQComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), "HRCQ");
		}

		public void TestHRCQComponentDoesNotContainHRCQ()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_IsHighwayRouteControlledQuantity = false;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new HRCQComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), ZString.Empty);

			AssertEquals(component.Write(null), ZString.Empty);
		}
	}
}
