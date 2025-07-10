using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemEntryNumCollection))]
	sealed class AsycudaPackedItemEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCorrectRelationshipSettings()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var headerCountry = header.AMA_RN_NKCountry;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem = pack.CreatePackedItemForTesting();
			var collection = new AsycudaPackedItemEntryNumCollection(packedItem);
			var item = collection.AddNew();
			AssertEquals("item.CE_ParentID", packedItem.PK, item.CE_ParentID);
			AssertEquals("item.CE_ParentTable", AsycudaPackedItemSchema.Constants.TableName, item.CE_ParentTable);
			AssertEquals("item.CE_RN_NKCountryCode", headerCountry, item.CE_RN_NKCountryCode);
			AssertEquals("item.Parent", packedItem, item.Parent);
			item.Delete();

			item = Factory.New<AsycudaPackedItemEntryNum>();
			collection.Add(item);
			AssertEquals("item.CE_ParentID", packedItem.PK, item.CE_ParentID);
			AssertEquals("item.CE_ParentTable", AsycudaPackedItemSchema.Constants.TableName, item.CE_ParentTable);
			AssertEquals("item.CE_RN_NKCountryCode", headerCountry, item.CE_RN_NKCountryCode);
			AssertEquals("item.Parent", packedItem, item.Parent);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			packedItem = newFactory.Load<AsycudaPackedItem>(packedItem.PK);
			collection = packedItem.CustomsEntryNumbers;
			AssertEquals(1, collection.Count);
			item = collection[0];
			AssertEquals("item.CE_ParentID", packedItem.PK, item.CE_ParentID);
			AssertEquals("item.CE_ParentTable", AsycudaPackedItemSchema.Constants.TableName, item.CE_ParentTable);
			AssertEquals("item.CE_RN_NKCountryCode", headerCountry, item.CE_RN_NKCountryCode);
			AssertEquals("item.Parent", packedItem, item.Parent);

			var item2 = Factory.New<AsycudaPackedItemEntryNum>();
			item2.CE_ParentTable = AsycudaPackedItemSchema.Constants.TableName;
			item2.CE_ParentID = packedItem.PK;
			item2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			collection.Load();
			AssertEquals(1, collection.Count);
			item = collection[0];
			AssertEquals("item.CE_ParentID", packedItem.PK, item.CE_ParentID);
			AssertEquals("item.CE_ParentTable", AsycudaPackedItemSchema.Constants.TableName, item.CE_ParentTable);
			AssertEquals("item.CE_RN_NKCountryCode", headerCountry, item.CE_RN_NKCountryCode);
			AssertEquals("item.Parent", packedItem, item.Parent);

			AssertEquals("item2.CE_ParentID", packedItem.PK, item2.CE_ParentID);
			AssertEquals("item2.CE_ParentTable", AsycudaPackedItemSchema.Constants.TableName, item2.CE_ParentTable);
			AssertEquals("item2.CE_RN_NKCountryCode", Core.Constants.CountryCodes.NewZealand, item2.CE_RN_NKCountryCode);
			AssertEquals("item2.Parent.PK", packedItem.PK, item2.Parent.PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.CreatePackedItemForTesting();
			return new AsycudaPackedItemEntryNumCollection(packedItem);
		}
	}
}
