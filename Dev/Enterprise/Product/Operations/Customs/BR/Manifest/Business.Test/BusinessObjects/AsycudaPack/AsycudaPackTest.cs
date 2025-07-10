using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	[TestedType(typeof(AsycudaPack))]
	public class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.BRManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill() => AssertType<AsycudaBill>(NewPack.Bill);

		public void TestContainer() => AssertType<AsycudaContainer>(NewPack.Container);

		public void TestLookups() => AssertType<AsycudaPackLookups>(NewPack.Lookups);

		public void TestBuilkType()
		{
			var pack = NewPack;
			pack.BulkType = BRBulkTypeList.Codes._8;
			AssertEquals("BulkType should be 8", "8", pack.BulkType);
			Factory.Save();

			var reloadedPack = Factory.Load<AsycudaPack>(pack.PK);
			AssertEquals("BulkType should be 8", "8", reloadedPack.BulkType);
		}

		AsycudaPack NewPack => (AsycudaPack)GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}

		public void TestGetWarningBeforeBeingDeletedForSentBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";

			var pack = bill1.Packs.AddNew();

			AssertEquals("This Bill is already sent to Customs.", pack.GetWarningBeforeBeingDeleted());

			header.AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertNotEquals("This Bill is already sent to Customs.", pack.GetWarningBeforeBeingDeleted());
		}
	}
}
