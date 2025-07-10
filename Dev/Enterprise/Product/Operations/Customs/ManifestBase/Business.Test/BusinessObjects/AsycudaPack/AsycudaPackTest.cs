using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackedItemRelationship()
		{
			var pack = Factory.New<AsycudaPack>();
			AssertEquals("IsManyPackedItemRelationship", false, pack.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", true, pack.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, pack.IsOnePackedItemRelationship);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			AssertEquals("IsManyPackedItemRelationship", false, pack.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, pack.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", true, pack.IsOnePackedItemRelationship);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("IsManyPackedItemRelationship", true, pack.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, pack.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, pack.IsOnePackedItemRelationship);
		}

		public void TestAddingOnePackedItemRelationshipPackShouldCreatingPackedItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("pack.PackedItems.Count", 1, pack.PackedItems.Count);
			AssertNotNull("pack.PackedItem", pack.PackedItem);
		}

		public void TestDeletingPackWillDeletePackedItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ASD32423";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem1 = pack.PackedItems.AddNewPackedItem();
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			var packedItem3 = pack.PackedItems.AddNewPackedItem();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			AssertEquals("pack.PackedItems.Count", 0, pack.PackedItems.Count);
			AssertNull("pack.PackedItem", pack.PackedItem);
			AssertEquals("AsycudaPackedItem hit count", 0, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			pack.Delete();
			AssertEquals("pack.IsDeleted", true, pack.IsDeleted);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertNull("packedItem1.IsDeleted", newFactory.Load<AsycudaPackedItem>(packedItem1.PK));
			AssertNull("packedItem2.IsDeleted", newFactory.Load<AsycudaPackedItem>(packedItem2.PK));
			AssertNull("packedItem3.IsDeleted", newFactory.Load<AsycudaPackedItem>(packedItem3.PK));
			AssertNoExceptionThrown(newFactory.Save);

			pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			packedItem1 = pack.PackedItems.AddNewPackedItem();
			packedItem2 = pack.PackedItems.AddNewPackedItem();
			packedItem3 = pack.PackedItems.AddNewPackedItem();
			var pickedPackedItem = new[] { packedItem1, packedItem2, packedItem3 }.OrderBy(x => x.PK).First();
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			AssertEquals("pack.PackedItems.Count", 3, pack.PackedItems.Count);
			AssertEquals("pack.PackedItem", pickedPackedItem.PK, pack.PackedItem.PK);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			packedItem1 = newFactory.Load<AsycudaPackedItem>(packedItem1.PK);
			packedItem2 = newFactory.Load<AsycudaPackedItem>(packedItem2.PK);
			packedItem3 = newFactory.Load<AsycudaPackedItem>(packedItem3.PK);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem2, packedItem3 }, pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(x => x.PackedItem));
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			pack.Delete();
			AssertEquals("pack.IsDeleted", true, pack.IsDeleted);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("packedItem1.IsDeleted", true, packedItem1.IsDeleted);
			AssertEquals("packedItem2.IsDeleted", true, packedItem2.IsDeleted);
			AssertEquals("packedItem3.IsDeleted", true, packedItem3.IsDeleted);
			AssertNoExceptionThrown(newFactory.Save);

			pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			packedItem1 = pack.PackedItems.AddNewPackedItem();
			packedItem2 = pack.PackedItems.AddNewPackedItem();
			packedItem3 = pack.PackedItems.AddNewPackedItem();
			pickedPackedItem = new[] { packedItem1, packedItem2, packedItem3 }.OrderBy(x => x.PK).First();
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("pack.PackedItems.Count", 3, pack.PackedItems.Count);
			AssertNull("pack.PackedItem", pack.PackedItem);
			AssertEquals("AsycudaPackedItem hit count", 0, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			packedItem1 = newFactory.Load<AsycudaPackedItem>(packedItem1.PK);
			packedItem2 = newFactory.Load<AsycudaPackedItem>(packedItem2.PK);
			packedItem3 = newFactory.Load<AsycudaPackedItem>(packedItem3.PK);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem2, packedItem3 }, pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(x => x.PackedItem));
			AssertEquals("AsycudaPackedItem hit count", 3, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			pack.Delete();
			AssertEquals("pack.IsDeleted", true, pack.IsDeleted);
			AssertEquals("AsycudaPackedItem hit count", 4, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("packedItem1.IsDeleted", true, packedItem1.IsDeleted);
			AssertEquals("packedItem2.IsDeleted", true, packedItem2.IsDeleted);
			AssertEquals("packedItem3.IsDeleted", true, packedItem3.IsDeleted);
			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestDeletingPackWillNotDeleteManyToManyPackedItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ASD32423";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();

			var pack = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem1 = pack.PackedItems.AddNewPackedItem();
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			var packedItem3 = pack.PackedItems.AddNewPackedItem();
			packedItem3.AsycudaPackPackedItemPivots.AddNew().APP_APA_Pack = pack2.PK;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			bill = newFactory.Load<AsycudaBill>(bill.PK);
			pack.PackedItemRelationshipOverrideForTesting = bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("pack.PackedItems.Count", 3, pack.PackedItems.Count);
			AssertNull("pack.PackedItem", pack.PackedItem);
			AssertEquals("AsycudaPackedItem hit count", 0, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			packedItem1 = newFactory.Load<AsycudaPackedItem>(packedItem1.PK);
			packedItem2 = newFactory.Load<AsycudaPackedItem>(packedItem2.PK);
			packedItem3 = newFactory.Load<AsycudaPackedItem>(packedItem3.PK);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem2, packedItem3 }, pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(x => x.PackedItem));
			AssertEquals("AsycudaPackedItem hit count", 3, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			pack.Delete();
			AssertEquals("pack.IsDeleted", true, pack.IsDeleted);
			AssertEquals("AsycudaPackedItem hit count", 4, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("packedItem1.IsDeleted", false, packedItem1.IsDeleted);
			AssertEquals("packedItem2.IsDeleted", false, packedItem2.IsDeleted);
			AssertEquals("packedItem3.IsDeleted", false, packedItem3.IsDeleted);
			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestPackedItemMethods2()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "SDSK3224";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem1 = pack.PackedItems.AddNewPackedItem();
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			var packedItem3 = pack.PackedItems.AddNewPackedItem();
			var packedItemPKs = new[] { packedItem1.PK, packedItem2.PK, packedItem3.PK };
			var pickedPackedItem = new[] { packedItem1, packedItem2, packedItem3 }.OrderBy(x => x.PK).First();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			AssertNull("pack.GetPackedItemFromCollection()", pack.GetPackedItemFromCollection());
			AssertEquals("AsycudaPackedItem hit count", 0, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			var packedItems = pack.GetAllPackedItems();
			AssertEquals("packedItems.Length", 3, packedItems.Length);
			AssertContainsExactElementsInAnyOrder(packedItemPKs, packedItems.Select(x => x.PK));
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("pack.PackedItems.Count", 0, pack.PackedItems.Count);
			AssertNull("pack.PackedItem", pack.PackedItem);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));

			newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			AssertEquals("pack.GetPackedItemFromCollection()", pickedPackedItem.PK, pack.GetPackedItemFromCollection().PK);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			packedItems = pack.GetAllPackedItems();
			AssertEquals("packedItems.Length", 3, packedItems.Length);
			AssertContainsExactElementsInAnyOrder(packedItemPKs, packedItems.Select(x => x.PK));
			AssertEquals("AsycudaPackedItem hit count", 2, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("pack.PackedItems.Count", 3, pack.PackedItems.Count);
			AssertContainsExactElementsInAnyOrder(packedItemPKs, pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(x => x.APP_API_Item));
			AssertEquals("pack.PackedItem", pickedPackedItem.PK, pack.PackedItem.PK);
			AssertEquals("AsycudaPackedItem hit count", 2, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));

			newFactory = new BusinessObjectFactory();
			pack = newFactory.Load<AsycudaPack>(pack.PK);
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("pack.GetPackedItemFromCollection()", pickedPackedItem.PK, pack.GetPackedItemFromCollection().PK);
			AssertEquals("AsycudaPackedItem hit count", 1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			packedItems = pack.GetAllPackedItems();
			AssertEquals("packedItems.Length", 3, packedItems.Length);
			AssertContainsExactElementsInAnyOrder(packedItemPKs, packedItems.Select(x => x.PK));
			AssertEquals("AsycudaPackedItem hit count", 2, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals("pack.PackedItems.Count", 3, pack.PackedItems.Count);
			AssertContainsExactElementsInAnyOrder(packedItemPKs, pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(x => x.APP_API_Item));
			AssertNull("pack.PackedItem", pack.PackedItem);
			AssertEquals("AsycudaPackedItem hit count", 2, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var container = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			var pivot = pack.Pivot;
			var packedItem = pack.PackedItems.AddNewPackedItem();
			Factory.Save();

			packedItem.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (pack.IsDeleted)
				{
					Assert("This is wrong Packed Item should not be deleted after the Pack", false);
				}
			};

			pivot.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (pack.IsDeleted)
				{
					Assert("This is wrong Pivot should not be deleted after the Pack", false);
				}
			};

			pack.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPack)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPackedItem)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink)));
		}

		public void TestBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(bill, pack.Bill);
		}

		public void TestDeleteAndPivotAndContainer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			AssertEquals(cont.PK, pack.ContainerPK);
			AssertEquals(cont, pack.Container);
			pack.Delete();
			AssertEquals(true, pack.IsDeleted);
			AssertNull(pack.Pivot);
			AssertEquals(false, cont.IsDeleted);
		}

		public void TestLoadPivotsBeforeDeletePack()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "KSD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var cont = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			var pivotPK = pack.Pivot.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var newPack = factory.Load<AsycudaPack>(pack.PK);
			newPack.Delete();
			var newPivot = factory.Load<AsycudaContainerBillOrPackageLink>(pivotPK);
			AssertNull(newPivot);
		}

		public void TestDoNotCreatePivotWhenNoContainersExist()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "KSD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertNull(pack.Container);
			AssertEquals(ZGuid.Empty, pack.ContainerPK);
			AssertNotNull(pack.ContainerPKInfo);
			AssertNoExceptionThrown(Factory.Save);
			var cont = header.Containers.AddNew();
			cont.ACN_ContainerNumber = "DANU1234567";
			AssertNotNull(pack.ContainerPKInfo);
			AssertEquals(ZGuid.Empty, pack.ContainerPK);
			pack.Validation.ValidateContainerPK();
			AssertNoNotifications(pack.ContainerPKInfo);
			pack.ContainerPK = cont.PK;
			AssertEquals(cont.PK, pack.ContainerPK);
			AssertEquals(cont, pack.Container);
			AssertNoNotifications(pack.ContainerPKInfo);
			AssertNoExceptionThrown(Factory.Save);
			pack.ContainerPK = ZGuid.NewZGuid();
			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}

		public void TestClusterKey()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var pack = Factory.New<AsycudaPack>();
			pack.APA_ABL_Bill = bill.PK;
			AssertEquals(0, pack.APA_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, pack.APA_ClusterKey);
		}

		public void TestLoadWithClusterKey()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			for (var i = 0; i < 500; i++)
			{
				var container1 = header.Containers.AddNew();
				var pack1 = bill.Packs.AddNew();
				_ = pack1.PackedItems.AddNewPackedItem();
				pack1.ContainerPK = container1.PK;
				var container2 = header.Containers.AddNew();
				var pack2 = bill.Packs.AddNew();
				_ = pack2.PackedItems.AddNewPackedItem();
				pack2.ContainerPK = container2.PK;
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			var clusterKey = headerReloaded.AMA_ClusterKey;
			_ = newFactory.Load<AsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_ClusterKey, clusterKey));
			_ = newFactory.Load<AsycudaPack>(new ZQuery(AsycudaPackSchema.APA_ClusterKey, clusterKey));
			_ = newFactory.Load<AsycudaPackedItem>(new ZQuery(AsycudaPackedItemSchema.API_ClusterKey, clusterKey));
			_ = newFactory.Load<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, clusterKey));
			foreach (AsycudaPack pack in headerReloaded.Bills[0].Packs)
			{
				_ = pack.PackedItems.Count;
				_ = pack.PackedItem;
				_ = pack.Pivot;
			}
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaPackedItem.Schema.TableName));
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaContainerBillOrPackageLink.Schema.TableName));
		}

		public void TestNoExceptionShouldThrowWhenSavingAfterCreatingNewPackThenDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ASD32423";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			header.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var bill = header.Bills.AddNew();
			Factory.Save();

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			pack.Delete();

			AssertEquals("pack.IsDeleted", true, pack.IsDeleted);
			AssertEquals("packedItem.IsDeleted", true, packedItem.IsDeleted);

			AssertNoExceptionThrown("No exception should throw when saving pack", () =>
			{
				Factory.Save();
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "KSD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}
	}
}
