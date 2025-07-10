using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region Posting Exception

	[Serializable]
	public class InterruptPostingException : Exception
	{
		public InterruptPostingException()
			: base(string.Empty)
		{
		}

		public InterruptPostingException(string message)
			: base(message)
		{
		}

		public InterruptPostingException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected InterruptPostingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	/// <summary>
	/// Exception class for reporting a wrong posting option
	/// </summary>
	[Serializable]
	public class NotSupportedPostingOptionException : OdysseyException
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public NotSupportedPostingOptionException()
			: base("Invalid Job Invoicing Option.")
		{
		}

#if NETFRAMEWORK
		public NotSupportedPostingOptionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	/// <summary>
	/// Exception class for reporting a critical posting error
	/// </summary>
	[Serializable]
	public class CriticalPostingErrorException : OdysseyException
	{
		public CriticalPostingErrorException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CriticalPostingErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public abstract class CriticalPostingErrorEventArgs : EventArgs
	{
		protected CriticalPostingErrorEventArgs(ZString errorMessage)
		{
			Message = errorMessage;
		}

		protected CriticalPostingErrorEventArgs(params BusinessObject[] businessEntities)
		{
			BusinessEntities = businessEntities;
		}

		public string ErrorMessage
		{
			get
			{
				StringCollectionX message = new StringCollectionX();

				if (BusinessEntities != null)
				{
					foreach (var businessEntity in BusinessEntities)
					{
						foreach (INotification notification in businessEntity.RowErrors)
						{
							if (!message.Contains(notification.Message))
							{
								message.Add(notification.Message);
							}
						}
					}
					if (message.Count == 0)
					{
						foreach (var businessEntity in BusinessEntities)
						{
							var notificationCollector = new ZNotificationCollector(businessEntity, false, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

							foreach (INotification notification in notificationCollector.GetErrors())
							{
								if (!message.Contains(notification.Message))
								{
									message.Add(notification.Message);
								}
							}
						}
					}
				}

				if (!Message.IsEmpty)
				{
					message.Add(Message);
				}

				return message.ToString();
			}
		}

		public readonly BusinessObject[] BusinessEntities;
		readonly ZString Message;
	}

	public class CriticalTransactionPostingErrorEventArgs : CriticalPostingErrorEventArgs
	{
		public CriticalTransactionPostingErrorEventArgs(params TransactionHeader[] headers)
			: base(headers)
		{
		}

		public TransactionHeader Header
		{
			get { return (TransactionHeader)BusinessEntities[0]; }
		}
	}

	public class CriticalChargePostingErrorEventArgs : CriticalPostingErrorEventArgs
	{
		public CriticalChargePostingErrorEventArgs(params BaseCharge[] charges)
			: base(charges.Cast<BusinessObject>().ToArray())
		{
		}

		public BaseCharge[] Charges
		{
			get { return BusinessEntities.Cast<BaseCharge>().ToArray(); }
		}
	}

	public class CriticalJobPostingErrorEventArgs : CriticalPostingErrorEventArgs
	{
		public CriticalJobPostingErrorEventArgs(params Job[] jobs)
			: base(jobs)
		{
		}

		public Job[] Jobs
		{
			get { return BusinessEntities.Cast<Job>().ToArray(); }
		}
	}

	public class CriticalPostingErrorWithMessageEventArgs : CriticalPostingErrorEventArgs
	{
		public CriticalPostingErrorWithMessageEventArgs(ZString errorMessage)
			: base(errorMessage)
		{
		}
	}

	#endregion

	/// <summary>
	/// Class to manage posting.
	/// If you want to implement a new type of posting, you should subclass it rather than create your own.
	/// </summary>
	public abstract partial class BasePostManager
	{
		protected BasePostManager(Job job, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null) : this()
		{
			Jobs = job != null ? new[] { job } : Array.Empty<Job>();
			this.apInvoicePostGUIProvider = apInvoicePostGUIProvider;
		}

		protected BasePostManager(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null) : this()
		{
			Argument.NotNull(fallbackFactory, "fallbackFactory");

			this.Jobs = jobs;
			this.fallbackFactory = fallbackFactory;
			this.apInvoicePostGUIProvider = apInvoicePostGUIProvider;
		}

		BasePostManager()
		{
			adjustPostedInvoiceHelper_constructorInitializedOnly = new AdjustPostedInvoiceHelper();
			apPaymentApprovalAmountUpdater_constructorInitializedOnly = new APPaymentApprovalAmountUpdater();
		}

		IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper => adjustPostedInvoiceHelper_constructorInitializedOnly;
		IAdjustPostedInvoiceHelper adjustPostedInvoiceHelper_constructorInitializedOnly;

		IAPPaymentApprovalAmountUpdater APPaymentApprovalAmountUpdater => apPaymentApprovalAmountUpdater_constructorInitializedOnly;
		IAPPaymentApprovalAmountUpdater apPaymentApprovalAmountUpdater_constructorInitializedOnly;

		#region Create Transactions

		public TransactionCreatorHashtable CreateTransactions(JobInvoicingPostingOption option)
		{
			TransactionCreatorHashtable result;
			using (new DisposableAction(
				() => { Factory.SuspendValidation(); Factory.SetContext(BusinessContext.PostManagerCreatingTransaction); },
				() => { Factory.ResumeValidation(); Factory.RemoveContext(BusinessContext.PostManagerCreatingTransaction); }
			))
			{
				result = CreateTransactionsCore(option);
			}
			CheckForCriticalErrors(result);
			CheckForNonCriticalWarnings(result);

			if (!CancelPosting)
			{
				CalculateOtherTaxes(result);
			}
			if (!CancelPosting)
			{
				AdjustPostedInvoice(result);
			}
			if (!CancelPosting)
			{
				APPaymentApprovalAmountUpdater.UpdateAmountsOnAllPaymentsAndInvoiceLinks(result);
			}

			return result;
		}

		TransactionCreatorHashtable CreateTransactionsCore(JobInvoicingPostingOption option)
		{
			Factory.SetContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel);
			var transactions = new TransactionCreatorHashtable();
			if (HasJobOnHold)
			{
				RaiseOnJobOnHoldEvent();
			}

			bool somethingPosted = false;
			try
			{
				switch (option)
				{
					case JobInvoicingPostingOption.Gateway:
						somethingPosted = CreateGatewayOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.Agent:
						somethingPosted = CreateAgentOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.LocalClient:
						somethingPosted = CreateLocalClientOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.Revenue:
						somethingPosted = CreateRevenueOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.Costs:
						somethingPosted = CreateCostsOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.ConsolCosts:
						somethingPosted = CreateConsolCostsOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.All:
						somethingPosted = CreateAllTransactions(transactions);
						break;
					case JobInvoicingPostingOption.Disbursement:
						somethingPosted = CreateDisbursementOnlyTransactions(transactions);
						break;
					case JobInvoicingPostingOption.CustomsDSBChargeAPOnly:
						somethingPosted = CreateSelectedAPInvoiceOnly(transactions);
						break;
					case JobInvoicingPostingOption.CustomsDSBChargeAROnly:
						somethingPosted = PostReceivablesCharges(JobInvoicingPostingOption.CustomsDSBChargeAROnly, transactions);
						break;
					case JobInvoicingPostingOption.AllSisterCompanyCharges:
						somethingPosted = PostReceivablesCharges(JobInvoicingPostingOption.AllSisterCompanyCharges, transactions);
						break;
					case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
						somethingPosted = PostReceivablesCharges(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly, transactions);
						break;
					default:
						throw new NotSupportedPostingOptionException();
				}

				if (somethingPosted)
				{
					AddBillingEvent(AccBillingCodes.GatewayBilling, option);
				}
				else
				{
					RaiseOnNothingPostedEvent();
					fCancelPosting = true;
				}
			}
			catch (InterruptPostingException ex)
			{
				if (!ex.Message.IsNullOrEmpty())
				{
					RaiseOnCriticalPostError(ex.Message);
				}

				fCancelPosting = true;
			}
			return transactions;
		}

		public bool PerformARCreditNoteLevelAuthorization(IPostingJobTransactionsApprovalGUIProvider arCreditNoteApprovalGUIProvider, bool isApprovedRequestPosting)
		{
			var postedCreditNotes = GetPosterARCreditNotes().ToArray();
			return new ARCreditNoteLevelAuthorizationWithApprovalRequest(arCreditNoteApprovalGUIProvider).PerformLevelAuthorization(postedCreditNotes, isApprovedRequestPosting);
		}

		IEnumerable<InvoicingBase> GetPosterARCreditNotes() =>
			from InvoicingBase transaction in Poster.PostedInvoices
			where transaction.AH_TransactionType == TransactionTypes.CreditNote
			select transaction; //here we have ARInvoice with converted transaction type, but it's not loaded as ARCreditNote instance

		protected void CheckForNonCriticalWarnings(TransactionCreatorHashtable transactions)
		{
			CheckIfInvoiceDateIsInTheFuture(transactions);
		}

#if DEBUG
		public Func<TransactionCreatorHashtable, bool> CheckForCriticalErrors_ForTestOnly { get; set; }

		public void SubstituteAdjustPostedInvoiceHelper_ForTestOnly(IAdjustPostedInvoiceHelper replacement) => adjustPostedInvoiceHelper_constructorInitializedOnly = replacement;
		public IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper_ExposedForTestOnly => AdjustPostedInvoiceHelper;

		public void SubstituteAPPaymentApprovalAmountUpdater_ForTestOnly(IAPPaymentApprovalAmountUpdater replacement) => apPaymentApprovalAmountUpdater_constructorInitializedOnly = replacement;
		public IAPPaymentApprovalAmountUpdater APPaymentApprovalAmountUpdater_ExposedForTestOnly => APPaymentApprovalAmountUpdater;

		public void CheckJobConsolCostValidation_FortestOnly(TransactionCreatorHashtable transactions) => CheckJobConsolCostValidation(transactions);

		public bool IsCallingCheckJobConsolCostValidation_ForTestOnly;
#endif

		void CheckJobConsolCostValidation(TransactionCreatorHashtable transactions)
		{
#if DEBUG
			IsCallingCheckJobConsolCostValidation_ForTestOnly = true;
#endif
			if (fCancelPosting)
			{
				return;
			}

			var headerWithError = new List<TransactionHeader>();
			foreach (var header in transactions.Values.OfType<InvoicingBase>())
			{
				var consolCostWithApportionChargeError = header.Lines.OfType<InvoicingLineBase>()
					.Select(line => line.RelatedJobCharge)
					.Select(c => c?.ParentConsolCost)
					.WhereNotNull()
					.Distinct()
					.Where(consolCost => {
						foreach (ApportionSplitCharge charge in consolCost.ApportionmentCharges)
						{
							if (charge.JR_LocalCostAmtInfo.HasError(AccountingMasterFilesConstants.ChargeLocalCostAmountCannotBeZeroErrorMessage))
							{
								return true;
							}
						}
						return false;
					});

				if (consolCostWithApportionChargeError.Any())
				{
					var errorMessage = string.Join(System.Environment.NewLine, consolCostWithApportionChargeError.Select(c => GetConsolCostErrorMessage(c)));
					header.AddRowError(errorMessage);
					headerWithError.Add(header);
				}
			}

			if (headerWithError.Any())
			{
				fCancelPosting = true;
				OnCriticalPostError?.Invoke(this, new CriticalTransactionPostingErrorEventArgs(headerWithError.ToArray()));
			}

			string GetConsolCostErrorMessage(JobConsolCost consolCost)
			{
				return $"Local amount of apportion charge (Consol: {consolCost.Consol.JK_UniqueConsignRef}, Charge Code: {consolCost.ChargeCode.AC_Code}) cannot be zero when Overseas Cost amount is non zero. Please check relative Consol Cost exchange rate settings.";
			}
		}

		protected virtual void CheckForCriticalErrors(TransactionCreatorHashtable transactions)
		{
#if DEBUG
			if (CheckForCriticalErrors_ForTestOnly?.Invoke(transactions) ?? false)
			{
				fCancelPosting = true;
			}
#endif

			foreach (TransactionHeader header in transactions.Values)
			{
				header.Validation.ValidateAH_GE();

				if (header.AH_InvoiceDateInfo.HasErrors() || (header.AH_GEInfo.HasErrors() && !string.IsNullOrWhiteSpace(GlbBranchCombinationValidation.CheckBranchDepartmentCombination(header.Branch, header.Department))))
				{
					fCancelPosting = true;
					RaiseOnCriticalPostError(header);
				}

				if (InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(header.AH_GC.ToGuid(), header.AH_InvoiceDateInfo, header.GetType()))
				{
					fCancelPosting = true;
					header.AddRowError(Res.GetString("4267DC63-F498-4809-A0C3-1F6A4B098834", @"Job={0}, Debtor={1}, Invoice Date={2}, Term={3}+{4}
The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.", header.JobNumber, header.Header.OH_Code, header.AH_InvoiceDate, header.AH_InvoiceTerm, header.AH_InvoiceTermDays));
					RaiseOnCriticalPostError(header);
				}
			}

			CheckEmtpyComplianceSubType(transactions);
			CheckInactiveBranch(transactions);
			CheckInvalidAgreedPaymentMethodOverride(transactions);
			CheckTaxMessage(transactions);

			InvoicingBase[] aPInvoicesAndCredits = transactions.GetAllAPInvoicesAndCreditNotes();
			foreach (InvoicingBase aPInv in aPInvoicesAndCredits)
			{
				CheckInvoiceOrCreditNoteHasLines(aPInv);

				bool transactionNumberExists = (AccountingUtils.APTransactionNumberExists(aPInv.AH_TransactionType, aPInv.AH_TransactionNum, aPInv.AH_OH, aPInv.AH_InvoiceDate)).HasError ||
					(AccountingUtils.UATransactionNumberExists(aPInv.AH_TransactionType, aPInv.AH_TransactionNum, aPInv.AH_OH, aPInv.PK, aPInv.AH_InvoiceDate)).HasError;
				if (transactionNumberExists)
				{
					fCancelPosting = true;
					aPInv.AddRowError(Res.GetString("DB77F6E8-8A16-474f-8FBF-588A6C74443E", "AP {0} number {1} is already used for the organization: {2}. Please use another transaction number.",
						aPInv.AH_TransactionType == TransactionTypes.Invoice ? InvoiceCaption : CreditNoteCaption,
						aPInv.AH_TransactionNum,
						aPInv.Header.OH_Code));
					RaiseOnCriticalPostError(aPInv);
				}
			}

			InvoicingBase[] aRInvoicesAndCredits = transactions.GetAllARInvoicesAndCreditNotes();
			foreach (InvoicingBase aRInv in aRInvoicesAndCredits)
			{
				CheckAROrgHeaderForPosting(aRInv);
				CheckInvoiceOrCreditNoteHasLines(aRInv);
				CheckInvoiceOrCreditNoteLines(aRInv);
				CheckInvoiceZeroBalanceAmount(aRInv);
			}

			CheckIfCreditNoteIsAllowed(transactions);
			CheckJobConsolCostValidation(transactions);
		}

		void CheckIfCreditNoteIsAllowed(TransactionCreatorHashtable transactions)
		{
			var apCreditNote = transactions.GetAllAPCreditNotes().FirstOrDefault();
			if (apCreditNote != null)
			{
				if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, apCreditNote.AH_GC))
				{
					fCancelPosting = true;
					apCreditNote.AddRowError(Res.GetString("4AB5A8F5-2939-4DE1-A164-D8CF56D0E712", "Posting of Payables Credit Notes is not permitted. The COST Charges being posted would produce at least one Payables Credit Note. Please review the prepared charges and correct appropriately. {0}", AccountingMasterFilesUtils.APCreditNoteDisallowed_RegistryOnlyMessage));
					RaiseOnCriticalPostError(apCreditNote);
				}
			}

			var arCreditNote = transactions.GetAllARCreditNotes().Union(GetPosterARCreditNotes()).FirstOrDefault();
			if (arCreditNote != null)
			{
				if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, arCreditNote.AH_GC))
				{
					fCancelPosting = true;
					arCreditNote.AddRowError(Res.GetString("7FB5CBE4-4BD0-4439-AA5E-F6521D5EC0F5", "Posting of Receivables Credit Notes is not permitted. The SELL Charges being posted would produce at least one Receivables Credit Note. Please review the prepared charges and correct appropriately. {0}", AccountingMasterFilesUtils.ARCreditNoteDisallowed_RegistryOnlyMessage));
					RaiseOnCriticalPostError(arCreditNote);
				}
			}
		}

		void CheckInvalidAgreedPaymentMethodOverride(TransactionCreatorHashtable transactions)
		{
			List<TransactionHeader> headersWithInvalidAgreedPaymentMethodsErrors = new List<TransactionHeader>();
			foreach (TransactionHeader header in transactions.Values)
			{
				if (!header.AH_AgreedPaymentMethodOverride.IsEmpty && !header.DisplayAgreedPaymentMethodsList.ContainsCode(header.AH_AgreedPaymentMethodOverride))
				{
					fCancelPosting = true;
					header.AddRowError(Res.GetString("47d1c35f-d8e0-4770-a295-cb6e9f773a3f", "Organization {0} cannot post {1} against invalid agreed payment method '{2}'. Please check the organization setup and registry setting at Organization > AR/AP > Agreed Payment Method.",
						header.Header.OH_Code, header.AH_TransactionType == TransactionTypes.Invoice ? InvoiceCaption : CreditNoteCaption, header.AH_AgreedPaymentMethodOverride));
					headersWithInvalidAgreedPaymentMethodsErrors.Add(header);
				}
			}
			if (headersWithInvalidAgreedPaymentMethodsErrors.Count > 0 && OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalTransactionPostingErrorEventArgs(headersWithInvalidAgreedPaymentMethodsErrors.ToArray()));
			}
		}

		void CheckTaxMessage(TransactionCreatorHashtable transactions)
		{
			var headersWithErrors = new List<TransactionHeader>();
			foreach (TransactionHeader header in transactions.Values)
			{
				if (header is TransactionHeaderWithLines headerWithLines)
				{
					foreach (TransactionLine line in headerWithLines.Lines)
					{
						line.Validation.ValidateAL_A9_VATClass();
						if (line.AL_A9_VATClassInfo.HasErrors())
						{
							fCancelPosting = true;
							header.AddRowError(line.AL_A9_VATClassInfo.GetErrors().ToUniqueMessageListString());
							headersWithErrors.Add(header);
							break;
						}
					}
				}
			}
			if (headersWithErrors.Count > 0 && OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalTransactionPostingErrorEventArgs(headersWithErrors.ToArray()));
			}
		}

		void CheckInactiveBranch(TransactionCreatorHashtable transactions)
		{
			List<TransactionHeader> headersWithInactiveBranchErrors = new List<TransactionHeader>();
			foreach (TransactionHeader header in transactions.Values)
			{
				if (!header.Branch.GB_IsActive)
				{
					fCancelPosting = true;
					header.AddRowError(Res.GetString("d308c321-148e-4621-8174-a7ee460d0c89", "{0} cannot be posted against inactive branch '{1}'. Please check the job branch.",
						header.AH_TransactionType == TransactionTypes.Invoice ? InvoiceCaption : CreditNoteCaption, header.Branch.GB_Code));
					headersWithInactiveBranchErrors.Add(header);
				}
			}
			if (headersWithInactiveBranchErrors.Count > 0 && OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalTransactionPostingErrorEventArgs(headersWithInactiveBranchErrors.ToArray()));
			}
		}

		void CheckEmtpyComplianceSubType(TransactionCreatorHashtable transactions)
		{
			if (AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value)
			{
				List<TransactionHeader> headersWithEmptyComplianceSubTypeErrors = new List<TransactionHeader>();
				foreach (TransactionHeader header in transactions.Values)
				{
					using (header.DoNotValidateEmptyComplianceSubTypeSuspender.GetSuspender())
					{
						header.Validation.ValidateAH_ComplianceSubType();
						if (header.AH_ComplianceSubTypeInfo.HasErrors())
						{
							fCancelPosting = true;
							header.AddRowError(header.AH_ComplianceSubTypeInfo.GetErrors().ToUniqueMessageListString());
							headersWithEmptyComplianceSubTypeErrors.Add(header);
						}
					}
				}
				if (headersWithEmptyComplianceSubTypeErrors.Count > 0 && OnCriticalPostError != null)
				{
					OnCriticalPostError(this, new CriticalTransactionPostingErrorEventArgs(headersWithEmptyComplianceSubTypeErrors.ToArray()));
				}
			}
		}

		internal static string InvoiceCaption { get { return Res.GetString("23ED7EB5-919C-4d19-9F27-2521F62A4E74", "Invoice"); } }
		internal static string CreditNoteCaption { get { return Res.GetString("D22D1820-74F5-44a6-B3B5-DF5ADFB1F134", "Credit Note"); } }

		void CheckInvoiceOrCreditNoteHasLines(InvoicingBase inv)
		{
			if ((inv.AH_TransactionType == TransactionTypes.Invoice || inv.AH_TransactionType == TransactionTypes.CreditNote) &&
				(inv.AH_Ledger == LedgerTypes.AccountsReceivable || inv.AH_Ledger == LedgerTypes.AccountsPayable) &&
				inv.Lines.Count == 0)
			{
				fCancelPosting = true;
				string errorMessage = Res.GetString("5031C97E-17CB-475D-85B1-9279051C28C6", "{0} {1} number {2} has no lines and cannot be posted. Please check your data and try again",
					inv.AH_Ledger,
					inv.AH_TransactionType == TransactionTypes.Invoice ? InvoiceCaption : CreditNoteCaption,
					inv.AH_TransactionNum);
				inv.AddRowError(errorMessage);
				RaiseOnCriticalPostError(inv);
				ErrorReporter.ReportOnce(errorMessage);
			}
		}

		void CheckInvoiceOrCreditNoteLines(InvoicingBase inv)
		{
			if (inv.InvoicingValidation != null && inv.InvoicingValidation.ValidateLines())
			{
				fCancelPosting = true;
				RaiseOnCriticalPostError(inv);
			}
		}

		void CheckAROrgHeaderForPosting(InvoicingBase invoice)
		{
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(invoice.Company.Country.Code);
			(var errorMessage, var isErrorMessage) = complianceInfo is IOrgHeaderPostingValidation ? ((IOrgHeaderPostingValidation)complianceInfo).CheckOrgHeaderForPosting(invoice) : (string.Empty, false);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				if (isErrorMessage)
				{
					fCancelPosting = true;
					invoice.AddRowError(errorMessage);
					RaiseOnCriticalPostError(invoice);
				}
				else
				{
					invoice.AddRowWarning(errorMessage);
				}
			}
		}

		void CheckInvoiceZeroBalanceAmount(InvoicingBase invoice)
		{
			if (invoice.AH_OSTotal == 0m && ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalid(invoice, out string errorMessage))
			{
				fCancelPosting = true;
				invoice.AddRowError(errorMessage);
				RaiseOnCriticalPostError(invoice);
			}
		}

#if DEBUG
		protected virtual
#endif
		UserMessageEventArgs BuildInvoiceDateInFutureWarningMessage(TransactionCreatorHashtable transactions)
		{
			var today = ZDateTime.Today;
			UserMessageEventArgs result = null;
			var invoices = CountrySpecificValidationHelper.GetTransactionsWithWarningAboutInvoiceDateInTheFuture(transactions, today);
			if (invoices.Any())
			{
				var warningMsg = new List<string>();
				warningMsg.Add(Res.GetString("BC54B263-9ADF-48EB-81FD-11EA86D24104", "Invoice Date is in the future. Please check your system date setting."));
				foreach (var invoice in invoices)
				{
					if (invoice.AH_InvoiceDate.Date > today)
					{
						warningMsg.Add(Res.GetString("86bd25db-8eb9-42ec-a938-35164351dbc2", "{0} {1} for Organization {2} has future invoice date '{3}'.",
							invoice.AH_Ledger, invoice.AH_TransactionType == TransactionTypes.Invoice ? InvoiceCaption : CreditNoteCaption, invoice.Header.OH_Code, invoice.AH_InvoiceDate.Date.ToShortDateString()));
					}
				}
				result = new UserMessageEventArgs(string.Join("\r\n", warningMsg));
			}
			return result;
		}

		public void RollbackPosting()
		{
			RollbackPostingCore();
		}

		protected virtual void RollbackPostingCore()
		{
			fCancelPosting = true;
		}

		public delegate ChangeTransactionDatesBusinessObject GetNewChangeTransactionDatesBusinessObject(SecurityCheckpoint pluginSecurity, BusinessObjectFactory factory, OperationsJobConfigurationCodes codes);

		public delegate void ChangeTransactionDatesEventHandler(object sender, ChangeTransactionDatesEventArgs e);
		public event ChangeTransactionDatesEventHandler ChangeTransactionDates;

		public class ChangeTransactionDatesEventArgs : EventArgs
		{
			public ChangeTransactionDatesBusinessObject ChangeTransactionDatesBusinessObject { get; set; }
			public QueryUserMsgBoxEventArgs Args { get; set; }
			public ZDateTime TransactionDate { get; set; }
			public ZDateTime PostDate { get; set; }
		}

		/*
		 * IMPORTANT, READ THIS FIRST BEFORE MAKING CHANGE TO THE BELOW METHOD.
		 * The Back Dating validations taking place on ChangeTransactionDatesBusinessObject in the below method are to be kept consistent with the logic in JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization.GetOverrideDatesStatus().
		 * Currently the common logic to validate Post Date and Invoice Date exists in ChangeTransactionDatesBusinessObjectBase.RunPreSaveValidationCore().
		 * If in future there is any change related to such validation then it should also be reflected in JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization.GetOverrideDatesStatus().
		 * Else both validations will be out of sync and perform validations differently, which is not intended behaviour.
		 */
		public bool PerformTransactionBackDating(IJobInvoicingPlugIn jobInvoicingPlugIn, GetNewChangeTransactionDatesBusinessObject getNewChangeTransactionDatesBusinessObject, TransactionCreatorHashtable transactions, BusinessObjectFactory transactionFactory)
		{
			bool continueProcessing = true;

			OperationsJobConfigurationCodes operationsJobConfigurationCodes = new OperationsJobConfigurationCodes(jobInvoicingPlugIn);

			InvoiceDateConfigurationHelper invoiceDateConfigurationHelper = new InvoiceDateConfigurationHelper(operationsJobConfigurationCodes, jobInvoicingPlugIn);
			InvoiceDateConfiguration invoiceDateConfiguration = invoiceDateConfigurationHelper.FindInvoiceDateConfiguration();

			ChangeTransactionDatesBusinessObject changeTransactionDateBusinessObject = getNewChangeTransactionDatesBusinessObject(jobInvoicingPlugIn != null ? jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity : null, transactionFactory, operationsJobConfigurationCodes);
			bool canPerformBackDating = invoiceDateConfigurationHelper.CanBackPost ||
										!changeTransactionDateBusinessObject.InvoiceDateInfo.ReadOnly ||
										!changeTransactionDateBusinessObject.PostDateInfo.ReadOnly;

			if (Poster.PostedInvoices.Count > 0 && canPerformBackDating)
			{
				ZDateTime invoiceDate = invoiceDateConfigurationHelper.CanBackPost ? invoiceDateConfigurationHelper.GetInvoiceDate(Poster.PostedInvoices[0].AH_InvoiceDate)
																					: Poster.PostedInvoices[0].AH_InvoiceDate;
				ZDateTime postDate = Poster.PostedInvoices[0].AH_PostDate;

				bool shouldUseDefaultARInvoiceAndPostDate = ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate();

				if (shouldUseDefaultARInvoiceAndPostDate)
				{
					invoiceDate = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(invoiceDate);
					postDate = invoiceDate;
				}

				if (((invoiceDateConfiguration != null && invoiceDateConfiguration.Today)
					|| AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate)
					&& !shouldUseDefaultARInvoiceAndPostDate)
				{
					changeTransactionDateBusinessObject.PostDate = postDate;
					changeTransactionDateBusinessObject.InvoiceDate = invoiceDate;

					List<string> revenueRecognitionOptions = new List<string>();
					foreach (Job job in Jobs)
					{
						string revenueRecognitionDates = job.RevenueRecognitionDates;
						if (!string.IsNullOrEmpty(revenueRecognitionDates))
						{
							revenueRecognitionOptions.Add(revenueRecognitionDates);
						}
					}

					var revenueRecognitionOptionsStrings = string.Join("; ", revenueRecognitionOptions.ToArray());
					if (revenueRecognitionOptionsStrings.Length > ChangeTransactionDatesBusinessObject.RevenueRecognitionDatesMaxLength)
					{
						revenueRecognitionOptionsStrings = Res.GetString("ed67b0f6-fa4b-4ddc-a1d7-b9e2f867bd16", "Too Many Recognition Dates to Display - See Job Profit Document");
					}

					changeTransactionDateBusinessObject.RevenueRecognitionDates = revenueRecognitionOptionsStrings;

					if (ChangeTransactionDates != null)
					{
						var eventArgs = new ChangeTransactionDatesEventArgs();
						eventArgs.ChangeTransactionDatesBusinessObject = changeTransactionDateBusinessObject;
						ChangeTransactionDates(this, eventArgs);
						Argument.NotNull(eventArgs.Args, "QueryUserMsgBoxEventArgs");

						QueryUserYesNoCancelEventArgs argsWithCancelState = eventArgs.Args as QueryUserYesNoCancelEventArgs;
						continueProcessing = !argsWithCancelState?.Cancel ?? true;
						if (eventArgs.Args.Response)
						{
							if (!ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(eventArgs.TransactionDate, eventArgs.PostDate, transactionFactory))
							{
								return false;
							}
						}
					}
				}
				else
				{
					if (!ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(invoiceDate, postDate, transactionFactory))
					{
						return false;
					}
				}
			}

			PostDateConfigurationHelper postDateConfigurationHelper = new PostDateConfigurationHelper(jobInvoicingPlugIn);
			PostDateConfiguration postDateConfiguration = postDateConfigurationHelper.FindPostDateConfiguration();
			InvoicingBase[] apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();

			if (postDateConfiguration != null && apTransactions.Length > 0)
			{
				ZDateTime now = ZDateTime.Now;

				foreach (var apInvoice in apTransactions)
				{
					if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, apInvoice.IsLocalCurrencyTransaction, apInvoice.AH_GC)
						&& !CheckInvoicePostingExchangeRateOption(apInvoice))
					{
						return false;
					}
				}

				foreach (PaymentApprovalBase payment in transactions.GetAllAPPaymentApprovals())
				{
					ZDateTime invoiceDate = payment.AV_PaymentDate;

					Charge chargeResult = (from Charge c in payment.RelatedCharges select c).OrderByDescending(x => x.JR_APInvoiceDate).FirstOrDefault();

					if (chargeResult != null)
					{
						invoiceDate = chargeResult.JR_APInvoiceDate;
					}

					ZDateTime postDate = postDateConfigurationHelper.GetPostDate(now, invoiceDate);
					if (!postDate.IsEmpty)
					{
						payment.AV_PostDate = postDate;
					}
				}
			}

			return continueProcessing;
		}

		public virtual void PerformTransactionDescriptionDefaulting(IJobInvoicingPlugIn plugIn, TransactionCreatorHashtable transactions)
		{
			if (plugIn != null)
			{
				var defaultDescription = GetDefaultJobTransactionDescription(plugIn);
				if (!defaultDescription.IsEmpty)
				{
					if (transactions.Count > 0)
					{
						var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
						foreach (InvoicingBase invoice in apTransactions)
						{
							invoice.AH_Desc = defaultDescription.Left(AutoAccTransactionHeader.Schema.AH_DescMaxLength);
						}

						var uaTransactions = transactions.GetAllUATransactions();
						foreach (InvoicingBase invoice in uaTransactions)
						{
							invoice.AH_Desc = defaultDescription.Left(AutoAccTransactionHeader.Schema.AH_DescMaxLength);
						}

						var paymentApprovals = transactions.GetAllAPPaymentApprovals();
						foreach (PaymentApprovalBase approval in paymentApprovals)
						{
							approval.AV_PaymentComment = defaultDescription.Left(AutoAccPaymentApproval.Schema.AV_PaymentCommentMaxLength);
						}

						Poster.ChangeTransactionDescriptionOnAllARInvoicesAndCFXLines(defaultDescription);
					}
				}
			}
		}

		protected virtual ZString GetDefaultJobTransactionDescription(IJobInvoicingPlugIn plugIn)
		{
			return plugIn.GetDefaultJobDescription();
		}

		protected virtual void CalculateOtherTaxes(TransactionCreatorHashtable result)
		{
			if (GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory))
			{
				foreach (var invoice in result.Values.OfType<InvoicingBase>())
				{
					var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
					var errorMessage = ObjectFactory.Get<ITaxProcessor>().ProcessTaxesOnPosting(taxParent);
					if (string.IsNullOrEmpty(errorMessage))
					{
						var errorMessages = new List<string>();
						foreach (var taxTransaction in invoice.TaxTransactionCollection)
						{
							if (taxTransaction.HasErrors)
							{
								errorMessages.Add(new HumanReadableNotificationCollector(taxTransaction, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString());
							}
						}

						if (errorMessages.Any())
						{
							errorMessage = errorMessages.ToStringWithNewLineBetweenStrings();
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						invoice.AddRowError(errorMessage);
						fCancelPosting = true;
						RaiseOnCriticalPostError(invoice);
						break;
					}
				}
			}
		}

		void AdjustPostedInvoice(TransactionCreatorHashtable result)
		{
			foreach (var invoice in result.Values.OfType<InvoicingBase>())
			{
				AdjustPostedInvoiceHelper.AdjustPostedInvoice(invoice);
			}
		}

		#endregion

		public int TotalNumberOfCharges
		{
			get { return Charges.Length; }
		}

		#region Charges To Be Posted

		protected virtual Charge[] Charges
		{
			get
			{
				if (fCharges == null)
				{
					ArrayList result = new ArrayList();
					foreach (Job job in Jobs)
					{
						result.AddRange(job.Charges);
					}

					fCharges = result.Cast<Charge>().Distinct().ToArray();
				}

				return fCharges;
			}
		}

		protected Charge[] fCharges;

		#endregion

		#region Receivables Charge Posting

		protected bool PostReceivablesCharges(JobInvoicingPostingOption postingOption, TransactionCreatorHashtable transactions)
		{
			bool result = false;

			IReceivablesPostingChargeCollection filteredCharges = EligibilityDecider.GetEligibleCharges(postingOption);
			ProcessEligibleCharges(filteredCharges);
			PostingChargeCollection distributedCharges1 = DistributeCharges(filteredCharges);

			bool reDistributeCharges = false;
			foreach (IReceivablesPostingChargeCollection distributedChargeCollection in distributedCharges1)
			{
				if (distributedChargeCollection.IsDisbursementChargeCollection)
				{
					ZDecimal supposedInvoiceLocalAmount = Poster.GetSupposedInvoiceAmount(distributedChargeCollection);

					OrgHeader debtor = Factory.Load<OrgHeader>(distributedChargeCollection.Debtor);

					if (Math.Abs(supposedInvoiceLocalAmount) < debtor.MiscServ.OM_ARTreatDisbursementsAsStandardValue)
					{
						foreach (IReceivablesPostingCharge charge in distributedChargeCollection)
						{
							if (charge.InvoiceType == InvoiceTypesList.Codes.DisbursementInForeignCurrency)
							{
								charge.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
								reDistributeCharges = true;
							}
							else if (charge.InvoiceType == InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching)
							{
								charge.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
								reDistributeCharges = true;
							}
							else if (charge.InvoiceType == InvoiceTypesList.Codes.DisbursementInvoice)
							{
								charge.InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
								reDistributeCharges = true;
							}
							else if (charge.InvoiceType == InvoiceTypesList.Codes.DisbursementInvoice_Batching)
							{
								charge.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
								reDistributeCharges = true;
							}
						}
					}
				}
			}

			if (reDistributeCharges)
			{
				distributedCharges1 = DistributeCharges(filteredCharges);

				#region reDistributeCharges_ForTest
#if DEBUG
				reDistributeCharges_ForTest = true;
#endif
				#endregion
			}

			foreach (IReceivablesPostingChargeCollection distributedCharges in distributedCharges1)
			{
				var charges = distributedCharges.Cast<Charge>().ToArray();
				if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, charges.First().IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR), transactions.Company.PK))
				{
					charges.ForEach(x => x.SetContext(BusinessContext.PostingReceivableCharges));
					try
					{
						ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsReceivable, charges, ZDateTime.Now);
					}
					finally
					{
						charges.ForEach(x => x.RemoveContext(BusinessContext.PostingReceivableCharges));
					}
				}
			}

			ApplyRounding(distributedCharges1);

			if (!CancelPosting)
			{
				foreach (IReceivablesPostingChargeCollection distributedChargeCollection in distributedCharges1)
				{
					if (!ValidateDistributedChargeCollection(distributedChargeCollection))
					{
						break;
					}

					Poster.Post(distributedChargeCollection);

					if (distributedChargeCollection.PostedInvoice != null)
					{
						result = true;
						transactions.AddARInvoice(distributedChargeCollection.PostedInvoice);
						OnInvoicePosted(postingOption, distributedChargeCollection);
					}
					if (CancelPosting)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		void ApplyRounding(PostingChargeCollection distributedCharges)
		{
			if (GlbCompany.CurrentCompany.Country.Code == Enterprise.Core.Constants.CountryCodes.Japan)
			{
				string roundingRule = AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.Value;
				if (roundingRule != Constants.RoundingRules.Codes.None)
				{
					new ChargeAmountsRounder().RoundChargeAmounts(distributedCharges, roundingRule);
					foreach (IReceivablesPostingChargeCollection distributedChargeCollection in distributedCharges)
					{
						if (fCancelPosting)
						{
							break;
						}

						foreach (Charge charge in distributedChargeCollection)
						{
							if (charge.HasErrors)
							{
								fCancelPosting = true;
								RaiseOnCriticalPostError(charge);
								break;
							}
						}
					}
				}
			}
		}

		protected virtual void OnInvoicePosted(JobInvoicingPostingOption postingOption, IReceivablesPostingChargeCollection distributedChargeCollection)
		{
		}

		protected virtual void ProcessEligibleCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			foreach (IPostingCharge charge in filteredCharges)
			{
				var chargeWithCost = charge as ChargeWithCost;
				var query = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, chargeWithCost.JR_JH);
				query.AddToFilter(JobHeaderSchema.JH_GC, chargeWithCost.JR_GC);
				Factory.AddFetchHint(JobHeaderSchema.Instance, query);
			}

			var chargesWithError = new List<BaseCharge>();
			foreach (var charge2 in filteredCharges)
			{
				charge2.InitializeSellAddressContact();
				var chargeAsIBusiness = charge2 as IBusiness;
				using (GetProcessEligibleChargesValidationDisposable(chargeAsIBusiness))
				{
					charge2.ValidateAll();
				}
				if (charge2.HasErrors)
				{
					fCancelPosting = true;
					chargesWithError.Add(charge2 as BaseCharge);
				}
			}

			RaiseOnCriticalPostError(chargesWithError);

			var jobsWithError = new List<Job>();

			var jobsToPost = (from IPostingCharge charge in filteredCharges where charge.Job != null select charge.Job).Distinct();
			foreach (Job job in jobsToPost)
			{
				if (job.JH_ProfitLossReasonCode.IsEmpty
					&& AccountingConfigurationRegistry.Instance.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(job.JH_Status)
					&& ((JobValidation)job.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(JobHeaderStatus.JobInvoiced.Code))
				{
					fCancelPosting = true;
					job.AddRowError(Res.GetString("6056597c-e2a5-4208-9114-7ab41da45833", "Job {0} status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices.", job.JH_JobNum));
					jobsWithError.Add(job);
				}
			}

			RaiseOnCriticalPostError(jobsWithError);
		}

		protected virtual bool ValidateDistributedChargeCollection(IReceivablesPostingChargeCollection distributedChargeCollection)
		{
			if (!AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value)
			{
				return distributedChargeCollection.IsBillInLocalCurrency
					? distributedChargeCollection.TotalValueInLocalCurrency != 0
					: distributedChargeCollection.TotalValueInForeignCurrency != 0;
			}

			return true;
		}

		/// <summary>
		/// Returns an IDisposable to force validation, or keep it suspended.
		/// By default, validation is forced, even if suspended in bizo / factory.
		/// </summary>
		protected virtual DisposableAction GetProcessEligibleChargesValidationDisposable(IBusiness charge)
			=> charge != null
				? new DisposableAction(() => charge.IgnoreValidationSuspended = true, () => charge.IgnoreValidationSuspended = false)
				: DisposableAction.NoAction;

		protected virtual PostingChargeCollection DistributeCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			return Distributor.DistributeCharges(filteredCharges);
		}

		PostingChargeEligibilityDecider EligibilityDecider
		{
			get
			{
				if (fEligibilityDecider == null)
				{
					fEligibilityDecider = GetEligibilityDecider(Charges);
				}
				return fEligibilityDecider;
			}
		}

		protected virtual PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			return new PostingChargeEligibilityDecider(charges);
		}

		PostingChargeEligibilityDecider fEligibilityDecider;

		protected PostingChargeDistributor Distributor
		{
			get
			{
				if (fDistributor == null)
				{
					fDistributor = GetDistributor();
				}
				return fDistributor;
			}
		}

		protected virtual PostingChargeDistributor GetDistributor()
		{
			return new PostingChargeDistributor();
		}

		PostingChargeDistributor fDistributor;

		public ChargePoster Poster
		{
			get
			{
				if (fPoster == null)
				{
					fPoster = GetPoster();
				}
				return fPoster;
			}
		}

		protected virtual ChargePoster GetPoster()
		{
			return new ChargePoster(Factory, this);
		}

		ChargePoster fPoster;

		#endregion

		#region Properties

		public BusinessObjectFactory Factory
		{
			get { return Jobs.Any() ? Jobs.First().Factory : fallbackFactory; }
		}

		protected readonly IEnumerable<Job> Jobs;
		protected readonly BusinessObjectFactory fallbackFactory;
		protected ZDateTime PostingTime = ZDateTime.Now;

		protected bool fCancelPosting;
		public bool CancelPosting
		{
			get { return fCancelPosting; }
		}

#if DEBUG
		public void SetCancelPostingForTestOnly(bool value)
		{
			fCancelPosting = value;
		}

		internal bool reDistributeCharges_ForTest;
#endif
		#endregion

		#region Jobs on hold

		protected bool HasJobOnHold
		{
			get { return JobOnHoldCount > 0; }
		}

		protected int JobOnHoldCount
		{
			get
			{
				int result = 0;
				foreach (Job job in Jobs)
				{
					if (job.IsWorkOnHold)
					{
						result++;
					}
				}
				return result;
			}
		}

		#endregion

		#region Create Transaction Methods

		protected virtual bool CreateGatewayOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateAgentOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateLocalClientOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateRevenueOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateCostsOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateConsolCostsOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateAllTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateDisbursementOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		protected virtual bool CreateSelectedAPInvoiceOnly(TransactionCreatorHashtable transactions)
		{
			throw new NotSupportedPostingOptionException();
		}

		#endregion

		#region Events

		#region OnJobOnHold Event

		public delegate void OnJobOnHoldEventHandler(object sender, OnJobOnHoldEventArgs e);
		public event OnJobOnHoldEventHandler OnJobOnHold;
		public class OnJobOnHoldEventArgs : EventArgs
		{
			public OnJobOnHoldEventArgs(IEnumerable<Job> jobs)
			{
				this.Jobs = jobs.Where(x => x.IsWorkOnHold).ToArray();
			}

			public readonly IEnumerable<Job> Jobs;
		}

		protected void RaiseOnJobOnHoldEvent()
		{
			if (OnJobOnHold != null)
			{
				OnJobOnHold(this, new OnJobOnHoldEventArgs(Jobs));
			}
		}

		#endregion

		#region OnNothingPosted

		public event EventHandler OnNothingPosted;
		protected void RaiseOnNothingPostedEvent()
		{
			if (OnNothingPosted != null)
			{
				OnNothingPosted(this, EventArgs.Empty);
			}
		}

		#endregion

		#region OnUserWarningNotification

		public event EventHandler<UserMessageEventArgs> OnUserWarningNotification;
		public void CheckIfInvoiceDateIsInTheFuture(TransactionCreatorHashtable transactions)
		{
			if (OnUserWarningNotification != null)
			{
				var args = BuildInvoiceDateInFutureWarningMessage(transactions);
				if (args != null)
				{
					OnUserWarningNotification(this, args);
				}
			}
		}

		#endregion

		#region OnCriticalPostError

		public event EventHandler<CriticalPostingErrorEventArgs> OnCriticalPostError;
		protected void RaiseOnCriticalPostError(TransactionHeader header)
		{
			if (OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalTransactionPostingErrorEventArgs(header));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		protected void RaiseOnCriticalPostError(BaseCharge charge)
		{
			if (OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalChargePostingErrorEventArgs(charge));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		protected void RaiseOnCriticalPostError(List<BaseCharge> chargesWithError)
		{
			if (chargesWithError.Count > 0 && OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalChargePostingErrorEventArgs(chargesWithError.ToArray()));
			}
		}

		protected void RaiseOnCriticalPostError(List<Job> jobsWithError)
		{
			if (jobsWithError.Count > 0 && OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalJobPostingErrorEventArgs(jobsWithError.ToArray()));
			}
		}

		protected void RaiseOnCriticalPostError(ZString errorMessage)
		{
			if (OnCriticalPostError != null)
			{
				OnCriticalPostError(this, new CriticalPostingErrorWithMessageEventArgs(errorMessage));
			}
		}

		#endregion

		#endregion

		#region InvoicePostingExchangeRateOption
		bool CheckInvoicePostingExchangeRateOption(InvoicingBase invoice)
		{
			var errorMessage = ExchangeRateCalculator.CheckInvoiceExchangeRate(invoice);
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				invoice.AddRowError(errorMessage);
				RaiseOnCriticalPostError(invoice);
				return false;
			}
			return true;
		}

		bool UpdateChargesAndLinesExchangeRateForBackDating(IEnumerable<InvoicingBase> invoices, BusinessObjectFactory transactionFactory)
		{
			foreach (var invoice in invoices)
			{
				ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, transactionFactory);
				if (!CheckInvoicePostingExchangeRateOption(invoice))
				{
					return false;
				}
			}
			return true;
		}

		public bool ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(ZDateTime invoiceDate, ZDateTime postDate, BusinessObjectFactory transactionFactory)
		{
			Poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(invoiceDate, postDate);

			var arInvoices = Poster.PostedInvoices
				.Cast<InvoicingBase>()
				.Where(x => x.AH_Ledger == LedgerTypes.AccountsReceivable)
				.GroupBy(x => new { Currency = x.AH_RX_NKTransactionCurrency }, x => x);

			foreach (var group in arInvoices)
			{
				var isExRateOptionApplicable = ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, group.First().IsLocalCurrencyTransaction, group.First().AH_GC);
				if (!isExRateOptionApplicable && (!group.AllSame(x => x.AH_GC) || !group.AllSame(x => x.IsLocalCurrencyTransaction)))
				{
					group.ForEach(x =>
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(transactionFactory).AddInfoWhenAllowed(x.PK, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction, () =>
						{
							return FormattableString.Invariant($"Group First IsLocalCurrencyTransaction = {group.First().IsLocalCurrencyTransaction}, Group First AH_GC = {group.First().AH_GC}, Invoice IsLocalCurrencyTransaction = {x.IsLocalCurrencyTransaction}, Invoice AH_GC = {x.AH_GC}");
						});
					} );
				}

				if (isExRateOptionApplicable && !UpdateChargesAndLinesExchangeRateForBackDating(group, transactionFactory))
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Billing

		protected void AddBillingEvent(string billingCode, JobInvoicingPostingOption option)
		{
			//add event only if it is Gateway
			if (ShouldAddBillingEvent(billingCode))
			{
				AddBillingEventCore(billingCode, option);
			}
		}

		protected virtual void AddBillingEventCore(string billingCode, JobInvoicingPostingOption option)
		{
			var firstJob = Jobs.Any() ? Jobs.First() : null;
			var parentPK = firstJob?.JH_ParentID ?? ZGuid.Empty;
			var parentTable = firstJob?.JH_ParentTableCode ?? ZString.Empty;

			if (billingCode == AccBillingCodes.GatewayBilling && !parentPK.IsEmpty && parentTable == JobConsolSchema.Constants.Prefix)
			{
				var eventCode = GetGSHBillingAuditEvent(option);
				AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, parentPK, parentTable, eventCode);
			}
		}

		protected string GetGSHBillingAuditEvent(JobInvoicingPostingOption option)
		{
			switch (option)
			{
				case JobInvoicingPostingOption.Revenue:
					return AccBillingEvents.Codes.RevenuePosted;
				case JobInvoicingPostingOption.Costs:
				case JobInvoicingPostingOption.ConsolCosts:
					return AccBillingEvents.Codes.CostPosted;
				case JobInvoicingPostingOption.All:
				case JobInvoicingPostingOption.LocalClient:
				case JobInvoicingPostingOption.Agent:
				case JobInvoicingPostingOption.Disbursement:
				case JobInvoicingPostingOption.CustomsDSBChargeAPOnly:
				case JobInvoicingPostingOption.CustomsDSBChargeAROnly:
				case JobInvoicingPostingOption.AllSisterCompanyCharges:
				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
				case JobInvoicingPostingOption.Gateway:
					//This event is used when user are not posting only cost or rev.
					return AccBillingEvents.Codes.GenericPostEvent;
				default:
					return string.Empty;
			}
		}

		protected virtual bool ShouldAddBillingEvent(string billingCode)
		{
			switch (billingCode)
			{
				case AccBillingCodes.GatewayBilling:
					return Jobs != null && Jobs.Any(j => j.IsEligibleForGSHBilling());
				default:
					return false;
			}
		}

		#endregion

		protected readonly IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider;
	}
}
