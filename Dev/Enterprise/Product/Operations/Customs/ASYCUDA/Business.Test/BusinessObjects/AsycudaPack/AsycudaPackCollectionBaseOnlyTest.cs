using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection<AsycudaPack, AsycudaBill>))]
	sealed class AsycudaPackCollectionBaseOnlyTest : AsycudaPackCollectionAbstractTest<AsycudaPackCollection<AsycudaPack, AsycudaBill>>
	{
		public void TestUpdateApportionmentDirty_RemovePack()
		{
			var headerSG = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			headerSG.FillWithValidTestData();

			var billSG = headerSG.Bills.AddNew();
			billSG.Packs.RemoveAndDeleteAll();

			var packWithSg = billSG.Packs.AddNew();
			var withSg = packWithSg.GetSGPackedItemForTesting();
			billSG.ApportionmentDirty = false;
			billSG.Packs.RemoveAndDelete(packWithSg);
			AssertEquals("Should be true as the Packs removes a pack data with SG.", true, billSG.ApportionmentDirty);

			var headerUS = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			headerUS.FillWithValidTestData();

			var billUS = headerUS.Bills.AddNew();
			billUS.Packs.RemoveAndDeleteAll();

			var packWithUs = billUS.Packs.AddNew();
			var withUs = packWithUs.PackedItemForTesting();
			billUS.ApportionmentDirty = false;
			billUS.Packs.RemoveAndDelete(packWithUs);
			AssertEquals("Should be false as the Packs does not remove a pack data with SG.", false, billUS.ApportionmentDirty);
		}

		public void TestAutoLinkBetweenPackAndContainerWhenContainerized()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var cont = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(cont.PK, pack.ContainerPK);
		}

		protected override AsycudaPackCollection<AsycudaPack, AsycudaBill> GetNewCollection(AsycudaBill bill) => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(bill);
	}
}
