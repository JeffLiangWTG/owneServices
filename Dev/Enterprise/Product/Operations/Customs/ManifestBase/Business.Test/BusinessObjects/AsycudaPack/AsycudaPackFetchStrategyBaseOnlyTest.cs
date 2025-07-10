using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	sealed class AsycudaPackFetchStrategyBaseOnlyTest : TestCaseWithFactory
	{
		/// <summary>No support for PackedItems</summary>
		public void TestClusterKeyFetchHintsNotAddedForRelationshipTypeNone()
		{
			var pack = SetupAndPersistDataForPack();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			var strategy = new AsycudaPackFetchStrategy(pack);
			CombineAssertions(() =>
			{
				strategy.AddFetchHintsForLoadChildEditableObjects();
				AssertEquals("AddFetchHintsForLoadChildEditableObjects", 0, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForValidate();
				AssertEquals("AddFetchHintsForValidate", 0, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForDelete();
				AssertEquals("AddFetchHintsForDelete", 0, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
			});
		}

		/// <summary>Support Multiple Packed Item per pack, All PackedItemPivot's loaded on cluster key so a single hit</summary>
		public void TestClusterKeyFetchHintsUsedForRelationshipTypeMany()
		{
			var pack = SetupAndPersistDataForPack();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var strategy = new AsycudaPackFetchStrategy(pack);
			CombineAssertions(() =>
			{
				strategy.AddFetchHintsForLoadChildEditableObjects();
				AssertEquals("AddFetchHintsForLoadChildEditableObjects", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForValidate();
				AssertEquals("AddFetchHintsForValidate", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForDelete();
				AssertEquals("AddFetchHintsForDelete", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
			});
		}

		/// <summary>Support One Packed Item per pack, All PackedItemPivot's loaded on cluster key so a single hit</summary>
		public void TestClusterKeyFetchHintsUsedForRelationshipTypeOne()
		{
			var pack = SetupAndPersistDataForPack();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var strategy = new AsycudaPackFetchStrategy(pack);
			CombineAssertions(() =>
			{
				strategy.AddFetchHintsForLoadChildEditableObjects();
				AssertEquals("AddFetchHintsForLoadChildEditableObjects", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForValidate();
				AssertEquals("AddFetchHintsForValidate", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
				strategy.AddFetchHintsForDelete();
				AssertEquals("AddFetchHintsForDelete", 1, newFactory.GetTableHitCount(AsycudaPackPackedItemPivot.Schema.TableName));
			});
		}

		AsycudaPack SetupAndPersistDataForPack()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ASD32423";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			Factory.Save();
			return pack;
		}
	}
}
