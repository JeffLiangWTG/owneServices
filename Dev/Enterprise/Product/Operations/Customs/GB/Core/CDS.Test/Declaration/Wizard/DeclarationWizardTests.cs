using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class DeclarationWizardTests : TestCaseWithFactory
	{
		DeclarationWizard declarationWizard;
		protected override void SetUp()
		{
			base.SetUp();

			declarationWizard = new DeclarationWizard(Factory);
			DeclarationWizardTestHelper.SetUpRefCusCodeListData(Factory);
		}

		public void TestFilteredWizardItemsIM()
		{
			declarationWizard.Filter[0] = new DeclarationWizardFilter { QuestionNumber = 1, Filter = "IM" };
			AssertEquals("Filtered items contains wrong value", 25, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[1] = new DeclarationWizardFilter { QuestionNumber = 2, Filter = "FC" };
			AssertEquals("Filtered items contains wrong value", 19, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[2] = new DeclarationWizardFilter { QuestionNumber = 3, Filter = "EXC" };
			AssertEquals("Filtered items contains wrong value", 2, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[3] = new DeclarationWizardFilter { QuestionNumber = 6, Filter = "A" };
			AssertEquals("Filtered items contains wrong value", 1, declarationWizard.FilteredWizardItems().Length);
		}

		public void TestFilteredWizardItemsCO()
		{
			declarationWizard.Filter[0] = new DeclarationWizardFilter { QuestionNumber = 1, Filter = "CO" };
			AssertEquals("Filtered items contains wrong value", 37, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[1] = new DeclarationWizardFilter { QuestionNumber = 2, Filter = "OP2" };
			AssertEquals("Filtered items contains wrong value", 2, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[2] = new DeclarationWizardFilter { QuestionNumber = 5, Filter = "A" };
			AssertEquals("Filtered items contains wrong value", 2, declarationWizard.FilteredWizardItems().Length);
		}

		public void TestFilteredWizardItemsEX()
		{
			declarationWizard.Filter[0] = new DeclarationWizardFilter { QuestionNumber = 1, Filter = "EX" };
			AssertEquals("Filtered items contains wrong value", 25, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[1] = new DeclarationWizardFilter { QuestionNumber = 2, Filter = "OP1" };
			AssertEquals("Filtered items contains wrong value", 4, declarationWizard.FilteredWizardItems().Length);

			declarationWizard.Filter[2] = new DeclarationWizardFilter { QuestionNumber = 5, Filter = "A" };
			AssertEquals("Filtered items contains wrong value", 4, declarationWizard.FilteredWizardItems().Length);
		}

		public void TestIsFirstQuestion()
		{
			declarationWizard.QuestionNumber = 1;
			Assert("Should be first question", declarationWizard.IsFirstQuestion());

			declarationWizard.QuestionNumber = 3;
			Assert("Should not be first question", !declarationWizard.IsFirstQuestion());
		}

		public void TestIsLastQuestion()
		{
			declarationWizard.QuestionNumber = 6;
			Assert("Should be first question", declarationWizard.IsLastQuestion());

			declarationWizard.QuestionNumber = 3;
			Assert("Should not be first question", !declarationWizard.IsLastQuestion());
		}

		public void TestIsFirstOrLastQuestion()
		{
			declarationWizard.QuestionNumber = 1;
			Assert("Should be first or last question", declarationWizard.IsFirstOrLastQuestion());

			declarationWizard.QuestionNumber = 6;
			Assert("Should be first or last question", declarationWizard.IsFirstOrLastQuestion());

			declarationWizard.QuestionNumber = 3;
			Assert("Should not be first or last question", !declarationWizard.IsFirstOrLastQuestion());
		}

		public void TestNextQuestion()
		{
			AssertEquals("Question number", 1, declarationWizard.QuestionNumber);

			declarationWizard.NextQuestion();
			AssertEquals("Question number", 2, declarationWizard.QuestionNumber);

			declarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizard.CurrentQuestion.Options[2].Selected = true;
			declarationWizard.NextQuestion();
			AssertEquals("Question number", 4, declarationWizard.QuestionNumber);

			declarationWizard.NextQuestion();
			AssertEquals("Question number", 5, declarationWizard.QuestionNumber);

			declarationWizard.NextQuestion();
			AssertEquals("Question number", 6, declarationWizard.QuestionNumber);

			declarationWizard.FinishWizard();

			AssertEquals("Incorrect filter count", 5, declarationWizard.Filter.Count(x => x != null));
		}

		public void TestPreviousQuestion()
		{
			AssertEquals("Question number", 1, declarationWizard.QuestionNumber);

			declarationWizard.NextQuestion();
			AssertEquals("Question number", 2, declarationWizard.QuestionNumber);

			declarationWizard.PreviousQuestion();
			AssertEquals("Question number", 1, declarationWizard.QuestionNumber);

			declarationWizard.NextQuestion();
			declarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizard.CurrentQuestion.Options[2].Selected = true;

			declarationWizard.NextQuestion();
			AssertEquals("Question number", 4, declarationWizard.QuestionNumber);
			declarationWizard.PreviousQuestion();
			AssertEquals("Question number", 2, declarationWizard.QuestionNumber);
			AssertEquals("Incorrect filter count", 1, declarationWizard.Filter.Count(x => x != null));
		}

		public void TestGetOptions()
		{
			var options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 3, options.Count);

			declarationWizard.NextQuestion();
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 4, options.Count);

			declarationWizard.NextQuestion();
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 6, options.Count);

			declarationWizard.NextQuestion();
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 2, options.Count);

			declarationWizard.NextQuestion();
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 2, options.Count);

			declarationWizard.IsMultiInstructionDeclaration = true;
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 1, options.Count);

			declarationWizard.NextQuestion();
			options = declarationWizard.GetOptions();
			AssertEquals("Incorrect options", 6, options.Count);
		}
	}
}
