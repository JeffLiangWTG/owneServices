using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class DeclarationWizardManagerTests : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			DeclarationWizardTestHelper.SetUpRefCusCodeListData(Factory);
		}
		public void TestEndToEndIM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 4, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "H1", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "IM", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);

			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H1 (Dec for release for free circulation or end-use)
	Style: IM (Import of Goods (All Other not covered by CO or EU))
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 9, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "H5", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "CO", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);

			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H5 (Dec for goods from the special fiscal territories)
	Style: CO (Import of Goods from a Special Territory of the Community)
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndEX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[2].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 4, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 5, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "B1", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "EX", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "1040000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);

			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: B1 (Export Standard Declaration or Re-export Standard Declaration)
	Style: EX (Export of Goods (All Other not covered by CO or EU))
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 1040 (Permanent Export)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndWithNoOptionsAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 9, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[2].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "I1", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "CO", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "F", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "5300000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);

			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: I1 (Import Simplified Dec)
	Style: CO (Import of Goods from a Special Territory of the Community)
	Substyle: F (Simplified declaration with regular use (pre-authorised) (Goods not arrived))
	Procedure: 53 (Entry to Temporary Admission)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndWithGoingBack()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 9, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[2].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.PreviousQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 9, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions()[2].Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[2].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = true;

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("DeclarationType", "H5", declaration.JE_DeclarationType);
			AssertEquals("MessageType", "IMP", declaration.JE_MessageType);
			AssertEquals("Entry Style", "CO", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);

			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H5 (Dec for goods from the special fiscal territories)
	Style: CO (Import of Goods from a Special Territory of the Community)
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndWithExistingEntryHeaderCreateNewCei()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var initialCEI = declaration.CusEntryInstruction.PK;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var cel = ceh.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cel.PK;

			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 4, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();
			var newInstruction = declaration.CustomsEntryInstructions[1];

			AssertEquals("Declaration Type", "H1", newInstruction.CEI_Style);
			AssertEquals("Entry Style", "IM", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", newInstruction.CEI_SubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);
			AssertEquals("Create new Entry Instruction", 2, declaration.CustomsEntryInstructions.Count);
			AssertEquals("Invoice line allocated to new instruction", declaration.InvoiceLines[0].JI_CEI, newInstruction.PK);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H1 (Dec for release for free circulation or end-use)
	Style: IM (Import of Goods (All Other not covered by CO or EU))
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)",
				declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, newInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndWithNoExistingEntryHeaderCreateNewCei()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var initialCEI = declaration.CusEntryInstruction.PK;

			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 4, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "H1", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "IM", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);
			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);
			AssertEquals("Invoice line allocated to initial instruction", declaration.InvoiceLines[0].JI_CEI, initialCEI);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H1 (Dec for release for free circulation or end-use)
	Style: IM (Import of Goods (All Other not covered by CO or EU))
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}

		public void TestEndToEndWithExistingEntryHeaderReuseExistingCei()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var initialCEI = declaration.CusEntryInstruction.PK;
			var ceh = declaration.CustomsEntryHeaders.AddNew();
			var cel = ceh.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cel.PK;

			var declarationWizardManager = new DeclarationWizardManager(declaration);

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 1 number", 1, question.QuestionNumber);
				AssertEquals("Question 1 text", "Where are your goods coming from/to?", question.QuestionText);
				AssertEquals("Question 1 incorrect number of options", 3, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 2 number", 2, question.QuestionNumber);
				AssertEquals("Question 2 text", "What kind of declaration are you doing?", question.QuestionText);
				AssertEquals("Question 2 incorrect number of options", 4, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 3 number", 3, question.QuestionNumber);
				AssertEquals("Question 3 text", "What options for this declaration type?", question.QuestionText);
				AssertEquals("Question 3 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 4 number", 4, question.QuestionNumber);
				AssertEquals("Question 4 text", "Have the goods arrived or not?", question.QuestionText);
				AssertEquals("Question 4 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 5 number", 5, question.QuestionNumber);
				AssertEquals("Question 5 text", "Create an additional entry instruction for the result?", question.QuestionText);
				AssertEquals("Question 5 incorrect number of options", 2, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[0].Selected = false;
			declarationWizardManager.DeclarationWizard.CurrentQuestion.Options[1].Selected = true;
			declarationWizardManager.DeclarationWizard.NextQuestion();

			CombineAssertions(() =>
			{
				var question = declarationWizardManager.DeclarationWizard.CurrentQuestion;
				AssertEquals("Question 6 number", 6, question.QuestionNumber);
				AssertEquals("Question 6 text", "Is your declaration full, supplementary or some other variety?", question.QuestionText);
				AssertEquals("Question 6 incorrect number of options", 6, declarationWizardManager.DeclarationWizard.GetOptions().Count);
				Assert("Correct option is selected", declarationWizardManager.DeclarationWizard.GetOptions().FirstOrDefault().Selected);
			});

			declarationWizardManager.FinishWizard();
			declarationWizardManager.PopulateDeclaration();

			AssertEquals("Declaration Type", "H1", declaration.JE_DeclarationType);
			AssertEquals("Entry Style", "IM", declaration.JE_EntryStyle);
			AssertEquals("Entry Sub Style", "A", declaration.JE_EntrySubStyle);
			AssertEquals("JI Procedure", "4000000", declaration.Invoices[0]?.InvoiceLines[0]?.JI_Procedure);
			AssertEquals("Create new Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);
			AssertEquals("Invoice line allocated to initial instruction", declaration.InvoiceLines[0].JI_CEI, initialCEI);

			AssertEquals("Summary Text", @"Your declaration is:

	Type: H1 (Dec for release for free circulation or end-use)
	Style: IM (Import of Goods (All Other not covered by CO or EU))
	Substyle: A (Standard customs declaration (Goods arrived))
	Procedure: 40 (Release to free circulation)", declarationWizardManager.DeclarationWizard.GetSummaryText(declaration, declaration.CusEntryInstruction, declarationWizardManager.DeclarationWizard.FilteredWizardItems().FirstOrDefault()));
		}
	}
}
