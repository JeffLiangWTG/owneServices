using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class GLJournalSecurityOverrideProviderTest : SecurityOverrideProviderWithApprovalRequestSupportTest<GLJournalSecurityOverrideProvider>
	{
		public void TestRequestLoginCredentials()
		{
			var testProvider = GetSecurityProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
			var security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			AssertNull("User presses Approval Request button.", security);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			AssertNull("User presses Cancel button.", security);

			var loginUserName = "User1";
			var loginPassword = "pass";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					((LoginForm)form).DoLoginForTest(loginUserName, loginPassword);
					ZFormModaliser.ResultToReturnFromShowDialog = ZFormModaliser.ResultToReturnFromShowDialog != DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
				});
			SecurityTestObject.CreateTestUser(true, Env.Security.None.Code, "USR", loginUserName, loginPassword);
			security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			AssertNotNull("User presses Ok button.", security);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			AssertNull("User presses Ok button and AllowOnTheSpotApprovalsOfGLJournals is off.", security);
			AssertEquals(@"The action cannot be performed because the Registry 'Allow On The Spot approvals for GL Journals' is set to No.

Please click on the Approval Request button to request approval instead.", UnitTestUserNotification.Instance.LastMessage.Text);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			AssertNotNull("User presses Ok button and AllowUsersToApproveOwnGLJournals is off, but not current user entered login details.", security);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			using (Env.Instance.SetTemporaryUserContext(loginUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				security = testProvider.RequestLoginCredentials_ForTestOnly(Env.Security.None);
			}
			AssertNull("User presses Ok button and AllowUsersToApproveOwnGLJournals is off and current user entered login details.", security);
			AssertEquals("The action cannot be performed because the Registry 'Allow Users to approve own Journals' is set to No.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetSecurityOverrideMessageCore()
		{
			var testProvider = GetSecurityProvider();

			var message = testProvider.GetSecurityOverrideMessageCore_ForTestOnly(Env.Security.None);
			AssertEquals("To override this security, a user with security rights to [None] must login. Please enter username and password details below.", message);

			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			message = testProvider.GetSecurityOverrideMessageCore_ForTestOnly(Env.Security.None);
			AssertEquals(@"To override this security, a user with security rights to [None] must login. Please enter username and password details below.
Note:
	the Registry 'Allow On The Spot approvals for GL Journals' is set to No.", message);

			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			message = testProvider.GetSecurityOverrideMessageCore_ForTestOnly(Env.Security.None);
			AssertEquals(@"To override this security, a user with security rights to [None] must login. Please enter username and password details below.
Note:
	the Registry 'Allow On The Spot approvals for GL Journals' is set to No
	the Registry 'Allow Users to approve own Journals' is set to No.", message);

			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			message = testProvider.GetSecurityOverrideMessageCore_ForTestOnly(Env.Security.None);
			AssertEquals(@"To override this security, a user with security rights to [None] must login. Please enter username and password details below.
Note:
	the Registry 'Allow Users to approve own Journals' is set to No.", message);
		}

		protected override bool ShouldPromptForGranted
		{
			get { return true; }
		}

		protected override GLJournalSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new GLJournalSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest);
		}

		protected override bool IsApprovalRequestButtonSupported
		{
			get { return true; }
		}
	}
}
