using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NonEComCHEDIMessageCollectionView))]
class NonEComCHEDIMessageCollectionViewTest : BusinessObjectCollectionViewTestCase<NonEComCHEDIMessageCollectionView>
{
	public void TestCollectionFiltering()
	{
		var ediMessage1 = entryHeader.Messages.AddNew();
		ediMessage1.EM_MessageType = MessageTypeCodeList.Codes.ECM;
		var ediMessage2 = entryHeader.Messages.AddNew();
		ediMessage2.EM_MessageType = MessageTypeCodeList.Codes.Import;
		var collection = GetCollectionToTest();
		AssertEquals(1, collection.Count);
		var ediMessages = collection.Cast<CHEDIMessage>();
		AssertNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == MessageTypeCodeList.Codes.ECM));
		AssertNotNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == MessageTypeCodeList.Codes.Import));
	}

	protected override NonEComCHEDIMessageCollectionView GetCollectionToTest() => entryHeader.NonEComMessages;

	protected override BusinessObject GetNewElementToAddToTheCollection() => entryHeader.Messages.AddNew();

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;
}
