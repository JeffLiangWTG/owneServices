using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKey()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_ABL_Bill = bill.PK;
			AssertEquals(0, packedItem.API_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, packedItem.API_ClusterKey);
		}

		public void TestAPI_LineNoRecalculatedWhenUpdatingAPI_ABL_Bill()
		{
			CombineAssertions(() =>
			{
				var bill = Factory.New<AsycudaBill>();
				var packedItem1 = Factory.New<AsycudaPackedItem>();
				packedItem1.API_ABL_Bill = bill.PK;
				AssertEquals("packedItem1.API_LineNo", (ZShort)1, packedItem1.API_LineNo);

				var packedItem2 = Factory.New<AsycudaPackedItem>();
				packedItem2.API_ABL_Bill = bill.PK;
				AssertEquals("packedItem2.API_LineNo", (ZShort)2, packedItem2.API_LineNo);

				var packedItem3 = Factory.New<AsycudaPackedItem>();
				packedItem3.API_ABL_Bill = bill.PK;
				AssertEquals("packedItem3.API_LineNo", (ZShort)3, packedItem3.API_LineNo);

				packedItem2.Delete();
				AssertEquals("packedItem1.API_LineNo after packedItem2 is deleted", (ZShort)1, packedItem1.API_LineNo);
				AssertEquals("packedItem3.API_LineNo after packedItem2 is deleted", (ZShort)2, packedItem3.API_LineNo);

				packedItem1.API_ABL_Bill = ZGuid.Empty;
				AssertEquals("packedItem3.API_LineNo after packedItem1 is detached", (ZShort)1, packedItem3.API_LineNo);
			});
		}

		public void TestRecordIsDeletedWhenParentHasANoneRelationship()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "SDS23423";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItemRelationshipOverrideForTesting = (AsycudaPackPackedItemPivotCollection.RelationshipType?)AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem1A = pack1.PackedItems.AddNewPackedItem();
			var packedItem1B = pack1.PackedItems.AddNewPackedItem();
			var pack2 = bill.Packs.AddNew();
			pack2.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			var packedItem2A = pack2.PackedItems.AddNewPackedItem();
			var packedItem2B = pack2.PackedItems.AddNewPackedItem();
			var pack3 = bill.Packs.AddNew();
			pack3.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var packedItem3A = pack3.PackedItem;
			var packedItem3B = pack3.PackedItems.AddNewPackedItem();
			Factory.Save();
			AssertEquals("packedItem1A.IsDeleted", false, packedItem1A.IsDeleted);
			AssertEquals("packedItem1B.IsDeleted", false, packedItem1B.IsDeleted);
			AssertEquals("packedItem2A.IsDeleted", true, packedItem2A.IsDeleted);
			AssertEquals("packedItem2B.IsDeleted", true, packedItem2B.IsDeleted);
			AssertEquals("packedItem3A.IsDeleted", false, packedItem3A.IsDeleted);
			AssertEquals("packedItem3B.IsDeleted", true, packedItem3B.IsDeleted);
		}

		public void TestRecordIsDeletedWhenIsNotMainOneForParentHWithAnOneRelationship()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var packedItem1 = pack.PackedItem;
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			var packedItem3 = pack.PackedItems.AddNewPackedItem();
			Factory.Save();
			AssertEquals("packedItem1.IsDeleted", false, packedItem1.IsDeleted);
			AssertEquals("packedItem2.IsDeleted", true, packedItem2.IsDeleted);
			AssertEquals("packedItem3.IsDeleted", true, packedItem3.IsDeleted);
			AssertEquals("pack.PackedItems.Count", 1, pack.PackedItems.Count);
			AssertSame(packedItem1, pack.PackedItems[0].PackedItem);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem = pack.PackedItems.AddNewPackedItem();
			return packedItem;
		}

		public void TestAddNewAsycudaPackShouldNotAffectExistingAsycudaPackedItems()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			header.AMA_ManifestType = "MGI";

			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_Tariff = "7403200030";
			pack1.PackedItem.API_CustomsQty = 30;
			pack1.PackedItem.API_CustomsUQ = "BOX";

			var pack2 = bill.Packs.AddNew();
			pack2.PackedItem.API_Tariff = "7403200031";
			pack2.PackedItem.API_CustomsQty = 15;
			pack2.PackedItem.API_CustomsUQ = "CAS";

			Factory.Save();

			pack1.PackedItem.API_Tariff = "8403200030";
			pack1.PackedItem.API_CustomsQty = 60;
			pack1.PackedItem.API_CustomsUQ = "BOT";

			pack2.PackedItem.API_Tariff = "8403200031";
			pack2.PackedItem.API_CustomsQty = 30;
			pack2.PackedItem.API_CustomsUQ = "CTN";

			var pack3 = bill.Packs.AddNew();
			pack3.PackedItem.API_Tariff = "8403200032";

			AssertEquals("Pack1 Tariff", "8403200030", pack1.PackedItem.API_Tariff);
			AssertEquals("Pack1 Customs Qty", 60m, pack1.PackedItem.API_CustomsQty);
			AssertEquals("Pack1 Customs Qty Unit", "BOT", pack1.PackedItem.API_CustomsUQ);

			AssertEquals("Pack2 Tariff", "8403200031", pack2.PackedItem.API_Tariff);
			AssertEquals("Pack2 Customs Qty", 30m, pack2.PackedItem.API_CustomsQty);
			AssertEquals("Pack2 Customs Qty Unit", "CTN", pack2.PackedItem.API_CustomsUQ);

			AssertEquals("Pack3 Tariff", "8403200032", pack3.PackedItem.API_Tariff);
		}

		public void TestAsycudaTaxes()
		{
			var packedItem = Factory.New<AsycudaManifestHeader>().Bills.AddNew().Packs.AddNew().PackedItem;
			var taxes = packedItem.AsycudaTaxes;
			AssertType<AsycudaPackedItemTaxCollection<AsycudaTax, AsycudaPackedItem>>(taxes);
			var packedItemTax = taxes.AddNew();
			AssertType<AsycudaTax>(packedItemTax);
		}
	}
}

