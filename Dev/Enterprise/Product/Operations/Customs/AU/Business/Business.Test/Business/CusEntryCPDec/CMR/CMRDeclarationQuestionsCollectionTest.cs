using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRDeclarationQuestionsCollection))]
	public class CMRDeclarationQuestionsCollectionTest : BusinessObjectCollectionTestCase
	{
		//public void TestTableCodeIsSet()
		//{
		//  AUOrgSupplierPart Product = Factory.New<AUOrgSupplierPart>();
		//  CMRDeclarationQuestionsCollection Collection = new CMRDeclarationQuestionsCollection(Product);
		//  CMRCusEntryCPDec NewBizO = Collection.AddNew();
		//  AssertEquals("Parent table Code", "OP", NewBizO.ON_ParentTableCode);

		//  Classification Class = Factory.New<Classification>();
		//  Collection = new CMRDeclarationQuestionsCollection(Class);
		//  NewBizO = Collection.AddNew();
		//  AssertEquals("Parent table code for classification", "CC", NewBizO.ON_ParentTableCode);
		//}

		//public void TestAreCPQuestionsAnswered()
		//{
		//  CMRDeclarationQuestionsCollection TestCollection = (CMRDeclarationQuestionsCollection)GetCollectionToTest();
		//  CMRCusEntryCPDec Question = TestCollection.AddNew();
		//  CMRCusEntryCPDec Question2 = TestCollection.AddNew();
		//  AssertEquals("There are questions not answered", false, TestCollection.AreAllCPDecQuestionsAnswered);

		//  Question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
		//  AssertEquals("Not All questions answered", false, TestCollection.AreAllCPDecQuestionsAnswered);

		//  Question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
		//  AssertEquals("All questions answered", true, TestCollection.AreAllCPDecQuestionsAnswered);
		//}

		public void TestGetQuestionWithID()
		{
			CMRDeclarationQuestionsCollection testCollection = (CMRDeclarationQuestionsCollection)GetCollectionToTest();
			CMRCusEntryCPDec item = (CMRCusEntryCPDec)GetNewElementToAddToTheCollection();
			item.ON_CPDecNum = 400;
			testCollection.Add(item);
			AssertEquals("HasQuestionWithID", item, testCollection.GetQuestionWithID(400));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CMRDeclarationQuestionsCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(CMRCusEntryCPDec));
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
