using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing.TestBase;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	abstract class LevelAuthorizationWithApprovalRequestTest<TransactionType, RequestType, DetailsType, GUIProviderType> : LevelAuthorizationWithApprovalRequestTestBase<TransactionType, RequestType, DetailsType, GUIProviderType>
		where TransactionType : ITransactionForApproval
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
		where GUIProviderType : class, IPostingTransactionApprovalGUIProvider
	{
		public virtual void TestLevelAuthorizationWithApprovalRequest_AddsServiceToServiceContainer()
		{
			var guiProviderMock = new Mock<GUIProviderType>();
			var guiProvider = guiProviderMock.Object;

			var request = Factory.New<RequestType>();
			request.XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;

			var factoryForApprovalRequests = new BusinessObjectFactory();
			guiProviderMock.Setup(x => x.FactoryForApprovalRequests).Returns(factoryForApprovalRequests);
			guiProviderMock.Setup(x => x.ShowLoginFormForTest).Returns(true);
			guiProviderMock.Setup(x => x.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), request.XP_ParentTableCode));
			guiProviderMock.Setup(x => x.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Mock<ISecurityOverrideProviderWithApprovalRequest>().Object);
			guiProviderMock.Setup(x => x.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);

			var helper = new Mock<ITransactionApprovalHelper<RequestType, DetailsType>>();
			helper.Setup(x => x.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			helper.Setup(x => x.PostApprovedRequestForThisPostingDetails()).Returns(request);

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var transaction = CreateTransactionHeader("tran1", guiProvider);
			CreateTransactionLine(transaction, job, 100);
			TransactionType[] transactions = new[] { transaction };

			Func<TransactionType, bool> isLevelAuthorizationRequired = x => true;
			Func<TransactionType, bool> checkLevelSecurityRights = x => true;
			Action<RequestType, TransactionType[]> initializeApprovalRequest = (x, y) => { };
			Func<RequestType, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
			PerformTransactionLevelAuthorization
			(
				guiProvider,
				transactions,
				Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
			);

			if (request is InvoicingBaseApprovalRequest<DetailsType> invoiceApprovalRequest)
			{
				if (invoiceApprovalRequest.ShouldAddAdditionalLogs_ForTestOnly)
				{
					var service = guiProvider.FactoryForApprovalRequests.ServiceContainer.GetService<ParentFactoryService>();
					AssertNotNull("ServiceContainer should contain service ParentFactoryService", service);
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void TestGetMaxTransaction()
		{
			Assert(true);
		}

		public virtual void TestGetOnlyTransactionWithoutRights()
		{
			Assert(true);
		}

		protected virtual TransactionApprovalRequest<DetailsType> PrepareOriginalRequest()
		{
			return null;
		}

		public virtual void TestApprovingUserIsSetOnInPlaceAuthorization()
		{
			Assert(true);
		}

		[ExpectNoExceptions]
		public virtual void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.CallBase = true;
			var transaction = CreateTransactionHeader("tran1", (GUIProviderType)guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			ZString approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			var helper = new Mock<ITransactionApprovalHelper<RequestType, DetailsType>>();

			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Mock<ISecurityOverrideProviderWithApprovalRequest>().Object);
			guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
				It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(true);
			var isLevelAuthorithationRequiredValue = false;
			var checkLevelSecurityRightValue = false;
			helper.Setup(m => m.IsRequestForTheSamePostingActionAndDetails(It.IsAny<RequestType>())).Returns(false);
			TransactionType[] transactions = new[] { transaction };
			guiWrapper.Setup(m => m.RequestToCompare)
				.Returns(Factory.New<APInvoiceChargesApprovalRequest>());
			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<TransactionType, bool> isLevelAuthorizationRequired = x =>
				{
					return isLevelAuthorithationRequiredValue;
				};
				Func<TransactionType, bool> checkLevelSecurityRights = x => !isLevelAuthorithationRequiredValue || checkLevelSecurityRightValue;
				Action<RequestType, TransactionType[]> initializeApprovalRequest = (x, y) => { };
				Func<RequestType, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						(GUIProviderType)guiWrapper.Object,
						transactions,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};

			guiWrapper.Setup(m => m.RollbackPosting());
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Invocations.Clear();

			helper.Setup(m => m.IsRequestForTheSamePostingActionAndDetails(It.IsAny<RequestType>())).Returns(true);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
		}

		public virtual void TestPerformTransactionLevelAuthorization_NoTransactionsAndPreview()
		{
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.CallBase = true;
			var transaction = CreateTransactionHeader("tran1", (GUIProviderType)guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			var transaction2 = CreateTransactionHeader("tran2", (GUIProviderType)guiWrapper.Object);
			CreateTransactionLine(transaction2, job, 200);
			var transaction3 = CreateTransactionHeader("tran3", (GUIProviderType)guiWrapper.Object);
			CreateTransactionLine(transaction3, job, 300);
			ZString approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			var helper = new Mock<ITransactionApprovalHelper<RequestType, DetailsType>>();
			helper.CallBase = true;
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Mock<ISecurityOverrideProviderWithApprovalRequest>().Object);
			var isLevelAuthorithationRequiredValue = false;
			var checkLevelSecurityRightValue = false;
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			TransactionType[] transactions = Array.Empty<TransactionType>();

			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<TransactionType, bool> isLevelAuthorizationRequired = x =>
				{
					if ((object)x == (object)transaction2)
					{
						return false;
					}
					return isLevelAuthorithationRequiredValue;
				};
				Func<TransactionType, bool> checkLevelSecurityRights = x => !isLevelAuthorithationRequiredValue || checkLevelSecurityRightValue;
				Action<RequestType, TransactionType[]> initializeApprovalRequest = (x, y) => { };
				Func<RequestType, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						(GUIProviderType)guiWrapper.Object,
						transactions,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			Assert("No transactions to authorize", continueProcessing);

			transactions = new[] { transaction, transaction2, transaction3 };
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(true);
			checkLevelSecurityRightValue = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			Assert("Preview with rights: continueProcessing", continueProcessing);

			isLevelAuthorithationRequiredValue = true;
			guiWrapper.Setup(m => m.RollbackPosting());
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			Assert("Preview without rights", continueProcessing);

			checkLevelSecurityRightValue = true;
			guiWrapper.Setup(m => m.RollbackPosting());
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			Assert("Preview without rights, but with authorization details entered: continueProcessing", continueProcessing);
		}

		public virtual void TestPerformTransactionLevelAuthorization_PostWithRight()
		{
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<GUIProviderType>();
			var transaction = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			var transaction2 = CreateTransactionHeader("tran2", guiWrapper.Object);
			CreateTransactionLine(transaction2, job, 200);
			var transaction3 = CreateTransactionHeader("tran3", guiWrapper.Object);
			CreateTransactionLine(transaction3, job, 300);
			ZString approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			var helper = new Mock<ITransactionApprovalHelper<RequestType, DetailsType>>();
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Mock<ISecurityOverrideProviderWithApprovalRequest>().Object);
			var isLevelAuthorithationRequiredValue = false;
			var checkLevelSecurityRightValue = false;
			var isInitializeCalled = false;
			TransactionType[] transactions = Array.Empty<TransactionType>();
			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<TransactionType, bool> isLevelAuthorizationRequired = x =>
				{
					if ((object)x == (object)transaction2)
					{
						return false;
					}
					return isLevelAuthorithationRequiredValue;
				};
				Func<TransactionType, bool> checkLevelSecurityRights = x => !isLevelAuthorithationRequiredValue || checkLevelSecurityRightValue;
				Action<RequestType, TransactionType[]> initializeApprovalRequest = (x, y) =>
				{
					isInitializeCalled = true;
					x.XP_ParentTableCode = approval_XP_ParentTableCode;
					AssertEquals("Transaction count need authorization", 0, y.Length);
				};
				Func<RequestType, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						transactions,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};
			transactions = new[] { transaction, transaction2, transaction3 };
			isLevelAuthorithationRequiredValue = false;
			isInitializeCalled = false;
			var factoryForApprovalRequests = new BusinessObjectFactory();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(factoryForApprovalRequests);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			helper.Setup(m => m.PostApprovedRequestForThisPostingDetails());
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
			helper.Verify();
			helper.Invocations.Clear();
			Assert("Posting transactions with rights: continueProcessing", continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(true);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(true);
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);

			if (ShouldCreateRequestOnEveryPosting)
			{
				helper.Setup(m => m.ApproveAndPostRequest(It.IsAny<ISecurityOverrideProvider>()));
			}
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			helper.Verify();
			helper.Verify(m => m.PostApprovedRequestForThisPostingDetails(), Times.Never());
			Assert("Bulk posting. Cancel existed requested requests silently: continueProcessing", continueProcessing);
			if (ShouldCreateRequestOnEveryPosting)
			{
				var approvals = factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery());
				AssertEquals("Approval request should exist", 1, approvals.Length);
				approvals[0].Delete();
			}
			else
			{
				AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			}
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			helper.Verify();
			Assert("Cancel existed requested requests silently: continueProcessing", continueProcessing);
			if (ShouldCreateRequestOnEveryPosting)
			{
				var approvals = factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery());
				AssertEquals("Approval request should exist", 1, approvals.Length);
				approvals[0].Delete();
			}
			else
			{
				AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			}
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);
		}

		public virtual void TestPerformTransactionLevelAuthorization_PostWithoutRight()
		{
			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<GUIProviderType>();
			var transaction = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(transaction, job, 100);
			var transaction2 = CreateTransactionHeader("tran2", guiWrapper.Object);
			CreateTransactionLine(transaction2, job, 200);
			var transaction3 = CreateTransactionHeader("tran3", guiWrapper.Object);
			CreateTransactionLine(transaction3, job, 300);
			ZString approval_XP_ParentTableCode = GetDefaultXP_ParentTableCode();
			var helper = new Mock<ITransactionApprovalHelper<RequestType, DetailsType>>();
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
			var securityProvider = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProvider.Object);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProvider.Object);
			var isLevelAuthorithationRequiredValue = false;
			var checkLevelSecurityRightValue = false;
			var isInitializeCalled = false;
			TransactionType[] transactions = Array.Empty<TransactionType>();
			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<TransactionType, bool> isLevelAuthorizationRequired = x =>
				{
					if ((object)x == (object)transaction2)
					{
						return false;
					}
					return isLevelAuthorithationRequiredValue;
				};
				Func<TransactionType, bool> checkLevelSecurityRights = x => !isLevelAuthorithationRequiredValue || checkLevelSecurityRightValue;
				Action<RequestType, TransactionType[]> initializeApprovalRequest = (x, y) =>
				{
					this.OnInitializeApprovalRequest(x, y);
					isInitializeCalled = true;
					x.XP_ParentTableCode = approval_XP_ParentTableCode;
					x.XP_GB_JobBranch = GlbBranch.CurrentBranch.PK;
					x.XP_GE_JobDepartment = GlbDepartment.CurrentDepartment.PK;
					if (IsSingleTransactionApprovalTest)
					{
						AssertEquals("Transaction count need authorization", 1, y.Length);
						AssertEquals("Transaction need authorization", transaction3, y[0]);
					}
					else
					{
						AssertEquals("Transaction count need authorization", 2, y.Length);
						AssertEquals("Transaction need authorization", transaction, y[0]);
						AssertEquals("Transaction need authorization", transaction3, y[1]);
					}
				};
				Func<RequestType, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						transactions,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};
			transactions = new[] { transaction, transaction2, transaction3 };
			isLevelAuthorithationRequiredValue = true;
			isInitializeCalled = false;
			var factoryForApprovalRequests = new BusinessObjectFactory();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(factoryForApprovalRequests);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			helper.Setup(m => m.PostApprovedRequestForThisPostingDetails());
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
			helper.Verify();
			helper.Invocations.Clear();
			Assert("Posting with approved request: continueProcessing", continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(true);

			var expectedMessage = @"There is an approved request for this posting action, but data for approval is different.
" + (IsSingleTransactionApprovalTest ?
@"You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.
" :
@"You do not have security rights to post transactions with these amounts. To continue posting of these transactions by an authorized user post individual transactions from billing tab.
");
			guiWrapper.Setup(m => m.NotifyBulkPostingIsNotAuthorized(It.IsAny<string>()));
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(true);
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			helper.Verify();
			helper.Verify(m => m.PostApprovedRequestForThisPostingDetails(), Times.Never());
			helper.Invocations.Clear();
			Assert("Bulk posting. There is approve request for different details: continueProcessing", !continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			expectedMessage = "There is an approved request for this posting action, but data for approval is different. This approved request must be canceled to continue posting.";
			var expectedCaption = GetExpectedMessageCaption();
			if (ProcessExistingRequestsWithoutShowingMessages)
			{
				helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
				securityProvider.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false);
			}
			else
			{
				guiWrapper.Setup(m => m.ShowMessage(expectedMessage, expectedCaption,
					ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning, ZDialogResult.OK)).Returns(ZDialogResult.Cancel);
			}
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Invocations.Clear();
			helper.Verify();

			if (ProcessExistingRequestsWithoutShowingMessages)
			{
				securityProvider.Verify();
			}
			else
			{
				helper.Verify(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails(), Times.Never());
			}
			Assert("There is approve request for different details which user don't want to cancel: continueProcessing", !continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			checkLevelSecurityRightValue = true;
			if (!ProcessExistingRequestsWithoutShowingMessages)
			{
				guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
			}
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			if (ShouldCreateRequestOnEveryPosting)
			{
				helper.Setup(m => m.ApproveAndPostRequest(It.IsAny<ISecurityOverrideProvider>()));
			}
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
			helper.Verify();
			helper.Invocations.Clear();
			Assert("There is approve request for different details which will be canceled: continueProcessing", continueProcessing);
			GenApprovalRequest[] approvals;
			if (ShouldCreateRequestOnEveryPosting)
			{
				approvals = factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery());
				AssertEquals("Approval request should exist", 1, approvals.Length);
				approvals[0].Delete();
			}
			else
			{
				AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			}
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			checkLevelSecurityRightValue = false;
			securityProvider.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false);
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			helper.Verify();
			helper.Verify(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus(), Times.Never());
			securityProvider.Verify();
			Assert("User without rights don't want to create approval request: continueProcessing", !continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			securityProvider.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
			expectedMessage =
$@"There is another request for this {GetRequestParentObjectNameInLowcase()}. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
			if (!ProcessExistingRequestsWithoutShowingMessages)
			{
				guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
				guiWrapper.Setup(m => m.ShowMessage(expectedMessage, expectedCaption,
					ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes)).Returns(ZDialogResult.No);
			}
			else
			{
				helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
				guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.Cancel);
			}
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(true);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Invocations.Clear();
			helper.Verify();
			securityProvider.Verify();
			Assert("User created approval request, but asked to cancel existing one and he doesn't want: continueProcessing", !continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			if (GetDefaultXP_ParentTableCode() == JobHeaderSchema.Constants.Prefix)
			{
				isInitializeCalled = false;
				guiWrapper.Setup(m => m.RollbackPosting());
				approval_XP_ParentTableCode = JobConsolSchema.Constants.Prefix;
				guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), approval_XP_ParentTableCode));
				expectedMessage =
	@"There is another request for this consol. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
				if (!ProcessExistingRequestsWithoutShowingMessages)
				{
					guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
					guiWrapper.Setup(m => m.ShowMessage(expectedMessage, expectedCaption,
						ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes)).Returns(ZDialogResult.Yes);
				}
				guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
				guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.Cancel);
				helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(true);
				helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
				continueProcessing = performTransactionLevelAuthorization();
				guiWrapper.Verify();
				guiWrapper.Invocations.Clear();
				helper.Verify();
				securityProvider.Verify();
				Assert("New request created, existed - canceled, but then user didn't want to save new request: continueProcessing", !continueProcessing);
				AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
				Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);
			}

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			if (!ProcessExistingRequestsWithoutShowingMessages)
			{
				guiWrapper.SetupSequence(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK).Returns(ZDialogResult.Yes);
			}
			guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.OK);
			helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(true);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
			helper.Verify();
			securityProvider.Verify();
			Assert("New request created and saved, existed canceled: continueProcessing", continueProcessing);
			approvals = factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery());
			AssertEquals("Approval request should exist", 1, approvals.Length);
			approvals[0].Delete();
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			if (!ProcessExistingRequestsWithoutShowingMessages)
			{
				guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
			}
			guiWrapper.Setup(m => m.ResetFactoryForApprovalRequests());
			guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.Cancel);
			helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Invocations.Clear();
			helper.Verify();
			securityProvider.Verify();
			Assert("New request created. There was no exited request. Then user didn't want to save new request: continueProcessing", !continueProcessing);
			AssertEquals("Approval request should not exist", 0, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);

			isInitializeCalled = false;
			guiWrapper.Setup(m => m.RollbackPosting());
			if (!ProcessExistingRequestsWithoutShowingMessages)
			{
				guiWrapper.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ZMessageBoxButtons>(), It.IsAny<ZMessageBoxIcon>(), It.IsAny<ZDialogResult>())).Returns(ZDialogResult.OK);
			}
			guiWrapper.Setup(m => m.ShowApprovalFormToSetDescription(It.IsAny<GenApprovalRequest>())).Returns(ZDialogResult.OK);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.ResetFactoryForApprovalRequests(), Times.Never());
			helper.Verify();
			securityProvider.Verify();
			Assert("Approval request created. There was no existed request: continueProcessing", continueProcessing);
			AssertEquals("Approval request should  exist", 1, factoryForApprovalRequests.Load<GenApprovalRequest>(new ZQuery()).Length);
			Assert("Initialize is called and inside it right amount of transactions is asserted", isInitializeCalled);
		}

		protected virtual ZString GetDefaultXP_ParentTableCode() => JobHeaderSchema.Constants.Prefix;

		protected virtual ZString GetRequestParentObjectNameInLowcase() => "job";

		protected virtual void OnInitializeApprovalRequest(RequestType x, TransactionType[] y)
		{
		}

		protected virtual bool ProcessExistingRequestsWithoutShowingMessages
		{
			get { return false; }
		}

		protected virtual bool ShouldCreateRequestOnEveryPosting
		{
			get { return false; }
		}
	}

	[TestedType(typeof(ARCreditNoteLevelAuthorizationWithApprovalRequest))]
	class ARCreditNoteLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTest<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails, IPostingJobTransactionsApprovalGUIProvider>
	{
		public override void TestGetMaxTransaction()
		{
			GlbBranch branchBNE = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			GlbBranch branchSYD = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));
			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid(), 1000m, 2000m);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.NonCurrentDepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(Env.CurrentDepartmentPK, Env.CurrentBranchPK, staff.PK, true);

			using (Env.SetTemporaryUserContext("newuser", branchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
			{
				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var creditNote1 = TestObjectCreator.CreateARCreditNote(1500m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, 1500m);

				var creditNote2 = TestObjectCreator.CreateARCreditNote(1500m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
				TestObjectCreator.AddLineToCreditNote(creditNote2, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.NonCurrentDepartment.PK, 1500m);

				var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(null);
				var transactions = new InvoicingBase[] { creditNote1, creditNote2 };
				var transactionsWithoutRights = levelAuthorization.GetOnlyTransactionWithoutRights(transactions);
				AssertEquals(1, transactionsWithoutRights.Count);
				AssertEquals(creditNote2, transactionsWithoutRights[0]);

				var result = levelAuthorization.GetMaxTransaction(transactions, transactionsWithoutRights.ToArray(), typeof(ARCreditNote));
				AssertEquals(2, result.TransactionsForAuthorisationCalculation.Count);
				Assert(result.TransactionsForAuthorisationCalculation.Exists(x => x.AH_GB == TestObjectCreator.NonCurrentBranch.PK && x.AH_GE == TestObjectCreator.NonCurrentDepartment.PK));
				Assert(result.TransactionsForAuthorisationCalculation.Exists(x => x.PK == creditNote2.PK));
			}
		}

		public override void TestGetOnlyTransactionWithoutRights()
		{
			GlbBranch branchBNE = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			GlbBranch branchSYD = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));
			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid(), 1000m, 2000m);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.NonCurrentDepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(Env.CurrentDepartmentPK, Env.CurrentBranchPK, staff.PK, true);

			using (Env.SetTemporaryUserContext("newuser", branchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
			{
				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var creditNote1 = TestObjectCreator.CreateARCreditNote(1500m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
				TestObjectCreator.AddLineToCreditNote(creditNote1, job.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, 1500m);
				var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(null);
				var result = levelAuthorization.GetOnlyTransactionWithoutRights(new InvoicingBase[] { creditNote1 });
				AssertEquals(0, result.Count);

				var creditNote2 = TestObjectCreator.CreateARCreditNote(1500m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
				TestObjectCreator.AddLineToCreditNote(creditNote2, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.NonCurrentDepartment.PK, 1500m);
				result = levelAuthorization.GetOnlyTransactionWithoutRights(new InvoicingBase[] { creditNote2 });
				AssertEquals(1, result.Count);
				AssertEquals(creditNote2, result[0]);

				result = levelAuthorization.GetOnlyTransactionWithoutRights(new InvoicingBase[] { creditNote1, creditNote2 });
				AssertEquals(1, result.Count);
				AssertEquals(creditNote2, result[0]);

				creditNote1.TransactionsForAuthorisationCalculation.Clear();
				creditNote2.TransactionsForAuthorisationCalculation.Clear();
				AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				result = levelAuthorization.GetOnlyTransactionWithoutRights(new InvoicingBase[] { creditNote1, creditNote2 });
				AssertEquals(0, result.Count);
			}
		}

		public override void TestApprovingUserIsSetOnInPlaceAuthorization()
		{
			SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "ONS", "OnSportUser", "password");
			SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "APP", "ApprovedUser", "password");
			var onSpotApprovingTestUser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"ONS");
			var approvedRequestTestUser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"APP");

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			var invoice1 = CreateTransactionHeader("tran1", guiWrapper.Object);
			var invoice2 = CreateTransactionHeader("tran1", guiWrapper.Object);
			var invoice3 = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(invoice1, job, 100);
			CreateTransactionLine(invoice2, job, 200);
			CreateTransactionLine(invoice3, job, 300);

			var newFactory = new BusinessObjectFactory();
			var request = newFactory.New<ARCreditNoteApprovalRequest>();
			request.Initialize(new[] { invoice3 }, job.PK, JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.All);
			request.UpdateApprovalUserAndStatus(approvedRequestTestUser.GS_Code, Constants.GenApprovalRequestApprovalStatus.Approved);
			request.XP_ApprovalDate = new ZDateTime(2019, 2, 2);
			newFactory.Save();

			var helper = new Mock<ITransactionApprovalHelper<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);

			var securityoverride = new SecurityCore(null, onSpotApprovingTestUser, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.UserSecurityOverride).Returns(securityoverride);
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), new ZString(JobHeaderSchema.Constants.Prefix)));

			var invoices = new[] { invoice1, invoice2 };

			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<InvoicingBase, bool> isLevelAuthorizationRequired = x => true;
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => true;
				Action<ARCreditNoteApprovalRequest, InvoicingBase[]> initializeApprovalRequest = (x, y) => { };
				Func<ARCreditNoteApprovalRequest, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						invoices,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};

			foreach (var invoice in invoices.Cast<ARCreditNote>())
			{
				AssertEquals("Approving user is set", Guid.Empty, invoice.ApprovingUserPKForTest);
				AssertEquals("Approval date should not be set", ZDateTime.Empty, invoice.ApprovalDate);
			}
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never);
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.VerifyAll();
			Assert("Postcondition: continueProcessing", continueProcessing);
			foreach (var invoice in invoices.Cast<ARCreditNote>())
			{
				AssertEquals("Approving user is set", onSpotApprovingTestUser.PK.ToGuid(), invoice.ApprovingUserPKForTest);
				AssertEquals("Approval date should not be set", ZDateTime.Empty, invoice.ApprovalDate);
			}

			helper.Reset();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			helper.Setup(m => m.PostApprovedRequestForThisPostingDetails()).Returns(request);
			invoices = new[] { invoice3 };

			foreach (var invoice in invoices.Cast<ARCreditNote>())
			{
				AssertEquals("Approving user is set", Guid.Empty, invoice.ApprovingUserPKForTest);
				AssertEquals("Approval date should not be set", ZDateTime.Empty, invoice.ApprovalDate);
			}
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.VerifyAll();
			Assert("Postcondition: continueProcessing", continueProcessing);
			foreach (var invoice in invoices.Cast<ARCreditNote>())
			{
				AssertEquals("Approving user is set", approvedRequestTestUser.PK.ToGuid(), invoice.ApprovingUserPKForTest);
				AssertNotEquals("Approval date should be set", ZDateTime.Empty, invoice.ApprovalDate);
			}
		}

		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(null);
			AssertEquals(true, levelAuthorization.IsMultipleApproverApplicable);
		}

		public void TestIsLevelAuthorizationRequiredWhenEnforceTwoApprover()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;
			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1000.Range = RangeCodes.UpTo;
			var above1000 = valuesForTest.AddNew();
			above1000.Amount = 1000;
			above1000.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above1000.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 250m;
			var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(null);
			Assert(!levelAuthorization.IsLevelAuthorizationRequired(creditNote));

			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			Assert(levelAuthorization.IsLevelAuthorizationRequired(creditNote));
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenTwoApproverMode_UserHasLevel1Right()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.TwoApprovers, true, false);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenTwoApproverMode_UserDoesNotHaveLevel1Right()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.TwoApprovers, false, false);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenSequentialApproverMode_UserHasLevel1Right()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.SequentialApprovers, true, false);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenSequentialApproverMode_UserDoesNotHaveLevel1Right()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.SequentialApprovers, false, false);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenTwoApproverMode_UserHasLevel1Right_Amending()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.TwoApprovers, true, true);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenTwoApproverMode_UserDoesNotHaveLevel1Right_Amending()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.TwoApprovers, false, true);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenSequentialApproverMode_UserHasLevel1Right_Amending()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.SequentialApprovers, true, true);
		}

		public void TestConfirmAndPreEditApprovalRequestByUserWhenSequentialApproverMode_UserDoesNotHaveLevel1Right_Amending()
		{
			AssertConfirmAndPreEditApprovalRequestByUser(Constants.AuthorizationMode.Codes.SequentialApprovers, false, true);
		}

		void AssertConfirmAndPreEditApprovalRequestByUser(ZString authorisationMode, bool userHasFirstLevelRight, bool isAmending)
		{
			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1000.Range = RangeCodes.UpTo;
			var above1000 = valuesForTest.AddNew();
			above1000.Amount = 1000;
			above1000.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above1000.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(Env.CurrentDepartmentPK, Env.CurrentBranchPK, staff.PK, userHasFirstLevelRight);
			using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
				creditNote.AH_OSExTaxAmount = 250m;

				var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(null);
				var levelAuthorizationForAmending = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(null, null);

				setting.AuthorizationMode = authorisationMode;
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
				var request = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
				request.Initialize(new InvoicingBase[] { creditNote }, creditNote.PK, "AH", JobInvoicingPostingOption.Revenue);
				if (!isAmending)
				{
					levelAuthorization.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
				}
				else
				{
					levelAuthorizationForAmending.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
				}
				AssertEquals(GlbStaff.CurrentUser.GS_Code, request.XP_GS_NKApprovingUser1);

				creditNote = Factory.NewWithValidTestData<ARCreditNote>();
				creditNote.AH_OSExTaxAmount = 1500m;
				setting.AuthorizationMode = authorisationMode;
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
				request = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
				request.Initialize(new InvoicingBase[] { creditNote }, creditNote.PK, "AH", JobInvoicingPostingOption.Revenue);
				if (!isAmending)
				{
					levelAuthorization.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
				}
				else
				{
					levelAuthorizationForAmending.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
				}
				if (userHasFirstLevelRight)
				{
					AssertEquals("tst", request.XP_GS_NKApprovingUser1);
				}
				else
				{
					AssertEquals("", request.XP_GS_NKApprovingUser1);
				}
			}
		}

		public override void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			Assert("Currently Preview mode is only available in AP Invoice Approval module, it's not applicable here.", true);
		}

		public void TestCreatingRequestsFromConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment1Job = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			shipment1Job.JH_GB = Env.CurrentBranchPK;
			shipment1Job.JH_GE = Env.CurrentDepartmentPK;
			var shipment2Job = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			shipment2Job.JH_GB = Env.CurrentBranchPK;
			shipment2Job.JH_GE = Env.CurrentDepartmentPK;

			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper.Setup(m => m.PostingOption).Returns(JobInvoicingPostingOption.All);
			var createRequestsMethod = typeof(ARCreditNoteLevelAuthorizationWithApprovalRequest).GetMethod("CreateApprovalRequests", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var levelAuthorization = new ARCreditNoteLevelAuthorizationWithApprovalRequest(guiWrapper.Object);

			var transaction = CreateTransactionHeader("tran1", guiWrapper.Object);
			transaction.AH_JH = shipment1Job.PK;
			CreateTransactionLine(transaction, shipment1Job, 100);
			var transaction2 = CreateTransactionHeader("tran2", guiWrapper.Object);
			transaction2.AH_JH = shipment2Job.PK;
			CreateTransactionLine(transaction2, shipment1Job, 200);
			var transaction3 = CreateTransactionHeader("tran3", guiWrapper.Object);
			transaction3.AH_JH = ZGuid.Empty;
			transaction3.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			CreateTransactionLine(transaction3, shipment2Job, 300);

			var transactions = new List<InvoicingBase>() { transaction, transaction2, transaction3 };
			AssertEquals(1, transactions.Count(x => x.IsConsolInvoice));
			var requests = new List<ARCreditNoteApprovalRequest>() { };

			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(shipment1Job.PK, new ZString(shipment1Job.TablePrefix)));
			createRequestsMethod.Invoke(levelAuthorization, new object[] { transactions, requests, false });
			AssertEquals("When not posting from consol do not split to individual requests", 1, requests.Count);
			AssertEquals("Parent should be Job", 1, requests.Count(x => x.XP_ParentTableCode == JobHeaderSchema.Constants.Prefix));
			requests.Clear();

			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, new ZString(consol.TablePrefix)));
			createRequestsMethod.Invoke(levelAuthorization, new object[] { transactions, requests, false });
			AssertEquals("When posting from consol, should create 1 request per transaction", 3, requests.Count);
			AssertEquals("Credit notes created from shipments should not have a consol parent", 2, requests.Count(x => x.XP_ParentTableCode == JobHeaderSchema.Constants.Prefix));
			AssertEquals("Credit notes created from consol should have a consol parent", 1, requests.Count(x => x.XP_ParentTableCode == JobConsolSchema.Constants.Prefix));
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingJobTransactionsApprovalGUIProvider postingGUIProvider, InvoicingBase[] transactions,
			Tuple<
					Func<InvoicingBase, bool>,
					Func<InvoicingBase, bool>,
					Action<ARCreditNoteApprovalRequest, InvoicingBase[]>,
					Func<ARCreditNoteApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			var helper = new ARCreditNoteLevelAuthorizationWithApprovalRequest(postingGUIProvider);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization(transactions.Cast<ARCreditNote>().ToArray(), false);
		}

		protected override InvoicingBase CreateTransactionHeader(ZString transactionNumber, IPostingJobTransactionsApprovalGUIProvider guiProvider)
		{
			return TestObjectCreator.CreateARCreditNote(transactionNumber, TestObjectCreator.Debtor);
		}

		protected override void CreateTransactionLine(InvoicingBase transaction, Job job, decimal localAmount)
		{
			TestObjectCreator.CreateARCreditNoteLine((ARCreditNote)transaction, job, TestObjectCreator.CC1, localAmount);
		}

		protected override string GetExpectedMessageCaption()
		{
			return "Accounts Receivable Credit Note Approval Request";
		}

		protected override bool IsSingleTransactionApprovalTest
		{
			get { return false; }
		}
	}

	[TestedType(typeof(ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest))]
	class ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTest<ARCreditNote, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails, IPostingTransactionApprovalGUIProvider>
	{
		ARCreditNote CreditNote;

		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(null, CreditNote);
			AssertEquals(true, levelAuthorization.IsMultipleApproverApplicable);
		}

		public void TestIsLevelAuthorizationRequiredWhenEnforceTwoApprover()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;
			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1000.Range = RangeCodes.UpTo;
			var above1000 = valuesForTest.AddNew();
			above1000.Amount = 1000;
			above1000.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above1000.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 250m;
			var levelAuthorization = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(null, null);
			Assert(!levelAuthorization.IsLevelAuthorizationRequired(creditNote));
			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			Assert(levelAuthorization.IsLevelAuthorizationRequired(creditNote));
		}

		public void TestConfirmAndPreEditApprovalRequestByUser()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;
			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var upTo1000 = valuesForTest.AddNew();
			upTo1000.Amount = 1000;
			upTo1000.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1000.Range = RangeCodes.UpTo;
			var above1000 = valuesForTest.AddNew();
			above1000.Amount = 1000;
			above1000.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above1000.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 250m;
			var levelAuthorization = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(null, null);
			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			var request = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.XP_GB_JobBranch = GlbBranch.CurrentBranch.PK;
			request.XP_GE_JobDepartment = GlbDepartment.CurrentDepartment.PK;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.DoubleLogin;
			levelAuthorization.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, request.XP_GS_NKApprovingUser1);

			creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OSExTaxAmount = 250m;
			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.SequentialApprovers;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
			request = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			request.XP_GB_JobBranch = GlbBranch.CurrentBranch.PK;
			request.XP_GE_JobDepartment = GlbDepartment.CurrentDepartment.PK;
			request.XP_GS_NKApprovingUser1 = ZString.Empty;
			request.PostingDetails.ApprovingOption = ApprovalCredentialOption.SequentialLogin;
			levelAuthorization.ConfirmAndPreEditApprovalRequestByUser(new[] { request }, creditNote);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, request.XP_GS_NKApprovingUser1);
		}

		public override void TestApprovingUserIsSetOnInPlaceAuthorization()
		{
			SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "ONS", "OnSportUser", "password");
			SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "APP", "ApprovedUser", "password");
			var onSpotApprovingTestUser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"ONS");
			var approvedRequestTestUser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"APP");

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			CreditNote = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(CreditNote, job, 100);

			var helper = new Mock<ITransactionApprovalHelper<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);

			var securityoverride = new SecurityCore(null, onSpotApprovingTestUser, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.UserSecurityOverride).Returns(securityoverride);
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProviderForARCreditNote(It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<ARCreditNoteApprovalRequest[]>())).Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(ZGuid.NewZGuid(), new ZString(JobHeaderSchema.Constants.Prefix)));

			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<ARCreditNote, bool> isLevelAuthorizationRequired = x => true;
				Func<ARCreditNote, bool> checkLevelSecurityRights = x => true;
				Action<ARCreditNoteApprovalRequest, ARCreditNote[]> initializeApprovalRequest = (x, y) => { };
				Func<ARCreditNoteApprovalRequest, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						null,
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};

			AssertEquals("Approving user is not set", Guid.Empty, CreditNote.ApprovingUserPKForTest);
			AssertEquals("Approval date should not be set", ZDateTime.Empty, CreditNote.ApprovalDate);
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			Assert("Postcondition: continueProcessing", continueProcessing);
			AssertEquals("Approving user is set", onSpotApprovingTestUser.PK.ToGuid(), CreditNote.ApprovingUserPKForTest);
			AssertEquals("Approval date should not be set", ZDateTime.Empty, CreditNote.ApprovalDate);

			var newFactory = new BusinessObjectFactory();
			var request = newFactory.New<ARCreditNoteApprovalRequest>();
			request.Initialize(new[] { CreditNote }, job.PK, JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.All);
			request.UpdateApprovalUserAndStatus(approvedRequestTestUser.GS_Code, Constants.GenApprovalRequestApprovalStatus.Approved);
			newFactory.Save();

			helper.Reset();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			helper.Setup(m => m.PostApprovedRequestForThisPostingDetails()).Returns(request);

			CreditNote.ApprovingUserPKList.Clear();
			AssertEquals("Approving user is not set", Guid.Empty, CreditNote.ApprovingUserPKForTest);
			AssertEquals("Approval date should not be set", ZDateTime.Empty, CreditNote.ApprovalDate);
			continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			Assert("Postcondition: continueProcessing", continueProcessing);
			AssertEquals("Approving user is set", approvedRequestTestUser.PK.ToGuid(), CreditNote.ApprovingUserPKForTest);
			AssertNotEquals("Approval date should be set", ZDateTime.Empty, CreditNote.ApprovalDate);
		}

		public override void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			Assert("Currently Preview mode is only available in AP Invoice Approval module, it's not applicable here.", true);
		}

		public override void TestPerformTransactionLevelAuthorization_NoTransactionsAndPreview()
		{
			Assert("At least one transaction should be here always. Preview mode is not applicable here.", true);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingTransactionApprovalGUIProvider postingGUIProvider, ARCreditNote[] transactions,
			Tuple<Func<ARCreditNote, bool>, Func<ARCreditNote, bool>, Action<ARCreditNoteApprovalRequest, ARCreditNote[]>, Func<ARCreditNoteApprovalRequest, ITransactionApprovalHelper>> testOnlyOverrides)
		{
			var helper = new ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(postingGUIProvider, CreditNote);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization();
		}

		protected override ARCreditNote CreateTransactionHeader(ZString transactionNumber, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			CreditNote = TestObjectCreator.CreateARCreditNote(transactionNumber, TestObjectCreator.Debtor);
			return CreditNote;
		}

		protected override void CreateTransactionLine(ARCreditNote transaction, Job job, decimal localAmount)
		{
			TestObjectCreator.CreateARCreditNoteLine(transaction, job, TestObjectCreator.CC1, localAmount);
		}

		protected override string GetExpectedMessageCaption()
		{
			return "Accounts Receivable Credit Note Approval Request";
		}
	}

	[TestedType(typeof(APInvoiceLevelAuthorizationWithApprovalRequest))]
	class APInvoiceLevelAuthorizationWithApprovalRequestTest : LevelAuthorizationWithApprovalRequestTest<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails, IPostingTransactionApprovalGUIProvider>
	{
		public override void TestIsSecondApproverApplicable()
		{
			var levelAuthorization = new APInvoiceLevelAuthorizationWithApprovalRequest(null, null, false);
			AssertEquals(false, levelAuthorization.IsMultipleApproverApplicable);
		}

		public override void TestPerformTransactionLevelAuthorizationForTransactionsPreviewOnlyCheckOriginalRquestIsModified()
		{
			Assert("Currently Preview mode is only available in AP Invoice Approval module, it's not applicable here.", true);
		}

		public override void TestPerformTransactionLevelAuthorization_NoTransactionsAndPreview()
		{
			Assert("At least one transaction should be here always. Preview mode is not applicable here.", true);
		}

		public override void TestApprovingUserIsSetOnInPlaceAuthorization()
		{
			SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, "TSU", "TestUser", "password");
			var testuser = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"TSU");

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingTransactionApprovalGUIProvider>();
			var invoice = CreateTransactionHeader("tran1", guiWrapper.Object);
			CreateTransactionLine(invoice, job, 100);

			var helper = new Mock<ITransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);

			var securityoverride = new SecurityCore(null, testuser, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			securityProviderMock.Setup(m => m.UserSecurityOverride).Returns(securityoverride);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(true);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.FactoryForApprovalRequests).Returns(Factory);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>()))
				.Returns(securityProviderMock.Object);
			guiWrapper.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction())
				.Returns(Tuple.Create(ZGuid.NewZGuid(), new ZString(JobHeaderSchema.Constants.Prefix)));

			Func<bool> performTransactionLevelAuthorization = () =>
			{
				Func<InvoicingBase, bool> isLevelAuthorizationRequired = x => true;
				Func<InvoicingBase, bool> checkLevelSecurityRights = x => true;
				Action<APInvoiceChargesApprovalRequest, InvoicingBase[]> initializeApprovalRequest = (x, y) => { };
				Func<APInvoiceChargesApprovalRequest, ITransactionApprovalHelper> getNewHelper = x => helper.Object;
				return PerformTransactionLevelAuthorization
					(
						guiWrapper.Object,
						new[] { invoice },
						Tuple.Create(isLevelAuthorizationRequired, checkLevelSecurityRights, initializeApprovalRequest, getNewHelper)
					);
			};

			AssertEquals("Approving user is not set", Guid.Empty, invoice.ApprovingUserPKForTest);
			var continueProcessing = performTransactionLevelAuthorization();
			guiWrapper.Verify();
			guiWrapper.Verify(m => m.RollbackPosting(), Times.Never());
			Assert("Postcondition: continueProcessing", continueProcessing);
			AssertEquals("Approving user is set", testuser.PK.ToGuid(), invoice.ApprovingUserPKForTest);
		}

		protected override bool PerformTransactionLevelAuthorization(IPostingTransactionApprovalGUIProvider postingGUIProvider, InvoicingBase[] transactions,
			Tuple<
					Func<InvoicingBase, bool>,
					Func<InvoicingBase, bool>,
					Action<APInvoiceChargesApprovalRequest, InvoicingBase[]>,
					Func<APInvoiceChargesApprovalRequest, ITransactionApprovalHelper>
				> testOnlyOverrides)
		{
			var helper = new APInvoiceLevelAuthorizationWithApprovalRequest(postingGUIProvider, transactions.Last(), alwaysCreateApprovalRequest);
			helper.IsLevelAuthorizationRequired_ForTestOnly = testOnlyOverrides.Item1;
			helper.CheckLevelSecurityRights_ForTestOnly = testOnlyOverrides.Item2;
			helper.InitializeApprovalRequest_ForTestOnly = testOnlyOverrides.Item3;
			helper.GetNewHelper_ForTestOnly = testOnlyOverrides.Item4;

			return helper.PerformLevelAuthorization();
		}

		protected override TransactionApprovalRequest<APInvoiceChargesApprovalRequestDetails> PrepareOriginalRequest()
		{
			return Factory.New<APInvoiceChargesApprovalRequest>();
		}

		protected override InvoicingBase CreateTransactionHeader(ZString transactionNumber, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			return TestObjectCreator.CreateInvoice(typeof(APInvoice), transactionNumber, organisation: TestObjectCreator.Creditor1);
		}

		protected override void CreateTransactionLine(InvoicingBase transaction, Job job, decimal localAmount)
		{
			TestObjectCreator.CreateInvoiceLine(transaction, job, TestObjectCreator.CC1, localAmount);
		}

		protected override string GetExpectedMessageCaption()
		{
			return "Accounts Payable Invoice Approval Request";
		}

		readonly bool alwaysCreateApprovalRequest;

		protected override ZString GetDefaultXP_ParentTableCode() => AccTransactionHeaderSchema.Constants.Prefix;

		protected override ZString GetRequestParentObjectNameInLowcase() => "transaction";
	}
}
