using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Module.Testing;

[TestedType(typeof(AEManifestBillModuleCollection))]
sealed class AEManifestBillModuleCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestFetchForView()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		bill.ABL_SplitBillNumber = "123456";
		Factory.Save();

		CombineAssertions(() =>
		{
			var factory = new BusinessObjectFactory();
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaBill)))
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaManifestHeader)))
			{
				var collection = new AEManifestBillModuleCollection(factory);
				collection.Load();
				AssertEquals(1, collection.Count);
				AssertEquals("1 extrra hint", 1, factory.ActiveTableFetchHints);
				AssertEquals("no table hint", 0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				var strategy = new AEManifestBillModuleCollectionFetchStrategy(collection);

				var billColumn = new TableColumn("", "Header+ABL_SplitBillNumber");
				var noBillColumn = new TableColumn("", "ABL_SplitBillNumber");
				var count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { billColumn });
				AssertEquals("Add 1 fetch hint for GenAddOnColumn", count, factory.ActiveTableFetchHints);
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals("1 extra hint", 1, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));

				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { noBillColumn });
				AssertEquals("Add 1 fetch hint for GenAddOnColumn.", count, factory.ActiveTableFetchHints);
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals("one more hint", 2, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
			}
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new AEManifestBillModuleCollection(Factory);
}
