using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AddLogCommentPopupForm))]
	public class AddLogCommentPopupFormTest : ZFormBasherTest
	{
		public void TestCloseButton()
		{
			using (AddLogCommentPopupForm form = GetNewForm())
			{
				form.Show();
				form.AcceptButton.PerformClick();
				Assert("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(DialogResult.None, form.DialogResult);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.BusinessEntity.LogComment = "meh";
				form.AcceptButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (AddLogCommentPopupForm form = GetNewForm())
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewForm();
		}

		AddLogCommentPopupForm GetNewForm()
		{
			AccQueryClaimBase claim = Factory.New<ARAccQueryClaim>();
			return new AddLogCommentPopupForm(claim);
		}
	}
}
