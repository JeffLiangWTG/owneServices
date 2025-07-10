using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class SealProviderTest : DataProviderTestCase<SealProvider>
	{
		public void TestSealNumber()
		{
			AssertEquals("5", GetProvider().SealNumber);
		}

		public void TestSealIds()
		{
			AssertContainsExactElementsInExactOrder(new[] { "1", "2", "3", "4", "5" }, GetProvider().SealIds);
		}

		protected override SealProvider GetProvider()
		{
			if (header is null)
			{
				header = Factory.New<TemporaryStorageHeader>();
				var container1 = header.Containers.AddNew();
				container1.ACN_Seal1 = "2";
				container1.ACN_Seal2 = "4";
				var container2 = header.Containers.AddNew();
				container2.ACN_Seal3 = "1";
				var container3 = header.Containers.AddNew();
				container3.AdditionalSeals.AddNew().BK_SealNumber = "3";
				container3.AdditionalSeals.AddNew().BK_SealNumber = "5";
				container3.AdditionalSeals.AddNew().BK_SealNumber = "";
			}
			return new SealProvider(header);
		}

		TemporaryStorageHeader header;
	}
}
