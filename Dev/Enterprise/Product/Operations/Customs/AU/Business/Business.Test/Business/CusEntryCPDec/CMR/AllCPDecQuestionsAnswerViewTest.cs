using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AllCPDecQuestionsViewCollection))]
	public class AllCPDecQuestionsAnswerViewTest : SubsetBusinessObjectCollectionTestCase<AllCPDecQuestionsViewCollection, CMRCusEntryCPDec>
	{
		public void TestAllowNew()
		{
			AllCPDecQuestionsViewCollection collection = new AllCPDecQuestionsViewCollection(CompleteCollection);
			AssertEquals("AllowNew", false, collection.AllowNew);
		}

		public void TestLoadQuestions()
		{
			CMRCusEntryCPDec questionAnswered = EntryLine.Questions.AddNew();
			questionAnswered.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec questionUnAnswered = EntryLine.Questions.AddNew();

			AllEntryLineCPDecQuestion completeCollection = new AllEntryLineCPDecQuestion(EntryHeader);
			completeCollection.Load();

			AllCPDecQuestionsViewCollection collection = new AllCPDecQuestionsViewCollection(completeCollection);
			EntryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.All;
			collection.Rebuild();
			AssertEquals("there should be two questions", 2, collection.Count);

			EntryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.Unanswered;
			collection.Rebuild();
			AssertEquals("there should be One questions", 1, collection.Count);
			AssertEquals("there should be One questions", questionUnAnswered, collection[0]);

			EntryHeader.CPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.Answered;
			collection.Rebuild();
			AssertEquals("there should be One questions", 1, collection.Count);
			AssertEquals("there should be One questions", questionAnswered, collection[0]);
		}

		protected override AllCPDecQuestionsViewCollection GetCollectionToTest()
		{
			return new AllCPDecQuestionsViewCollection(CompleteCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return EntryLine.Questions.AddNew();
		}

		AllEntryLineCPDecQuestion CompleteCollection
		{
			get
			{
				if (fCompleteCollection == null)
				{
					fCompleteCollection = new AllEntryLineCPDecQuestion(EntryHeader);
				}
				return fCompleteCollection;
			}
		}
		AllEntryLineCPDecQuestion fCompleteCollection;

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<JobDeclaration>();
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					fEntryLine = EntryHeader.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;
	}
}
