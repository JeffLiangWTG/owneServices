using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class PackagingProviderTest : DataProviderTestCase<PackagingProvider>
	{
		#region Public test methods

		public void TestPackageType()
		{
			AssertEquals("PackageType", "PT4", Provider.PackageType);
		}

		public void TestPackageQuantity()
		{
			AssertEquals("PackageQuantity", 110, Provider.PackageQuantity);
		}

		public void TestShippingMarks()
		{
			AssertEquals("ShippingMarks", "Mark4", Provider.ShippingMarks);
		}
		#endregion

		#region Overridings & inherits

		protected override PackagingProvider GetProvider() => new PackagingProvider(type, quantity, shippingMarks);

		readonly string type = "PT4";
		readonly int quantity = 110;
		readonly string shippingMarks = "Mark4";

		#endregion
	}
}
