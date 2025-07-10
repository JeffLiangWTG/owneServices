using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class MaterialFormDescriptionComponentTest : TestCaseWithFactory
	{
		public void TestMaterialFormDescriptionComponentContainsMaterialFormDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_MaterialFormDescription = "ABCD";
			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new MaterialFormDescriptionComponent() as IUNDGSummaryWriterComponent;
			AssertEquals(component.Write(wrapper), "ABCD");
		}

		public void TestMaterialFormDescriptionComponentDoesNotContainMaterialFormDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_MaterialFormDescription = ZString.Empty;
			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			var component = new MaterialFormDescriptionComponent() as IUNDGSummaryWriterComponent;
			AssertEquals(component.Write(wrapper), ZString.Empty);
			AssertEquals(component.Write(null), ZString.Empty);
		}
	}
}
