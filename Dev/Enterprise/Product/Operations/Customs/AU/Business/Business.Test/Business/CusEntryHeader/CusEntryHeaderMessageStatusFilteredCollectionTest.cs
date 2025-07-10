using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderMessageStatusFilteredCollection))]
	public class CusEntryHeaderMessageStatusFilteredCollectionTest : Customs.Business.Testing.CusEntryHeaderMessageStatusSubsetCollectionTest<CusEntryHeaderMessageStatusFilteredCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("users should not be able to add any elements to this collection", false, TestCollection.AllowNew);
		}

		public void TestAreAllMandatoryCPDecQuestionsAnswered()
		{
			TestDec.CustomsEntryHeaders.RemoveAndDeleteAll();

			CusEntryHeader entryOriginal = TestDec.CustomsEntryHeaders.AddNew();
			entryOriginal.CH_Status = CustomsEntryStatus.NotSent.Code;
			CMRCusEntryCPDec questionAnswered = entryOriginal.Questions.AddNew();
			questionAnswered.ON_AnswerCode = "Y";
			AssertEquals("PreCondition:Answered", true, questionAnswered.IsAnswered);

			CusEntryHeader entryAmendment = TestDec.CustomsEntryHeaders.AddNew();
			entryAmendment.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			CMRCusEntryCPDec questionNotAnswered = entryAmendment.Questions.AddNew();
			questionNotAnswered.ON_AnswerCode = "";
			AssertEquals("PreCondition:Not answered", false, questionNotAnswered.IsAnswered);

			CusEntryHeaderMessageStatusFilteredCollection collection = new CusEntryHeaderMessageStatusFilteredCollection(TestDec.ActiveEntryHeaders);
			collection.MessageStatusFilter = Customs.Business.EntryMessageStatusFilterType.CanSendOriginal;
			AssertEquals("There should be 1 element", 1, collection.Count);
			AssertEquals("It should be EntryOriginal", entryOriginal, collection[0]);
			AssertEquals("Mandatory questions are answered", true, collection.AreAllCPDecQuestionsAnswered());

			collection.MessageStatusFilter = Customs.Business.EntryMessageStatusFilterType.CanSendAmendment;
			AssertEquals("There should be 1 element", 1, collection.Count);
			AssertEquals("It should be EntryAmendment", entryAmendment, collection[0]);
			AssertEquals("Mandatory questions are not answered", false, collection.AreAllCPDecQuestionsAnswered());
		}

		public void TestFilter()
		{
			TestDec.CustomsEntryHeaders.RemoveAndDeleteAll();

			CusEntryHeader entryOriginal = TestDec.CustomsEntryHeaders.AddNew();
			entryOriginal.CH_Status = CustomsEntryStatus.NotSent.Code;

			CusEntryHeader entryPreLodged = TestDec.CustomsEntryHeaders.AddNew();
			entryPreLodged.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;

			CusEntryHeader entryFormalLodgeFailed = TestDec.CustomsEntryHeaders.AddNew();
			entryFormalLodgeFailed.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;

			CusEntryHeader entryAmendment = TestDec.CustomsEntryHeaders.AddNew();
			entryAmendment.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			CusEntryHeader entryWithdrawn = TestDec.CustomsEntryHeaders.AddNew();
			entryWithdrawn.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;

			CusEntryHeaderMessageStatusFilteredCollection collection = new CusEntryHeaderMessageStatusFilteredCollection(TestDec.ActiveEntryHeaders);
			AssertEquals("Initially there are four elements", 4, collection.Count);

			collection.MessageStatusFilter = Customs.Business.EntryMessageStatusFilterType.CanSendOriginal;
			AssertEquals("There should be 3 elements", 3, collection.Count);
			AssertEquals("EntryOriginal should be there", true, collection.Contains(entryOriginal));
			AssertEquals("EntryPreLodged should be there", true, collection.Contains(entryPreLodged));
			AssertEquals("EntryFormalLodgeFailed should be there", true, collection.Contains(entryFormalLodgeFailed));

			collection.MessageStatusFilter = Customs.Business.EntryMessageStatusFilterType.CanSendAmendment;
			AssertEquals("There should be 1 element", 1, collection.Count);
			AssertEquals("EntryAmendment should be there", true, collection.Contains(entryAmendment));

			collection.MessageStatusFilter = Customs.Business.EntryMessageStatusFilterType.CanSendWithdraw;
			AssertEquals("There should be 1 element", 1, collection.Count);
			AssertEquals("EntryAmendment should be there", true, collection.Contains(entryAmendment));
		}

		protected override CusEntryHeaderMessageStatusFilteredCollection GetCollectionToTest()
		{
			return TestCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TestDec.CustomsEntryHeaders.AddNew();
		}

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = JobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		CusEntryHeaderMessageStatusFilteredCollection TestCollection
		{
			get
			{
				if (fTestCollection == null)
				{
					fTestCollection = new CusEntryHeaderMessageStatusFilteredCollection(TestDec.ActiveEntryHeaders);
				}
				return fTestCollection;
			}
		}
		CusEntryHeaderMessageStatusFilteredCollection fTestCollection;
	}
}
