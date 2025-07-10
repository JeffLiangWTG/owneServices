using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection<CusEntryHeader>))]
	public class CusEntryHeaderCollectionTest : Customs.Business.Testing.CusEntryHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			return new CusEntryHeaderCollection<CusEntryHeader>(testDec, Factory);
		}

		public void TestHasAnyEntryEverSentAMessage()
		{
			var testCollection = (ICusEntryHeaderCollection<Customs.Business.CusEntryHeader>)GetCollectionToTest();
			AssertEquals(false, testCollection.HasAnyEntryGotTransactionsWithCustoms);
			var cusEntryheader = testCollection.AddNew();
			cusEntryheader.CH_MessageType = "IMP";
			testCollection.Load();
			AssertEquals(false, testCollection.HasAnyEntryGotTransactionsWithCustoms);
			testCollection[0].Messages.AddNew();
			testCollection.Load();
			AssertEquals(true, testCollection.HasAnyEntryGotTransactionsWithCustoms);
		}

		public void TestHasAnyEntryWhichMessagesCannotBeChanged()
		{
			var testCollection = (ICusEntryHeaderCollection<Customs.Business.CusEntryHeader>)GetCollectionToTest();
			AssertEquals("HasAnyEntryWhichMessagesCannotBeChanged ", false, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

			var cusEntryheader = testCollection.AddNew();
			cusEntryheader.CH_MessageType = "IMP";
			testCollection.Load();

			AssertEquals("HasAnyEntryWhichMessagesCannotBeChanged ", false, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);

			cusEntryheader.CH_EntryStatus = "AWR";
			AssertEquals("HasAnyEntryWhichMessagesCannotBeChanged ", true, testCollection.HasAnyEntryWhichMessagesCannotBeChanged);
		}
	}
}
