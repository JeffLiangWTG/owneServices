using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	[TestedType(typeof(APInvoiceChargesLevelAuthorizationWithApprovalRequest))]
	class APInvoiceChargesLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTest<APInvoiceCharges, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails, IPostingJobTransactionsApprovalGUIProvider>
	{
		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new APInvoiceChargesLevelAuthorizationWithApprovalRequest(new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "JH", null));
			AssertEquals(false, levelAuthorization.IsMultipleApproverApplicable);
		}

		[ExpectNoExceptions]
		public void TestProcessExistingRequestsWithoutShowingMessages()
		{
			SetupAuthorizationRegistry();

			var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			if (user == null)
			{
				SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, testUserCode, testUserLogin, "password");
			}
			using (Env.SetTemporaryUserContext(testUserLogin, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
				var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(false);
				guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapper
					.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
					.Returns(securityProviderMock.Object);
				guiWrapper.Verify(
					m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(),
						It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>()), Times.Never);
				var helper = new Mock<ITransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>>();
				helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
				helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(true);
				helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(true);
				helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(true);
				helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(true);
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
				var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 1, TestObjectCreator.Creditor1, null, 0, null);
				invoiceCharges.Charges.Add(charge);
				var result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
				guiWrapper.VerifyAll();
				helper.VerifyAll();
			}
		}

		public void TestCanceledRequestIsNotCollectedInSomeCases()
		{
			SetupAuthorizationRegistry();

			Mock<IPostingJobTransactionsApprovalGUIProvider> guiWrapper;
			APInvoiceCharges transaction1;
			APInvoiceCharges transaction2;
			APInvoiceCharges transaction3;
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			CreateAPInvoiceCharges(job, out guiWrapper, out transaction1, out transaction2, out transaction3);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(transaction3, job.PK, job.TablePrefix);
			Factory.Save();

			ReleaseFactory();

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(true);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(false);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			var result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2 });
			AssertEquals("For preview we shouldn't add canceled requests.", 2, result.Length);
			AssertEquals(transaction1, result[0]);
			AssertEquals(transaction2, result[1]);

			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
			var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			if (user == null)
			{
				SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, testUserCode, testUserLogin, "password");
			}
			using (Env.SetTemporaryUserContext(testUserLogin, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2 });
				AssertEquals("When we continuer posting with request we should add canceled request", 3, result.Length);
				AssertEquals(transaction1, result[0]);
				AssertEquals(transaction2, result[1]);
				AssertEquals("tran3", result[2].InvoiceNumber);
			}

			securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false);
			using (Env.SetTemporaryUserContext(testUserLogin, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2 });
				AssertEquals("When there is not invoices to continue posting we shouldn't add canceled requests.", 2, result.Length);
				AssertEquals(transaction1, result[0]);
				AssertEquals(transaction2, result[1]);
			}

			result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2 });
			AssertEquals("When we post invoices we should add canceled request", 3, result.Length);
			AssertEquals(transaction1, result[0]);
			AssertEquals(transaction2, result[1]);
			AssertEquals("tran3", result[2].InvoiceNumber);

			guiWrapper.Setup(m => m.RequestToCompare).Returns(Factory.New<APInvoiceChargesApprovalRequest>());
			var helper = new Mock<ITransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2 });
			AssertEquals("When selected request is posted we shouldn't add canceled requests.", 2, result.Length);
			AssertEquals(transaction1, result[0]);
			AssertEquals(transaction2, result[1]);
		}

		public void TestSecurityOverrideProviderIsSharedAcrossInvoices()
		{
			SetupAuthorizationRegistry();

			Mock<IPostingJobTransactionsApprovalGUIProvider> guiWrapper;
			APInvoiceCharges transaction1;
			APInvoiceCharges transaction2;
			APInvoiceCharges transaction3;
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			CreateAPInvoiceCharges(job, out guiWrapper, out transaction1, out transaction2, out transaction3);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);

			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { transaction1, transaction2, transaction3 });

			var anotherSecurityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(anotherSecurityProviderMock.Object);
			AssertEquals("Provider overridden to return the same for all transactions.", transaction1.PostingGUIProvider.GetNewSecurityOverrideProvider(false), securityProviderMock.Object);
			AssertEquals("Provider overridden to return the same for all transactions.", transaction3.PostingGUIProvider.GetNewSecurityOverrideProvider(false), securityProviderMock.Object);
			AssertEquals("The same provider is set.", SecurityOverrideProviderSource.Get(transaction1).Provider, securityProviderMock.Object);
			AssertEquals("The same provider is set.", SecurityOverrideProviderSource.Get(transaction2).Provider, securityProviderMock.Object);
			AssertEquals("The same provider is set.", SecurityOverrideProviderSource.Get(transaction3).Provider, securityProviderMock.Object);
			AssertEquals("Maximum transaction was a source of a provider and so doesn't have it overridden.", transaction2.PostingGUIProvider.GetNewSecurityOverrideProvider(false), anotherSecurityProviderMock.Object);
		}

		public override void TestApprovingUserIsSetOnInPlaceAuthorization()
		{
			SetupAuthorizationRegistry();
			var testuser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			if (testuser == null)
			{
				SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, testUserCode, testUserLogin, "password");
			}
			testuser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			var invoiceCharges = new APInvoiceCharges("Creditor1", "tran1", job.PK, job.TablePrefix, guiWrapper.Object);
			CreateTransactionLine(invoiceCharges, job, 100);

			var securityoverride = new SecurityCore(null, testuser, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.UserSecurityOverride).Returns(securityoverride);
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper
				.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper
				.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create<ZGuid, ZString>(ZGuid.NewZGuid(), JobHeaderSchema.Constants.Prefix));
			guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);

			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.IsLevelAuthorizationRequired_ForTestOnly = x => true;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => true;

			AssertEquals("Approving user is not set", Guid.Empty, invoiceCharges.ApprovingUserPKForTest);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
			AssertEquals("Approving user is set", testuser.PK.ToGuid(), invoiceCharges.ApprovingUserPKForTest);
		}

		public override void TestLevelAuthorizationWithApprovalRequest_AddsServiceToServiceContainer()
		{
			Assert("Not applicable", true);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingJobTransactionsApprovalGUIProvider postingGUIProvider, APInvoiceCharges[] transactions,
			Tuple<
					Func<APInvoiceCharges, bool>,
					Func<APInvoiceCharges, bool>,
					Action<APInvoiceChargesApprovalRequest, APInvoiceCharges[]>,
					Func<APInvoiceChargesApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			var transactionsToProcess = transactions.Length > 0 ? new[] { transactions.Last() } : transactions;
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(transactionsToProcess);

			return !transactionsToProcess.Any(x => !x.ContinueProcessing);
		}

		protected override TransactionApprovalRequest<APInvoiceChargesApprovalRequestDetails> PrepareOriginalRequest()
		{
			return Factory.New<APInvoiceChargesApprovalRequest>();
		}

		protected override APInvoiceCharges CreateTransactionHeader(ZString transactionNumber, IPostingJobTransactionsApprovalGUIProvider guiProvider)
		{
			var invoiceCharges = new APInvoiceCharges("Creditor1", "123", ZGuid.Empty, "", null);
			invoiceCharges.SetMockGUIProviderForTest(guiProvider);

			return invoiceCharges;
		}

		protected override void CreateTransactionLine(APInvoiceCharges invoiceCharges, Job job, decimal localAmount)
		{
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, localAmount, TestObjectCreator.Creditor1, null, 0, null);
			invoiceCharges.Charges.Add(charge);
			invoiceCharges.SetJobForTest(job);
		}

		protected override string GetExpectedMessageCaption()
		{
			return "Accounts Payable Invoice Approval Request";
		}

		protected override bool ProcessExistingRequestsWithoutShowingMessages
		{
			get { return true; }
		}

		static void SetupAuthorizationRegistry()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void CreateAPInvoiceCharges(Job job, out Mock<IPostingJobTransactionsApprovalGUIProvider> guiWrapper, out APInvoiceCharges transaction1, out APInvoiceCharges transaction2, out APInvoiceCharges transaction3)
		{
			guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			transaction1 = new APInvoiceCharges("Creditor1", "tran1", job.PK, job.TablePrefix, guiWrapper.Object);
			CreateTransactionLine(transaction1, job, 100);
			transaction2 = new APInvoiceCharges("Creditor1", "tran2", job.PK, job.TablePrefix, guiWrapper.Object);
			CreateTransactionLine(transaction2, job, 200);
			transaction3 = new APInvoiceCharges("Creditor1", "tran3", job.PK, job.TablePrefix, guiWrapper.Object);
			CreateTransactionLine(transaction2, job, 150);
		}

		const string testUserCode = "tst";
		const string testUserLogin = "newuser";
	}
}
