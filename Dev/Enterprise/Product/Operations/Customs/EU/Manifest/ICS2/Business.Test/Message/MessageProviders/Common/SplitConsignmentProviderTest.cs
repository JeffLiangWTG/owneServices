using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SplitConsignmentProviderTest : DataProviderTestCase<SplitConsignmentProvider>
	{
		public void TestNewOrNull()
		{
			var validHeader = Factory.New<AsycudaManifestHeader>();
			var provider = SplitConsignmentProvider.NewOrNull(validHeader);
			AssertNotNull("Provider should not be null for valid header", provider);

			provider = SplitConsignmentProvider.NewOrNull(null);
			AssertNull("Provider should be null for invalid header", provider);
		}

		public void TestSplitConsignmentIndicator()
		{
			AssertEquals("1", Provider.SplitConsignmentIndicator);

			header.SplitConsignmentIndicator = false;
			AssertEquals("0", Provider.SplitConsignmentIndicator);
		}

		public void TestPreviousMRN()
		{
			AssertEquals("PreviousMRN", header.PreviousMRN, "TestMRN");
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.PreviousMRN = "TestMRN";
			header.SplitConsignmentIndicator = true;
			var bill = header.Bills.AddNew();
		}
		AsycudaManifestHeader header;

		protected sealed override SplitConsignmentProvider GetProvider() => SplitConsignmentProvider.NewOrNull(header);
	}
}
