using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBaseValidation : InvoicingBaseCommonValidation
	{
		public InvoiceBaseValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		#region Public method for Calculated Properties

		public void ValidateExpectedInvoiceTotal()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ExpectedInvoiceTotalInfo);
		}

		public void ValidateValidateExpectedInvoiceTotal()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ValidateExpectedInvoiceTotalInfo);
		}

		public void ValidateExpectedInvoiceTaxTotal()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ExpectedInvoiceTaxTotalInfo);
		}

		public void ValidateExpectedInvoiceExclTaxTotal()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ExpectedInvoiceExclTaxTotalInfo);
		}

		#endregion

		protected override void CheckAH_GovernmentAllocatedID()
		{
			base.CheckAH_GovernmentAllocatedID();

			base.CheckAH_GovernmentAllocatedID_BasedOnRegistry(Parent);
		}

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			base.CheckAH_OA_InvoiceAddressOverride();

			ValidateAH_GovernmentAllocatedID();
		}

		protected override void CheckAH_OSTaxAmount()
		{
			base.CheckAH_OSTaxAmount();
			if (!Parent.IsForeignCurrencyInvoice)
			{
				CheckTaxCreditedExceedInvoiced();
			}
		}

		protected override void CheckAH_LocalTaxAmount()
		{
			base.CheckAH_LocalTaxAmount();
			if (Parent.IsForeignCurrencyInvoice)
			{
				CheckTaxCreditedExceedInvoiced();
			}
		}

		void CheckTaxCreditedExceedInvoiced()
		{
			IAmending amending = Parent as IAmending;
			if (amending != null && amending.IsAmendingTransaction)
			{
				if ((Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (Parent.AH_TransactionType == TransactionTypes.Invoice || Parent.AH_TransactionType == TransactionTypes.CreditNote))
				{
					if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable && Parent.AH_GSTAmount >= 0  // we are not interested if the amending transacton is to debit tax.
						|| Parent.AH_Ledger == LedgerTypes.AccountsPayable && Parent.AH_GSTAmount <= 0)
					{
						return;
					}
					else  // credit tax
					{
						bool isTaxCreditedExceedInvoiced = false;
						ZDecimal totalAmendingTransactionsTax = 0;
						var originalTransaction = amending.OriginalTransaction as InvoicingBase;
						if (originalTransaction == null)
						{
							return;
						}
						foreach (InvoicingBase transaction in originalTransaction.GetRelatedAmendingTransactions())
						{
							totalAmendingTransactionsTax += transaction.AH_GSTAmount;
						}
						var originalTransactionTax = originalTransaction.AH_GSTAmount;
						if (originalTransaction.AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							if (originalTransactionTax >= 0)
							{
								if (originalTransactionTax + totalAmendingTransactionsTax < 0)
								{
									isTaxCreditedExceedInvoiced = true;
								}
							}
							else if (totalAmendingTransactionsTax < 0)
							{
								isTaxCreditedExceedInvoiced = true;
							}
						}
						else if (originalTransaction.AH_Ledger == LedgerTypes.AccountsPayable)
						{
							if (originalTransactionTax <= 0)
							{
								if (originalTransactionTax + totalAmendingTransactionsTax > 0)
								{
									isTaxCreditedExceedInvoiced = true;
								}
							}
							else if (totalAmendingTransactionsTax > 0)
							{
								isTaxCreditedExceedInvoiced = true;
							}
						}

						if (isTaxCreditedExceedInvoiced)
						{
							if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable && AccountingConfigurationRegistry.Instance.AmendingTransactionTaxBehavior.Value
								|| Parent.AH_Ledger == LedgerTypes.AccountsPayable && AccountingConfigurationRegistry.Instance.AmendingTransactionTaxBehaviorForAP.Value)
							{
								string errorMessage = Parent.AH_Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("9f554a4d-34d3-46f3-8db3-7da615f67c58", "This amending transaction cannot be posted. Posting this transaction would result in the Receivables Organization being credited more tax than they have been invoiced in the related transaction/s. Please review the charges you are attempting to credit.")
													: Res.GetString("268B79CF-9BFF-4392-9FA1-5A2185668337", "This amending transaction cannot be posted. Posting this transaction would result in the Payables Organization being debited more tax than they have been invoiced in the related transaction/s. Please review the charges you are attempting to debit.");
								if (Parent.IsForeignCurrencyInvoice)
								{
									Parent.AH_LocalTaxAmountInfo.AddError(errorMessage);
								}
								else
								{
									Parent.AH_OSTaxAmountInfo.AddError(errorMessage);
								}
							}
							else
							{
								string warningMessage = Parent.AH_Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("78cf5acd-4abb-4632-9f7e-280dd2410262", "Posting this transaction will result in the Receivables Organization being credited more Tax than has been invoiced to your Receivables Organization in the related transaction/s. Do you want to continue posting?")
														: Res.GetString("1F21F2EE-3E01-4F6F-B941-040BAEC5AD13", "Posting this transaction will result in the Payables Organization being debited more Tax than has been invoiced to your Payables Organization in the related transaction/s. Do you want to continue posting?");
								if (Parent.IsForeignCurrencyInvoice)
								{
									Parent.AH_LocalTaxAmountInfo.AddWarning(warningMessage);
								}
								else
								{
									Parent.AH_OSTaxAmountInfo.AddWarning(warningMessage);
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckAH_OSTotalAmount_Implementation()
		{
			base.CheckAH_OSTotalAmount_Implementation();
			if (!Parent.IsForeignCurrencyInvoice)
			{
				CheckLocalTotalCreditedExceedInvoiced();
			}
		}

		protected override void CheckAH_LocalTotalAmount()
		{
			base.CheckAH_LocalTotalAmount();
			if (Parent.IsForeignCurrencyInvoice)
			{
				CheckLocalTotalCreditedExceedInvoiced();
			}
		}

		void CheckLocalTotalCreditedExceedInvoiced()
		{
			var amending = Parent as IAmending;
			if (amending != null && amending.IsAmendingTransaction)
			{
				if ((Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (Parent.AH_TransactionType == TransactionTypes.Invoice || Parent.AH_TransactionType == TransactionTypes.CreditNote))
				{
					if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable && Parent.AH_LocalTotal >= 0
						|| Parent.AH_Ledger == LedgerTypes.AccountsPayable && Parent.AH_LocalTotal <= 0)
					{
						return;
					}
					else
					{
						var isLocalTotalCreditedExceedInvoiced = false;
						var totalAmendingTransactionsLocalTotal = 0m;
						var originalTransaction = amending.OriginalTransaction as InvoicingBase;
						if (originalTransaction == null)
						{
							return;
						}
						foreach (InvoicingBase transaction in originalTransaction.GetRelatedAmendingTransactions())
						{
							totalAmendingTransactionsLocalTotal += transaction.AH_LocalTotal;
						}
						var originalTransactionLocalTotal = originalTransaction.AH_LocalTotal;
						if (originalTransaction.AH_Ledger == LedgerTypes.AccountsReceivable && (originalTransactionLocalTotal + totalAmendingTransactionsLocalTotal < 0)
							|| originalTransaction.AH_Ledger == LedgerTypes.AccountsPayable && (originalTransactionLocalTotal + totalAmendingTransactionsLocalTotal > 0))
						{
							isLocalTotalCreditedExceedInvoiced = true;
						}

						if (isLocalTotalCreditedExceedInvoiced)
						{
							if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable && AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.Value
								|| Parent.AH_Ledger == LedgerTypes.AccountsPayable && AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehaviorForAP.Value)
							{
								var errorMessage = Parent.AH_Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("174FE85B-BF42-4c7e-A2C2-EF8C9BDFD095", "This amending transaction cannot be posted. Posting this transaction would result in the Receivable Organization being credited more than they have been invoiced in the related transaction/s. Please review the charges you are attempting to credit.")
													: Res.GetString("C2F6D49F-54F5-4490-98B3-440338860CDF", "This amending transaction cannot be posted. Posting this transaction would result in the Payables Organization being debited more than they have been invoiced in the related transaction/s. Please review the charges you are attempting to debit.");
								if (Parent.IsForeignCurrencyInvoice)
								{
									Parent.AH_LocalTotalAmountInfo.AddError(errorMessage);
								}
								else
								{
									Parent.AH_OSTotalAmountInfo.AddError(errorMessage);
								}
							}
							else
							{
								var warningMessage = Parent.AH_Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("FC244D4C-EAB7-4253-9D29-E97C420242D6", "Posting this transaction will result in the Receivables Organization being credited more than they have been invoiced in the related transaction/s. Do you want to continue posting?")
													: Res.GetString("F99703A7-D1D2-4897-8EEE-B482BF6B15CB", "Posting this transaction will result in the Payables Organization being debited more than they have been invoiced in the related transaction/s. Do you want to continue posting?");
								if (Parent.IsForeignCurrencyInvoice)
								{
									Parent.AH_LocalTotalAmountInfo.AddWarning(warningMessage);
								}
								else
								{
									Parent.AH_OSTotalAmountInfo.AddWarning(warningMessage);
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon()
		{
			base.CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon();

			CheckAH_OSTotalAmount_Implementation();
		}

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();
			MandatoryValidation.CheckEntered(Parent.AH_DueDateInfo);
		}

		/// <remarks>Keep this filter in synch with the code in various methods of InvoiceLiteralNumberGenerator. If you change this logic you should check
		/// if those queries also need to change.</remarks>
		protected override void CheckAH_TransactionNum()
		{
			base.CheckAH_TransactionNum();

			if (Parent.AH_Ledger == LedgerTypes.AccountsPayable || Parent.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || Parent.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				if (!Parent.IsSelfBillingInvoice) // self billed costs have a separate number fountain (similar to AR) that's set when posting
				{
					MandatoryValidation.CheckEntered(Parent.AH_TransactionNumInfo);
					var jobNumbers = ZString.Empty;

					if (!Parent.AH_OH.IsEmpty && !Parent.AH_TransactionNum.IsEmpty)
					{
						var transactionType = Parent.AH_TransactionType;
						var previousTransactionNumberData = new AccountingUtils.PreviousSameNumberTransactionDetails();

						if (Parent.IsNewUnapprovedOrWithRequest)
						{
							transactionType = AccountingUtils.ConvertTransactionTypeFromINToUASafe(transactionType); //as for transaction number validation should behave as UA invoice
						}

						if ((previousTransactionNumberData = AccountingUtils.APTransactionNumberExists(transactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.AH_InvoiceDate)).HasNotification)
						{
							AccountingUtils.AddTransactionNumInfoNotification(previousTransactionNumberData, Res.GetString("88d0b0d5-bf27-46f7-a537-8268d0fc3a19", "The transaction number is already in use. Please select another one."), Parent);
						}
						else if ((previousTransactionNumberData = AccountingUtils.UATransactionNumberExists(transactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
						{
							AccountingUtils.AddTransactionNumInfoNotification(previousTransactionNumberData, Res.GetString("33fad7f3-95e9-41af-a60f-95d48839e42b", "The transaction number is already in use by Unapproved Invoice. Please select another one."), Parent);
						}
						else if ((previousTransactionNumberData = AccountingUtils.PATransactionNumberExists(transactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
						{
							AccountingUtils.AddTransactionNumInfoNotification(previousTransactionNumberData, Res.GetString("e6aaeb87-e315-4d9b-953e-7a6922f2a072", "The transaction number is already in use by Transaction Pending Allocation. Please select another one."), Parent);
						}
						else if ((previousTransactionNumberData = AccountingUtils.INTransactionNumberExists(transactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
						{
							AccountingUtils.AddTransactionNumInfoNotification(previousTransactionNumberData, Res.GetString("978e3658-4224-46e3-a638-90942bf47cb7", "The transaction number is already in use by Incomplete Transaction. Please select another one."), Parent);
						}
						else if (AccountingUtils.IsTransactionNumUsedInJobInvoicing(transactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, out jobNumbers))
						{
							Parent.AH_TransactionNumInfo.AddError(Res.GetString("f57fcfb4-15cd-4707-94a0-794779e6c395", "The transaction number is already in use on Job Invoicing of the following Job(s): {0}. Please select another one.", jobNumbers));
						}
					}
				}
			}
		}

		protected override void CheckAH_OHCore()
		{
			base.CheckAH_OHCore();

			RunCreditLimitChecking(Parent.AH_OHInfo);
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
			CheckAmendingTransactionAH_OH();
			CheckAgreedPaymentMethodOverrideAH_OH();
			CheckOrgsRegistrationNumberAH_OH();
			ValidateAH_TransactionNum();

			if (Parent.AH_OH.IsValid && !TransactionCreationRestrictionHelper.Instance.CheckOrgHeaderAllowsPosting(Parent, out ResourceString errorMessage, out bool isErrorMessage))
			{
				if (isErrorMessage)
				{
					Parent.AH_OHInfo.AddError(errorMessage);
				}
				else
				{
					Parent.AH_OHInfo.AddWarning(errorMessage);
				}
			}
		}

		void CheckOrgsRegistrationNumberAH_OH()
		{
			if (CountrySpecificValidationHelper.NeedToCheckCompanyAndOrgsRegistrationNumber())
			{
				if (Parent.Header != null && Parent.Header.PrimaryRegistrationNumber.Number.IsEmpty)
				{
					Parent.AH_OHInfo.AddWarning(Res.GetString("877D1EF6-601D-4334-8CFA-F5D32833C178", "Tax Registration Number is missing. This is mandatory for your country/region reporting."));
				}
			}
		}

		void CheckAgreedPaymentMethodOverrideAH_OH()
		{
			if (!Parent.AH_AgreedPaymentMethodOverride.IsEmpty && !Parent.DisplayAgreedPaymentMethodsList.ContainsCode(Parent.AH_AgreedPaymentMethodOverride))
			{
				Parent.AH_OHInfo.AddError(Res.GetString("cd42fb04-7d29-4ea0-a8c2-ace3deba7547", "This organization has an invalid agreed payment method '{0}'. Please check organization setup and the registry setting at Organization > AR/AP > Agreed Payment Method.", Parent.AH_AgreedPaymentMethodOverride));
			}
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			MandatoryValidation.CheckEntered(Parent.AH_RX_NKTransactionCurrencyInfo);
		}

		protected override void CheckAH_InvoiceTerm()
		{
			base.CheckAH_InvoiceTerm();
			ListValidation.ErrorIfInvalidCode(InvoiceTransaction.AH_InvoiceTermInfo, InvoiceTransaction.InvoiceTerms_List);
		}

		protected override void CheckAH_ComplianceSubType()
		{
			base.CheckAH_ComplianceSubType();

			if (!Parent.AH_ComplianceSubTypeInfo.HasErrors() &&
				Parent.IsInDatabase && Parent.AH_ComplianceSubTypeInfo.HasChanges)
			{
				var reports = Parent.GetFinalisedComplianceReports();
				if (reports.Any())
				{
					var reportTypes = new ZStringBuilder(reports.Select(x => x.ACR_ReportType.Trim()));
					Parent.AH_ComplianceSubTypeInfo.AddError(
						Res.GetString("ba51dd8a-40a6-4104-bbab-429581db839b", "Cannot be changed because transaction is already included in finalized Compliance Reports of the following types: {0}.",
							reportTypes.ToStringWithDelimiterBetweenAppends(", ")));
				}
			}

			if (Parent.IsComplianceNumberAllocationMandatory &&
				(Parent.HasContext(APInvoiceChargesApprovalRequest.Context.Editing) || Parent.AH_TransactionType == TransactionTypes.UACreditNote))
			{
				return;
			}

			if (Parent.IsBeingCreatedPostedAllocatedApprovedOrIncomplete
				&& Parent.DoNotValidateEmptyComplianceSubTypeSuspender.IsSuspended
				&& Parent.AH_ComplianceSubType.IsEmpty
				&& Parent.GetMatchingComplianceSubType().IsEmpty)
			{
				Parent.AH_ComplianceSubTypeInfo.AddError(ComplianceSequenceNumberAllocationErrorMessages.EmptyComplianceSubTypeExceptionMessage);
			}

			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty);
			if (complianceSubTypeValidation != null)
			{
				var errorMessageForComplianceSubTypeValidation = complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(Parent);
				if (!errorMessageForComplianceSubTypeValidation.IsEmpty)
				{
					Parent.AH_ComplianceSubTypeInfo.AddError(errorMessageForComplianceSubTypeValidation);
				}

				var warningMessageForComplianceSubTypeValidation = complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(Parent);
				if (!warningMessageForComplianceSubTypeValidation.IsEmpty)
				{
					Parent.AH_ComplianceSubTypeInfo.AddWarning(warningMessageForComplianceSubTypeValidation);
				}
			}

			if (!Parent.AH_ComplianceSubTypeInfo.HasErrors())
			{
				CheckIsComplianceSubTypeValueProtectedAndNotChanged();
			}
		}

		protected virtual void CheckExpectedInvoiceTotal()
		{
			var oSTotal = InvoiceTransaction.AH_OSTotalAmount;
			var expectedInvoiceTotal = InvoiceTransaction.ExpectedInvoiceTotal;
			var oSCurrencyDecimals = InvoiceTransaction.OSCurrencyDecimals;

			if (InvoiceTransaction.ValidateExpectedInvoiceTotal && InvoiceTransaction.Lines.Count > 0 &&
				oSTotal != expectedInvoiceTotal &&
				!InvoiceTransaction.IsSuspendingExpectedInvoiceTotalValidation)
			{
				InvoiceTransaction.ExpectedInvoiceTotalInfo.AddError(Res.GetString("d2860f92-ed5e-4ecd-b534-a44e2373857f", @"The invoice total of {0} {1} does not equal to the expected amount of {2} {1}.
The difference is {3} {1}.", oSTotal.ToString(oSCurrencyDecimals),
							InvoiceTransaction.AH_RX_NKTransactionCurrency,
							expectedInvoiceTotal.ToString(oSCurrencyDecimals),
							InvoiceTransaction.UnallocatedInvoiceTotal.ToString(oSCurrencyDecimals)));
			}

			if (InvoiceTransaction.ValidateExpectedInvoiceTotal && InvoiceTransaction.IsExpectedTaxTotalVisible
				&& (expectedInvoiceTotal != InvoiceTransaction.ExpectedInvoiceExclTaxTotal + InvoiceTransaction.ExpectedInvoiceTaxTotal))
			{
				InvoiceTransaction.ExpectedInvoiceTotalInfo.AddWarning(Res.GetString("e11388e3-0f4d-476b-a91b-af9b64531bac", "The Expected Total Incl. Tax should equal the Expected Total Tax plus the Expected Total Excl. Tax."));
			}
		}

		protected virtual void CheckExpectedInvoiceTaxTotal()
		{
			var oSTaxAmount = InvoiceTransaction.AH_OSTaxAmount;
			var expectedInvoiceTaxTotal = InvoiceTransaction.ExpectedInvoiceTaxTotal;
			var oSCurrencyDecimals = InvoiceTransaction.OSCurrencyDecimals;

			if (InvoiceTransaction.ValidateExpectedInvoiceTotal &&
				InvoiceTransaction.IsExpectedTaxTotalVisible &&
				InvoiceTransaction.Lines.Count > 0 &&
				oSTaxAmount != expectedInvoiceTaxTotal)
			{
				var message = Res.GetString("d20a5d75-94cd-41c5-bb0c-4ded24abf7ec", @"The total invoice tax amount of {0} {1} does not equal to the expected amount of {2} {1}.
The difference is {3} {1}.", oSTaxAmount.ToString(oSCurrencyDecimals),
							InvoiceTransaction.AH_RX_NKTransactionCurrency,
							expectedInvoiceTaxTotal.ToString(oSCurrencyDecimals),
							InvoiceTransaction.UnallocatedInvoiceTaxTotal.ToString(oSCurrencyDecimals));

				if (AccountingConfigurationRegistry.Instance.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Value && !InvoiceTransaction.IsSuspendingExpectedInvoiceTotalValidation)
				{
					InvoiceTransaction.ExpectedInvoiceTaxTotalInfo.AddError(message);
				}
				else
				{
					InvoiceTransaction.ExpectedInvoiceTaxTotalInfo.AddWarning(message);
				}
			}
		}

		protected virtual void CheckExpectedInvoiceExclTaxTotal()
		{
			var oSExTaxAmount = InvoiceTransaction.AH_OSExTaxAmount;
			var expectedInvoiceExclTaxTotal = InvoiceTransaction.ExpectedInvoiceExclTaxTotal;
			var oSCurrencyDecimals = InvoiceTransaction.OSCurrencyDecimals;

			if (InvoiceTransaction.ValidateExpectedInvoiceTotal &&
				InvoiceTransaction.IsExpectedTaxTotalVisible &&
				InvoiceTransaction.Lines.Count > 0 &&
				oSExTaxAmount != expectedInvoiceExclTaxTotal)
			{
				var message = Res.GetString("08cc190b-bb4a-42f4-ae13-19f123627517", @"The total invoice excluding tax amount of {0} {1} does not equal to the expected amount of {2} {1}.
The difference is {3} {1}.", oSExTaxAmount.ToString(oSCurrencyDecimals),
							InvoiceTransaction.AH_RX_NKTransactionCurrency,
							expectedInvoiceExclTaxTotal.ToString(oSCurrencyDecimals),
							InvoiceTransaction.UnallocatedInvoiceExclTaxTotal.ToString(oSCurrencyDecimals));

				if (AccountingConfigurationRegistry.Instance.EnforceExpectedTotalTaxAndExcludingTaxAmountValidation.Value && !InvoiceTransaction.IsSuspendingExpectedInvoiceTotalValidation)
				{
					InvoiceTransaction.ExpectedInvoiceExclTaxTotalInfo.AddError(message);
				}
				else
				{
					InvoiceTransaction.ExpectedInvoiceExclTaxTotalInfo.AddWarning(message);
				}
			}
		}

		protected virtual void CheckValidateExpectedInvoiceTotal()
		{
		}

		protected void CheckValidateExpectedInvoiceTotalCore()
		{
			ZPropertyInfo info = InvoiceTransaction.ValidateExpectedInvoiceTotalInfo;
			if (Parent.IsValidationOfValidateExpectedInvoiceTotalEnabled && (ZBool)info.Value != AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value)
			{
				if (!info.HasErrors())
				{
					info.AddError(Res.GetString("d6c33b37-0ba9-4500-b421-29b033ad993e", @"You do not have sufficient security rights to modify the 'Expected Total' tick box.
Please contact your system administrator for right to modify this field.
The location of this security right is as follows: {0}", SecurityItemPathOfAllowChangeExpectedTotalValue()));
				}
			}
			else
			{
				if (info.HasErrors())
				{
					info.ClearAllNotifications();
				}
			}
		}

		string SecurityItemPathOfAllowChangeExpectedTotalValue()
		{
			return Parent switch
			{
				APInvoice => Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.DisplayTextPathToSecurityRight,
				APCreditNote => Env.Security.AllowAPCreditNoteChangeDefaultExpectedTotalValue.DisplayTextPathToSecurityRight,
				APAdjustmentNote => Env.Security.AllowAPAdjustNoteChangeDefaultExpectedTotalValue.DisplayTextPathToSecurityRight,
				_ => string.Empty,
			};
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateValidateExpectedInvoiceTotal();
			ValidateExpectedInvoiceTotal();
			ValidateExpectedInvoiceExclTaxTotal();
			ValidateExpectedInvoiceTaxTotal();
			ValidateGSTInclusiveAmounts();
			ValidateIsSelfBillingInvoice();
			ValidateConsolCostsWithoutLines();
			ValidateAH_Calc_AmendStatusCode();
			ValidateReasonCode();
			ValidateReasonDescription();
			ValidateOriginalTransactionReference();
			ValidateLines();
		}

		public bool ValidateLines()
		{
			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Parent.Company.GC_RN_NKCountryCode) as IInstanceProvider<ITransactionLinesValidation>;

			ClearRowErrors(provider?.Get()?.GetErrorMessages());

			var error = provider?.Get()?.ValidateInvoiceLines(Parent.Lines.OfType<AccTransactionLines>());
			if (error != null)
			{
				Parent.AddRowError(error);
				return true;
			}

			return false;
		}

		public void ValidateReasonCode()
		{
			ValidateCalculatedProperty(Parent.ReasonCodeInfo);
		}

		public void ValidateReasonDescription()
		{
			ValidateCalculatedProperty(Parent.ReasonDescriptionInfo);
		}

		public void ValidateOriginalTransactionReference()
		{
			ValidateCalculatedProperty(Parent.OriginalTransactionReferenceInfo);
		}

		protected override void ValidateComplianceSequenceNotNullCore()
		{
			if ((Parent.ShouldAllocateComplianceNumberOnPosting() || Parent.IsIncompletInvoiceOrCreditNote)
				&& Parent.IsComplianceNumberAllocationMandatory
				&& Parent.AH_TransactionType != TransactionTypes.UACreditNote
				&& !Parent.HasContext(BusinessContext.InterCompanyInvoiceImport)
				&& !Parent.HasContext(BusinessContext.APBulkInvoicePoster)
				&& !Parent.ValidateEmptyComplianceSequenceSuspender.IsSuspended
				&& !Parent.HasContext(APInvoiceChargesApprovalRequest.Context.Editing))
			{
				var complianceSequence = Parent.ComplianceSequenceFromSubType;
				if (complianceSequence == null)
				{
					InvoiceTransaction.AddRowError(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
				}
			}
		}

		void ValidateConsolCostsWithoutLines()
		{
			var errorMessage = ResString.GetMultilingualString("DA8771DB-35F5-4EB3-A574-2B71953CD204", "There are consol costs without related transaction lines, please apportion all consol costs again.");
			InvoiceTransaction.RemoveRowError(errorMessage);

			if (InvoiceTransaction.ConsolCosting.ConsolCosts.Count == 0)
			{
				return;
			}

			var lineRelatedConsolCostPKs = InvoiceTransaction.Lines.Cast<InvoicingLineBase>().Where(x => !x.ImportedApportionmentID.IsEmpty).Select(x => x.ImportedApportionmentID).ToHashSet();
			foreach (ConsolCosting.JobConsolCost consolCost in InvoiceTransaction.ConsolCosting.ConsolCosts)
			{
				if (!lineRelatedConsolCostPKs.Contains(consolCost.PK))
				{
					InvoiceTransaction.AddRowError(errorMessage);
				}
			}
		}

		public void ValidateOrgDependantLineItems()
		{
			if (!ValidateAllInProgressSuspender.IsSuspended)
			{
				foreach (InvoicingLineBase line in InvoiceTransaction.Lines)
				{
					InvoicingLineBaseValidation lineValidation = line.Validation as InvoicingLineBaseValidation;
					if (lineValidation != null)
					{
						lineValidation.ValidateDependantLineItems();
					}
				}
			}
		}

		protected void CheckDependantInvoiceLines()
		{
			InvoiceTransaction.Lines.RunPreSaveValidation();
		}

		#region GSTInclusiveAmount

		public void ValidateGSTInclusiveAmounts()
		{
			ValidateCalculatedProperty(InvoiceTransaction.GSTInclusiveAmountsInfo);
		}

		protected virtual void CheckGSTInclusiveAmounts()
		{
			if (!ValidateAllInProgressSuspender.IsSuspended)
			{
				InvoicingLineBaseCollection lines = InvoiceTransaction.Lines;
				InvoicingLineBaseValidation validation;
				foreach (InvoicingLineBase line in lines)
				{
					validation = line.Validation as InvoicingLineBaseValidation;
					if (validation != null && line.GSTInclusiveAmountInfo.HasErrors())
					{
						validation.ValidateGSTInclusiveAmount();
					}
				}
			}
		}

		#endregion

		protected override void CheckAH_PostDateNotInPast()
		{
			if (!Parent.ShouldUseDefaultARInvoiceAndPostDate())
			{
				base.CheckAH_PostDateNotInPast();
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			var fieldInfo = Parent.AH_InvoiceDateInfo;

			if (Parent.ShouldUseDefaultARInvoiceAndPostDate())
			{
				fieldInfo.AddWarning(ARDefaultInvoiceAndPostDateCalculator.DefaultInvoiceDateReadOnlyWarningText);
			}
			ValidateAH_TransactionNum();
			CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(Parent.AH_InvoiceDateInfo);

			if (InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(Parent.AH_GC.ToGuid(), Parent.AH_InvoiceDateInfo, Parent.GetType()))
			{
				Parent.AH_InvoiceDateInfo.AddError(AccountingConstants.InvoiceDateIsInTheFutureErrorMessage);
			}

			if ((Parent.IsAPInvoiceOrCreditNoteOrAdjustmentNote || Parent.IsUAInvoiceOrCreditNote)
				&& Parent.IsPreventedInvoiceDateGreaterThanPostDate
				&& Parent.AH_InvoiceDate.Date > Parent.AH_PostDate.Date)
			{
				fieldInfo.AddError(ResString.GetMultilingualString("D1A22BFD-4901-4B10-A9FC-499B7D3B1CC3", @"Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date."));
			}

			if (Parent.AH_InvoiceDate.IsValid)
			{
				CheckForComplianceNumberAllocationDate(fieldInfo, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
			}

			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IInvoiceDateValidation>;
			var error = provider?.Get()?.ValidateInvoiceDate(Parent);
			if (error != null)
			{
				Parent.AH_InvoiceDateInfo.AddError(error);
			}

			if (!Parent.AH_InvoiceDateInfo.HasErrors()
				&& Parent.IsInDatabase
				&& !Parent.ShouldSetExchangeRateWhenSetInvoiceDate
				&& AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(Parent.GetExRateLedger(), Parent.IsLocalCurrencyTransaction, Parent.Company.PK) == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code)
			{
				Parent.AH_InvoiceDateInfo.AddWarning(Res.GetString("750645F5-F12B-45DE-A409-7EF74635E2D2", "Overriding the 'Invoice Date' will not result in the exchange rate being updated with reference to the 'AP Invoice Posting Exchange Rate Option' registry value."));
			}
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();
			var fieldInfo = Parent.AH_PostDateInfo;

			if (Parent.ShouldUseDefaultARInvoiceAndPostDate())
			{
				fieldInfo.AddWarning(ARDefaultInvoiceAndPostDateCalculator.DefaultPostDateReadOnlyWarningText);
			}

			if (Parent.AH_PostDate.IsValid)
			{
				CheckForComplianceNumberAllocationDate(fieldInfo, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
			}
		}

		void CheckForComplianceNumberAllocationDate(ZPropertyInfo dateInfo, string allocationDateCode)
		{
			if (!Parent.HasContext(BusinessContext.OverrideInvoiceReference) &&
				Parent.ComplianceNumberAllocationDateOption == allocationDateCode &&
				Parent.ShouldAllocateComplianceNumberOnPosting() && Parent.ComplianceSequenceFromSubType != null)
			{
				try
				{
					Parent.CheckAllocationDateEarlierThanLastDateUsed();
					InvoicingBase.EnsureNoPastTransactionsWithEmptyComplNum(Parent);
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					dateInfo.AddError(ex.UserFriendlyMessage);
				}
			}
		}

		public void ValidateAH_Calc_AmendStatusCode()
		{
			ValidateCalculatedProperty(Parent.AH_Calc_AmendStatusCodeInfo);
		}

		protected virtual void CheckAH_Calc_AmendStatusCode()
		{
			var amendStatusCodeValidationInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeValidationProvider>;
			var amendStatusCodeValidationProvider = amendStatusCodeValidationInstanceProvider?.Get();
			amendStatusCodeValidationProvider?.ValidateAmendStatusCode(Parent);
		}

		void ClearRowErrors(string[] errors)
		{
			if (errors != null)
			{
				foreach (var error in errors)
				{
					InvoiceTransaction.RemoveRowError(error);
				}
			}
		}

		#region Implementation

		protected void CheckAmendingTransactionAH_OH()
		{
			IAmending amending = Parent as IAmending;
			if (amending != null && amending.IsAmendingTransaction && amending.OriginalTransaction != null && Parent.AH_OH != amending.OriginalTransactionAccountPK)
			{
				Parent.AH_OHInfo.AddError(Res.GetString("005e03db-fbea-4cf9-ad5c-743a6b566a95", "This is an amending transaction and must have the same account as the original transaction."));
			}
		}

		protected void RunCreditLimitChecking(ZPropertyInfo info)
		{
			if (Parent.IsCreditLimitCheckApplicable)
			{
				if (Parent.Header != null && ((Parent.Header.CompanyData.OB_IsDebtor && Parent.AH_Ledger.Equals(LedgerTypes.AccountsReceivable))))
				{
					IAmending amending = Parent as IAmending;
					if (amending == null || !amending.IsAmendingTransaction)
					{
						Parent.Header.CreditChecker.ValidateIsCreditOnHold(info, Parent.AH_Ledger);
					}

					Parent.Header.CreditChecker.ValidateIsCreditLimitExceeded(info, Parent.AH_Ledger, Parent.AH_OutstandingAmount);
				}
			}
		}

		#endregion
	}
}
