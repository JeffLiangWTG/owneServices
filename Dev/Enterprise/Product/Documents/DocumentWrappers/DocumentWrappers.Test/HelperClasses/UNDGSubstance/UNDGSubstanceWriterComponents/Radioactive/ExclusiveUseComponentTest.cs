using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ExclusiveUseComponentTest : TestCaseWithFactory
	{
		public void TestExclusiveUseComponent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_IsExclusiveUse = true;

			AssertEquals("Exclusive Use", GetComponentText(dataItem));

			dataItem.DI_IsExclusiveUse = false;
			AssertEquals(string.Empty, GetComponentText(dataItem));
		}

		string GetComponentText(UNDGDataItem dataItem)
		{
			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			var component = new ExclusiveUseComponent() as IUNDGSummaryWriterComponent;
			return component.Write(wrapper);
		}
	}
}
