using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartonGroupSequence))]
	sealed class CartonGroupSequenceTest : RegistryBusinessObjectTemplateTestCase<CartonGroupSequence>
	{
		public void TestDefaultValues()
		{
			AssertEquals("Product", (ZByte)1, BizObj.Product);
			AssertEquals("Carrier", (ZByte)2, BizObj.Carrier);
			AssertEquals("Consignee", (ZByte)3, BizObj.Consignee);
			AssertEquals("Client", (ZByte)4, BizObj.Client);
			AssertEquals("Warehouse", (ZByte)5, BizObj.Warehouse);
		}

		public void TestDefaultValues_Constructor()
		{
			var cartonGroupSequence = new CartonGroupSequence();
			AssertEquals("Product", (ZByte)1, cartonGroupSequence.Product);
			AssertEquals("Carrier", (ZByte)2, cartonGroupSequence.Carrier);
			AssertEquals("Consignee", (ZByte)3, cartonGroupSequence.Consignee);
			AssertEquals("Client", (ZByte)4, cartonGroupSequence.Client);
			AssertEquals("Warehouse", (ZByte)5, cartonGroupSequence.Warehouse);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Product = 9;
			BizObj.Carrier = 9;
			BizObj.Consignee = 9;
			BizObj.Client = 9;
			BizObj.Warehouse = 9;

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();

			AssertHasErrors("Please enter a 'Product' within the range 0 to 5.", BizObj.ProductInfo);
			AssertHasErrors("Please enter a 'Carrier' within the range 0 to 5.", BizObj.CarrierInfo);
			AssertHasErrors("Please enter a 'Consignee' within the range 0 to 5.", BizObj.ConsigneeInfo);
			AssertHasErrors("Please enter a 'Client' within the range 0 to 5.", BizObj.ClientInfo);
			AssertHasErrors("Please enter a 'Warehouse' within the range 0 to 5.", BizObj.WarehouseInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CartonGroupSequence GetBusinessObjectToClone()
		{
			return new CartonGroupSequence()
			{
				Product = 2,
				Warehouse = 3,
				Client = 4,
				Consignee = 5,
				Carrier = 1,
			};
		}

		protected override CartonGroupSequence GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
