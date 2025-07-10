using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}
	}
}
