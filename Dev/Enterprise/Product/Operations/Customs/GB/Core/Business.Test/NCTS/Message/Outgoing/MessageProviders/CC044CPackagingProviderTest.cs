using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class CC044CPackagingProviderTest : DataProviderTestCase<CC044CPackagingProvider>
	{
		public void TestSequenceNumber()
		{
			package.B5_SequenceNumber = 2;
			package.B5_TypeOfDifference = "NEW";
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfPackages()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = "NEW";
			AssertEquals("KG", Provider.TypeOfPackages);
		}

		public void TestTypeOfPackages_Conditional()
		{
			package.B5_UnitType = "KG";
			package.B5_TypeOfDifference = "MIS";
			AssertNullOrEmpty(Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			package.B5_UnitCount = 5;
			package.B5_TypeOfDifference = "NEW";
			AssertEquals(5, Provider.NumberOfPackages);
		}

		public void TestNumberOfPackages_Conditional()
		{
			package.B5_UnitCount = 5;
			package.B5_TypeOfDifference = "MIS";
			AssertEquals(0, Provider.NumberOfPackages);
		}

		public void TestShippingMarks()
		{
			package.B5_MarksAndNumbers = "marks";
			package.B5_TypeOfDifference = "NEW";
			AssertEquals("marks", Provider.ShippingMarks);
		}

		public void TestShippingMarks_Conditional()
		{
			package.B5_MarksAndNumbers = "marks";
			package.B5_TypeOfDifference = "MIS";
			AssertNullOrEmpty(Provider.ShippingMarks);
		}

		protected override CC044CPackagingProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			package = Factory.New<NctsPackage>();
			provider = new CC044CPackagingProvider(package);
		}

		CC044CPackagingProvider provider;
		NctsPackage package;
	}
}
