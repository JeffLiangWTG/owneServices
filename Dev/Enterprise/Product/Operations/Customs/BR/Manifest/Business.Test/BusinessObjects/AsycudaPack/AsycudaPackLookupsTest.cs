using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBulkTypes()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var asycudaPack = header.Bills.AddNew().Packs.AddNew();
			var bulkTypesList = asycudaPack.Lookups.BulkTypes;
			AssertEquals(11, bulkTypesList.Count);
			AssertContainsExactElementsInExactOrder(expectedBulkTypes, bulkTypesList.GetAllCodes());
		}

		static readonly string[] expectedBulkTypes = new string[] { "1", "10", "2", "3", "4", "5", "6", "7", "8", "9", "99" };
	}
}
