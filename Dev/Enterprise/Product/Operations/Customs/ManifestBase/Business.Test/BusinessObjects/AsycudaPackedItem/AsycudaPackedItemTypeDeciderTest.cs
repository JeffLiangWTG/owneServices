using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.Customs.ManifestBase.Testing
{
	sealed class AsycudaPackedItemTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			using (ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header1.FillWithValidTestData();
				header1.AMA_JobReference = "123";
				var header1Bill = header1.Bills.AddNew();
				var header1Pack = header1Bill.Packs.AddNew();
				var header1PackedItem = header1Pack.PackedItem;
				var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
				header2.AMA_JobReference = "456";
				header2.AMA_ApplicationCode = "NVC";
				header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				var header2Bill = header2.Bills.AddNew();
				var header2Pack = header2Bill.Packs.AddNew();
				header2Pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
				var header2PackedItem = header2Pack.PackedItems.AddNewPackedItem();
				Factory.Save();

				Assert(new BusinessObjectFactory().Load<AsycudaPackedItem>(header1PackedItem.PK) is Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem);
				Assert(new BusinessObjectFactory().Load<AsycudaPackedItem>(header2PackedItem.PK) is Integration.Customs.ASYCUDA.IAsycudaPackedItem);
			}
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaPackedItem), new AsycudaPackedItemTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaPackedItem), new AsycudaPackedItemTypeDecider().GetTypeForBinding());
		}
	}
}
