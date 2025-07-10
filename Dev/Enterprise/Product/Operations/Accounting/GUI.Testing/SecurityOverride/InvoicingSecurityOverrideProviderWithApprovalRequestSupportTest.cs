using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoicingSecurityOverrideProviderWithApprovalRequestSupportTest : InvoicingSecurityOverrideProviderTest
	{
		protected override InvoicingSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest, keepLoginFormResultAfterFirstUserAnswer);
		}

		protected override bool IsApprovalRequestButtonSupported
		{
			get { return true; }
		}

		protected override bool IsKeepLoginFormResultAfterFirstUserAnswerSupported
		{
			get { return true; }
		}

		InvoicingSecurityOverrideProvider GetSecurityProviderWithMultipleApproverOption(bool supportMultipleApprover, ARCreditNoteApprovalRequest[] aRCreditNoteApprovalRequests = null)
		{
			return new InvoicingSecurityOverrideProvider(true, false, false, supportMultipleApprover, aRCreditNoteApprovalRequests);
		}

		public override void TestPromptForTemporaryAccessBasedOnRequiresMultipleApproversOption()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateShipment("S001");
			using (var job = testObjectCreator.CreateJob(shipment))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var request = Factory.New<ARCreditNoteApprovalRequest>();
				request.Initialize(new[] { arCreditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.Default });
				var provider = GetSecurityProviderWithMultipleApproverOption(false, new[] { request });
				provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
				Assert(!provider.RequiresTwoApprovers);
				AssertEquals("Should prompt LoginFormWithRequest when RequiresTwoApprovers is false", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"You do not have security rights to post a credit note/adjustment note for this amount.
A user with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have an authorized user enter their username and password below.

To queue a request for approval and postpone posting, press 'Approval Request' button.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
				provider = GetSecurityProviderWithMultipleApproverOption(true, new[] { request });
				provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
				Assert(provider.RequiresTwoApprovers);
				AssertEquals("Should prompt LoginFormWithRequestAndNoCredentials when RequiresTwoApprovers is true", typeof(LoginFormWithRequestAndNoCredentials), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Credit notes of this value require approval by two authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for 2nd approval.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.SequentialApprovers });
				provider = GetSecurityProviderWithMultipleApproverOption(true, new[] { request });
				provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
				Assert(provider.RequiresSequentialApprovals);
				AssertEquals("Should prompt LoginFormWithRequestAndNoCredentials when RequiresSequentialApprovals is true", typeof(LoginFormWithRequestAndNoCredentials), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Credit notes of this value require approval by multiple authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for additional approvals.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		public override void TestSecurityOverrideMessageForInvoiceLevels()
		{
			var firstLevelSecurity = new SecurityCheckpoint(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, SecurityCore.Captions.New, null, null, false);
			var secondLevelSecurity = new SecurityCheckpoint(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, SecurityCore.Captions.New, null, null, false);
			var testProvider = GetSecurityProvider();
			string expectedAdditionalText = testProvider.InvoiceLevelsSecurityOverrideMessage_ForTestOnly;
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(firstLevelSecurity));
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(secondLevelSecurity));

			testProvider = GetSecurityProvider(true);
			expectedAdditionalText += "\r\n\r\nTo queue a request for approval and postpone posting, press 'Approval Request' button.";
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(firstLevelSecurity));
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(secondLevelSecurity));
		}
	}
}
