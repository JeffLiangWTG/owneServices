using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Term
{
	[GuiTest]
	class TermsAcknowledgementCheckerTest : TestCase
	{
		public void TestCheckTermAcknowledged()
		{
			using (var form = new ZForm())
			{
				form.Show();
				var checker = new TermsAcknowledgementChecker(new TermsForTest(), form);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var result = checker.CheckTermAcknowledged().GetAwaiter().GetResult();
				AssertEquals("Enterprise.ZArchitecture.GUI.TermsProgressForm", ZFormModaliser.LastFormShownForTest.GetType().FullName);
				AssertEquals(false, ZFormModaliser.LastFormShownForTest.Visible);
				AssertEquals(true, ZFormModaliser.LastFormShownForTest.IsDisposed);
				AssertEquals(true, result);
			}
		}

		public void TestCheckTermAcknowledged_NoParentForm()
		{
			using (var form = new ZForm())
			{
				form.Show();
				var checker = new TermsAcknowledgementChecker(new TermsForTest(), null);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var result = checker.CheckTermAcknowledged().GetAwaiter().GetResult();
				AssertNull("Should not show progress form when parent form is null", ZFormModaliser.LastFormShownForTest);
				AssertEquals(true, result);
			}
		}

		public void TestCheckTermAcknowledged_CurrentUserNotAllowed()
		{
			using (var form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				var term = new TermsForTest()
				{
					IsCurrentUserAllowedToAcknowledgeAgreement_Exposed = false,
					ErrorMessageForAcknowledgementNotAllowed_Exposed = "Current user not allowed to...",
				};
				var checker = new TermsAcknowledgementChecker(term, form);
				var result = checker.CheckTermAcknowledged().GetAwaiter().GetResult();
				AssertEquals("Enterprise.ZArchitecture.GUI.TermsProgressForm", ZFormModaliser.LastFormShownForTest.GetType().FullName);
				AssertEquals("Current user not allowed to...", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, result);
			}
		}

		public void TestCheckTermAcknowledged_Cancel()
		{
			using (var form = new ZForm())
			{
				form.Show();
				var checker = new TermsAcknowledgementChecker(new TermsForTest(), form);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var result = checker.CheckTermAcknowledged().GetAwaiter().GetResult();
				AssertEquals(false, result);
			}
		}

		public void TestTestCheckTermAcknowledged_HasAcknowledged()
		{
			var term = new TermsForTest();
			using (var form = new ZForm())
			{
				form.Show();
				var checker = new TermsAcknowledgementChecker(term, form);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var result = checker.CheckTermAcknowledged().GetAwaiter().GetResult();
				AssertEquals(true, result);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
		}

		class TermsForTest : BaseTermsAgreement
		{
			public override string Type => string.Empty;

			protected override bool IsLocalDisplayConditionSatisfied() => true;
			public override async Task<bool> TryLoadTerm() => await Task.FromResult(true);
			public override async Task TryPostAcknowledgement() => await Task.FromResult(true);
			public override bool IsCurrentUserAllowedToAcknowledgeAgreement => IsCurrentUserAllowedToAcknowledgeAgreement_Exposed;
			public override string ErrorMessageForAcknowledgementNotAllowed => ErrorMessageForAcknowledgementNotAllowed_Exposed;
			public bool IsCurrentUserAllowedToAcknowledgeAgreement_Exposed { get; set; } = true;
			public string ErrorMessageForAcknowledgementNotAllowed_Exposed { get; set; }
		}

		#endregion
	}
}
