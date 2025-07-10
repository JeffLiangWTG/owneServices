#define CODE_ANALYSIS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public abstract partial class PostManagerGUIWrapper
	{
		public PostManagerGUIWrapper(IJobHeaderParent jobParent, JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, Form parentForm, Action refreshJobCharges = null)
		{
			this.JobParent = jobParent;
			this.TransactionFactory = new BusinessObjectFactory();
			this.PostingOption = postingOption;
			this.PlugInFactory = plugInFactory;
			this.ParentForm = parentForm;
			this.IsPostedForBatchInvoicing = false;
			this.RefreshJobChargesAction = refreshJobCharges;

			bulkPostingDataCollector = new BulkPostingDataCollector();
			AfterCreationBeforePostTransactionActions = ObjectFactory.Get<IPostManagerCreatedTransactionActions>();
		}

		readonly Action RefreshJobChargesAction;

		public AllowedDeliveryOptions AllowedReportDeliveryOption
		{
			get { return allowedReportDeliveryOption; }
			set { allowedReportDeliveryOption = value; }
		}
		AllowedDeliveryOptions allowedReportDeliveryOption = AllowedDeliveryOptions.All;

		protected readonly JobInvoicingPostingOption PostingOption;
		protected readonly BusinessObjectFactory PlugInFactory;
		protected virtual JobInvoicingSecurityHelper GetSecurityHelper(IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			return new JobInvoicingSecurityHelper(jobInvoicingPlugIn == null ? null : jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity, false);
		}

		protected BusinessObjectFactory TransactionFactory { get; set; }
		protected IJobHeaderParent JobParent { get; set; }

		protected IEnumerable<Job> OriginalJobs
		{
			get
			{
				return originalJobs.Where(x => !x.IsDeleted);
			}
			set
			{
				originalJobs = value;
			}
		}
		IEnumerable<Job> originalJobs = new List<Job>();

		protected IEnumerable<Job> Jobs
		{
			get
			{
				return jobs.Where(x => !x.IsDeleted);
			}
			set
			{
				jobs = value;
			}
		}
		IEnumerable<Job> jobs = new List<Job>();

		protected bool IsPostedForBatchInvoicing { get; set; }
		public Form ParentForm { get; set; }

		public IPostManagerCreatedTransactionActions AfterCreationBeforePostTransactionActions { get; }

		internal protected class BulkPostingDataCollector
		{
			public BulkPostingDataCollector()
			{
				AllPostedInvoicePKs = new List<ZGuid>();
				AllAPPaymentPKs = new List<ZGuid>();
				AllAPInvoiceApprovalRequestPKs = new List<ZGuid>();
				AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs = new List<ZGuid>();
				allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed = new Dictionary<ZGuid, bool>();
			}

			public BulkPostingDataCollector(BulkPostingDataCollector copyFrom)
			{
				AllPostedInvoicePKs = new List<ZGuid>(copyFrom.AllPostedInvoicePKs);
				AllAPPaymentPKs = new List<ZGuid>(copyFrom.AllAPPaymentPKs);
				AllAPInvoiceApprovalRequestPKs = new List<ZGuid>(copyFrom.AllAPInvoiceApprovalRequestPKs);
				AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs = new List<ZGuid>(copyFrom.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs);
				allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed = new Dictionary<ZGuid, bool>(copyFrom.allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed);
			}

			public List<ZGuid> AllPostedInvoicePKs { get; }
			public List<ZGuid> AllAPPaymentPKs { get; }
			public List<ZGuid> AllAPInvoiceApprovalRequestPKs { get; }
			public List<ZGuid> AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs { get; }
			public IEnumerable<ZGuid> AllAPInvoicesAndCreditNotesPKs => allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed.Keys;
			public IReadOnlyDictionary<ZGuid, bool> AllAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed => allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed;

			public void AddToAllAPInvoicesAndCreditNotesPKs(IEnumerable<ZGuid> pks, bool isOverrideRequisitionDetailsAllowed)
			{
				foreach (var pk in pks)
				{
					allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed.Add(pk, isOverrideRequisitionDetailsAllowed);
				}
			}
			Dictionary<ZGuid, bool> allAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed { get; }
		}

		protected BulkPostingDataCollector bulkPostingDataCollector { get; private set; }

		#region Job Management

		protected virtual BasePostManager PostManager
		{
			get { return fPostManager ?? (fPostManager = GetNewPostManager()); }
		}

		BasePostManager fPostManager;

		protected abstract BasePostManager GetNewPostManager();

		/// <summary>
		/// It is absolutely required to re-load jobs for posting,
		/// because using jobs from Consol or Shipment factory and updating them down the line
		/// causes major problems when the posting process fails.
		/// </summary>
		/// <param name="jobs">Jobs from the existing factory</param>
		/// <returns>Collection of jobs in the new factory</returns>
		protected IEnumerable<Job> LoadJobsInNewFactory(IEnumerable<Job> jobs)
		{
			var originalJobsList = OriginalJobs.ToList();
			var jobsNotInOriginalJobsList = jobs.Where(x => !originalJobsList.Select(y => y.PK).Contains(x.PK));
			OriginalJobs = originalJobsList.Concat(jobsNotInOriginalJobsList);

			var jobsQuery = new ZQuery(JobHeaderSchema.PK, jobs.Select(x => x.PK));
			var result = TransactionFactory.Load<Job>(new JobCollection(TransactionFactory, jobsQuery).CompleteFilter);
			foreach (Job job in result)
			{
				job.InitializeParentFromGenericJobWithSettingDefaults();
			}

			return result;
		}

		protected IEnumerable<Job> LoadJobsInNewFactory(Job oldJob)
		{
			return oldJob != null ? LoadJobsInNewFactory(new[] { oldJob }) : Array.Empty<Job>();
		}

		#endregion

		#region Posting

		enum PostManagerAction
		{
			Post,
			Preview,
			PrepareTransactionsForPreview,
			BulkPost,
		}

		bool IsPostAction(PostManagerAction actionToCheck) => (new PostManagerAction[] { PostManagerAction.Post, PostManagerAction.BulkPost }).Contains(actionToCheck);
		bool IsSilentAction(PostManagerAction actionToCheck) => (new PostManagerAction[] { PostManagerAction.BulkPost, PostManagerAction.PrepareTransactionsForPreview }).Contains(actionToCheck);

		public void Preview()
		{
			action = PostManagerAction.Preview;
			Perform();
		}

		public virtual void Post()
		{
			action = PostManagerAction.Post;
			Perform();
		}

		PostManagerAction action;

		TransactionCreatorHashtable Perform()
		{
			SetUpEvents();

			AskShouldUseImmediateRevenueRecognisedDate();

			bool continueProcessing = PerformPrePostValidation();
			continueProcessing &= Jobs.Any();
			if (continueProcessing)
			{
				PostManager.OnCriticalPostError += TestPostManager_CriticalCheckError;
				try
				{
					TransactionCreatorHashtable transactions = null;
					transactions = PostManager.CreateTransactions(PostingOption);

					if (!PostManager.CancelPosting)
					{
						HookInvoiceOnComplianceSequenceFailedToAssign(transactions);
						HookInvoiceOnNegativeComplianceFailedToCreate(transactions);
						var notifier = new PostManagerUserNotifier(this);

						// NOTE: please implement actions after creation and before posting in AfterCreationBeforePostTransactionValidator, to simplify testing.
						continueProcessing = continueProcessing && CheckNonPostingIfNoPaymentApprovalSecurity(transactions);
						continueProcessing = continueProcessing && !AfterCreationBeforePostTransactionActions.AreAnyInvoicesWhereOrgIsNotARorAP(transactions, notifier, PostManager);
						continueProcessing = continueProcessing && ValidateTransactionForPosting(transactions);

						continueProcessing = continueProcessing && (PostManager.CancelPosting || CheckInvalidPostingGroups(transactions));

						if (continueProcessing)
						{
							NotifyInvoiceTermOverriddenMessages(PostManager);
						}

						continueProcessing = continueProcessing && PerformTransactionBackDating(transactions);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformInvPostDateCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformInvTaxDateCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformInvComplianceCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformARCreditNoteLevelAuthorization(PostManager, ARCreditNoteApprovalGUIProvider);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformSurchargeLineDeptCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformInvoiceExchangeRateCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformCashAdvanceRelatedCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformExporterExemptionCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformInvoicingTermsCheck(transactions, notifier);
						continueProcessing = continueProcessing && AfterCreationBeforePostTransactionActions.PerformDuplicateTransactionNumberCheckForPayableTransactions(transactions, notifier);
						// NOTE: please implement actions after creation and before posting in AfterCreationBeforePostTransactionValidator, to simplify testing.

						if (continueProcessing)
						{
							PostManager.OnUserWarningNotification += PostManager_NotifyUserWarning;
							PerformIfInvoiceDateIsInTheFutureCheck(transactions);
							if (PostManager.CancelPosting)
							{
								SaveOnlyARCreditNoteApprovalRequests();
							}
							else
							{
								PostManager.PerformTransactionDescriptionDefaulting(JobInvoicingPlugIn, transactions);

								if (action == PostManagerAction.Preview)
								{
									PreviewTransactions(transactions);
								}
								else if (action == PostManagerAction.PrepareTransactionsForPreview)
								{
									return transactions;
								}
								else if (IsPostAction(action))
								{
									continueProcessing &= PromptForCardSecurityCodes(transactions);

									if (continueProcessing)
									{
										var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes().ToArray();
										ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(apTransactions);

										var arTransactions = transactions.GetAllARInvoicesAndCreditNotes().ToArray();
										ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(arTransactions);

										if (IsPostAction(action) && RefreshJobChargesAction != null &&
												PostManager.TotalNumberOfCharges > AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.Value)
										{
											TransactionFactory.RefreshEnabled = false;
										}

										PostTransactions(transactions);

										if (!IsPostedForBatchInvoicing)
										{
											PromptForRequisitionDateAndStatus();
										}
										NotifyPaymentCreationErrorMessages(transactions);
									}
								}
							}
						}
					}

					if (PostManager.CancelPosting || !continueProcessing)
					{
						ImportUnsavedCharges();
						ReloadJobCharges();
					}
					else
					{
						RefreshChargesWhenPosted();
						if (RefreshJobChargesAction != null && !TransactionFactory.RefreshEnabled)
						{
							ReloadChargesInPlugInFactory();
							RefreshJobChargesAction();
						}
					}
				}
				catch (DuplicatedInvoiceException e)
				{
					NotifyPostValidationError(Res.GetString("4843F29B-A5FE-4610-B7FD-C7B9EC849D71",
						"Duplicated transaction (Creditor: {0}, Invoice #: {1}) are going to be created. It can be caused by having the same invoice details on charges in different jobs.",
						e.Creditor, e.InvoiceNumber));
				}
				finally
				{
					PostManager.OnCriticalPostError -= TestPostManager_CriticalCheckError;
					PostManager.OnUserWarningNotification -= PostManager_NotifyUserWarning;
				}
			}

			return null;
		}

		void RefreshChargesWhenPosted()
		{
			if (TransactionFactory.HasContext(NewChargeLoadActionOnPosting.RefreshChargesWhenPosted))
			{
				OriginalJobs.ForEach(x => x.RefreshCharges());
			}
		}

		void SaveOnlyARCreditNoteApprovalRequests()
		{
			if (PostingOption == JobInvoicingPostingOption.All)
			{
				NotifyPostingWarning(Res.GetString("407e75e0-04bf-47a7-a041-ea3730cbef2e", "Posting is canceled. Only AR Credit Note requests will be created. To Post other transaction types post them separately from unauthorized AR Credit Notes."));
			}
			BusinessObjectFactory.SaveTogether(FactoryForARCreditNoteApprovalRequests);
		}

		void HookInvoiceOnComplianceSequenceFailedToAssign(TransactionCreatorHashtable transactions)
		{
			foreach (TransactionHeader transaction in transactions.Values)
			{
				if (transaction is ARInvoice || transaction is ARCreditNote || transaction is ARAdjustmentNote)
				{
					InvoicingBase invoice = transaction as InvoicingBase;
					invoice.OnComplianceSequenceFailedToAssign += new EventHandler(ComplianceSequenceFailedToAssign);
					invoice.OnDigitalSignatureFailedToSign += new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
				}
			}
		}

		void HookInvoiceOnNegativeComplianceFailedToCreate(TransactionCreatorHashtable transactions)
		{
			foreach (TransactionHeader transaction in transactions.Values)
			{
				if (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote)
				{
					var invoice = transaction as InvoicingBase;
					invoice.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
				}
			}
		}

		void AskShouldUseImmediateRevenueRecognisedDate()
		{
			if (IsPostAction(action))
			{
				foreach (Job job in Jobs)
				{
					foreach (Charge charge in job.Charges)
					{
						if (!charge.IsCostPosted || !charge.IsRevenuePosted)
						{
							job.ShouldUseImmediateRevenueRecognisedDate += new EventHandler<UserQueryEventArgs>(job_ShouldUseImmediateRevenueRecognisedDate);
							try
							{
								job.AskShouldUseImmediateRevenueRecognisedDate(charge.ChargeCode);
							}
							finally
							{
								job.ShouldUseImmediateRevenueRecognisedDate -= new EventHandler<UserQueryEventArgs>(job_ShouldUseImmediateRevenueRecognisedDate);
							}
						}
					}
				}
			}
		}

		void NegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(AccountingConstants.GetComplianceDocumentNegativeMessage());
		}

		void ComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			var msg = InvoicingBase.GetMessageForComplianceSequenceErrors(e);
			Globals.Message.ShowWarning(msg);
			new ComplianceNumberAllocationFailureEmail(((InvoicingBase)sender)).Send();
		}

		void DigitalSignatureFailedToSign(object sender, UserMessageEventArgs e)
		{
			Globals.Message.ShowWarning(e.Message);
			new DigitalSignatureSigningFailureEmail(((InvoicingBase)sender), e.Message).Send();
		}

		void NotifyPaymentCreationErrorMessages(TransactionCreatorHashtable transactions)
		{
			var messages = transactions.GetAllAPPaymentApprovals()
				.Where(p => p.PaymentCreationErrorMessages.HasErrors())
				.Select(p => string.Join(System.Environment.NewLine, Res.GetString("7219AACB-D874-43A1-983D-D161BA34D714", "{0} could not be posted due to the following errors:", p.GetDescription()),
					string.Join(System.Environment.NewLine, p.PaymentCreationErrorMessages.GetUniqueMessageList())));
			if (messages.Any())
			{
				var message = string.Join(System.Environment.NewLine + System.Environment.NewLine, messages);
				if (IsSilentAction(action))
				{
					RequestPreviewOrBulkPostErrorMessages = message;
				}
				else
				{
					Globals.Message.ShowInformation(message, Res.GetString("0B1FE224-6BBA-4CC0-940E-79EE618027CF", "Payments not created"));
				}
			}
		}

		void NotifyInvoiceTermOverriddenMessages(BasePostManager postManager)
		{
			if (IsPostAction(action))
			{
				var messages = postManager.Poster.PostedInvoices.ToArray<InvoicingBase>().Where(x => !string.IsNullOrEmpty(x.MessageForInvoiceTermOverriding)).Select(x => x.MessageForInvoiceTermOverriding).ToList();
				if (messages.Count > 0)
				{
					var message = Res.GetString("e4d50ef2-cb5f-42b4-9b23-e1f4cc52d85a",
@"Invoice Term overridden for following Debtor(s) --

{0}", string.Join(System.Environment.NewLine, messages));
					if (IsSilentAction(action))
					{
						RequestPreviewOrBulkPostErrorMessages = message;
					}
					else
					{
						Globals.Message.ShowWarning(message, Res.GetString("e506e508-e70c-4236-b322-bb41d4edc88c", "Invoice Term"));
					}
				}
			}
		}

		bool CheckNonPostingIfNoPaymentApprovalSecurity(TransactionCreatorHashtable transactions)
		{
			if (IsPostAction(action))
			{
				var paymentApprovals = transactions.GetAllAPPaymentApprovals();
				var paymentTypeSpecificWarningMessages = new SortedSet<ZString>();

				foreach (var paymentApproval in paymentApprovals)
				{
					if (!paymentApproval.UserHasPaymentTypeSecurityToPost())
					{
						paymentTypeSpecificWarningMessages.Add(
							Res.GetString("354f442a-73b6-4eaf-9935-747f45e172ef", "{0} - for which you require access to security function: {1}",
							paymentApproval.Lookups.PaymentMethods[paymentApproval.AV_PaymentType, StringComparison.Ordinal].Description,
							paymentApproval.PaymentTypeSecurityNeededToPost.DisplayTextPathToSecurityRight
							));
					}
				}

				if (paymentTypeSpecificWarningMessages.Count > 0)
				{
					var paymentTypesLiteral = string.Join("\r\n", paymentTypeSpecificWarningMessages.ToArray());
					var warningMessage = Res.GetString("64393029-10db-469c-aa44-11ec4da9faae", @"You are posting one or more payment transactions as part of this action.
You do not have the security rights to post one or more of these payment transactions due to security on the 'Payment Type'.

The payment types are as follows:

{0}

If you click 'Yes', these payments will be created in the 'Payment Processing' module as 'Approved, but not Posted' payments.
If you click 'No', this will cancel your action and no transactions will be posted.", paymentTypesLiteral);

					return ConfirmContinueWithPostValidationWarning(
						new PostManagerNotification(CargoWise.ComponentModel.NotificationType.Warning, warningMessage, PostManagerValidationType.PaymentApprovalSecurityValidation));
				}
			}

			return true;
		}

		void ImportUnsavedCharges()
		{
			if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Japan &&
										IsPostAction(action) &&
										AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.Value == Constants.RoundingRules.Codes.JapanYenWithCharge)
			{
				foreach (Job job in Jobs)
				{
					foreach (Charge charge in job.Charges)
					{
						if (!charge.IsInDatabase && charge.HasContext(NewChargeLoadActionOnPosting.AddChargeWhenPostingCancelled))
						{
							Charge chargeInOldFactory = (Charge)PlugInFactory.ImportFromAnotherFactory(charge);
							chargeInOldFactory.InvoicingJob.Charges.Load();
							chargeInOldFactory.InvoicingJob.HasChanges = true;
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		bool PromptForCardSecurityCodes(TransactionCreatorHashtable transactions)
		{
			bool continuePosting = true;
			var cardSecurityCodes = new Dictionary<ZGuid, ZString>();
			foreach (var payment in transactions.GetAllAPPaymentApprovals())
			{
				if (payment.AV_PaymentType == ReceiptTypes.eNettCreditCard
					&& payment.BankAccount != null
					&& payment.BankAccount.IsCreditCardOrLinkedAccount)
				{
					if (IsPostedForBatchInvoicing)
					{
						continuePosting = false;
						NotifyPostValidationError(Res.GetString("007DB208-1CB9-4AE8-8B20-AFEBE26F08D2", "Created payment requires credit card security code. Please post this job individually."));
						break;
					}

					var cardSecurityCode = ZString.Empty;
					if (!cardSecurityCodes.TryGetValue(payment.BankAccount.PK, out cardSecurityCode))
					{
						var codeBO = new PaymentCreditCardSecurityCode();
						codeBO.Message = Res.GetString("20d4c08f-b4c5-4da9-8175-7b29ffa62560", "Enter the Credit Card Security Code for the {0} credit card with number {1}.", payment.BankAccount.AB_Code, payment.BankAccount.AB_AccountNum);
						var form = new CreditCardSecurityCodeForm(codeBO);
						ZFormModaliser.ShowDialogAndDispose(form);
						if (codeBO.Continue)
						{
							cardSecurityCode = codeBO.CardSecurityCode;
							cardSecurityCodes.Add(payment.BankAccount.PK, cardSecurityCode);
						}
						else
						{
							continuePosting = false;
							break;
						}
					}
					payment.CreditCardSecurityCode = cardSecurityCode;
				}
			}

			return continuePosting;
		}

		protected virtual void PromptForRequisitionDateAndStatus()
		{
			PromptForRequisitionDateAndStatus(bulkPostingDataCollector);
		}

		protected static void PromptForRequisitionDateAndStatus(BulkPostingDataCollector bulkPostingDataCollector)
		{
			if (AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.Value)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				APTransactionHeaderCollection collection = new APTransactionHeaderCollection(factory);

				var allAPInvoicesAndCreditNotes = factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllAPInvoicesAndCreditNotesPKs));
				foreach (InvoicingBase invoice in allAPInvoicesAndCreditNotes.Where(x => x is APInvoice && x.AH_FullyPaidDate.IsEmpty))
				{
					var isAllowed = bulkPostingDataCollector.AllAPInvoicesAndCreditNotesPKsWithIsOverrideRequisitionDetailsAllowed[invoice.PK];
					if (isAllowed)
					{
						collection.AddFromDatabase(invoice.PK);
					}
				}

				if (collection.Count > 0)
				{
					APTransactionHeaderCollectionHolder holder = new APTransactionHeaderCollectionHolder(factory, collection);
					ZFormModaliser.ShowDialogAndDispose(new APInvoiceRequisitionForm(holder));
				}
			}
		}

		protected virtual IJobInvoicingPlugIn JobInvoicingPlugIn
		{
			get { return JobParent as IJobInvoicingPlugIn; }
		}

		protected virtual bool PerformTransactionBackDating(TransactionCreatorHashtable transactions)
		{
			var continueProcessing = true;
			PostManager.ChangeTransactionDates += PostManager_ChangeTransactionDates;
			try
			{
				continueProcessing = PostManager.PerformTransactionBackDating(JobInvoicingPlugIn, GetNewChangeTransactionDatesBusinessObject, transactions, TransactionFactory);
			}
			finally
			{
				PostManager.ChangeTransactionDates -= PostManager_ChangeTransactionDates;
			}

			return continueProcessing;
		}

		void PerformIfInvoiceDateIsInTheFutureCheck(TransactionCreatorHashtable transactions)
		{
			PostManager.CheckIfInvoiceDateIsInTheFuture(transactions);
		}

		void PostManager_ChangeTransactionDates(object sender, BasePostManager.ChangeTransactionDatesEventArgs e)
		{
			NotificationHelper.SetChangeTransactionDateBusinessObject(e.ChangeTransactionDatesBusinessObject);

			QueryUserMsgBoxEventArgs args = QueryUserToPerformTransactionBackDating();
			e.Args = args;
			e.TransactionDate = NotificationHelper.TransactionDate;
			e.PostDate = NotificationHelper.PostDate;
		}

		protected virtual QueryUserMsgBoxEventArgs QueryUserToPerformTransactionBackDating()
		{
			QueryUserMsgBoxEventArgs args = GetNewQueryUserEventArgs();
			NotificationHelper.QueryUser(args);
			return args;
		}

		bool ValidateTransactionForPosting(TransactionCreatorHashtable transactions)
		{
			bool continueProcessing = true;

			if (ShouldValidatePostingChargeCodeGLChequeDetails)
			{
				foreach (string key in transactions.Keys)
				{
					TransactionHeader transaction = transactions[key];
					if (transaction is TransactionHeaderWithLines)
					{
						if (!IsChargeCodeAndGLAccountInTransactionLinesValid((TransactionHeaderWithLines)transaction))
						{
							continueProcessing = false;
							break;
						}
					}
				}

				if (continueProcessing && IsPostAction(action))
				{
					foreach (PaymentApprovalBase paymentApproval in transactions.GetAllAPPaymentApprovals())
					{
						if (MessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(paymentApproval.ChequeBook))
						{
							continueProcessing = false;
							break;
						}
					}
				}
			}

			return continueProcessing;
		}

		bool PerformPrePostValidation()
		{
			var validationResult = GetPostManagerValidation(Jobs, PostingOption, OriginalJobs).Validate();
			string errorMessage = string.Empty;
			string warningMessage = string.Empty;
			if ((validationResult == null || validationResult.Type != CargoWise.EntityFramework.NotificationType.Error)
				&& ShouldValidatePostingEditSecurityLock)
			{
				var plugin = JobParent as IJobInvoicingPlugIn;
				if (plugin != null && plugin.InvoicingSupporter.EditSecurityLock && !SecurityHelperClass.RequestEditAuthorisation(plugin))
				{
					errorMessage = Res.GetString("25f902d7-629c-4917-aef5-cc16913decd3", "Operation was canceled by user.");
				}
			}

			if (validationResult != null && validationResult.Type == CargoWise.EntityFramework.NotificationType.Error)
			{
				errorMessage = validationResult.Message;

				if (validationResult.ValidationType == PostManagerValidationType.DifferencesInReloadedChargesValidation)
				{
					ReloadOriginalCharges();
				}
			}

			if (validationResult != null && validationResult.Type == CargoWise.EntityFramework.NotificationType.Warning)
			{
				warningMessage = validationResult.Message;
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				NotifyPostValidationError(errorMessage);
				return false;
			}

			if (!string.IsNullOrEmpty(warningMessage))
			{
				return ConfirmContinueWithPostValidationWarning(validationResult);
			}

			return true;
		}

		void ReloadOriginalCharges()
		{
			PlugInFactory.ClearQueryCache(JobChargeSchema.Constants.TableName);

			foreach (Job job in OriginalJobs)
			{
				job.Charges.Load();
				job.Charges.Reload();
				job.UtcTimeJobWasLoaded = ZDateTime.UtcNow;
			}
		}

		protected virtual bool IsBulkPosting { get { return false; } }
		protected virtual bool ShouldValidatePostingEditSecurityLock { get { return true; } }
		protected virtual bool ShouldValidatePostingChargeCodeGLChequeDetails { get { return true; } }

		void NotifyPostingWarning(string message)
		{
			if (IsPostAction(action))
			{
				NotifyPostingWarningCore(message);
			}
		}

		protected virtual void NotifyPostingWarningCore(string message)
		{
			if (IsSilentAction(action))
			{
				RequestPreviewOrBulkPostErrorMessages = message;
			}
			else
			{
				Globals.Message.ShowWarning(message);
			}
		}

		protected virtual void NotifyPostValidationError(string error)
		{
			if (IsSilentAction(action))
			{
				RequestPreviewOrBulkPostErrorMessages = error;
			}
			else
			{
				Globals.Message.ShowError(error);
			}
		}

		class PostManagerUserNotifier : IPostManagerUserNotifier
		{
			public PostManagerUserNotifier(PostManagerGUIWrapper parent)
			{
				this.parent = Argument.NotNull(parent, nameof(parent));
			}
			readonly PostManagerGUIWrapper parent;

			public void NotifyPostingWarning(string error) => parent.NotifyPostingWarning(error);

			public void NotifyPostValidationError(string warning) => parent.NotifyPostValidationError(warning);
		}

		protected virtual bool ConfirmContinueWithPostValidationWarning(PostManagerNotification notification)
		{
			var result = true;
			if (IsPostAction(action) && IsValidationTypeSupportWarningCheck(notification.ValidationType))
			{
				var warning = notification.Message;
				if (IsSilentAction(action))
				{
					RequestPreviewOrBulkPostErrorMessages = Res.GetString("3F5827AB-B53F-4520-9F85-28652FA710B5", "Warning: {0}", warning);
					result = false;
				}
				else
				{
					string caption = string.Empty;
					switch (notification.ValidationType)
					{
						case PostManagerValidationType.CreditLimitValidation:
							caption = Res.GetString("fdba9f84-e831-4b9a-983b-3291d793bbe1", "Credit Limit Warning");
							break;
						case PostManagerValidationType.CustomsDisbursementChargesDuplicateCheckValidation:
							caption = Res.GetString("6bf9d366-1ba2-47c9-af05-2e94f9dc22fa", "Duplicate Charges Warning");
							break;
						case PostManagerValidationType.PaymentApprovalSecurityValidation:
							caption = Res.GetString("5b2f7a7f-68cd-4073-8424-84d7a2c8779f", "Payment Approvals");
							break;
						case PostManagerValidationType.MissingTaxRegistrationNumberValidation:
							caption = Res.GetString("36C6D4A9-95D4-4A4B-AA24-7AB4B21CBA25", "Missing tax registration information");
							break;
						case PostManagerValidationType.TaxDateValidationForAR:
							caption = Res.GetString("91202ECA-D27B-4B1D-B395-FB13E9F7CA1C", "Tax date warning for AR Posting");
							break;
						case PostManagerValidationType.TaxDateValidationForAP:
							caption = Res.GetString("B74350CC-DECC-4187-9935-029825E1536F", "Tax date warning for AP Posting");
							break;
						default:
							break;
					}
					result = Globals.Message.Show(warning + System.Environment.NewLine + System.Environment.NewLine + Res.GetString("43806ed5-50db-4d6c-9ac5-c5b0b019ee9c", "Continue with posting?"),
					caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
				}
			}
			return result;
		}

		bool IsValidationTypeSupportWarningCheck(PostManagerValidationType validationType) =>
					validationType == PostManagerValidationType.CreditLimitValidation && AccountingConfigurationRegistry.Instance.ShowCreditLimitWarningOnJobPosting.Value
					|| validationType == PostManagerValidationType.CustomsDisbursementChargesDuplicateCheckValidation
					|| validationType == PostManagerValidationType.PaymentApprovalSecurityValidation
					|| validationType == PostManagerValidationType.MissingTaxRegistrationNumberValidation
					|| validationType == PostManagerValidationType.TaxDateValidationForAR
					|| validationType == PostManagerValidationType.TaxDateValidationForAP;

		protected virtual bool CheckInvalidPostingGroups(TransactionCreatorHashtable transactions)
		{
			var errorBuilder = new ZStringBuilder();

			foreach (var transaction in transactions.GetAllAPInvoicesAndCreditNotes())
			{
				if (transaction.HasInvalidPostingGroups)
				{
					string message = Res.GetString("7f42ed21-ffec-47d8-8994-9eb5e23b9014",
						"{0} {1} Number {2}.",
						transaction.AH_Ledger,
						transaction.AH_TransactionType == TransactionTypes.Invoice ? Res.GetString("d451f13e-b177-43bc-b7b6-1d1b6c1122f3", "Invoice") : Res.GetString("afb9ff2d-23c3-48a8-ae98-4f16c4d9d43e", "Credit Note"),
						transaction.AH_TransactionNum);

					errorBuilder.AppendLine(message);
				}
			}

			if (!errorBuilder.IsEmpty)
			{
				errorBuilder.Append(Res.GetString("37e8a483-3780-43ae-9903-0142491d8d94", "You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?"));
				var message = errorBuilder.ToString();
				if (IsSilentAction(action))
				{
					RequestPreviewOrBulkPostErrorMessages = message;
					return false;
				}

				return Globals.Message.Show(message, Res.GetString("43806ed5-50db-4d6c-9ac5-c5b0b019ee9c", "Continue with posting?"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
			}

			return true;
		}

#if DEBUG
		internal event EventHandler DoStuffBeforeSaveForTest;
		public bool DoTestPostTransactions { get; set; }

		void SetupTestAllocator(TransactionCreatorHashtable transactions)
		{
			Test_Allocator = new PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator(transactions, TransactionFactory);
			Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
			Test_AllocatorsList.Add(Test_Allocator);
			if (DoStuffBeforeSaveForTest != null)
			{
				DoStuffBeforeSaveForTest(this, new EventArgs());
			}
		}
#endif

		protected ITransactionParticipant[] GetAllTransactionParticipants(TransactionCreatorHashtable transactions)
		{
			ITransactionParticipant[] participants;
#if DEBUG
			if (Globals.IsTest && Test_Allocator != null && !Globals.GetIsUnitTestingProductionFunctionality())
			{
				participants = Test_Allocator.GetFactoriesForTest();
			}
			else
			{
#endif
				var allocator = new PaymentBatchChequeNumberAllocator(transactions, TransactionFactory);
				participants = allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving();
#if DEBUG
			}
#endif
			List<ITransactionParticipant> allParticipants = new List<ITransactionParticipant>(participants);
			foreach (var payment in transactions.GetAllAPPaymentApprovals())
			{
				if (payment.AV_PaymentType == ReceiptTypes.eNettCreditCard
					&& payment.BankAccount != null
					&& payment.BankAccount.IsCreditCardOrLinkedAccount)
				{
					allParticipants.Add(new eNettPaymentTransactionParticipant(payment));
				}
			}
			allParticipants.Add(FactoryForARCreditNoteApprovalRequests);
			return allParticipants.ToArray();
		}

		void PostTransactions(TransactionCreatorHashtable transactions)
		{
			if (PostTransactionsCore(transactions))
			{
				bulkPostingDataCollector.AllPostedInvoicePKs.AddRange(PostManager.Poster.PostedInvoices.ToArray<InvoicingBase>().OrderBy(x => x.InvoiceNumber).Select(x => x.PK));
				bulkPostingDataCollector.AllAPPaymentPKs.AddRange(transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving().Select(x => x.PK));
				bulkPostingDataCollector.AllAPInvoiceApprovalRequestPKs.AddRange(transactions.GetAllAPInvoiceApprovalRequests().Select(x => x.PK));
				bulkPostingDataCollector.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs.AddRange(transactions.GetAllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes().Select(x => x.PK));
				bulkPostingDataCollector.AddToAllAPInvoicesAndCreditNotesPKs(
					transactions.GetAllAPInvoicesAndCreditNotes().Select(x => x.PK),
					GetSecurityHelper(JobInvoicingPlugIn).CheckIsAllowedForSecurityCheckPoint(SecurityCore.OverrideRequisitionDetails));
			}
		}

		protected virtual bool PostTransactionsCore(TransactionCreatorHashtable transactions)
		{
			bool reloadCharges = false;
			try
			{
#if DEBUG
				if (Globals.IsTest && DoTestPostTransactions)
				{
					SetupTestAllocator(transactions);
					SavePostingFactoriesForTest(transactions);
				}
				else
				{
#endif
					SavePostingFactories(transactions);
#if DEBUG
				}
#endif
				if (!IsPostedForBatchInvoicing)
				{
					OverrideTransactionDescription(PostManager.Poster.PostedInvoices.GetPKs().ToArray());
					CheckCreditLimitExceeded(transactions);
					PrintInvoices(transactions);
				}

				return true;
			}
			catch (ComplianceSequenceRelatedException e)
			{
				reloadCharges = true;
				NotifyPostValidationError(e.UserFriendlyMessage);
			}
			catch (AllocationSaveException e)
			{
				reloadCharges = true;
				NotifyPostValidationError(e.UserFriendlyMessage);
			}
			catch (AllocationChequeBookException e)
			{
				reloadCharges = true;
				NotifyPostValidationError(e.UserFriendlyMessage);
			}
			catch (OnSavingCriticalCheckException e)
			{
				reloadCharges = true;
				var errorMessage = e.Message;
				if (IsSilentAction(action))
				{
					RequestPreviewOrBulkPostErrorMessages = errorMessage;
				}
				else
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
			catch (ENettProcessCreditCardException e)
			{
				reloadCharges = true;
				Globals.Message.ShowError(e.UserFriendlyMessage + "\r\n" + Res.GetString("0a891898-619c-4d44-a78c-008353dd94c5", "ComPay Error: ({0}) {1}", e.eNettErrorCode, e.eNettErrorMessage), Res.GetString("673dc185-2e0d-4a83-8135-05163de32bb7", "ComPay Error"));
				transactions.RemoveAndDeleteAll();
			}
			catch (ZSaveConcurrencyException)
			{
				reloadCharges = true;
				var errorMessage = Res.GetString("74b1526f-df95-4a2c-ba7c-d26974ebcd81",
					"Please try to post these charges again. Posting has failed. One or more of the charges you are attempting to post has been modified by another user. The other users changes have been merged with yours.");
				if (IsSilentAction(action))
				{
					RequestPreviewOrBulkPostErrorMessages = errorMessage;
				}
				else
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			finally
			{
				if (reloadCharges)
				{
					ReloadJobCharges();
				}
			}

			return false;
		}

		void SavePostingFactories(TransactionCreatorHashtable transactions)
		{
			TransactionFactory.SetContext(BusinessContext.BulkTransactionSaving);
			try
			{
				BusinessObjectFactory.SaveTogether(GetAllTransactionParticipants(transactions));
			}
			finally
			{
				TransactionFactory.RemoveContext(BusinessContext.BulkTransactionSaving);
			}
		}

		void PreviewTransactions(TransactionCreatorHashtable transactions)
		{
			var previewTransactions = GetPreviewTransactions(transactions, (InvoicingBase[])PostManager.Poster.PostedInvoices.ToArray(typeof(InvoicingBase)));
			if (previewTransactions.Count > 0)
			{
				PreviewTransactionsCore(previewTransactions);
			}
		}

		protected virtual void PreviewTransactionsCore(ArrayList transactions)
		{
			InvoicesPreviewer invoicesPreviewer = null;
			InvoicePreviewForm previewForm = null;
			try
			{
				invoicesPreviewer = new InvoicesPreviewer(transactions, PostingOption, GetSecurityHelper(JobInvoicingPlugIn));
				previewForm = new InvoicePreviewForm(invoicesPreviewer, AllowDeliveryDuringPreviewInvoice, this is ConsolInvoicingPostManagerGUIWrapper);
				invoicesPreviewer = null;
				previewForm.DeliveryOptions = allowedReportDeliveryOption;
				ZFormModaliser.Show(previewForm, ParentForm);
				previewForm = null;
			}
			finally
			{
				if (previewForm != null)
				{
					previewForm.Dispose();
				}
				else if (invoicesPreviewer != null)
				{
					invoicesPreviewer.Dispose();
				}
			}
		}

		ArrayList GetPreviewTransactions(TransactionCreatorHashtable transactions, InvoicingBase[] aRInvoices)
		{
			var previewTransactions = new ArrayList();

			if (PostingOption == JobInvoicingPostingOption.All)
			{
				previewTransactions.AddRange(aRInvoices);

				foreach (string key in transactions.Keys)
				{
					TransactionHeader transaction = transactions[key];
					if (transaction is APInvoice && transaction.AH_InvoiceAmount == 0)
					{
						previewTransactions.Add(transaction);
					}
				}
			}
			else
			{
				foreach (string key in transactions.Keys)
				{
					previewTransactions.Add(transactions[key]);
				}
			}

			return previewTransactions;
		}

		protected void CheckCreditLimitExceeded(TransactionCreatorHashtable transactions)
		{
			InvoicingBase.CheckCreditLimitExceeded(transactions.Values.Cast<InvoicingBase>());
		}

		protected virtual bool AllowDeliveryDuringPreviewInvoice
		{
			get { return PostingOption != JobInvoicingPostingOption.Costs; }
		}

		bool IsChargeCodeAndGLAccountInTransactionLinesValid(TransactionHeaderWithLines transaction)
		{
			bool result = true;
			foreach (TransactionLine line in transaction.Lines)
			{
				string errorMessage = line.ErrorMessageIfInvalidAL_AC_AL_AG();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					NotifyPostValidationError(errorMessage + TransactionLine.GetTransactionDetailsInErrorMessage(line.TransactionHeader.AH_Ledger, line.TransactionHeader.AH_TransactionType) + (line.TransactionHeader.AH_TransactionNum.IsEmpty ? "" : Res.GetString("b4bdc5bc-598d-488c-a6f7-d9c293c063aa", ", Transaction Num.: {0}", line.TransactionHeader.AH_TransactionNum + ".")));
					result = false;
					break;
				}
			}
			return result;
		}

		AccountingMessageHelper MessageHelper
		{
			get { return messageHelper ?? (messageHelper = new AccountingMessageHelper()); }
		}
		AccountingMessageHelper messageHelper;

		protected void TestPostManager_CriticalCheckError(object sender, CriticalPostingErrorEventArgs e)
		{
			NotifyPostValidationError(e.ErrorMessage);
		}

		void PostManager_NotifyUserWarning(object sender, UserMessageEventArgs e)
		{
			NotifyPostingWarning(e.Message);
		}

		protected virtual PostManagerValidation GetPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new PostManagerValidation(jobs, postingOption, originalJobs);
		}

		protected void ReloadJobCharges()
		{
			foreach (Job job in Jobs)
			{
				job.Charges.Reload();
			}
			//enett transaction participants may fail even if database transactions are successful
			//without the below code, the business objects in memory would be in an inconsistent state
			foreach (Job job in OriginalJobs)
			{
				job.Charges.Reload();
			}
		}

		protected void ReloadChargesInPlugInFactory()
		{
			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(PlugInFactory);
			foreach (Job job in Jobs)
			{
				foreach (Charge charge in job.Charges)
				{
					if (charge.JR_AL_APLine.IsValid)
					{
						PlugInFactory.AddFetchHint(AccTransactionLinesSchema.PK, charge.JR_AL_APLine);
					}
					if (charge.JR_AL_ARLine.IsValid)
					{
						PlugInFactory.AddFetchHint(AccTransactionLinesSchema.PK, charge.JR_AL_ARLine);
					}
				}
			}
		}

		protected ZDateTime GetLastDayOfLastMonth()
		{
			ZDateTime firstDayOfThisMonth = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1, 23, 59, 0);
			ZDateTime lastDayOfLastMonth = firstDayOfThisMonth.AddDays(-1);
			return lastDayOfLastMonth;
		}

		protected virtual QueryUserMsgBoxEventArgs GetNewQueryUserEventArgs()
		{
			return new QueryUserYesNoCancelEventArgs("", false);
		}

		protected virtual ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory)
		{
			return new ChangeTransactionDatesBusinessObject(pluginSecurity, factory);
		}

		protected virtual ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes)
		{
			return new ChangeTransactionDatesBusinessObject(pluginSecurity, factory, codes);
		}

		protected virtual void PrintInvoices(TransactionCreatorHashtable transactions)
		{
			var allTransactions = GetTransactionsForPrinting(transactions);

			var printer = new JobInvoicingPrinter(JobParent);
			printer.Print(ParentForm, InvoicePrintContext.PostFromBilling, allTransactions.ToArray());
		}

		TransactionHeader[] GetTransactionsForPrinting(TransactionCreatorHashtable transactions)
		{
			var arInvoices = PostManager.Poster.PostedInvoices.ToArray<InvoicingBase>().OrderBy(x => x.InvoiceNumber).AsEnumerable();
			var apInvoicesAndCreditNotes = transactions.GetAllAPInvoicesAndCreditNotes();
			var apPayments = transactions.GetAllAPPaymentsCreatedFormApprovalsOnSaving();
			var apApprovalRequests = AskUserAndGetRequestRelatedInvoicesToPreview(transactions.GetAllAPInvoiceApprovalRequests());

#if DEBUG
			(arInvoices, apInvoicesAndCreditNotes, apPayments, apApprovalRequests)
				= AlterTransactionsForPrinting_ForTestOnly(arInvoices, apInvoicesAndCreditNotes, apPayments, apApprovalRequests);
#endif

			ReportErrorWhenPrintingNullTransaction(arInvoices, nameof(arInvoices));
			ReportErrorWhenPrintingNullTransaction(apInvoicesAndCreditNotes, nameof(apInvoicesAndCreditNotes));
			ReportErrorWhenPrintingNullTransaction(apPayments, nameof(apPayments));
			ReportErrorWhenPrintingNullTransaction(apApprovalRequests, nameof(apApprovalRequests));

			var allTransactions = new List<TransactionHeader>();
			allTransactions.AddRange(apApprovalRequests);
			allTransactions.AddRange(arInvoices);
			allTransactions.AddRange(apInvoicesAndCreditNotes);
			allTransactions.AddRange(apPayments);

			return allTransactions.Where(t => t != null).ToArray();
		}

		static APInvoice[] AskUserAndGetRequestRelatedInvoicesToPreview(APInvoiceChargesApprovalRequest[] requests)
		{
			var invoicesToPreview = new List<APInvoice>();

			if (requests.Length > 0 && AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.Value)
			{
				var result = Globals.Message.Show(Res.GetString("3C6EC96F-D732-480C-BDC1-77A5E9A62ADB", "Do you want to preview a Cost Confirmation Document for transactions related to new approval requests?"), Res.GetString("0281b346-3ec6-42fd-8f80-8e78b79ffbdb", "Print Cost Confirmation Document"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				if (result == DialogResult.Yes)
				{
					foreach (var request in requests)
					{
						var getInvoiceToPreviewResult = GetInvoiceToPreviewRequestCostConfirmationDocument(request);

						if (!string.IsNullOrEmpty(getInvoiceToPreviewResult.ErrorMessage))
						{
							invoicesToPreview.Clear();
							Globals.Message.ShowWarning(Res.GetString("3A725A31-76CF-490E-968C-E9B4FD35EFE4",
								@"A Cost Confirmation Document for transactions related to new approval requests will not be generated because of following errors: 
Request ({0}) - {1}",
								request.FormatedRequestId, getInvoiceToPreviewResult.ErrorMessage));
							break;
						}
						invoicesToPreview.Add(getInvoiceToPreviewResult.InvoiceToPreview);
					}
				}
			}

			return invoicesToPreview.ToArray();
		}

		void ReportErrorWhenPrintingNullTransaction(IEnumerable<TransactionHeader> transactions, string collectionName)
		{
			if (transactions.Any(x => x == null))
			{
				var nonNullTransactions = transactions.Where(t => t != null);

				#region SuppressResourceStringsCheckRegion
				// Reason = Developer only exception; not visible to users.

				var otherTransactionsMessagePart
					= nonNullTransactions.Any()
					? string.Join(", ", nonNullTransactions.Select(t => FormattableString.Invariant($"{t.AH_Ledger} {t.AH_TransactionType} {t.AH_TransactionNum} ({t.Header?.OH_Code})")))
					: "<none>";
				var jobMessagePart = JobParent != null
					? FormattableString.Invariant($"{JobParent.GetType().Name} ({JobParent.JobNumber})")
					: "<null>";
				var message = FormattableString.Invariant($"Null transactions to be printed was found in PostManagerGUIWrapper '{collectionName}' source collection. You cannot print a null transaction. The null transaction has been filtered from list of transactions to be printed. PostManagerGUIWrapper type: {GetType().Name}. Other transactions: {otherTransactionsMessagePart}. Job type: {jobMessagePart}.");

				#endregion

				ExceptionReporter.Instance.ReportDeveloperException("Acc_PostManagerGUIWrapper_NullTransactionForPrinting_" + collectionName, message, new Exception(message));
			}
		}

		protected virtual void SetUpEvents()
		{
			PostManager.OnJobOnHold += new BasePostManager.OnJobOnHoldEventHandler(JobsOnHold);
			PostManager.OnNothingPosted += new EventHandler(NothingPostedHandler);
		}

		void JobsOnHold(object sender, BasePostManager.OnJobOnHoldEventArgs e)
		{
			if (IsPostAction(action))
			{
				JobsOnHoldCore(GetJobOnHoldMessage(e.Jobs));
			}
		}

		protected virtual void JobsOnHoldCore(string message)
		{
			if (IsSilentAction(action))
			{
				RequestPreviewOrBulkPostErrorMessages = message;
			}
			else
			{
				Globals.Message.ShowWarning(message, Res.GetString("5e33560e-638e-40fe-a1f8-e68c6b3a047a", "Work On Hold"));
			}
		}

		protected abstract string GetJobOnHoldMessage(IEnumerable<Job> jobs);

		protected virtual string GetNothingPostedMessage()
		{
			var zeroBalanceWarningMsg = AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value
				? Res.GetString("A8DC9A50-7A9A-492F-93CB-441F43F57000", "* Amounts are zero")
				: Res.GetString("FC849222-5580-43E8-BE2E-8645971127C4", "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices");

			return Res.GetString("8BF3EDF1-8947-41F0-ABE4-8FF669169FFC",
@"No appropriate charges were found for posting, due to one of the following reasons:
{0}
* Posting Costs: Creditor, AP Invoice Number or Date are not entered or invalid
* Posting Revenue: Job status is set to 'Invoice on hold' / 'Work on hold' or Debtor is not entered or invalid
* Job has Ready For Financial Closure status", zeroBalanceWarningMsg);
		}

		void NothingPostedHandler(object sender, EventArgs e)
		{
			if (!PostManager.CancelPosting)
			{
				NothingPostedHandlerCore(GetNothingPostedMessage());
			}
		}

		protected virtual void NothingPostedHandlerCore(string message)
		{
			if (IsSilentAction(action))
			{
				RequestPreviewOrBulkPostErrorMessages = message;
			}
			else
			{
				Globals.Message.ShowInformation(message, Res.GetString("2c5e364a-c36d-4490-bf46-717b6c7e31a3", "Nothing was posted"));
			}
		}

		protected virtual void job_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = Globals.Message.Show(e.QueryMessage, Res.GetString("53dc2f11-b98a-4f5b-9960-07358042ec78", "Job") + " " + ((Job)sender).JH_JobNum, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes;
		}

		protected ChangeTransactionDatesNotificationSubscriberGuiHelper NotificationHelper
		{
			get { return notificationHelper ?? (notificationHelper = new ChangeTransactionDatesNotificationSubscriberGuiHelper()); }
		}
		protected ChangeTransactionDatesNotificationSubscriberGuiHelper notificationHelper;

		protected static void OverrideTransactionDescription(ZGuid[] postedARTransactions)
		{
			if (postedARTransactions.Length > 0 &&
				AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.Value &&
				Env.Security.AROverrideTransactionDescription.IsAllowed)
			{
				OverrideTransactionDescriptionForm form = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(new BusinessObjectFactory(), postedARTransactions));
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		#endregion

		#region Test
#if DEBUG

		#region Posting Tests

		protected ZBool Test_DeactivateChequeBookOnAllocation = ZBool.False;
		protected PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator Test_Allocator;
		protected List<PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator> Test_AllocatorsList
		{
			get
			{
				if (fTest_AllocatorsList == null)
				{
					fTest_AllocatorsList = new List<PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator>();
				}
				return fTest_AllocatorsList;
			}
		}
		List<PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator> fTest_AllocatorsList;

		protected virtual void SavePostingFactoriesForTest(TransactionCreatorHashtable transactions)
		{
			BusinessObjectFactory.SaveTogether(GetAllTransactionParticipants(transactions));
		}

		#endregion
#endif
		#endregion
	}
}
