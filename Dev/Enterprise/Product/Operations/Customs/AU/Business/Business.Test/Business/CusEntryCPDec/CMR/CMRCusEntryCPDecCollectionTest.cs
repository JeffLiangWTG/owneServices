using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCusEntryCPDecCollection))]
	public class CMRCusEntryCPDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_UPEIndicator_Hidden = false;

			var question1 = Factory.New<CMRCusEntryCPDec>();
			question1.ON_CH = entryHeader.PK;
			question1.ON_CPDecNum = 1;

			var question2 = Factory.New<CMRCusEntryCPDec>();
			question2.ON_CH = entryHeader.PK;
			question2.ON_CPDecNum = 3;

			var question3 = Factory.New<CMRCusEntryCPDec>();
			question3.ON_CH = entryHeader.PK;
			question3.ON_CPDecNum = 375;

			var collection = new CMRCusEntryCPDecCollection(entryHeader);
			collection.Load();

			var expectedPks = new[] { question1.PK, question2.PK, question3.PK };
			AssertContainsExactElementsInAnyOrder("Should load all related questions as default.", expectedPks, collection.GetPKs());

			collection = new CMRCusEntryCPDecCollection(entryHeader);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			collection.Load();

			AssertEquals(1, collection.Count);
			AssertCollectionContains("Should contains the question1 as the CP Dec Num is not 3 or 375.", question1, collection);
			AssertCollectionNotContains("Should not contains the question2 as the UPE is ticked and the CP Dec Num is 3.", question2, collection);
			AssertCollectionNotContains("Should not contains the question3 as the UPE is ticked and the CP Dec Num is 375.", question3, collection);
		}

		public void TestHaveDeclarationQuestionsBeenModified()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRCusEntryCPDec decQuestion3 = entryHeader.Questions.AddNew();
			decQuestion3.ON_CPDecNum = 3;
			decQuestion3.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion6 = entryHeader.Questions.AddNew();
			decQuestion6.ON_CPDecNum = 6;
			decQuestion6.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion7 = entryHeader.Questions.AddNew();
			decQuestion7.ON_CPDecNum = 7;
			decQuestion7.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion8 = entryHeader.Questions.AddNew();
			decQuestion8.ON_CPDecNum = 8;
			decQuestion8.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion9 = entryHeader.Questions.AddNew();
			decQuestion9.ON_CPDecNum = 9;
			decQuestion9.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion375 = entryHeader.Questions.AddNew();
			decQuestion375.ON_CPDecNum = 375;
			decQuestion375.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec decQuestion4 = entryHeader.Questions.AddNew();
			decQuestion4.ON_CPDecNum = 4;
			decQuestion4.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			Factory.Save();

			Assert("no changes", !entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion3.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion3.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion6.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion6.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion7.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion7.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion8.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion8.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion9.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion9.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion375.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("changes", entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
			decQuestion375.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			decQuestion4.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Assert("no changes", !entryHeader.Questions.HaveDeclarationQuestionsBeenModified);
		}

		public void TestConstructorWithOrgHeader()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDecCollection collection = new CMRCusEntryCPDecCollection(orgAttachee, org);
			AssertEquals("Master", org.PK, collection.Master.PK);
		}

		public void TestTableCodeIsSet()
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			CMRCusEntryCPDecCollection collection = new CMRCusEntryCPDecCollection(pivot);
			CMRCusEntryCPDec newBizO = collection.AddNew();
			AssertEquals("Parent table Code for Pivot", "CI", newBizO.ON_ParentTableCode);

			Classification @class = Factory.New<Classification>();
			collection = new CMRCusEntryCPDecCollection(@class);
			newBizO = collection.AddNew();
			AssertEquals("Parent table code for Classification", "CC", newBizO.ON_ParentTableCode);

			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			collection = new CMRCusEntryCPDecCollection(consolidatedDeclaration);
			newBizO = collection.AddNew();
			AssertEquals("Parent table code for Consolidated Declaration", "CRD", newBizO.ON_ParentTableCode);
		}

		public void TestAreCPQuestionsAnswered()
		{
			CMRCusEntryCPDecCollection testCollection = (CMRCusEntryCPDecCollection)GetCollectionToTest();
			CMRCusEntryCPDec question = testCollection.AddNew();
			CMRCusEntryCPDec question2 = testCollection.AddNew();
			AssertEquals("There are questions not answered", false, testCollection.AreAllCPDecQuestionsAnswered);

			question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			AssertEquals("Not All questions answered", false, testCollection.AreAllCPDecQuestionsAnswered);

			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			AssertEquals("All questions answered", true, testCollection.AreAllCPDecQuestionsAnswered);
		}

		public void TestHasQuestionWithID()
		{
			CMRCusEntryCPDecCollection testCollection = (CMRCusEntryCPDecCollection)GetCollectionToTest();
			var item = (CMRCusEntryCPDec)GetNewElementToAddToTheCollection();
			item.ON_CPDecNum = 400;
			item.ON_CPDecStartDate = new ZDateTime(2011, 2, 1);
			testCollection.Add(item);
			Assert("HasQuestionWithID", testCollection.HasQuestionWithID(400));
			Assert("HasQuestionWithID", testCollection.HasQuestionWithID(400, new ZDateTime(2011, 2, 1)));
			Assert("HasQuestionWithID", !testCollection.HasQuestionWithID(400, new ZDateTime(2011, 2, 2)));
			Assert("HasQuestionWithID", !testCollection.HasQuestionWithID(401, new ZDateTime(2011, 2, 1)));
		}

		public void TestGetQuestionWithID()
		{
			CMRCusEntryCPDecCollection testCollection = (CMRCusEntryCPDecCollection)GetCollectionToTest();
			CMRCusEntryCPDec item = (CMRCusEntryCPDec)GetNewElementToAddToTheCollection();
			item.ON_CPDecNum = 400;
			testCollection.Add(item);
			AssertEquals("HasQuestionWithID", item, testCollection.GetQuestionWithID(400));
		}

		public void TestAllowNew()
		{
			CMRCusEntryCPDecCollection testCollection = (CMRCusEntryCPDecCollection)GetCollectionToTest();
			AssertEquals("Should not allow new", false, testCollection.AllowNew);
		}

		public void TestCachedQuestionsAreNotRetrieved()
		{
			CMRCusEntryCPDecCollection testCollection = (CMRCusEntryCPDecCollection)GetCollectionToTest();
			CMRCusEntryCPDec normal = testCollection.AddNew();

			JobDeclaration declaration = JobDeclaration.New(Factory);
			EntryLine.Header.CH_JE = declaration.PK;

			CMRCusEntryCPDec cached = testCollection.AddNew();
			cached.ON_JE = declaration.PK;

			AssertEquals("Two items", 2, testCollection.Count);
			testCollection.Load();
			AssertEquals("One item", 1, testCollection.Count);
		}

		public void TestNewQuestionValidation()
		{
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			var collection = new CMRCusEntryCPDecCollection(consolidatedDeclaration);
			var newQuestion = collection.AddNew();
			AssertEquals("Consolidated Entry question validation is suspended", true, newQuestion.IsValidationSuspended);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			consolidatedDeclaration.JobDeclarations.Add(declaration);
			collection = new CMRCusEntryCPDecCollection(entryHeader);
			newQuestion = collection.AddNew();
			AssertEquals("Declaration entryheader question validation is suspended", true, newQuestion.IsValidationSuspended);

			var entryLine = entryHeader.MergedLines.AddNew();
			collection = new CMRCusEntryCPDecCollection(entryLine);
			newQuestion = collection.AddNew();
			AssertEquals("Declaration entryline question validation is suspended", true, newQuestion.IsValidationSuspended);
		}

		public void TestCollectionAttachedToConsolidatedDeclaration()
		{
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			var collection = new CMRCusEntryCPDecCollectionForTesting(consolidatedDeclaration);
			AssertEquals(true, collection.GetIsAttachedToConsolidatedDeclaration());

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntryStatus = ZString.Empty;
			collection = new CMRCusEntryCPDecCollectionForTesting(entryHeader);
			AssertEquals(false, collection.GetIsAttachedToConsolidatedDeclaration());

			var entryLine = entryHeader.AllEntryLines.AddNew();
			collection = new CMRCusEntryCPDecCollectionForTesting(entryLine);
			AssertEquals(false, collection.GetIsAttachedToConsolidatedDeclaration());

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			consolidatedDeclaration.JobDeclarations.Add(declaration);
			Factory.Save();

			var reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var reloadedEntryHeader = reloadedDeclaration.CustomsEntryHeaders[0];
			var newCollection = new CMRCusEntryCPDecCollectionForTesting(reloadedEntryHeader);
			AssertEquals(true, newCollection.GetIsAttachedToConsolidatedDeclaration());

			var newEntryLine = reloadedEntryHeader.MergedLines.AddNew();
			newCollection = new CMRCusEntryCPDecCollectionForTesting(newEntryLine);
			AssertEquals(true, newCollection.GetIsAttachedToConsolidatedDeclaration());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CMRCusEntryCPDecCollection(EntryLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(CMRCusEntryCPDec));
		}

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
					fEntryLine = entryHeader.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;
	}

	sealed class CMRCusEntryCPDecCollectionForTesting : CMRCusEntryCPDecCollection
	{
		public CMRCusEntryCPDecCollectionForTesting(ICPQAAttachee parent)
			: base(parent)
		{
		}

		public bool GetIsAttachedToConsolidatedDeclaration() => IsAttachedToConsolidatedDeclaration;
	}
}
