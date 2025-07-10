using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class SecurityOverrideProviderWithApprovalRequestSupportTest<SecurityProviderType> : TestCaseWithFactory
				where SecurityProviderType : SecurityOverrideProviderWithApprovalRequestSupport
	{
		public void TestKeepLoginFormResultAfterFirstUserAnswer()
		{
			var testProvider = GetSecurityProvider(keepLoginFormResultAfterFirstUserAnswer: false);
			var loginUserName = "User1";
			var loginPassword = "pass";
			var securityForTest = Env.Security.Payables;
			SecurityTestObject.CreateTestUser(true, securityForTest.Code, "USR", loginUserName, loginPassword);
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				((LoginForm)form).DoLoginForTest(loginUserName, loginPassword);
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
			Assert("User presses OK button.", security.IsAllowed);
			Assert("ShouldApprovalRequestBeCreated", !testProvider.ShouldApprovalRequestBeCreated);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
			security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
			Assert("User presses Approval Request button.", !security.IsAllowed);
			Assert("ShouldApprovalRequestBeCreated", testProvider.ShouldApprovalRequestBeCreated);

			testProvider = GetSecurityProvider(keepLoginFormResultAfterFirstUserAnswer: true);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
			Assert("User presses OK button.", security.IsAllowed);
			Assert("ShouldApprovalRequestBeCreated", !testProvider.ShouldApprovalRequestBeCreated);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
			security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
			if (IsKeepLoginFormResultAfterFirstUserAnswerSupported)
			{
				Assert("User presses OK button first time", security.IsAllowed);
				Assert("ShouldApprovalRequestBeCreated", !testProvider.ShouldApprovalRequestBeCreated);

				testProvider = GetSecurityProvider(keepLoginFormResultAfterFirstUserAnswer: true);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
				security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
				Assert("User presses Approval Request button.", !security.IsAllowed);
				Assert("ShouldApprovalRequestBeCreated", testProvider.ShouldApprovalRequestBeCreated);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(securityForTest);
				Assert("User presses Approval Request button first time.", !security.IsAllowed);
				Assert("ShouldApprovalRequestBeCreated", testProvider.ShouldApprovalRequestBeCreated);
			}
			else
			{
				Assert("User presses Approval Request button.", !security.IsAllowed);
				Assert("ShouldApprovalRequestBeCreated", testProvider.ShouldApprovalRequestBeCreated);
			}
		}

		public void TestShouldPromptForGranted()
		{
			var testProvider = GetSecurityProvider();
			AssertEquals(ShouldPromptForGranted, testProvider.ShouldPromptForGranted_ForTestOnly);
		}

		public void TestShouldApprovalRequestBeCreated()
		{
			var testProvider = GetSecurityProvider();
			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.Ignore;
			AssertEquals(true, testProvider.ShouldApprovalRequestBeCreated);

			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.OK;
			AssertEquals(false, testProvider.ShouldApprovalRequestBeCreated);

			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.Cancel;
			AssertEquals(false, testProvider.ShouldApprovalRequestBeCreated);
		}

		public void TestShouldApprovalRequestBeCreatedWhenAlwaysCreateApprovalRequestIsSet()
		{
			var testProvider = GetSecurityProvider(true, true);
			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.Ignore;
			AssertEquals(true, testProvider.ShouldApprovalRequestBeCreated);

			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.OK;
			AssertEquals(IsApprovalRequestButtonSupported, testProvider.ShouldApprovalRequestBeCreated);

			testProvider.LastLoginFormResult_ForTestOnly = DialogResult.Cancel;
			AssertEquals(IsApprovalRequestButtonSupported, testProvider.ShouldApprovalRequestBeCreated);
		}

		public virtual void TestSecurityOverrideMessage()
		{
			var testProvider = GetSecurityProvider();
			string expectedAdditionalText = "To override this security, a user with security rights to [None] must login. Please enter username and password details below.";
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.None));

			testProvider = GetSecurityProvider(true);
			if (IsApprovalRequestButtonSupported)
			{
				expectedAdditionalText += "\r\n\r\nTo queue a request for approval and postpone posting, press 'Approval Request' button.";
			}
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.None));
		}

		public void TestCreateNewLoginForm()
		{
			var testProvider = GetSecurityProvider();
			using (Form form = testProvider.CreateNewLoginForm_ForTestOnly())
			{
				AssertType(typeof(LoginForm), form);
			}

			testProvider = GetSecurityProvider(true);
			using (Form form = testProvider.CreateNewLoginForm_ForTestOnly())
			{
				if (IsApprovalRequestButtonSupported)
				{
					AssertType(typeof(LoginFormWithRequest), form);
				}
				else
				{
					AssertType(typeof(LoginForm), form);
				}
			}
		}

		protected abstract SecurityProviderType GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false);

		protected abstract bool ShouldPromptForGranted { get; }

		protected abstract bool IsApprovalRequestButtonSupported { get; }

		protected virtual bool IsKeepLoginFormResultAfterFirstUserAnswerSupported { get { return false; } }
	}
}
