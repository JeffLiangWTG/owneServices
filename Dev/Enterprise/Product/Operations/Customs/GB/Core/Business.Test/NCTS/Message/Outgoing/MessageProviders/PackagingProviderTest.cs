using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class PackagingProviderTest : DataProviderTestCase<PackagingProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("PAK", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals(10, Provider.NumberOfPackages);
		}

		public void TestShippingMarks()
		{
			AssertEquals("shippingmarks", Provider.ShippingMarks);
		}

		protected override PackagingProvider GetProvider()
		{
			var package = Factory.New<NctsPackage>();
			package.B5_SequenceNumber = 2;
			package.B5_UnitType = "PAK";
			package.B5_UnitCount = 10;
			package.B5_MarksAndNumbers = "shippingmarks";
			return new PackagingProvider(package);
		}
	}
}
