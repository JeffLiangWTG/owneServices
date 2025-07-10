using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class FissileExceptedComponentTest : TestCaseWithFactory
	{
		public void TestFissileExceptedComponentContainsFissileExcepted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_IsFissileExcepted = true;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new FissileExceptedComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), "Fissile Excepted");
		}

		public void TestFissileExceptedComponentDoesNotContainFissileExcepted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_IsFissileExcepted = false;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new FissileExceptedComponent() as IUNDGSummaryWriterComponent;

			AssertEquals(component.Write(wrapper), ZString.Empty);

			AssertEquals(component.Write(null), ZString.Empty);
		}
	}
}
