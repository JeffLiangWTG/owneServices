using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComCHEDIMessageCollectionView))]
class EComCHEDIMessageCollectionViewTest : BusinessObjectCollectionViewTestCase<EComCHEDIMessageCollectionView>
{
	public void TestCollectionFiltering()
	{
		var ediMessage1 = (CHEDIMessage)GetNewElementToAddToTheCollection();
		var ediMessage2 = (CHEDIMessage)GetNewElementToAddToTheCollection();
		ediMessage2.EM_MessageType = MessageTypeCodeList.Codes.Import;
		var collection = GetCollectionToTest();
		AssertEquals(1, collection.Count);
		var ediMessages = collection.Cast<CHEDIMessage>();
		AssertNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == "IMP"));
		AssertNotNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == MessageTypeCodeList.Codes.ECM));
	}

	protected override EComCHEDIMessageCollectionView GetCollectionToTest() => entryHeader.EComMessages;

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = MessageTypeCodeList.Codes.ECM;
		return message;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;
}
