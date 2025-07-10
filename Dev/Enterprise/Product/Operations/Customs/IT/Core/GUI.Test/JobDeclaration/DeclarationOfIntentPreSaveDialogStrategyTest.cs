using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class DeclarationOfIntentPreSaveDialogStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DeclarationOfIntentPreSaveDialogStrategy(null));
		AssertNoExceptionThrown(() => new DeclarationOfIntentPreSaveDialogStrategy(doiRefresher));
	}

	public void TestRunPreSaveActionShouldNotAsk()
	{
		doiRefresher.ShouldAsk = false;
		ShowPreSaveDialog();
		Assert(!doiRefresher.HasBeenOverwritten);
	}

	public void TestRunPreSaveActionShouldAskAnswerNo()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

		doiRefresher.ShouldAsk = true;
		ShowPreSaveDialog();
		Assert(!doiRefresher.HasBeenOverwritten);
	}

	public void TestRunPreSaveActionShouldAskAnswerYes()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

		doiRefresher.ShouldAsk = true;
		ShowPreSaveDialog();
		Assert(doiRefresher.HasBeenOverwritten);
		AssertEquals("Do you want to update the data for document 01DI with a placeholder ('X')?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	void ShowPreSaveDialog() => new DeclarationOfIntentPreSaveDialogStrategy(doiRefresher).ShowPreSaveDialogs(ContinueWithSave.Yes);

	protected override void SetUp()
	{
		base.SetUp();
		doiRefresher = new DeclarationOfIntentRefresherForTest();
	}

	DeclarationOfIntentRefresherForTest doiRefresher;

	class DeclarationOfIntentRefresherForTest : IDeclarationOfIntentRefresher
	{
		public ZBool HasBeenOverwritten { get; private set; }
		public void OverwritePlaceholderSupportingDocumentValues()
		{
			HasBeenOverwritten = true;
		}

		public ZBool ShouldAsk { get; set; }
		public ZBool ShouldAskToOverwrite() => ShouldAsk;
	}
}
