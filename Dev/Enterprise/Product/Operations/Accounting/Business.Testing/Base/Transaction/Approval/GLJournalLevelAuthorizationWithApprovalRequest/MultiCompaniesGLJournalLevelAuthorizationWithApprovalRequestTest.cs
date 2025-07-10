using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	[TestedType(typeof(MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequest))]
	class MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequestTest : GLJournalLevelAuthorizationWithApprovalRequestTest
	{
		public void TestApprovalRequestHasDescription()
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
			var settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings.Amount = 0;
			settings.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingTransactionApprovalGUIProvider>();
			var transaction = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			CreateTransactionLine(transaction, job, -100);

			var helper = new Mock<ITransactionApprovalHelper<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.OK);
			var securityProvider = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProvider.Object);
			securityProvider.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
			Func<GLJournal, bool> isLevelAuthorizationRequired = x => true;
			Func<GLJournal, bool> checkLevelSecurityRights = x => false;
			Func<GLJournalApprovalRequest, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
			Action<GLJournalApprovalRequest, GLJournal[]> initializeApprovalRequest = null;

			PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						new[] { transaction },
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);

			var approvalRequests = guiWrapper.Object.FactoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, null));

			AssertEquals(1, approvalRequests.Length);
			AssertEquals(transaction.AH_Desc, approvalRequests[0].XP_ReasonDescription);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingTransactionApprovalGUIProvider postingGUIProvider, GLJournal[] transactions,
			Tuple<
					Func<GLJournal, bool>,
					Func<GLJournal, bool>,
					Action<GLJournalApprovalRequest, GLJournal[]>,
					Func<GLJournalApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			var helper = new MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequest(postingGUIProvider, transactions.Last(), alwaysCreateApprovalRequest);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization();
		}

		readonly bool alwaysCreateApprovalRequest = true;
	}
}
