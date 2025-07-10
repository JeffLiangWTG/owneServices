using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaContainer()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.MXManifest.IAsycudaContainer>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainer>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}
	}
}
