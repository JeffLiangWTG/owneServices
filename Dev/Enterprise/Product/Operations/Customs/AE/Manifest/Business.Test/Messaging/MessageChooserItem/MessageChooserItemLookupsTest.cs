using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MessageChooserItemLookupsTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestEntryTypeList()
	{
		var list = MessageChooserItem.Lookups.EntryTypeList;
		NUnit.Framework.Assert.That(list.CodesAsString, Is.EqualTo("1, 4, 9"), "CodesAsString");
	}

	[ExpectNoExceptions]
	public void TestSubjectCodeList()
	{
		var list = MessageChooserItem.Lookups.SubjectCodeList;
		NUnit.Framework.Assert.That(list.CodesAsString, Is.EqualTo("RIR, CXL, RIJ, SPL, SWB, CHG"), "CodesAsString");
	}

	MessageChooserItem MessageChooserItem => messageChooserItem ??= GetNewMessageChooserItem();
	MessageChooserItem messageChooserItem;

	MessageChooserItem GetNewMessageChooserItem()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
		return chooser.ChooserItems[0];
	}
}
