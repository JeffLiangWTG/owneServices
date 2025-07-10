using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackedItemTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "BBK";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header1Bill = header1.Bills.AddNew();
			var header1BillPack = header1Bill.Packs.AddNew();
			header1BillPack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var header1BillPackedItem = header1BillPack.PackedItems.AddNewPackedItem();
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header2BillPack = header2Bill.Packs.AddNew();
			header2BillPack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var header2BillPackedItem = header2BillPack.PackedItems.AddNewPackedItem();
			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Bill = header3.Bills.AddNew();
			var header3BillPack = header3Bill.Packs.AddNew();
			header3BillPack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var header3Container = header3.Containers.AddNew();
			header3BillPack.ContainerPK = header3Container.PK;
			var header3BillPackedItem = header3BillPack.PackedItems.AddNewPackedItem();
			var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header4.AMA_JobReference = "012";
			header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			var header4Bill = header4.Bills.AddNew();
			var header4BillPack = header4Bill.Packs.AddNew();
			header4BillPack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var header4Container = header4.Containers.AddNew();
			header4BillPack.ContainerPK = header4Container.PK;
			var header4BillPackedItem = header4BillPack.PackedItems.AddNewPackedItem();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPackedItem), header1BillPackedItem.PK) is Integration.Customs.ManifestBase.IAsycudaPackedItem);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPackedItem), header2BillPackedItem.PK) is AsycudaPackedItem);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPackedItem), header3BillPackedItem.PK) is AsycudaPackedItem);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPackedItem), header4BillPackedItem.PK) is AsycudaPackedItem);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaPackedItem), new AsycudaPackedItemTypeDecider().GetTypeForNew());
			AssertEquals(typeof(AsycudaPackedItem), new BusinessObjectFactory().New<AsycudaPackedItem>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaPackedItem), new AsycudaPackedItemTypeDecider().GetTypeForBinding());
		}
	}
}
