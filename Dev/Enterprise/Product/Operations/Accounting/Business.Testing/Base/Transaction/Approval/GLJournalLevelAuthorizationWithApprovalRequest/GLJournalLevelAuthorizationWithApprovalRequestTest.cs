using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	[TestedType(typeof(GLJournalLevelAuthorizationWithApprovalRequest))]
	class GLJournalLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTest<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails, IPostingTransactionApprovalGUIProvider>
	{
		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new GLJournalLevelAuthorizationWithApprovalRequest(null, null, false);
			AssertEquals(false, levelAuthorization.IsMultipleApproverApplicable);
		}

		#region TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings

		public void TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings()
		{
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings(ApprovalAuthorizationSettingsStatus.AnyNewJournal);
			TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings(ApprovalAuthorizationSettingsStatus.Normal);
		}

		public void TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings_InDB()
		{
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings(ApprovalAuthorizationSettingsStatus.AnyExistingJournal);
		}

		void TestPopUpsWhenCreatingNewApprovalRequestInTheThresholdAuthorizationSettings(ApprovalAuthorizationSettingsStatus testStatus)
		{
			SetupNewApprovalAuthorizationSettings(testStatus);

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingTransactionApprovalGUIProvider>();
			var transaction = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			CreateTransactionLine(transaction, job, -100);
			if (testStatus == ApprovalAuthorizationSettingsStatus.AnyExistingJournal)
			{
				transaction.Factory.Save();
			}

			ZString approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			var helper = new Mock<ITransactionApprovalHelper<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>>();
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			var securityProvider = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProvider.Object);
			var isLevelAuthorithationRequiredValue = false;
			var checkLevelSecurityRightValue = false;
			GLJournalApprovalRequest lastInitializedRequest = null;
			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<GLJournal, bool> isLevelAuthorizationRequired = x =>
				{
					return isLevelAuthorithationRequiredValue;
				};
				Func<GLJournal, bool> checkLevelSecurityRights = x => !isLevelAuthorithationRequiredValue || checkLevelSecurityRightValue;
				Func<GLJournalApprovalRequest, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				Action<GLJournalApprovalRequest, GLJournal[]> initializeApprovalRequest = (x, y) =>
				{
					this.OnInitializeApprovalRequest(x, y);
					lastInitializedRequest = x;
				};
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						new[] { transaction },
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};

			var factoryForApprovalRequests = new BusinessObjectFactory();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(factoryForApprovalRequests);
			var initialApprovalRequestCount = factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length;
			string expectedMessage;
			var expectedCaption = GetExpectedMessageCaption();

			isLevelAuthorithationRequiredValue = true;
			guiWrapper.Setup(m => m.RollbackPosting());
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			var tempApprovalRequest = factoryForApprovalRequests.New<GLJournalApprovalRequest>();
			tempApprovalRequest.Initialize(transaction);
			expectedMessage = String.Format(
@"There is another request for this {0}. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?"
, tempApprovalRequest.ReferenceType.ToLower());
			tempApprovalRequest.Delete();
			helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			if (testStatus == ApprovalAuthorizationSettingsStatus.Normal)
			{
				securityProvider.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.OK);
			}

			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			helper.Verify();
			securityProvider.Verify();

			AssertEquals("Approval request should exist", initialApprovalRequestCount + 1, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);

			if (testStatus != ApprovalAuthorizationSettingsStatus.Normal)
			{
				AssertEquals(lastInitializedRequest.XP_ReasonDescription, "APPROVAL REQUEST FOR GENERAL LEDGER JOURNAL");
				securityProvider.Verify(m => m.ShouldApprovalRequestBeCreated, Times.Never);
				guiWrapper.Verify(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>()), Times.Never);
			}
		}

		#endregion

		public override void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			Assert("Currently Preview mode is only available in AP Invoice Approval module, it's not applicable here.", true);
		}

		public override void TestPerformTransactionLevelAuthorization_NoTransactionsAndPreview()
		{
			Assert("At least one transaction should be here always. Preview mode is not applicable here.", true);
		}

		void SetupNewApprovalAuthorizationSettings(ApprovalAuthorizationSettingsStatus setupStatus)
		{
			switch (setupStatus)
			{
				case ApprovalAuthorizationSettingsStatus.Normal:
					var header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", "OV", "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
					var header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", "TS", "ACCOUNT 1", Constants.AccountType.ProfitAndLossAccount, Constants.DebitCredit.Credit);
					var header3 = TestObjectCreator.CreateAccGLHeader("1111.11.33", "LI", "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

					GLJournalApprovalThresholdCollection newValue = new GLJournalApprovalThresholdCollection();
					GLJournalApprovalThreshold threshold1 = newValue.AddNew();
					threshold1.Type = "RSN";
					threshold1.ReportSection = "OV";

					var settings1 = threshold1.AuthorisationSettings.AddNew();
					settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
					settings1.Amount = 100.00m;
					settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired;

					var settings2 = threshold1.AuthorisationSettings.AddNew();
					settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
					settings2.Amount = 100.00m;
					settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

					GLJournalApprovalThreshold threshold2 = newValue.AddNew();
					threshold2.Type = "GLA";
					threshold2.GLAccount = header2.PK;

					var settings3 = threshold2.AuthorisationSettings.AddNew();
					settings3.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
					settings3.Amount = 100.00m;
					settings3.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

					var settings4 = threshold2.AuthorisationSettings.AddNew();
					settings4.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
					settings4.Amount = 100.00m;
					settings4.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;

					GLJournalApprovalThreshold threshold3 = newValue.AddNew();
					threshold3.Type = "ALL";

					var settings5 = threshold3.AuthorisationSettings.AddNew();
					settings5.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
					settings5.Amount = 100.00m;
					settings5.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

					var settings6 = threshold3.AuthorisationSettings.AddNew();
					settings6.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
					settings6.Amount = 100.00m;
					settings6.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

					AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
					break;

				case ApprovalAuthorizationSettingsStatus.AnyNewJournal:
					{
						var newAnyValue = new GLJournalApprovalThresholdCollection();
						var newThreshold = newAnyValue.AddNew();
						newThreshold.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
						AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newAnyValue);
					}
					break;

				case ApprovalAuthorizationSettingsStatus.AnyExistingJournal:
					{
						var newAnyValue = new GLJournalApprovalThresholdCollection();
						var newThreshold = newAnyValue.AddNew();
						newThreshold.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
						AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newAnyValue);
					}
					break;
			}
		}

		protected override void OnInitializeApprovalRequest(GLJournalApprovalRequest x, GLJournal[] y)
		{
			x.Initialize(y[0]);

			base.OnInitializeApprovalRequest(x, y);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingTransactionApprovalGUIProvider postingGUIProvider, GLJournal[] transactions,
			Tuple<
					Func<GLJournal, bool>,
					Func<GLJournal, bool>,
					Action<GLJournalApprovalRequest, GLJournal[]>,
					Func<GLJournalApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			var helper = new GLJournalLevelAuthorizationWithApprovalRequest(postingGUIProvider, transactions.Last(), alwaysCreateApprovalRequest);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization();
		}

		protected override GLJournal CreateTransactionHeader(ZString transactionNumber, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			return TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
		}

		protected override void CreateTransactionLine(GLJournal transaction, Job job, decimal localAmount)
		{
			TestObjectCreator.CreateGLJournalLine(transaction, Math.Abs(localAmount), localAmount > 0 ? DebitCredit.DR : DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
		}

		protected override string GetExpectedMessageCaption()
		{
			return "General Ledger Journal Approval Request";
		}

		protected override bool ShouldCreateRequestOnEveryPosting
		{
			get { return true; }
		}

		protected override ZString GetDefaultXP_ParentTableCode() => AccTransactionHeaderSchema.Constants.Prefix;

		protected override ZString GetRequestParentObjectNameInLowcase() => "journal";

		readonly bool alwaysCreateApprovalRequest;

		enum ApprovalAuthorizationSettingsStatus
		{
			Normal,
			AnyNewJournal,
			AnyExistingJournal
		}
	}
}
