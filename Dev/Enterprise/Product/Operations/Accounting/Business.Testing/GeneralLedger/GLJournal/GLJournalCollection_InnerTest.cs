using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalCollection))]
	public class GLJournalCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		public void TestCompanyFilter()
		{
			TestCollection.Load();
			int existingCount = TestCollection.Count;

			var testJournal1 = Factory.New<GLJournal>();
			var testJournal2 = Factory.New<GLJournal>();
			var testJournal3 = Factory.New<GLJournal>();

			testJournal3.AH_GC = ZGuid.NewZGuid();

			TestCollection.Load();
			AssertEquals(2, TestCollection.Count - existingCount);
		}

		public void TestLedgerFilter()
		{
			TestCollection.Load();
			int existingCount = TestCollection.Count;

			var testJournal1 = Factory.New<GLJournal>();
			var testJournal2 = Factory.New<GLJournal>();
			var testJournal3 = Factory.New<GLJournal>();

			testJournal3.AH_Ledger = LedgerTypes.AccountsReceivable;
			TestCollection.Load();
			AssertEquals(2, TestCollection.Count - existingCount);
		}

		public void TestTransactionTypeFilter()
		{
			var glJournalCollection = new GLJournalCollection(Factory);

			var testJournal1 = Factory.New<GLJournal>();
			var testJournal2 = Factory.New<GLJournal>();
			var testJournal3 = Factory.New<GLJournal>();
			var testJournal4 = Factory.New<GLJournal>();
			var testJournal5 = Factory.New<GLJournal>();

			testJournal1.AH_TransactionType = TransactionTypes.GLStandardJournal;
			testJournal2.AH_TransactionType = TransactionTypes.GLReversingJournal;
			testJournal3.AH_TransactionType = TransactionTypes.GLAutoJournal;
			testJournal4.AH_TransactionType = TransactionTypes.GLNoteJournal;
			testJournal5.AH_TransactionType = TransactionTypes.Invoice;

			glJournalCollection.Load();

			var actualTransactionTypes = glJournalCollection.Cast<AccTransactionHeader>().Select(x => x.AH_TransactionType.ToString()).ToArray();
			AssertContainsExactElementsInAnyOrder(new string[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLNoteJournal }, actualTransactionTypes);
		}

		protected GLJournalCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new GLJournalCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GLJournalCollection(Factory);
		}
	}
}
