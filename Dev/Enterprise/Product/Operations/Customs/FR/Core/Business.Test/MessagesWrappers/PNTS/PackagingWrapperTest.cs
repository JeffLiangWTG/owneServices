using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class PackagingWrapperTest : Customs.Business.Testing.DataProviderTestCase<PackagingWrapper>
	{
		public void TestNumberOfPackages()
		{
			AssertEquals("Wrapper NumberOfPackages should equal packaging APA_PackQty.", "3", Provider.NumberOfPackages);
		}

		public void TestShippingMarks()
		{
			AssertEquals("Wrapper ShippingMarks should equal packaging APA_MarksAndNumbers.", "Marks & numbers", Provider.ShippingMarks);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("Wrapper TypeOfPackages should equal packaging APA_PackUQ.", "CTN", Provider.TypeOfPackages);
		}

		protected override PackagingWrapper GetProvider()
		{
			var packaging = Factory.New<AsycudaPack>();
			packaging.APA_PackQty = 3;
			packaging.APA_MarksAndNumbers = "Marks & numbers";
			packaging.APA_PackUQ = "CTN";
			return PackagingWrapper.New(packaging);
		}
	}
}
