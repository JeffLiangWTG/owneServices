using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(PackagingProvider))]
	sealed class PackagingProviderTest : PackagingProviderAbstractTest<PackagingProvider>
	{
		public override void TestTypeOfPackages()
		{
			package.B5_UnitType = "PAK";
			AssertEquals("PAK", Provider.TypeOfPackages);
		}

		public override void TestNumberOfPackages()
		{
			package.B5_UnitCount = 10;
			AssertEquals(10, Provider.NumberOfPackages);
		}

		public override void TestNumberOfPackages_Bulk()
		{
			package.B5_UnitCount = 10;
			package.B5_UnitType = "VG";
			AssertNull(Provider.NumberOfPackages);
		}

		public override void TestShippingMarks()
		{
			package.B5_MarksAndNumbers = "shippingmarks";
			AssertEquals("shippingmarks", Provider.ShippingMarks);
		}
	}
}
