using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPackFetchStrategy))]
	sealed class AsycudaPackFetchStrategyTest : TestCaseWithFactory
	{
		public void TestAddCommonFetchHints_DoesNotAddFetchHintForAsycudaPackedItem()
		{
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var pack = bill.Packs.AddNew();

				var packedItem = bill.PackedItems.AddNew();
				var packPackedItemPivot = pack.PackedItems.AddNew();
				packPackedItemPivot.APP_API_Item = packedItem.PK;

				Factory.Save();

				var fetchStrage = new AsycudaPackFetchStrategy(pack);
				fetchStrage.AddFetchHintsForValidate();
				AssertCollectionNotContains(AsycudaPackedItem.Schema.TableName, Factory.GetAllFetchHintedTableNames());
			}
		}
	}
}
