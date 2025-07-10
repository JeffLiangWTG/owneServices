using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAsycudaPackedItemCollectionMaster()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var supportingDocumentCollection = new SupportingDocumentCollection(header);
			AssertType<AsycudaManifestHeader>(supportingDocumentCollection.Master);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new SupportingDocumentCollection(header);
		}
	}
}
