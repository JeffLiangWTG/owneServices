using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackSynchronisationTargetCollectionTest : TestCaseWithFactory
	{
		public void TestAsycudaPackSynchronisationTargetCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			AssertNoExceptionThrown(() =>
			{
				var synchronisationTargetCollection = (ISailingSynchronisationTargetCollection<BillOfLadingPackLine, AsycudaPack>)new AsycudaPackSynchronisationTargetCollection(bill.Packs);

				using (var enumerator = synchronisationTargetCollection.GetEnumerator())
				{
					enumerator.MoveNext();
					AssertSame(pack, enumerator.Current);
				}

				synchronisationTargetCollection.Delete(pack);
				AssertCollectionNotContains("Should be deleted.", pack, bill.Packs);
				Assert("Should be deleted.", pack.IsDeleted);

				var newPack = synchronisationTargetCollection.AddNew();
				AssertCollectionContains("Should add a new pack to the target collection.", newPack, bill.Packs);
			});
		}
	}
}
