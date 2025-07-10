using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(MessageChooserItem))]
sealed class MessageChooserItemTest : NonPersistentBusinessObjectTestCase
{
	[ExpectNoExceptions]
	public void TestSetDefaultValues_EntryType()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.EntryType, Is.EqualTo(EntryTypes.Codes.Original).Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestEntryType_Caption()
	{
		NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(MessageChooserItem.EntryTypeInfo).Caption, Is.EqualTo("Entry Type"));
	}

	[ExpectNoExceptions]
	public void TestEntryType_MaxLength()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.EntryTypeInfo.MaxLength, Is.EqualTo(1));
	}

	[ExpectNoExceptions]
	public void TestBOLNumber_Caption()
	{
		NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(MessageChooserItem.BOLNumberInfo).Caption, Is.EqualTo("BOL Number"));
	}

	[ExpectNoExceptions]
	public void TestCustomsStatus_Caption()
	{
		NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(MessageChooserItem.CustomsStatusInfo).Caption, Is.EqualTo("Customs Status"));
	}

	[ExpectNoExceptions]
	public void TestSubjectCode_Caption()
	{
		NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(MessageChooserItem.SubjectCodeInfo).Caption, Is.EqualTo("Subject Code"));
	}

	[ExpectNoExceptions]
	public void TestSubjectCode()
	{
		MessageChooserItem.EntryType = EntryTypes.Codes.Cancellation;
		NUnit.Framework.Assert.That(MessageChooserItem.SubjectCode.ToString(), Is.EqualTo("CXL"));
	}

	[ExpectNoExceptions]
	public void TestSubjectCode_MaxLength()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.SubjectCodeInfo.MaxLength, Is.EqualTo(3));
	}

	[ExpectNoExceptions]
	public void TestSubject_MaxLength()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.SubjectInfo.MaxLength, Is.EqualTo(512));
	}

	[ExpectNoExceptions]
	public void TestCargoType_Caption()
	{
		NUnit.Framework.Assert.That(DataBoundResourceStrings.GetDataForProperty(MessageChooserItem.CargoTypeInfo).Caption, Is.EqualTo("Cargo Type"));
	}

	[ExpectNoExceptions]
	public void TestLookups()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.Lookups, Is.TypeOf<MessageChooserItemLookups>());
	}

	[ExpectNoExceptions]
	public void TestValidation()
	{
		NUnit.Framework.Assert.That(MessageChooserItem.Validation, Is.TypeOf<MessageChooserItemValidation>());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
		return chooser.ChooserItems[0];
	}

	MessageChooserItem MessageChooserItem => messageChooserItem ??= (MessageChooserItem)GetNewBusinessObject();
	MessageChooserItem messageChooserItem;
}
