using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>))]
	sealed class AsycudaPackPackedItemPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var pack = Factory.New<AsycudaPack>();
			return new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(pack);
		}

		public void TestGetRelatedPivot()
		{
			var pack = Factory.New<AsycudaPack>();
			var pack2 = Factory.New<AsycudaPack>();
			var packedItem = Factory.New<AsycudaPackedItem>();
			var asycudaPackPackedItemPivotCollection = new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(packedItem);

			var pivot = asycudaPackPackedItemPivotCollection.AddNew();

			pivot.APP_APA_Pack = pack.PK;

			CombineAssertions(() =>
			{
				AssertNull("Null should be return if the parameter in GetRelatedPivot is null", asycudaPackPackedItemPivotCollection.GetRelatedPivot(null));
				AssertEquals("There should be a pivot corresponding to pack.", pivot.APP_APA_Pack, asycudaPackPackedItemPivotCollection.GetRelatedPivot(pack).APP_APA_Pack);
				AssertNull("There should be not pivot corresponding to pack2.", asycudaPackPackedItemPivotCollection.GetRelatedPivot(pack2));
			});
		}

		public void TestAddPivotFor()
		{
			var pack = Factory.New<AsycudaPack>();
			var pack2 = Factory.New<AsycudaPack>();
			var packedItem = Factory.New<AsycudaPackedItem>();
			var asycudaPackPackedItemPivotCollection = new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(packedItem);

			AssertEquals("Prerequisite: no pivot in collection.", 0, asycudaPackPackedItemPivotCollection.Count);

			CombineAssertions(() =>
			{
				AssertEquals("There should be a pivot corresponding to pack.", pack.PK, asycudaPackPackedItemPivotCollection.AddPivotFor(pack).APP_APA_Pack);
				AssertEquals("A pivot should have been added.", 1, asycudaPackPackedItemPivotCollection.Count);
				AssertEquals("Same Pivot should be use for the same pack.", pack.PK, asycudaPackPackedItemPivotCollection.AddPivotFor(pack).APP_APA_Pack);
				AssertEquals("Same Pivot should be use for the same pack, count hasn't change.", 1, asycudaPackPackedItemPivotCollection.Count);
				AssertEquals("Same Pivot should be use for the same pack.", pack2.PK, asycudaPackPackedItemPivotCollection.AddPivotFor(pack2).APP_APA_Pack);
				AssertEquals("A pivot should have been added.", 2, asycudaPackPackedItemPivotCollection.Count);
			});
		}

		public void TestDeletePivotFor()
		{
			var pack = Factory.New<AsycudaPack>();
			var pack2 = Factory.New<AsycudaPack>();
			var packedItem = Factory.New<AsycudaPackedItem>();
			var asycudaPackPackedItemPivotCollection = new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(packedItem);

			AssertEquals("Prerequisite: no pivot in collection.", 0, asycudaPackPackedItemPivotCollection.Count);

			AssertNoExceptionThrown("there should be no exception even when we want to delete pivot in a empty collection.", () => asycudaPackPackedItemPivotCollection.DeletePivotFor(pack));

			var pivot1 = asycudaPackPackedItemPivotCollection.AddPivotFor(pack);
			var pivot2 = asycudaPackPackedItemPivotCollection.AddPivotFor(pack2);
			AssertEquals("2 pivots should have been added.", 2, asycudaPackPackedItemPivotCollection.Count);

			asycudaPackPackedItemPivotCollection.DeletePivotFor(pack);

			CombineAssertions(() =>
			{
				AssertEquals("There should be only 1 pivot left.", 1, asycudaPackPackedItemPivotCollection.Count);
				AssertEquals("There should be a pivot corresponding to pack2.", pivot2.APP_APA_Pack, asycudaPackPackedItemPivotCollection.GetRelatedPivot(pack2).APP_APA_Pack);
				AssertNull("There should be not pivot corresponding to pack.", asycudaPackPackedItemPivotCollection.GetRelatedPivot(pack));
			});
		}
	}
}
