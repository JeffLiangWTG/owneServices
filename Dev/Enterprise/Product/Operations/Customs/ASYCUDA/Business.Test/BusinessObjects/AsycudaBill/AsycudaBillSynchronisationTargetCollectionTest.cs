using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaBillSynchronisationTargetCollectionTest : TestCaseWithFactory
	{
		public void TestAsycudaBillSynchronisationTargetCollection()
		{
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			manifestHeader.FillWithValidTestData();
			var bills = manifestHeader.Bills;
			var bill = bills.AddNew();

			AssertNoExceptionThrown(() =>
			{
				var synchronisationTargetCollection = (ISailingSynchronisationTargetCollection<BillOfLading, AsycudaBill>)new AsycudaBillSynchronisationTargetCollection(bills);

				using (var enumerator = synchronisationTargetCollection.GetEnumerator())
				{
					var current = enumerator.Current;
				}

				synchronisationTargetCollection.Delete(bill);

				AssertCollectionNotContains("Should be deleted.", bill, bills);
				Assert("Should be deleted.", bill.IsDeleted);

				var newBill = synchronisationTargetCollection.AddNew();

				AssertCollectionContains("Should add a new bill in the target collection.", newBill, bills);
			});
		}
	}
}
