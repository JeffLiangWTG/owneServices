using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ARManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}
	}
}
