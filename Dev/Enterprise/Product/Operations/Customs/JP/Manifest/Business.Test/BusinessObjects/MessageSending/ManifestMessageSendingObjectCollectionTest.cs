using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObjectCollection))]
	sealed class ManifestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ManifestMessageSendingObjectCollection>
	{
		protected override ManifestMessageSendingObjectCollection GetCollectionToTest() => new (Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new ManifestMessageSendingObject(bill);
		}
	}
}
