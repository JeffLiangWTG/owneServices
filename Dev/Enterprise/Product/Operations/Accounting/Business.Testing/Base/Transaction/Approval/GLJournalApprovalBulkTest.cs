using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	[TestedType(typeof(GLJournalApprovalBulk))]
	class NewGLJournalApprovalBulkTest : TransactionApprovalBulkTest<GLJournalApprovalBulk, GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails, GLJournal>
	{
		[TestDate(2011, 03, 10)]
		public void TestApproveWhenOwnJournalApprovalIsNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusChange(x => x.Approve(), Core.Constants.GenApprovalRequestApprovalStatus.Approved, allowToApproveOwnRequest: false, loginUnderAnotherUser: false);
		}

		[TestDate(2011, 03, 10)]
		public void TestApproveWhenOwnJournalApprovalIsNotAllowed_UnderAnotherUser()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusChange(x => x.Approve(), Core.Constants.GenApprovalRequestApprovalStatus.Approved, allowToApproveOwnRequest: false, loginUnderAnotherUser: true);
		}

		[TestDate(2011, 03, 10)]
		public void TestRejectWhenOwnJournalApprovalIsNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusChange(x => x.Reject(), Core.Constants.GenApprovalRequestApprovalStatus.Rejected, allowToApproveOwnRequest: false, loginUnderAnotherUser: false);
		}

		[TestDate(2011, 03, 10)]
		public void TestRejectWhenOwnJournalApprovalIsNotAllowed_UnderAnotherUser()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusChange(x => x.Reject(), Core.Constants.GenApprovalRequestApprovalStatus.Rejected, allowToApproveOwnRequest: false, loginUnderAnotherUser: true);
		}

		public virtual void TestPostApprovalForNewJournalRelinkEdocsBackToJournal()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();

			var approvalRequest = SetupRequestForFirstApprovalLevel();

			((IDocManagerSupport)approvalRequest).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
			Factory.ChildFactories.Add((BusinessObjectFactory)((IDocManagerSupport)approvalRequest).DocManagerInfo.MasterFactory);
			Factory.Save();

			DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			StorageMain storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, approvalRequest.PK));
			AssertNotNull("Precondition - eDocs link to request", storageMain);
			AssertEquals("GJR", storageMain.SM_Type);

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk.Approve();
			Factory.Save();

			var mockGUIProvider = GetIPostingTransactionApprovalGUIProvider();
			approvalRequestBulk.PostApprovalsAndRemovePosted(mockGUIProvider.Object);
			mockGUIProvider.Verify();

			DocumentFactory newDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			storageMain = newDocumentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, approvalRequest.PK));
			AssertNotNull("There should still be a storageMain to the request", storageMain);
			AssertEquals("Expect no eDdocs on request", 0, storageMain.eDocs.Count);
			storageMain = newDocumentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, approvalRequest.XP_ParentID));
			AssertNotNull("Journal should have storageMain", storageMain);
			AssertEquals("GLJ", storageMain.SM_Type);
			AssertEquals("Expect 2 eDdocs on journal", 2, storageMain.eDocs.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestPostApprovalForNewJournalRelinkEdocsBackToJournal_ThrowsLastExternalStorageException()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
				Factory.Save();

				var approvalRequest = SetupRequestForFirstApprovalLevel();

				((IDocManagerSupport)approvalRequest).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
				Factory.ChildFactories.Add((BusinessObjectFactory)((IDocManagerSupport)approvalRequest).DocManagerInfo.MasterFactory);
				Factory.Save();

				DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, approvalRequest.PK));
				AssertNotNull("Precondition - eDocs link to request", storageMain);
				AssertEquals("GJR", storageMain.SM_Type);

				// Setup to throw LastExternalStorageException
				byte[] tIFfileContents = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif");
				StorageDocs document = (StorageDocs)storageMain.AddFileOrDocument(tIFfileContents, new AddFileOrDocumentDto
				{
					FileName = "small.tif",
					DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument,
				});
				document.SC_ImageData = ZBlob.Empty;
				storageMain.Factory.Save();

				SetupSecurity();
				var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
				var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
				approvalRequestBulk.Approve();
				Factory.Save();

				var mockGUIProvider = GetIPostingTransactionApprovalGUIProvider();
				AssertNoExceptionThrown("S3 exception should be handled", () => approvalRequestBulk.PostApprovalsAndRemovePosted(mockGUIProvider.Object));
				mockGUIProvider.Verify();

				AssertHasRowErrorContaining(approvalRequest, AllocateErrorMessage.LastExternalStorageExceptionMessage);

				DocumentFactory newDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				storageMain = newDocumentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, approvalRequest.PK));
				AssertNotNull("There should still be a storageMain to the request", storageMain);
				AssertNotEquals("Expect eDocs still on request because of exception", 0, storageMain.eDocs.Count);
			}
		}

		public virtual void TestApproveDoesNotSaveJournal()
		{
			var approvalRequest = SetupRequestForFirstApprovalLevel();
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk.Approve();
			Factory.Save();
			AssertEquals("approvalRequest.XP_ApprovalStatus", Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);
			AssertEquals("Journal was not saved", ZGuid.Empty, approvalRequest.XP_ParentID);
			AssertEquals("Journal was not saved", ZString.Empty, approvalRequest.PostingDetails.Journal.AH_TransactionNum);
		}

		public virtual void TestCancelDoesNotSaveJournal()
		{
			var approvalRequest = SetupRequestForFirstApprovalLevel();
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk.Cancel();
			Factory.Save();
			AssertEquals("approvalRequest.XP_ApprovalStatus", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
			AssertEquals("Journal was not saved", ZGuid.Empty, approvalRequest.XP_ParentID);
			AssertEquals("Journal was not saved", ZString.Empty, approvalRequest.PostingDetails.Journal.AH_TransactionNum);
		}

		public virtual void TestRejectDoesNotSaveJournal()
		{
			var approvalRequest = SetupRequestForFirstApprovalLevel();
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk.Reject();
			Factory.Save();
			AssertEquals("approvalRequest.XP_ApprovalStatus", Core.Constants.GenApprovalRequestApprovalStatus.Rejected, approvalRequest.XP_ApprovalStatus);
			AssertEquals("Journal was not saved", ZGuid.Empty, approvalRequest.XP_ParentID);
			AssertEquals("Journal was not saved", ZString.Empty, approvalRequest.PostingDetails.Journal.AH_TransactionNum);
		}

		protected override ZString ParentName => "journal";

		protected override GLJournalApprovalRequest GetNewApprovalRequest()
		{
			return GetNewApprovalRequest(CreateJournal(10));
		}

		protected GLJournalApprovalRequest GetNewApprovalRequest(GLJournal journal)
		{
			var request = base.GetNewApprovalRequest();
			request.Initialize(journal);

			return request;
		}

		protected override GLJournal GetParentFromApprovalRequest(GLJournalApprovalRequest request)
		{
			return request.GetLinkedJournal().journal;
		}

		protected void ModifyJournal(GLJournal parent)
		{
			TestObjectCreator.CreateGLJournalLine(parent, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(parent, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
		}

		protected void AssertJournalIsModified(string message, GLJournal parent, bool isModified)
		{
			AssertEquals(message, isModified ? 4 : 2, parent.Lines.Count);
		}

		protected override void ModifyParentToHaveValidationError(GLJournal parent)
		{
			var newFactory = new BusinessObjectFactory();
			var glAccount = newFactory.Load<AccGLHeader>(parent.Lines[0].AL_AG);
			glAccount.AG_IsActive = false;
			newFactory.Save();
		}

		protected override string GetErrorMessageForParentWithValidationError()
		{
			return
@"GL Post To Account: This GL Post To Account is inactive - it may not be used.
GL Post To Account: This account is currently marked as inactive";
		}

		protected override void AssertParentIsPosted(string message, GLJournal parent, bool isPosted)
		{
			AssertEquals(message, isPosted, parent.IsInDatabase);
		}

		protected override GLJournalApprovalRequest SetupRequestForFirstApprovalLevel()
		{
			var journal = CreateJournal(100);
			var request = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			request.Initialize(journal);

			return request;
		}

		protected override GLJournalApprovalRequest SetupRequestForSecondApprovalLevel(bool usingNonCurrentBranchDepartment = false)
		{
			var journal = CreateJournal(300);
			var request = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			request.Initialize(journal);

			return request;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewApprovalBulk(new DefaultAccessSecurityProvider(), Factory.NewWithValidTestData<GLJournalApprovalRequest>());
		}

		protected override GLJournalApprovalBulk GetNewApprovalBulk(ISecurityOverrideProvider interactiveSecurityOverrideProvider, params GLJournalApprovalRequest[] approvalRequests)
		{
			return new GLJournalApprovalBulk(Factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override void PostApprovalsAndRemovePosted(GLJournalApprovalBulk approvalRequestBulk, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			approvalRequestBulk.PostApprovalsAndRemovePosted(guiProvider);
		}

		protected override void SetupSecurity(bool disallowSecondLevel = false, bool disallowTwoLevels = false, Guid? branchPK = null, Guid? departmentPK = null)
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = !disallowTwoLevels;
			Env.Security.GeneralLedgerJournal_SecondApproval.IsAllowed = !disallowSecondLevel && !disallowTwoLevels;

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
			var settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings.Amount = 200;
			settings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			settings.Amount = 200;
			settings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			SetGLJournalApprovalThresholdSetupValue(newValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, Env.Security.GeneralLedgerJournals.Code, "tst", "newuser", "password");
		}

		protected virtual GLJournal CreateJournal(decimal amount)
		{
			var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var journal = newFactoryTestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			newFactoryTestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			newFactoryTestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			return journal;
		}

		protected virtual void SetGLJournalApprovalThresholdSetupValue(GLJournalApprovalThresholdCollection newValue)
		{
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}
	}

	[TestedType(typeof(GLJournalApprovalBulk))]
	sealed class SavedGLJournalApprovalBulkTest : NewGLJournalApprovalBulkTest
	{
		public override void TestParentPostedAndRemoveFromApprovals()
		{
			SetUpTestPeriods();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };

			var mockGUIProvider = GetIPostingTransactionApprovalGUIProvider();

			var parent = CreateJournal(10);
			ModifyJournal(parent);
			var approvalRequest = GetNewApprovalRequest(parent);

			AssertJournalIsModified("The initial parent should be modified.", parent, true);
			parent = new BusinessObjectFactory().Load<GLJournal>(parent.PK);
			AssertJournalIsModified("The reloaded parent status should not be modified before posting.", parent, false);

			var approvalRequestBulk2 = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk2.Approve();
			Factory.Save();

			PostApprovalsAndRemovePosted(approvalRequestBulk2, mockGUIProvider.Object);
			mockGUIProvider.Verify();

			parent = GetParentFromApprovalRequest(approvalRequest);
			AssertJournalIsModified("The reloaded parent status should be modified after posting.", parent, true);
		}

		public void TestAggregateOnPostingApprovals()
		{
			SetUpTestPeriods();
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var journalLine1 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			var journalLine2 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var approvalRequest = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			approvalRequest.Initialize(journal);
			approvalRequest.XP_SystemCreateUser = "NIL";
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);

			var mockGUIProvider = GetIPostingTransactionApprovalGUIProvider();

			AssertEquals("PreCondition: Should be 1 approval req", approvalRequestBulk.Approvals.Count, 1);
			approvalRequestBulk.Approve();
			Factory.Save();

			AssertEquals("PreCondition: approval should not be removed before successfully posted.", approvalRequestBulk.Approvals.Count, 1);
			AssertEquals("PreCondition: Approval request should be approved.", Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequest.XP_ApprovalStatus);

			approvalRequestBulk.PostApprovalsAndRemovePosted(mockGUIProvider.Object);
			mockGUIProvider.Verify();

			AssertEquals("PreCondition: approval should be removed after approving.", approvalRequestBulk.Approvals.Count, 0);
			AssertEquals("PreCondition: Approval request should be posted.", Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest.XP_ApprovalStatus);

			var aggregate1 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestObjectCreator.ExchangeGainLossControlAccount.PK));
			AssertEquals("Aggregation succeed for ExchangeGainLossControlAccount account", 1, aggregate1.Length);
			AssertEquals("Amount should match with journal line", journalLine1.AL_LineAmount, aggregate1[0].AA_Amount);

			var aggregate2 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK));
			AssertEquals("Aggregation succeed for ExchangeGainLossAdjustmentAccount account", 1, aggregate2.Length);
			AssertEquals("Amount should match with journal line", journalLine2.AL_LineAmount, aggregate2[0].AA_Amount);
		}

		public void TestAggregateLoadsOriginalJournalLinsOnPostingApprovals()
		{
			SetUpTestPeriods();
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var journalLine1 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			var journalLine2 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var approvalRequest1 = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			approvalRequest1.Initialize(journal);
			approvalRequest1.XP_SystemCreateUser = "NIL";
			Factory.Save();

			SetupSecurity();
			var mockSecurityProvider = new Mock<SecurityOverrideProvider> { CallBase = true };
			var approvalRequestBulk1 = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest1);
			var mockGUIProvider = GetIPostingTransactionApprovalGUIProvider();

			approvalRequestBulk1.Approve();
			Factory.Save();
			approvalRequestBulk1.PostApprovalsAndRemovePosted(mockGUIProvider.Object);
			mockGUIProvider.Verify();

			var aggregate1 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestObjectCreator.ExchangeGainLossControlAccount.PK));
			AssertEquals("Aggregation succeed for ExchangeGainLossControlAccount account", 1, aggregate1.Length);

			var newFactory = new BusinessObjectFactory();
			var approvalRequest2 = newFactory.NewWithValidTestData<GLJournalApprovalRequest>();
			journalLine1.UnsignedLocalLineAmount = 2000;
			journalLine2.UnsignedLocalLineAmount = 2000;
			approvalRequest2.Initialize(journal);
			newFactory.Save();

			var approvalRequestBulk2 = new GLJournalApprovalBulk(newFactory, new DefaultAccessSecurityProvider(), approvalRequest2);
			approvalRequestBulk2.Approve();
			newFactory.Save();

			approvalRequestBulk2.PostApprovalsAndRemovePosted(mockGUIProvider.Object);
			mockGUIProvider.Verify();

			aggregate1 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, TestObjectCreator.ExchangeGainLossControlAccount.PK));
			AssertEquals("Aggregation succeed for ExchangeGainLossControlAccount account", 2, aggregate1.Length);
		}

		public override void TestPostApprovalForNewJournalRelinkEdocsBackToJournal()
		{
			Assert("Not applicable for saved journals.", true);
		}

		public override void TestPostApprovalForNewJournalRelinkEdocsBackToJournal_ThrowsLastExternalStorageException()
		{
			Assert("Not applicable for saved journals.", true);
		}

		public override void TestApproveDoesNotSaveJournal()
		{
			Assert("Not applicable for saved journals.", true);
		}

		public override void TestCancelDoesNotSaveJournal()
		{
			Assert("Not applicable for saved journals.", true);
		}

		public override void TestRejectDoesNotSaveJournal()
		{
			Assert("Not applicable for saved journals.", true);
		}

		protected override GLJournal CreateJournal(decimal amount)
		{
			var journal = base.CreateJournal(amount);
			journal.Factory.Save();

			return journal;
		}

		protected override void SetGLJournalApprovalThresholdSetupValue(GLJournalApprovalThresholdCollection newValue)
		{
			AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}

		protected override string GetErrorMessageForParentWithValidationError()
		{
			return "GL Post To Account: This account is currently marked as inactive";
		}
	}
}
