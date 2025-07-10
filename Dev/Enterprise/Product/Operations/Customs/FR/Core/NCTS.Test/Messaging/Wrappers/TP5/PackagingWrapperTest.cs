using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class PackagingWrapperTest : Customs.Business.Testing.DataProviderTestCase<PackagingWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber should not be mapped at all.", string.Empty, Provider.SequenceNumber);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("TypeOfPackages should be mapped to B5_UnitType.", "CTN", Provider.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("NumberOfPackages should be mapped to NumberOfPackages.", "22", Provider.NumberOfPackages);
		}

		public void TestShippingMarks()
		{
			AssertEquals("ShippingMarks should be mapped to B5_MarksAndNumbers.", "1/1 Explosive", Provider.ShippingMarks);
		}

		protected override PackagingWrapper GetProvider()
		{
			var package =  Factory.New<NctsPackage>();
			package.B5_UnitType = "CTN";
			package.B5_UnitCount = 22;
			package.B5_MarksAndNumbers = "1/1 Explosive";
			package.B5_SequenceNumber = 99;
			return PackagingWrapper.New(package);
		}
	}
}
