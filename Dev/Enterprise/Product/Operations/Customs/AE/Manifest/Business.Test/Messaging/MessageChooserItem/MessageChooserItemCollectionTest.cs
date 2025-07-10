using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(MessageChooserItemCollection))]
sealed class MessageChooserItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageChooserItemCollection>
{
	[ExpectNoExceptions]
	public void TestAddNewType()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
		var collection = new MessageChooserItemCollection();
		var item = collection.AddNew(chooser, bill, true);
		NUnit.Framework.Assert.That(item, Is.TypeOf<MessageChooserItem>());
	}

	protected override MessageChooserItemCollection GetCollectionToTest() => new MessageChooserItemCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
		return new MessageChooserItem(chooser,  bill, true);
	}
}
