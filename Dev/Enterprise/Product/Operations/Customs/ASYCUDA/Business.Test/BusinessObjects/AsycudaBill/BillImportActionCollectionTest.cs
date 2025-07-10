using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(BillImportActionCollection))]
	sealed class BillImportActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillImportActionCollection>
	{
		protected override BillImportActionCollection GetCollectionToTest() => new BillImportActionCollection(Header.Bills);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BillImportAction(Header.Bills.AddNew());

		AsycudaManifestHeader Header => header ?? (header = Factory.New<AsycudaManifestHeader>());
		AsycudaManifestHeader header;
	}
}
