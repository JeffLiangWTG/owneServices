using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CPQAForm))]
	sealed class CPQAFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestValidateOnLoad()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryHeaderMessageStatusFilteredCollection entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			using (InheritedCPQAForm testForm = new InheritedCPQAForm(entryHeaders))
			{
				testForm.Show();
				AssertEquals("should have called ValidateEntries", true, testForm.ValidateEntriesCalled);
			}
		}

		public void TestOKButton_ClickWithQuestionsNotAnswered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question2 = entryHeader.Questions.AddNew();
			AssertEquals("There are questions not answered", false, entryHeader.Questions.AreAllCPDecQuestionsAnswered);
			CusEntryHeaderMessageStatusFilteredCollection entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				form.oKButton.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "There are questions that are not answered yet, or have warnings. Are you sure you wish to continue?";
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}
		}

		public void TestOKButton_ClickWithQuestionsAnswered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			CMRCusEntryCPDec question2 = entryHeader.Questions.AddNew();
			question2.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			AssertEquals("There are questions answered", true, entryHeader.Questions.AreAllCPDecQuestionsAnswered);
			CusEntryHeaderMessageStatusFilteredCollection entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				form.oKButton.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "There are questions that are not answered yet, or have warnings. Are you sure you wish to continue?";
				AssertEquals("Notification for CMR", false, userNotification.Contains(message));
			}
		}

		public void TestOKButton_ClickWithQuestionsAnsweredIncorrectly()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.AllEntryLines.AddNew();
			CMRCusEntryCPDec headerQuestion = entryHeader.Questions.AddNew();
			headerQuestion.ON_AnswerCode = "Yn";
			CusEntryHeaderMessageStatusFilteredCollection entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				form.oKButton.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "There are questions that are not answered correctly. Please fix the errors and try again!";
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}

			UnitTestUserNotification.Instance.ClearMessages();
			headerQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			CMRCusEntryCPDec lineQuestion = entryLine.Questions.AddNew();
			lineQuestion.ON_AnswerCode = "Yn";
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				form.oKButton.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "There are questions that are not answered correctly. Please fix the errors and try again!";
				AssertEquals("Notification for CMR", true, userNotification.Contains(message));
			}

			UnitTestUserNotification.Instance.ClearMessages();
			lineQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				form.oKButton.PerformClick();
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				string message = "There are questions that are not answered correctly. Please fix the errors and try again!";
				AssertEquals("Notification for CMR", false, userNotification.Contains(message));
			}
		}

		public void TestConsolidatedMemberDeclarationDoesNotShowLodgementQuestions()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			Factory.Save();
			var aggregatedDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			var aggregatedEntryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(aggregatedDeclaration.ActiveEntryHeaders);
			using (CPQAForm form = new CPQAForm(aggregatedEntryHeaders))
			{
				form.Show();
				var headerQAControl = form.FindSingleOrDefault<CPQAsForHeaderControl>("cpqAsForHeaderControl2");
				AssertEquals("Lodgement Questions does not show", true, headerQAControl.Visible);
				var messageLabel = form.declarationQuestionsGroupBox.FindSingleOrDefault<ZLabel>("LodgementQuestionsMessageLable");
				AssertNull("Lodgement Questions Message Lable", messageLabel);
			}

			var declaration = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			using (CPQAForm form = new CPQAForm(entryHeaders))
			{
				form.Show();
				var headerQAControl = form.FindSingleOrDefault<CPQAsForHeaderControl>("cpqAsForHeaderControl2");
				AssertEquals("Lodgement Questions does not show", false, headerQAControl.Visible);
				var messageLabel = form.declarationQuestionsGroupBox.FindSingleOrDefault<ZLabel>("LodgementQuestionsMessageLabel");
				var expectedMessage = "General Lodgement Questions are located in the Consolidated Entry.";
				AssertEquals("Label Message", expectedMessage, messageLabel.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			testDec = Factory.New<JobDeclaration>();
			CusEntryHeader header = testDec.CustomsEntryHeaders.AddNew();
			header.Questions.AddNew();
			CusEntryLine entryLine = header.MergedLines.AddNew();
			entryLine.Questions.AddNew();
			CusEntryHeaderMessageStatusFilteredCollection entryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(testDec.ActiveEntryHeaders);
			return new CPQAForm(entryHeaders);
		}

		JobDeclaration testDec;

		sealed class InheritedCPQAForm : CPQAForm
		{
			public InheritedCPQAForm(CusEntryHeaderMessageStatusFilteredCollection entryHeaders) : base(entryHeaders)
			{
			}

			public bool ValidateEntriesCalled;
			protected override void ValidateEntries()
			{
				ValidateEntriesCalled = true;
				base.ValidateEntries();
			}
		}
	}
}
