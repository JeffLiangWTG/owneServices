using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComplaintMessageSendingObjectLineCollection))]
internal class EComplaintMessageSendingObjectLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EComplaintMessageSendingObjectLineCollection>
{
	protected override EComplaintMessageSendingObjectLineCollection GetCollectionToTest()
	{
		return new EComplaintMessageSendingObjectLineCollection(EntryHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new EComplaintMessageSendingObjectLine(EntryHeader);
	}

	CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Factory.New<CusEntryHeader>());
	CusEntryHeader entryHeader;
}
