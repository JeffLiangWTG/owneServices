using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class SequencedPackagingProviderTest : DataProviderTestCase<SequencedPackagingProvider>
	{
		public void TestPackageType()
		{
			AssertNull("Package Type", sequencedPackagingProvider.PackageType);
		}

		public void TestPackageQuantity()
		{
			AssertEquals("Package Quantity",10, sequencedPackagingProvider.PackageQuantity);
			AssertEquals("Package Quantity",0, GetProviderWithNullPack().PackageQuantity);
		}

		public void TestShippingMarks()
		{
			AssertEquals("Shipping Marks", "MarksAndNumbers", sequencedPackagingProvider.ShippingMarks);
			AssertNull("Shipping Marks", GetProviderWithNullPack().ShippingMarks);
		}

		public void TestSequenceNumber()
		{
			AssertNull("Sequence Number", sequencedPackagingProvider.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			pack = Factory.New<AsycudaPack>();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "MarksAndNumbers";

			sequencedPackagingProvider = new SequencedPackagingProvider(pack);
		}
		AsycudaPack pack;
		SequencedPackagingProvider sequencedPackagingProvider;

		protected override SequencedPackagingProvider GetProvider()
		{
			return sequencedPackagingProvider;
		}

		SequencedPackagingProvider GetProviderWithNullPack() => new SequencedPackagingProvider(null);
	}
}
