using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeaderCollection))]
sealed class CusEntryHeaderCollectionTest : EU.Business.Declaration.Testing.CusEntryHeaderCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var dec = Factory.New<JobDeclaration>();
		return new CusEntryHeaderCollection(dec, Factory);
	}

	public new void TestHasAnyEntryWhichMessagesCannotBeChanged()
	{
		var testCollection = (CusEntryHeaderCollection)GetCollectionToTest();
		AssertEquals($"When there are no entries, {nameof(testCollection.HasAnyEntryWhichMessagesCannotBeChanged)}", false, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		var entry = testCollection.AddNew();
		entry.CH_MessageType = "IMP";
		testCollection.Load();

		AssertEquals(GetAssertionMessage(), false, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
		AssertEquals(GetAssertionMessage(), true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_Status = ITMessageStatusList.Codes.AcknowledgedOriginal;
		AssertEquals(GetAssertionMessage(), true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_Status = "";
		entry.CH_EntryStatus = "";
		AssertEquals(GetAssertionMessage(), false, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
		AssertEquals(GetAssertionMessage(), true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_EntryStatus = ITEntryStatusList.Codes.ImportCleared;
		AssertEquals(GetAssertionMessage(), true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		entry.CH_EntryStatus = ITEntryStatusList.Codes.NbRejected;
		AssertEquals(GetAssertionMessage(), true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

		string GetAssertionMessage() => $"When {nameof(entry.CH_Status)} = '{entry.CH_Status}' and {nameof(entry.CH_EntryStatus)} = '{entry.CH_EntryStatus}', {nameof(testCollection.HasAnyEntryWhichMessagesCannotBeChanged)}";
	}
}
