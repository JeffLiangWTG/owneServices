using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(MessageChooserItemCollection))]
	sealed class MessageChooserItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageChooserItemCollection>
	{
		public void TestAddNew_Parameters()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var collection = new MessageChooserItemCollection();
			var item = collection.AddNew(chooser, bill, true);
			AssertEquals(typeof(MessageChooserItem), item.GetType());
			AssertEquals(1, collection.Count);
			AssertCollectionContains(item, collection);
		}

		protected override MessageChooserItemCollection GetCollectionToTest() => new MessageChooserItemCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			return chooser.ChooserItems[0];
		}
	}
}
