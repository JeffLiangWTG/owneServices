using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class ARCreditNoteLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteLevelAuthorizationWithApprovalRequest(IPostingJobTransactionsApprovalGUIProvider postingGUIProvider)
			: base(postingGUIProvider)
		{
		}

		public bool PerformLevelAuthorization(InvoicingBase[] transactions, bool isApprovedRequestPosting)
		{
			if (((IPostingJobTransactionsApprovalGUIProvider)postingGUIProvider).IsForPreviewOnly)
			{
				return PerformTransactionLevelAuthorizationForTransactionPreviewOnly(transactions);
			}
			else
			{
				return PerformTransactionLevelAuthorization(transactions, out bool canContinueReversing, isApprovedRequestPosting);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool CheckLevelSecurityRights(InvoicingBase transaction, out Dictionary<Guid, Guid> creditNoteApprovingUserMapping, bool isFromAprovalModule = false)
		{
			return transaction.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, false, isFromAprovalModule);
		}

		protected override List<ARCreditNoteApprovalRequest> GetApprovalRequestsIncludingChildRequests(List<ARCreditNoteApprovalRequest> approvalRequests)
		{
			var result = new List<ARCreditNoteApprovalRequest>();
			foreach (var request in approvalRequests)
			{
				result.AddRange(request.ChildRequests);
			}
			result.AddRange(approvalRequests);
			return result;
		}

		protected override void IncludeTransactionLinesForCalculationIfApplicable(InvoicingBase transaction)
		{
			if (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.Value)
			{
				if (transaction.TransactionsForAuthorisationCalculation.Count == 0 && transaction.Lines.Count > 0)
				{
					transaction.TransactionsForAuthorisationCalculation.AddRange(transaction.GetLineLevelTransactionsGroupedByBranchAndDept());
				}
			}
		}

		protected override void SetApprovingUser(Guid userPK, List<InvoicingBase> transactions)
		{
			transactions.ForEach(x => x.ApprovingUserPK = userPK);
			base.SetApprovingUser(userPK, transactions);
		}

		protected override void OnPostApprovedRequestForThisPostingDetails(ARCreditNoteApprovalRequest postedRequest, List<InvoicingBase> approvedTransactions)
		{
			SetApprovingDetailsForTransactionsMatchingRequest(approvedTransactions, postedRequest);
			base.OnPostApprovedRequestForThisPostingDetails(postedRequest, approvedTransactions);
		}

		protected virtual Type GetTransactionType()
		{
			return typeof(ARCreditNote);
		}

		protected override void SetTransactionsForAuthorisationCalculation(InvoicingBase transactionWrapper, InvoicingBase[] transactions)
		{
			foreach (var transaction in transactions)
			{
				if (!(transactionWrapper).TransactionsForAuthorisationCalculation.Contains(transaction))
				{
					(transactionWrapper).TransactionsForAuthorisationCalculation.Add(transaction);

					if (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.Value)
					{
						transactionWrapper.TransactionsForAuthorisationCalculation.AddRange(transaction.GetLineLevelTransactionsGroupedByBranchAndDept());
					}
				}
			}
		}
		protected override ISecurityOverrideProviderWithApprovalRequest SetSecurityProvider(bool isForPreviewOnly, InvoicingBase maxTransaction, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvals = null)
		{
			var securityProvider = postingGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(!isForPreviewOnly, supportMultipleApprover, approvals);
			SecurityOverrideProviderSource.Get(maxTransaction).Provider = securityProvider;

#if DEBUG
			if (Globals.IsTest && !postingGUIProvider.ShowLoginFormForTest)
			{
				ZString testUserCode = "tst";
				const string testUserLogin = "newuser";
				const string testUserPassword = "password";
				var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, testUserCode);
				if (user == null)
				{
					Security.Testing.SecurityTestObject.CreateTestUser(true, postingGUIProvider.SecurityItemForTest ?? Env.Security.ReceivablesTransactions.Code, testUserCode, testUserLogin, testUserPassword);
				}
				var testProvider = new NonInteractiveSecurityOverrideProvider();
				testProvider.OverrideLogin = testUserLogin;
				testProvider.OverridePassword = testUserPassword;
				SecurityOverrideProviderSource.Get(maxTransaction).Provider = testProvider;
			}
#endif

			return securityProvider;
		}

#if DEBUG
		public
#else
		protected
#endif
		override InvoicingBase GetMaxTransaction(InvoicingBase[] transactions, InvoicingBase[] transactionsWithoutRight, Type transactionType)
		{
			var maxTransaction = (InvoicingBase)(new BusinessObjectFactory()).New(transactionType);
			foreach (var transaction in transactionsWithoutRight)
			{
				maxTransaction.TransactionsForAuthorisationCalculation.AddRange(transaction.TransactionsForAuthorisationCalculation);
			}
			foreach (var transaction in transactionsWithoutRight)
			{
				if (!maxTransaction.TransactionsForAuthorisationCalculation.Contains(transaction))
				{
					maxTransaction.TransactionsForAuthorisationCalculation.Add(transaction);
				}
			}
			return maxTransaction;
		}

		protected override bool IsLevelAuthorizationRequiredCore(InvoicingBase transaction)
		{
			return transaction.EnforceTwoApproversWhenPostingARCredit
				|| transaction.EnforceSequentialApproversWhenPostingARCredit
				|| transaction.LevelAuthorizationRequired;      // if registry ON, then always require authorization since double login required
		}

		protected override bool CheckLevelSecurityRightsCore(InvoicingBase transaction)
		{
			Dictionary<Guid, Guid> creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
			return CheckLevelSecurityRights(transaction, out creditNoteApprovingUserMapping);
		}

#if DEBUG
		public
#else
		protected
#endif
		override bool IsMultipleApproverApplicable => true;

		protected override void InitializeApprovalRequestCore(ARCreditNoteApprovalRequest request, InvoicingBase[] transactions)
		{
			var parent = GUIProviderParent;
			if (transactions.Any())
			{
				var transactionParent = GetJobParentOfTransaction(transactions.First());
				if (transactionParent.Item2 != JobConsolSchema.Constants.Prefix)
				{
					parent = transactionParent;
				}
			}
			request.Initialize(transactions, parent.Item1, parent.Item2, postingGUIProvider.PostingOption);
		}

#if DEBUG
		public
#else
		protected
#endif
		override bool ConfirmAndPreEditApprovalRequestByUser(TransactionApprovalRequest<ARCreditNoteApprovalRequestDetails>[] approvalRequests, InvoicingBase maxTransaction)
		{
			Guid toGuid(ZGuid pk) => pk.IsValid ? pk.ToGuid() : Guid.Empty;
			var canPrepopulateApprovingUser1 = false;
			var approvalRequest = approvalRequests.First();
			var registry = approvalRequest.XP_ApprovalType ==
							GenApprovalRequestApprovalType.ARCreditNoteForReversal
							? AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings
							: AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings;
			var modeAndSetting = registry.GetFallBackValueAtAllLevels(Guid.Empty, toGuid(approvalRequest.XP_GB_JobBranch), toGuid(approvalRequest.XP_GE_JobDepartment));
			if (!maxTransaction.LevelAuthorizationRequired && maxTransaction.CheckCurrentLoginUserHasSecurityRightsForAllTransactionsForAuthorisationCalculation())
			{
				canPrepopulateApprovingUser1 = true;
			}
			else if (modeAndSetting.AuthorizationMode == AuthorizationMode.Codes.SequentialApprovers)
			{
				var loginController = new UserLoginController();
				var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, approvalRequest.XP_GB_JobBranch.ToGuid(), approvalRequest.XP_GE_JobDepartment.ToGuid());
				if (userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed)
				{
					canPrepopulateApprovingUser1 = true;
				}
			}

			if (canPrepopulateApprovingUser1)
			{
				PrepopulateApprovingUser1ForMultipleLogin(approvalRequest);
			}
			return base.ConfirmAndPreEditApprovalRequestByUser(approvalRequests, maxTransaction);
		}

		protected void PrepopulateApprovingUser1ForMultipleLogin(TransactionApprovalRequest<ARCreditNoteApprovalRequestDetails> approvalRequest)
		{
			if (!approvalRequest.IsInDatabase && approvalRequest.XP_GS_NKApprovingUser1.IsEmpty &&
				(approvalRequest.PostingDetails.ApprovingOption == ApprovalCredentialOption.DoubleLogin ||
				 approvalRequest.PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin))
			{
				approvalRequest.XP_GS_NKApprovingUser1 = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(ARCreditNoteApprovalRequest request)
		{
			return new ARCreditNoteApprovalHelper(request);
		}

		protected override void CreateApprovalRequests(List<InvoicingBase> postedTransactions, List<ARCreditNoteApprovalRequest> approvalRequests, bool shouldCreateRequestPerTransaction)
		{
			if (!shouldCreateRequestPerTransaction && postedTransactions.Any() && IsPostedFromConsol)
			{
				foreach (var transactions in postedTransactions.GroupBy(GetJobParentOfTransaction))
				{
					CreateApprovalRequestCore(transactions.ToArray(), approvalRequests);
				}
			}
			else
			{
				base.CreateApprovalRequests(postedTransactions, approvalRequests, shouldCreateRequestPerTransaction);
			}
		}

		Tuple<ZGuid, ZString> GetJobParentOfTransaction(InvoicingBase transaction) => transaction.IsConsolInvoice ? GUIProviderParent : new Tuple<ZGuid, ZString>(transaction.AH_JH, new ZString(JobHeaderSchema.Constants.Prefix));
		Tuple<ZGuid, ZString> GUIProviderParent => postingGUIProvider.GetParentIdAndTableCodeForJobPostingAction();
		bool IsPostedFromConsol => GUIProviderParent.Item2 == JobConsolSchema.Constants.Prefix;
	}

	public class ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<ARCreditNote, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteForAmendingLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, ARCreditNote creditNote, ILogger serviceLogger = null)
			: base(postingGUIProvider, serviceLogger)
		{
			this.creditNote = creditNote;
		}
		readonly ARCreditNote creditNote;

		public bool PerformLevelAuthorization()
		{
			return PerformTransactionLevelAuthorization(new[] { creditNote }, out bool canContinueReversing);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest SetSecurityProvider(bool isForPreviewOnly, ARCreditNote maxTransaction, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvals = null)
		{
			var securityProvider = postingGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(!isForPreviewOnly, supportMultipleApprover, approvals);
			SecurityOverrideProviderSource.Get(maxTransaction).Provider = securityProvider;

#if DEBUG
			if (Globals.IsTest && !postingGUIProvider.ShowLoginFormForTest)
			{
				ZString testUserCode = "tst";
				const string testUserLogin = "newuser";
				const string testUserPassword = "password";
				var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, testUserCode);
				if (user == null)
				{
					Security.Testing.SecurityTestObject.CreateTestUser(true, postingGUIProvider.SecurityItemForTest ?? Env.Security.ReceivablesTransactions.Code, testUserCode, testUserLogin, testUserPassword);
				}
				var testProvider = new NonInteractiveSecurityOverrideProvider();
				testProvider.OverrideLogin = testUserLogin;
				testProvider.OverridePassword = testUserPassword;
				SecurityOverrideProviderSource.Get(maxTransaction).Provider = testProvider;
			}
#endif
			return securityProvider;
		}

#if DEBUG
		public
#else
		protected
#endif
		override ARCreditNote GetMaxTransaction(ARCreditNote[] transactions, ARCreditNote[] transactionsWithoutRights, Type transactionType)
		{
			var maxTransaction = (ARCreditNote)(new BusinessObjectFactory()).New(transactionType);
			foreach (var transaction in transactionsWithoutRights)
			{
				maxTransaction.TransactionsForAuthorisationCalculation.AddRange(transaction.TransactionsForAuthorisationCalculation);
			}
			foreach (var transaction in transactionsWithoutRights)
			{
				if (!maxTransaction.TransactionsForAuthorisationCalculation.Contains(transaction))
				{
					maxTransaction.TransactionsForAuthorisationCalculation.Add(transaction);
				}
			}
			return maxTransaction;
		}

		protected override List<ARCreditNoteApprovalRequest> GetApprovalRequestsIncludingChildRequests(List<ARCreditNoteApprovalRequest> approvalRequests)
		{
			var result = new List<ARCreditNoteApprovalRequest>();
			foreach (var request in approvalRequests)
			{
				result.AddRange(request.ChildRequests);
			}
			result.AddRange(approvalRequests);
			return result;
		}

		protected override void UpdateBranchDepartmentToOriginalTransactionForAmendingOnly(ARCreditNote transaction)
		{
			if (transaction is IAmending)   // update branch/department to be the same as the original transaction
			{
				var amending = transaction as IAmending;
				if (amending.OriginalTransaction != null && amending.OriginalTransaction is AccTransactionHeader)
				{
					transaction.AH_GB = ((AccTransactionHeader)amending.OriginalTransaction).AH_GB;
					transaction.AH_GE = ((AccTransactionHeader)amending.OriginalTransaction).AH_GE;
				}
			}
		}

		protected override void IncludeTransactionLinesForCalculationIfApplicable(ARCreditNote transaction)
		{
			if (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.Value)
			{
				if (transaction.TransactionsForAuthorisationCalculation.Count == 0 && transaction.Lines.Count > 0)
				{
					transaction.TransactionsForAuthorisationCalculation.AddRange(transaction.GetLineLevelTransactionsGroupedByBranchAndDept());
				}
			}
		}

		protected override bool IsLevelAuthorizationRequiredCore(ARCreditNote transaction)
		{
			return transaction.EnforceTwoApproversWhenPostingARCredit || transaction.EnforceSequentialApproversWhenPostingARCredit
				|| transaction.LevelAuthorizationRequired;      // if registry ON, then always require authorization since double login required;
		}

		protected override bool CheckLevelSecurityRightsCore(ARCreditNote transaction)
		{
			Dictionary<Guid, Guid> creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
			return ARCreditNoteLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(transaction, out creditNoteApprovingUserMapping);
		}

		protected override void SetApprovingUser(Guid userPK, List<ARCreditNote> transactions)
		{
			transactions.ForEach(x => x.ApprovingUserPK = userPK);
			base.SetApprovingUser(userPK, transactions);
		}

		protected override void OnPostApprovedRequestForThisPostingDetails(ARCreditNoteApprovalRequest postedRequest, List<ARCreditNote> approvedTransactions)
		{
			SetApprovingDetailsForTransactionsMatchingRequest(approvedTransactions.Cast<InvoicingBase>().ToList(), postedRequest);
			base.OnPostApprovedRequestForThisPostingDetails(postedRequest, approvedTransactions);
		}

#if DEBUG
		public
#else
		protected
#endif
		override bool IsMultipleApproverApplicable => true;

#if DEBUG
		public
#else
		protected
#endif
		override bool ConfirmAndPreEditApprovalRequestByUser(TransactionApprovalRequest<ARCreditNoteApprovalRequestDetails>[] approvalRequests, ARCreditNote maxTransaction)
		{
			var canPrepopulateApprovingUser1 = false;
			var approvalRequest = approvalRequests.First();
			if (!approvalRequest.IsInDatabase && approvalRequest.XP_GS_NKApprovingUser1.IsEmpty
				&& (approvalRequest.PostingDetails.ApprovingOption == ApprovalCredentialOption.DoubleLogin
					|| approvalRequest.PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin))
			{
				var modeAndSetting = AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(Guid.Empty, approvalRequest.XP_GB_JobBranch.ToGuid(), approvalRequest.XP_GE_JobDepartment.ToGuid());
				if (!maxTransaction.LevelAuthorizationRequired)
				{
					canPrepopulateApprovingUser1 = true;
				}
				else if (modeAndSetting.AuthorizationMode == AuthorizationMode.Codes.SequentialApprovers)
				{
					var loginController = new UserLoginController();
					var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, approvalRequest.XP_GB_JobBranch.ToGuid(), approvalRequest.XP_GE_JobDepartment.ToGuid());
					if (userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed)
					{
						canPrepopulateApprovingUser1 = true;
					}
				}

				if (canPrepopulateApprovingUser1)
				{
					approvalRequest.XP_GS_NKApprovingUser1 = GlbStaff.CurrentUser.GS_Code;
				}
			}

			return base.ConfirmAndPreEditApprovalRequestByUser(approvalRequests, maxTransaction);
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(ARCreditNoteApprovalRequest request)
		{
			return new ARCreditNoteApprovalHelper(request);
		}

		protected override void InitializeApprovalRequestCore(ARCreditNoteApprovalRequest request, ARCreditNote[] transactions)
		{
			var amending = creditNote as IAmending;
			var originalTransaction = amending.OriginalTransaction;
			request.Initialize(new[] { creditNote }, originalTransaction.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.All);
		}
	}

	public class APInvoiceLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceLevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, InvoicingBase invoice, bool alwaysCreateApprovalRequest)
			: base(postingGUIProvider)
		{
			this.invoice = invoice;
			this.alwaysCreateApprovalRequest = alwaysCreateApprovalRequest;
			postingGUIProvider?.FactoryForApprovalRequests.ServiceContainer.AddService(new ParentFactoryService(invoice.Factory));
		}
		readonly InvoicingBase invoice;
		readonly bool alwaysCreateApprovalRequest;

		public bool PerformLevelAuthorization(bool isApprovedRequestPosting = false)
		{
			return PerformTransactionLevelAuthorization(new[] { invoice }, out bool canContinueReversing, isApprovedRequestPosting);
		}

		public static bool CheckLevelSecurityRights(InvoicingBase transaction)
		{
			return APInvoiceChargesLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(transaction, new UnapprovedTransactionValidationHelper());
		}

		protected override bool IsLevelAuthorizationRequiredCore(InvoicingBase transaction)
		{
			var requiredCheckpoint = ValidationHelper.RequiredSecurityCheckPoint(transaction);
			return alwaysCreateApprovalRequest || !requiredCheckpoint.IsAllowed;
		}

		protected override bool CheckLevelSecurityRightsCore(InvoicingBase transaction)
		{
			return !alwaysCreateApprovalRequest && APInvoiceChargesLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(transaction, ValidationHelper);
		}

		protected override void InitializeApprovalRequestCore(APInvoiceChargesApprovalRequest request, InvoicingBase[] transactions)
		{
			request.InitializeInvoiceRelated(invoice);
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(APInvoiceChargesApprovalRequest request)
		{
			return new APInvoiceChargesApprovalHelper(request);
		}

		protected override void SetApprovingUser(Guid userPK, List<InvoicingBase> tranasctions)
		{
			invoice.ApprovingUserPK = userPK;
		}

		UnapprovedTransactionValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new UnapprovedTransactionValidationHelper()); }
		}
		UnapprovedTransactionValidationHelper validationHelper;
	}

	public abstract class LevelAuthorizationWithApprovalRequest<TransactionType, RequestType, DetailsType>
		where TransactionType : ITransactionForApproval
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public LevelAuthorizationWithApprovalRequest(IPostingTransactionApprovalGUIProvider postingGUIProvider, ILogger serviceLogger = null)
		{
			this.serviceLogger = serviceLogger;
			this.postingGUIProvider = postingGUIProvider;
		}

		protected readonly ILogger serviceLogger;
		protected readonly IPostingTransactionApprovalGUIProvider postingGUIProvider;

		protected bool PerformTransactionLevelAuthorizationForTransactionPreviewOnly(TransactionType[] transactions, RequestType originalRequest = null)
		{
			if (!transactions.Any())
			{
				return true;
			}

			if (originalRequest != null)  // need to compare with original request to see any changes are made.
			{
				try
				{
					if (DetectOriginalRequestHasBeenModified(originalRequest, transactions))
					{
						string errorMsg = Res.GetString("1ad15c61-a629-45ed-bb02-1901b0d9a5ca", "Can't preview this request because source details have been modified since then.");
						postingGUIProvider.ShowMessage(errorMsg, Res.GetString("05c78585-bd24-4dd1-b8e4-6643528bbaed", "Approval Request Preview"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
						postingGUIProvider.RollbackPosting();
					}
				}
				catch (ExternalStorageException)
				{
					var errorMsg = AllocateErrorMessage.LastExternalStorageExceptionMessage;
					postingGUIProvider.ShowMessage(errorMsg, Res.GetString("D691CFF3-0032-469D-9349-45C4640F1633", "External Storage Problem"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
					postingGUIProvider.RollbackPosting();
				}
			}
			else  // if preview from job invoicing menu, request is null, then still need to check current login user's security rights.
			{
				var maxTransaction = transactions.MaxBy(x => (decimal)x.AH_LocalTotalAmount);
				SetTransactionsForAuthorisationCalculation(maxTransaction, transactions);

				var securityProvider = SetSecurityProvider(true, maxTransaction);
				if (!CheckLevelSecurityRights(maxTransaction))
				{
					postingGUIProvider.RollbackPosting();
				}
			}

			return true;
		}

		protected virtual void SetTransactionsForAuthorisationCalculation(TransactionType transaction, TransactionType[] transactions)
		{
		}

		protected virtual List<RequestType> GetApprovalRequestsIncludingChildRequests(List<RequestType> approvalRequests)
		{
			return approvalRequests;
		}

		bool DetectOriginalRequestHasBeenModified(RequestType originalRequest, TransactionType[] transactions)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbStaff createdByUser = newFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, originalRequest.XP_SystemCreateUser)) ?? GlbStaff.CurrentUser;
			using (Env.SetTemporaryUserContext(createdByUser.PK.ToGuid(), originalRequest.XP_GB_RequestingBranch.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))  // switch to the creating user context temporarily in order to create matchable request
			{
				var postedTransactions = GetOnlyTransactionWithoutRights(transactions);
				var approvalRequest = newFactory.New<RequestType>();
				InitializeApprovalRequest(approvalRequest, postedTransactions.ToArray());

				ITransactionApprovalHelper<RequestType, DetailsType> approvalHelper = GetNewHelper(approvalRequest) as ITransactionApprovalHelper<RequestType, DetailsType>;
				if (approvalHelper != null)
				{
					return !approvalHelper.IsRequestForTheSamePostingActionAndDetails(originalRequest);
				}
				return false;
			}
		}

#if DEBUG
		public
#else
	protected
#endif
		virtual bool IsMultipleApproverApplicable => false;

#if DEBUG
		public
#else
		protected
#endif
		virtual TransactionType GetMaxTransaction(TransactionType[] transactions, TransactionType[] transactionsWithoutRights, Type transactionType)
		{
			return transactions.MaxBy(x => (decimal)x.AH_LocalTotalAmount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		protected bool PerformTransactionLevelAuthorization(TransactionType[] transactions, out bool canContinueReversing, bool isApprovedRequestPosting = false, bool shouldCreateRequestPerTransaction = false)
		{
			canContinueReversing = false;
			if (!transactions.Any())
			{
				return true;
			}

			var transactionsWithoutRights = GetOnlyTransactionWithoutRights(transactions);

			var continueProcessing = true;
			var approvalRequests = new List<RequestType>() { };
			var approvalHelpers = new List<ITransactionApprovalHelper>() { };
			var shouldResetFactory = true;
			var shouldApprovalRequestsBeDeleted = true;
			try
			{
				CreateApprovalRequests(transactionsWithoutRights, approvalRequests, shouldCreateRequestPerTransaction);
				approvalRequests.ForEach(x => approvalHelpers.Add(GetNewHelper(x)));

				var transactionType = transactions[0].GetType();
				var maxTransaction = GetMaxTransaction(transactions, transactionsWithoutRights.ToArray(), transactionType);
				var approvalRequestsIncludingChildren = GetApprovalRequestsIncludingChildRequests(approvalRequests);
				var securityProvider = SetSecurityProvider(false, maxTransaction, transactionsWithoutRights.Count > 0 && IsMultipleApproverApplicable, approvalRequestsIncludingChildren.ToArray());   // 3rd parameter indicates whether 2nd security approver feature is applicable to derived authroization class
				var helpersForApprovedRequests = approvalHelpers.Where(x => x.IsThereApprovedRequestForThisPostingDetails).ToList();
				if (!shouldCreateRequestPerTransaction && helpersForApprovedRequests.Any())
				{
					var postedRequests = new List<GenApprovalRequest>();
					foreach (var helper in helpersForApprovedRequests)
					{
						var postedRequest = helper.PostApprovedRequestForThisPostingDetails();
						postedRequests.Add(postedRequest);
					}
					postedRequests.ForEach(x => OnPostApprovedRequestForThisPostingDetails(x as RequestType, transactionsWithoutRights));
					shouldResetFactory = false;
				}
				else if (isApprovedRequestPosting)
				{
					var errorMsg = Res.GetString("C1A2339F-0C09-4580-9108-011C19747C8A", "Can't post this request because source details have been modified since then.");
					postingGUIProvider.ShowMessage(errorMsg, Res.GetString("6A40DE63-2614-4BF8-A214-05C200D5978A", "Approval Request Posting"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
					serviceLogger?.Debug((NoResString)"Can't post this request because source details have been modified since then.");
				}
				else
				{
					var errorTextForBulkPosting = new ZStringBuilder();
					var messageCaption = GetMessageCaption(maxTransaction);
					var isUserWithRights = !IsLevelAuthorizationRequired(maxTransaction);
					var continueWithApprovalRequestCreation = true;
					continueWithApprovalRequestCreation = CheckIfThereAreApprovedRequestsForThisPostingActionButAnotherPostingDetails(shouldCreateRequestPerTransaction, approvalHelpers, errorTextForBulkPosting, messageCaption, isUserWithRights, continueWithApprovalRequestCreation);
					continueWithApprovalRequestCreation = HandleBulkPosting(transactions, errorTextForBulkPosting, isUserWithRights, continueWithApprovalRequestCreation);

					var isPostingApproved = false;
					if (continueWithApprovalRequestCreation)
					{
						if (shouldCreateRequestPerTransaction && !isUserWithRights && helpersForApprovedRequests.Count == approvalHelpers.Count)
						{
							serviceLogger?.Debug($"Continue with approval request creation and call ProcessApprovedRequests on type {GetType()}.");
							canContinueReversing = true;
							shouldResetFactory = false;
							ProcessApprovedRequests(approvalHelpers, transactionsWithoutRights);
						}
						else if (isUserWithRights || CheckLevelSecurityRights(maxTransaction)) //here user can decide and tell securityProvider ShouldApprovalRequestBeCreated
						{
							AskUserAndCancelRequestedApprovalsForThisPostingAction(approvalHelpers, askUser: false);
							UpdateExisitingApprovalRequestsStatus(transactions);
							if (ShouldCreateRequestOnEveryPosting)
							{
								approvalHelpers.First().ApproveAndPostRequest(securityProvider);
								shouldApprovalRequestsBeDeleted = false;
							}
							isPostingApproved = true;
							shouldResetFactory = false;
							canContinueReversing = true;

							if (!isUserWithRights && securityProvider.UserSecurityOverride != null)
							{
								SetApprovingUser(securityProvider.UserSecurityOverride.UserPK, transactionsWithoutRights);
							}

							serviceLogger?.Debug($"Continue with approval request creation and user with rights or level security rights checked. {nameof(shouldResetFactory)} is {shouldResetFactory}.");
						}
						else if (ShouldAlwaysCreateRequest(maxTransaction) || securityProvider.ShouldApprovalRequestBeCreated)
						{
							continueWithApprovalRequestCreation = AskUserAndCancelRequestedApprovalsForThisPostingAction(approvalHelpers, messageCaption, approvalRequests.First().ReferenceType,
									askUser: approvalHelpers.Any(x => x.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus));

							shouldResetFactory = PopulateTransactionsWithoutApprovedRequest(approvalHelpers, helpersForApprovedRequests, transactionsWithoutRights);

							if (continueWithApprovalRequestCreation && ConfirmAndPreEditApprovalRequestByUser(approvalRequests.ToArray(), maxTransaction))
							{
								if (helpersForApprovedRequests.Count > 0)
								{
									DeleteNewApprovalRequestCreatedForApprovedRequest(helpersForApprovedRequests);
								}
								shouldApprovalRequestsBeDeleted = false;
								shouldResetFactory = false;
								ShowExitMessage(false);
							}

							serviceLogger?.Debug($"Continue with approval request creation and should always create request. {nameof(shouldResetFactory)} is {shouldResetFactory}.");
						}
						else
						{
							shouldResetFactory = PopulateTransactionsWithoutApprovedRequest(approvalHelpers, helpersForApprovedRequests, transactionsWithoutRights);
							ShowExitMessage(true);

							serviceLogger?.Debug($"Continue with approval request creation with mo matching case. {nameof(shouldResetFactory)} is {shouldResetFactory}.");
						}
					}
					else
					{
						serviceLogger?.Debug($"Omit to continue with approval request creation. {nameof(errorTextForBulkPosting)}: {errorTextForBulkPosting}.");
					}

					RollBackPostingIfPostingNotApproved(isPostingApproved);
				}
			}
			catch (ExternalStorageException)
			{
				var errorMsg = AllocateErrorMessage.LastExternalStorageExceptionMessage;
				postingGUIProvider.ShowMessage(errorMsg, Res.GetString("BAA3740E-2259-45EA-83F3-4E4C334A0E55", "External Storage Problem"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
				postingGUIProvider.RollbackPosting();
				serviceLogger?.Warning($"ExternalStorageException performing transaction level authorization. {nameof(errorMsg)}: {errorMsg}");
			}
			catch (Exception ex)
			{
				serviceLogger?.Error((NoResString)"Exception performing transaction level authorization", ex);
				throw;
			}
			finally
			{
				continueProcessing = CleanUpApprovalRequests(continueProcessing, approvalRequests, shouldResetFactory, shouldApprovalRequestsBeDeleted);
				serviceLogger?.Debug($"Completed performing transaction level authorization. {nameof(continueProcessing)} is {continueProcessing}, {nameof(shouldResetFactory)} is {shouldResetFactory}.");
			}
			return continueProcessing;
		}

		protected virtual bool PopulateTransactionsWithoutApprovedRequest(List<ITransactionApprovalHelper> approvalHelpers, List<ITransactionApprovalHelper> helpersForApprovedRequests, List<TransactionType> approvedTransactions)
		{
			return true;
		}

		protected virtual void ProcessApprovedRequests(List<ITransactionApprovalHelper> approvalHelpers, List<TransactionType> approvedTransactions)
		{
		}

		protected virtual void ShowExitMessage(bool isCancellingOutFromLoginForm)
		{
		}

		protected virtual void DeleteNewApprovalRequestCreatedForApprovedRequest(List<ITransactionApprovalHelper> helpersForApprovedRequests)
		{
		}

		protected virtual void UpdateExisitingApprovalRequestsStatus(TransactionType[] transactions)
		{
		}

		protected void SetApprovingDetailsForTransactionsMatchingRequest(List<InvoicingBase> approvedTransactions, ARCreditNoteApprovalRequest postedRequest)
		{
			if (postedRequest == null)
			{
				return;
			}
			IEnumerable<InvoicingBase> transactionsForRequest = null;
			switch (postedRequest.XP_ParentTableCode)
			{
				case AccTransactionHeaderSchema.Constants.Prefix:
					transactionsForRequest = approvedTransactions.Where(x => x.PK == postedRequest.XP_ParentID || x.OriginalTransactionReference == postedRequest.XP_ParentID);
					break;
				case JobHeaderSchema.Constants.Prefix:
					transactionsForRequest = approvedTransactions.Where(x => x.IsJobRelated && (x.Job?.PK == postedRequest.XP_ParentID || x.Lines.Cast<InvoicingLineBase>().Any(y => y.AL_JH.IsValid)));
					break;
				case JobConsolSchema.Constants.Prefix:
					transactionsForRequest = approvedTransactions.Where(x => x.IsConsolInvoice && x.Consol?.PK == postedRequest.XP_ParentID);
					break;
			}

			foreach (var invoice in transactionsForRequest)
			{
				invoice.ApprovingUserPKList = postedRequest.GetApprovingUserPKs();
				invoice.ApprovalDate = postedRequest.XP_ApprovalDate;
			}
		}

		bool HandleBulkPosting(TransactionType[] transactions, ZStringBuilder errorTextForBulkPosting, bool isUserWithRights, bool continueWithApprovalRequestCreation)
		{
			if (postingGUIProvider.IsBulkPosting && !isUserWithRights)
			{
				errorTextForBulkPosting.AppendLine(GetSecurityError(transactions.Length > 1));
				postingGUIProvider.NotifyBulkPostingIsNotAuthorized(errorTextForBulkPosting.ToString());
				continueWithApprovalRequestCreation = false;
			}

			return continueWithApprovalRequestCreation;
		}

		bool CheckIfThereAreApprovedRequestsForThisPostingActionButAnotherPostingDetails(bool shouldCreateRequestPerTransaction, List<ITransactionApprovalHelper> approvalHelpers, ZStringBuilder errorTextForBulkPosting, string messageCaption, bool isUserWithRights, bool continueWithApprovalRequestCreation)
		{
			var helper = approvalHelpers.FirstOrDefault(x => x.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails);

			if (helper != null)
			{
				continueWithApprovalRequestCreation = AskUserAndCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(approvalHelpers, errorTextForBulkPosting, messageCaption, shouldCreateRequestPerTransaction, askUser: !isUserWithRights);

				if (!continueWithApprovalRequestCreation)
				{
					LogComparisonDataWithTheOtherPostingDetails(helper);
				}
			}

			return continueWithApprovalRequestCreation;
		}

		void LogComparisonDataWithTheOtherPostingDetails(ITransactionApprovalHelper approvalHelper)
		{
			if (serviceLogger != null)
			{
				var comparisonData = approvalHelper.GetComparisonDataWithTheOtherPostingDetails();

				if (!string.IsNullOrWhiteSpace(comparisonData))
				{
					serviceLogger.Debug($"Posting details were changed. Data for comparison follows: {System.Environment.NewLine}{comparisonData}");
				}
			}
		}

		void RollBackPostingIfPostingNotApproved(bool isPostingApproved)
		{
			if (!isPostingApproved)
			{
				postingGUIProvider.RollbackPosting();
			}
		}

		bool CleanUpApprovalRequests(bool continueProcessing, List<RequestType> approvalRequests, bool shouldResetFactory, bool shouldApprovalRequestsBeDeleted)
		{
			if (shouldApprovalRequestsBeDeleted && approvalRequests.Count > 0)
			{
				approvalRequests.ForEach(x => x.Delete());
			}
			if (shouldResetFactory)
			{
				postingGUIProvider.ResetFactoryForApprovalRequests();
				continueProcessing = false;
			}
			else if (approvalRequests.Count > 0)
			{
				approvalRequests.Where(x => !x.IsDeleted).ForEach(x => x.PrepareFoSaving());
			}

			return continueProcessing;
		}

		protected virtual void CreateApprovalRequests(List<TransactionType> postedTransactions, List<RequestType> approvalRequests, bool shouldCreateRequestPerTransaction)
		{
			if (shouldCreateRequestPerTransaction)
			{
				postedTransactions.ForEach(x => CreateApprovalRequestCore(new[] { x }, approvalRequests));
			}
			else
			{
				CreateApprovalRequestCore(postedTransactions.ToArray(), approvalRequests);
			}
		}

		protected void CreateApprovalRequestCore(TransactionType[] postedTransactions, List<RequestType> approvalRequests)
		{
			var approvalRequest = CreateNewApprovalRequest(postingGUIProvider.FactoryForApprovalRequests);
			InitializeApprovalRequest(approvalRequest, postedTransactions);
			approvalRequests.Add(approvalRequest);
		}

		protected virtual string GetMessageCaption(TransactionType maxTransaction)
		{
			return Res.GetString("0569E93F-C9D1-4CF3-9DDE-636111D69ED5", "{0} Approval Request", maxTransaction.HumanReadableName);
		}

		protected virtual RequestType CreateNewApprovalRequest(BusinessObjectFactory factory)
		{
			return factory.New<RequestType>();
		}

		ZString GetSecurityError(bool isMultipleTransaction)
		{
			var securityError = ZString.Empty;
			if (isMultipleTransaction)
			{
				securityError = Res.GetString("C631120C-050D-42E7-BC0D-7AC349D07E53", "You do not have security rights to post transactions with these amounts. To continue posting of these transactions by an authorized user post individual transactions from billing tab.");
			}
			else
			{
				securityError = Res.GetString("cfd6e6aa-f8b0-4a79-9f01-f7aec914c8ff", "You do not have security rights to post the transaction with this amount. To continue posting of this transaction by an authorized user post it from billing tab.");
			}
			return securityError;
		}

		protected virtual void SetApprovingUser(Guid userPK, List<TransactionType> tranasctions)
		{
		}

		protected virtual bool ShouldAlwaysCreateRequest(TransactionType maxTransaction)
		{
			return false;
		}

#if DEBUG
		public
#else
		protected
#endif
		virtual bool ConfirmAndPreEditApprovalRequestByUser(TransactionApprovalRequest<DetailsType>[] approvalRequests, TransactionType maxTransaction)
		{
			var result = true;
			if (postingGUIProvider != null)
			{
				var userAnswer = postingGUIProvider.ShowApprovalFormToSetDescription(approvalRequests.First());
				result = userAnswer == ZDialogResult.OK;
			}
			return result;
		}

		protected virtual bool ShouldCreateRequestOnEveryPosting { get { return false; } }

		protected virtual bool ProcessExistingRequestsWithoutShowingMessages { get { return false; } }

		protected virtual void OnPostApprovedRequestForThisPostingDetails(RequestType postedRequest, List<TransactionType> approvedTransaction) { }

		protected virtual void OnCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(bool isUserConfirmationRequired) { }

		protected virtual void OnCancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus(bool isUserConfirmationRequired) { }

#if DEBUG
		public
#endif
		List<TransactionType> GetOnlyTransactionWithoutRights(TransactionType[] transactions)
		{
			var postedTransactions = new List<TransactionType>(transactions.Length);
			foreach (var transaction in transactions)
			{
				UpdateBranchDepartmentToOriginalTransactionForAmendingOnly(transaction);
				IncludeTransactionLinesForCalculationIfApplicable(transaction);
				if (IsLevelAuthorizationRequired(transaction))
				{
					postedTransactions.Add(transaction);
				}
			}
			return postedTransactions;
		}

		protected virtual void UpdateBranchDepartmentToOriginalTransactionForAmendingOnly(TransactionType transaction)
		{
		}

		protected virtual void IncludeTransactionLinesForCalculationIfApplicable(TransactionType transaction)
		{
		}

		bool AskUserAndCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(List<ITransactionApprovalHelper> approvalHelpers, ZStringBuilder errorTextForBulkPosting, string messageCaption, bool shouldCreateRequestPerTransactions, bool askUser = true)
		{
			var cancelExistingRequest = true;
			if (askUser)
			{
				var messageText = shouldCreateRequestPerTransactions ? Res.GetString("acf5c7cc-b38a-46e1-be5b-3ba5585a93f9", "There are approved requests for this posting action, but data for approvals are different.") : Res.GetString("3AF03324-3130-49BF-B543-F4AC022D5CBE", "There is an approved request for this posting action, but data for approval is different.");
				if (postingGUIProvider.IsBulkPosting)
				{
					errorTextForBulkPosting.AppendLine(messageText);
				}
				else if (!ProcessExistingRequestsWithoutShowingMessages)
				{
					messageText = messageText + " " + (shouldCreateRequestPerTransactions ? Res.GetString("6e6bcf23-f255-4ef3-b0ea-8d6f4d3846e2", "These approved requests must be canceled to continue posting.") : Res.GetString("8AB14386-B9CD-424C-81E4-C21173513DCE", "This approved request must be canceled to continue posting."));
					var userAnswer = postingGUIProvider.ShowMessage(messageText, messageCaption, ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Warning, ZDialogResult.OK);
					cancelExistingRequest = userAnswer == ZDialogResult.OK;
				}
			}

			if (cancelExistingRequest)
			{
				foreach (var approvalHelper in approvalHelpers)
				{
					if (approvalHelper.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails())
					{
						OnCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(askUser);
					}
				}
			}
			return cancelExistingRequest;
		}

		protected bool AskUserAndCancelRequestedApprovalsForThisPostingAction(List<ITransactionApprovalHelper> approvalHelpers, string messageCaption = "", string approvalRequestReferenceType = "", bool askUser = true)
		{
			var cancelExistingRequest = true;
			if (!ProcessExistingRequestsWithoutShowingMessages && askUser)
			{
				var messageText = string.Empty;
				if (approvalHelpers.Count == 1)
				{
					messageText = Res.GetString("4B6EDE86-E646-40E9-96AF-16C0FBC01636",
@"There is another request for this {0}. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?",
					approvalRequestReferenceType.ToLower(CultureInfo.CurrentCulture));
				}
				else
				{
					messageText = Res.GetString("23b6e3b4-ae5b-46f2-8f16-940b6d033a54",
@"At least one of the transactions already has a request. Only one request is permitted for each transaction.

Do you want to cancel previous request and queue this one for approval?");
				}
				var userAnswer = postingGUIProvider.ShowMessage(messageText, messageCaption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);
				cancelExistingRequest = userAnswer == ZDialogResult.Yes;
			}

			if (cancelExistingRequest)
			{
				foreach (var approvalHelper in approvalHelpers)
				{
					if (approvalHelper.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus())
					{
						OnCancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus(askUser);
					}
				}
			}
			return cancelExistingRequest;
		}

		protected virtual ISecurityOverrideProviderWithApprovalRequest SetSecurityProvider(bool isForPreviewOnly, TransactionType maxTransaction, bool supportMultipleApprover = false, RequestType[] approvals = null)
		{
			var securityProvider = postingGUIProvider.GetNewSecurityOverrideProvider(!isForPreviewOnly, supportMultipleApprover);
			SecurityOverrideProviderSource.Get(maxTransaction).Provider = securityProvider;

#if DEBUG
			SetSecurityProvider_ForTestOnly(maxTransaction);
#endif

			return securityProvider;
		}

#if DEBUG
		protected virtual void SetSecurityProvider_ForTestOnly(TransactionType maxTransaction)
		{
			if (Globals.IsTest && !postingGUIProvider.ShowLoginFormForTest)
			{
				ZString testUserCode = "tst";
				const string testUserLogin = "newuser";
				const string testUserPassword = "password";
				var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, testUserCode);
				if (user == null)
				{
					Security.Testing.SecurityTestObject.CreateTestUser(true, postingGUIProvider.SecurityItemForTest ?? Env.Security.ReceivablesTransactions.Code, testUserCode, testUserLogin, testUserPassword);
				}
				var testProvider = new NonInteractiveSecurityOverrideProvider();
				testProvider.OverrideLogin = testUserLogin;
				testProvider.OverridePassword = testUserPassword;
				SecurityOverrideProviderSource.Get(maxTransaction).Provider = testProvider;
			}
		}
#endif

#if DEBUG
		public
#endif
		bool IsLevelAuthorizationRequired(TransactionType transaction)
		{
#if DEBUG
			if (Globals.IsTest && IsLevelAuthorizationRequired_ForTestOnly != null)
			{
				return IsLevelAuthorizationRequired_ForTestOnly(transaction);
			}
#endif
			return IsLevelAuthorizationRequiredCore(transaction);
		}
		protected abstract bool IsLevelAuthorizationRequiredCore(TransactionType transaction);

		bool CheckLevelSecurityRights(TransactionType transaction)
		{
#if DEBUG
			if (Globals.IsTest && CheckLevelSecurityRights_ForTestOnly != null)
			{
				return CheckLevelSecurityRights_ForTestOnly(transaction);
			}
#endif
			return CheckLevelSecurityRightsCore(transaction);
		}
		protected abstract bool CheckLevelSecurityRightsCore(TransactionType transaction);

		void InitializeApprovalRequest(RequestType request, TransactionType[] transactions)
		{
#if DEBUG
			if (Globals.IsTest && InitializeApprovalRequest_ForTestOnly != null)
			{
				InitializeApprovalRequest_ForTestOnly(request, transactions);
			}
			else
#endif
			{
				InitializeApprovalRequestCore(request, transactions);
			}
		}
		protected abstract void InitializeApprovalRequestCore(RequestType request, TransactionType[] transactions);

		protected ITransactionApprovalHelper GetNewHelper(RequestType request)
		{
#if DEBUG
			if (Globals.IsTest && GetNewHelper_ForTestOnly != null)
			{
				return GetNewHelper_ForTestOnly(request);
			}
#endif
			return GetNewHelperCore(request);
		}
		protected abstract ITransactionApprovalHelper GetNewHelperCore(RequestType request);

#if DEBUG
		public Func<TransactionType, bool> IsLevelAuthorizationRequired_ForTestOnly;
		public Func<TransactionType, bool> CheckLevelSecurityRights_ForTestOnly;
		public Action<RequestType, TransactionType[]> InitializeApprovalRequest_ForTestOnly;
		public Func<RequestType, ITransactionApprovalHelper> GetNewHelper_ForTestOnly;
#endif
	}
}
