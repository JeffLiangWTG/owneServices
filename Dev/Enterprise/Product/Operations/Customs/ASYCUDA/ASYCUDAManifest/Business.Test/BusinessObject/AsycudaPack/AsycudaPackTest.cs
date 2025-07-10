using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
