using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineCollection))]
	public class GLJournalLineCollection_InnerTest : DependentTransactionLineCollectionTest
	{
		public void TestCopyDownPreviousItemDetails()
		{
			AssertCopyDownPreviousItemDetails();
		}

		public void TestCopyDownPreviousItemDetailsForBulkUpdate()
		{
			using (TestCollection.SuspendListChanged())
			{
				AssertCopyDownPreviousItemDetails(true);
			}
			AssertEquals("Sequence number of first line", (ZShort)1, TestCollection[0].AL_Sequence);
			AssertEquals("New Row should have sequence number", (ZShort)2, TestCollection[1].AL_Sequence);
			AssertEquals("New Line should have sequence number", (ZShort)3, TestCollection[2].AL_Sequence);
		}

		void AssertCopyDownPreviousItemDetails(bool suspendCopy = false)
		{
			Journal.AH_Desc = "Journal Desc";
			((IBindingList)TestCollection).AddNew();

			if (!suspendCopy)
			{
				AssertEquals("Sequence number of first line", (ZShort)1, TestCollection[0].AL_Sequence);
			}

			TestCollection[0].UnsignedOSLineAmount = 50.00M;
			TestCollection[0].DebitCreditSign = nameof(DebitCredit.CR);
			TestCollection[0].AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestCollection[0].AL_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Description should default to Master Description", "Journal Desc", TestCollection[0].AL_Desc);
			TestCollection[0].AL_Desc = "Description";

			((ICancelAddNew)TestCollection).EndNew(0);

			((IBindingList)TestCollection).AddNew();
			AssertEquals("New Row should carry forward Amount", suspendCopy ? 0m : 50.00M, TestCollection[1].UnsignedOSLineAmount);
			AssertEquals("New Row should carry forward opposite DR/CR", nameof(DebitCredit.DR), TestCollection[1].DebitCreditSign);
			AssertEquals("New Row should carry forward Branch", GlbBranch.CurrentBranch.PK, TestCollection[1].AL_GB);
			AssertEquals("New Row should carry forward Department", GlbDepartment.CurrentDepartment.PK, TestCollection[1].AL_GE);
			AssertEquals("New Row should carry forward Description", suspendCopy ? "Journal Desc" : "Description", TestCollection[1].AL_Desc);
			if (!suspendCopy)
			{
				AssertEquals("New Row should have sequence number", (ZShort)2, TestCollection[1].AL_Sequence);
			}

			TestCollection[1].HasChanges = true;
			((ICancelAddNew)TestCollection).EndNew(1);

			((IBindingList)TestCollection).AddNew();
			TestCollection[2].UnsignedOSLineAmount = 200m;
			TestCollection[2].DebitCreditSign = nameof(DebitCredit.CR);
			((ICancelAddNew)TestCollection).EndNew(2);

			((IBindingList)TestCollection).Remove(TestCollection[1]);

			((IBindingList)TestCollection).AddNew();
			AssertEquals("New Line Amount should be Journal Balance", suspendCopy ? 0m : 250.00m, TestCollection[2].UnsignedOSLineAmount);
			AssertEquals("New Debit/Credit Sign should be DR", nameof(DebitCredit.DR), TestCollection[2].DebitCreditSign);
			if (!suspendCopy)
			{
				AssertEquals("New Line should have sequence number", (ZShort)4, TestCollection[2].AL_Sequence);
			}
		}

		public void TestCopyDownPreviousItemDetailsWithInvalidData()
		{
			AssertCopyDownPreviousItemDetailsWithInvalidData();
		}

		public void TestCopyDownPreviousItemDetailsWithInvalidDataForBulkUpdate()
		{
			using (TestCollection.SuspendListChanged())
			{
				AssertCopyDownPreviousItemDetailsWithInvalidData(true);
			}
		}

		void AssertCopyDownPreviousItemDetailsWithInvalidData(bool suspendCopy = false)
		{
			((IBindingList)TestCollection).AddNew();
			TestCollection[0].UnsignedOSLineAmount = 0M;
			TestCollection[0].DebitCreditSign = "";
			TestCollection[0].AL_GE = ZGuid.Invalid;
			TestCollection[0].AL_GB = ZGuid.Empty;
			TestCollection[0].AL_Desc = "";
			((ICancelAddNew)TestCollection).EndNew(0);

			((IBindingList)TestCollection).AddNew();
			AssertEquals("New Row should carry forward Amount", 0M, TestCollection[1].UnsignedOSLineAmount);
			AssertEquals("New Row should carry forward DR as default when carrying forward an invalid row", nameof(DebitCredit.DR), TestCollection[1].DebitCreditSign);
			AssertEquals("New Row should carry forward Branch", suspendCopy ? GlbBranch.CurrentBranch.PK : ZGuid.Empty, TestCollection[1].AL_GB);
			AssertEquals("New Row should carry forward Department", suspendCopy ? GlbDepartment.CurrentDepartment.PK : ZGuid.Invalid, TestCollection[1].AL_GE);
			AssertEquals("New Row should carry forward Description", suspendCopy ? "GENERAL LEDGER JOURNAL" : "", TestCollection[1].AL_Desc);
		}

		public void TestSetPostDateForAllLines()
		{
			GLJournalLine line1 = TestCollection.AddNew();
			GLJournalLine line2 = TestCollection.AddNew();

			ZDateTime postDate = ZDateTime.Now.AddMonths(2);
			Journal.AH_PostDate = postDate;
			TestCollection.SetPostDateForAllLines();

			AssertEquals("All Lines should have the post date set to the header value now", postDate.Date, line1.AL_PostDate.Date);
			AssertEquals("All Lines should have the post date set to the header value now", postDate.Date, line2.AL_PostDate.Date);
		}

		public void TestSetReverseDateForAllLines()
		{
			GLJournalLine line1 = TestCollection.AddNew();
			GLJournalLine line2 = TestCollection.AddNew();

			ZDateTime reverseDate = ZDateTime.Now.AddMonths(2);
			Journal.AH_DueDate = reverseDate;
			TestCollection.SetReverseDateForAllLines();

			AssertEquals("All Lines should have the post date set to the header value now", reverseDate.Date, line1.AL_ReverseDate.Date);
			AssertEquals("All Lines should have the post date set to the header value now", reverseDate.Date, line2.AL_ReverseDate.Date);
		}

		public void TestSetTransactionLineTypeForAllLines()
		{
			GLJournalLine line1 = TestCollection.AddNew();
			GLJournalLine line2 = TestCollection.AddNew();

			ZString transactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			Journal.AH_TransactionType = transactionType;
			TestCollection.SetLineTypeForAllLines();

			AssertEquals("All Lines should have the correct transaction line type", transactionType, line1.AL_LineType);
			AssertEquals("All Lines should have the correct transaction line type", transactionType, line2.AL_LineType);
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		protected GLJournalLineCollection TestCollection;
		protected GLJournal Journal;

		public void TestAddNewWhenGLNoteJournal()
		{
			Factory.Save();

			var creator = new TestObjectCreator(Factory);

			var journal = creator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			creator.CreateGLJournalLine(journal, 10, DebitCredit.CR, creator.ExchangeGainLossControlAccount.PK);
			var fixedAmount = journal.GetSumOfLines(GLJournalLine.Schema.AL_LocalExTaxAmount);
			Assert("basic data assume", fixedAmount == -10m && journal.GLJournalLines.Count == 1);

			journal.GLJournalLines.AddNew();
			Assert("should only just add one new item", journal.GLJournalLines.Count == 2 && journal.GetSumOfLines(GLJournalLine.Schema.AL_LocalExTaxAmount) == fixedAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Journal = Factory.New<GLJournal>();
			TestCollection = Journal.GLJournalLines;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<GLJournal>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new GLJournalLineCollection(parent);
		}
	}
}
