using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(WebShipmentDeliver))]
	sealed class WebShipmentDeliverTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				30, typeof(WebShipmentCustom).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			WebShipmentDeliver deliver = new WebShipmentDeliver();
			AssertEquals(false, deliver.IsSpecified);

			deliver.Available = ZDateTime.Now;
			AssertEquals(true, deliver.IsSpecified);
			deliver.Available = ZDateTime.Empty;

			deliver.StorageCommences = ZDateTime.Now;
			AssertEquals(true, deliver.IsSpecified);
			deliver.StorageCommences = ZDateTime.Empty;

			deliver.DeliveryFrom = ZDateTime.Now;
			AssertEquals(true, deliver.IsSpecified);
			deliver.DeliveryFrom = ZDateTime.Empty;

			deliver.DeliveryRequiredBy = ZDateTime.Now;
			AssertEquals(true, deliver.IsSpecified);
			deliver.DeliveryRequiredBy = ZDateTime.Empty;

			deliver.CartageAdvised = ZDateTime.Now;
			AssertEquals(true, deliver.IsSpecified);
			deliver.CartageAdvised = ZDateTime.Empty;
		}
	}
}
