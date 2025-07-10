using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(MessageChooser))]
sealed class MessageChooserTest : NonPersistentBusinessObjectTestCase
{
	[ExpectNoExceptions]
	public void TestChooseItems()
	{
		NUnit.Framework.Assert.That(MessageChooser.ChooserItems, Is.TypeOf<MessageChooserItemCollection>());
	}

	protected override BusinessObject GetNewBusinessObject() => GetMessageChooser();

	MessageChooser MessageChooser => messageChooser ??= GetMessageChooser();
	MessageChooser messageChooser;

	MessageChooser GetMessageChooser()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();

		var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
		return chooser;
	}
}
