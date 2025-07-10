using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalValidation : AccPaymentApprovalValidation
	{
		public PaymentApprovalValidation(PaymentApprovalBase parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCreditCardSecurityCode();
			ValidateMatchStatus();
			ValidateMatchStatusReasonCode();
		}

		public void ValidateMatchStatus()
		{
			ValidateCalculatedProperty(Parent.MatchStatusInfo);
		}

		public void ValidateMatchStatusReasonCode()
		{
			ValidateCalculatedProperty(Parent.MatchStatusReasonCodeInfo);
		}

		protected override void CheckAV_AB_FundingBankAccount()
		{
			base.CheckAV_AB_FundingBankAccount();

			if (Parent.IsEPayment && Parent.IsInDatabase)
			{
				if (Parent.FundingCurrency != Parent.OriginalFundingCurrency)
				{
					CheckActiveDealExist(Parent.AV_AB_FundingBankAccountInfo);
				}
			}
		}

		protected override void CheckAV_EPaymentReasonCode()
		{
			base.CheckAV_EPaymentReasonCode();
			if (Parent.IsEPayment)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AV_EPaymentReasonCodeInfo, Parent.Lookups.PaymentReasons);
				if (!Parent.AV_EPaymentReasonCodeInfo.HasErrors())
				{
					if (Parent.AV_EPaymentReasonCode.IsEmpty && EPaymentProviderCodes.Codes.PaymentReasonMandatoryProviders.Contains<string>(Parent.EPaymentProvider))
					{
						Parent.AV_EPaymentReasonCodeInfo.AddError(Res.GetString("a0944f8a-52bf-4c85-942c-8bb2cf571753", "Specifying a Payment Reason is mandatory requirement of your FX provider. Please select a reason from the drop down list of accepted Payment Reasons."));
					}
				}
			}
		}

		protected override void CheckAV_RX_NKPaymentCurrency()
		{
			base.CheckAV_RX_NKPaymentCurrency();

			if (Parent.BankAccount != null)
			{
				ZString bankAccountCurrency = Parent.BankAccount.AB_RX_NKAccountCurrency;
				if (bankAccountCurrency != Parent.AV_RX_NKPaymentCurrency && bankAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					Parent.AV_RX_NKPaymentCurrencyInfo.AddError(Res.GetString("ea789a50-33d4-4ee7-abf1-1ebcb1eca65b", "The payment currency must match the bank's currency"));
				}
			}

			if (Parent.UseExchangeRateFromENettWebService)
			{
				Parent.AV_RX_NKPaymentCurrencyInfo.AddWarning(Res.GetString("f5b69eee-3694-414c-bf81-e12eebf1a5df", "The exchange rate is shown for your reference only. You might be prompted with an updated exchange rate during posting."));
			}

			CheckActiveDealExist(Parent.AV_RX_NKPaymentCurrencyInfo);
		}

		protected override void CheckAV_PaymentDate()
		{
			base.CheckAV_PaymentDate();
			MandatoryValidation.CheckEntered(Parent.AV_PaymentDateInfo);
			if (Parent.IsEditOrViewPaymentBatch && Parent.PaymentBatch.APB_PaymentDate != Parent.AV_PaymentDate)
			{
				Parent.AV_PaymentDateInfo.AddError(AccountingConstants.GetIsDiscrepancyWithPaymentBatchErrorMessage(Parent.AV_PaymentDateInfo.Description));
			}
		}

		protected override void CheckAV_PostDate()
		{
			base.CheckAV_PostDate();

			MandatoryValidation.CheckEntered(Parent.AV_PostDateInfo);
			PeriodValidation.CheckDateFallsIntoValidPeriod(Parent.AV_PostDateInfo);
			CheckAV_PostDateNotInPast();
			CheckAV_PostDateNotInFuture();
		}

		protected void CheckAV_PostDateNotInFuture()
		{
			if (!Parent.AV_PostDateInfo.HasErrors())
			{
				if (Parent.AV_PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						Parent.AV_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						Parent.AV_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		protected virtual void CheckAV_PostDateNotInPast()
		{
			if (!Parent.AV_PostDateInfo.HasErrors() && Parent.AV_PostDate.Date < ZDateTime.Today)
			{
				if (!Parent.AllowBackPosting)
				{
					Parent.AV_PostDateInfo.AddWarning(PaymentApprovalBase.UpdatePostDateWarning);
				}
				else
				{
					if (Parent.IsPosted)
					{
						Parent.AV_PostDateInfo.AddWarning(PreviousPostDateWarning);
					}
				}
			}
		}

		public static ZString PreviousPostDateWarning
		{
			get
			{
				return Res.GetString("f887d456-146e-412a-b274-385e6a2c8d45", "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
			}
		}

		protected override void CheckAV_OHIsNotEmpty()
		{
			MandatoryValidation.CheckEntered(Parent.AV_OHInfo, Res.GetString("169f8991-db6d-4886-9acf-9bfe9ff3a737", "Organization"));
		}

		protected override void CheckAV_OH()
		{
			base.CheckAV_OH();
			ListValidation.ErrorIfInvalidPK(Parent.AV_OHInfo);
			ValidateOrganisationDDRDetail();
			if (Parent.AV_OH.IsValid && Parent.AV_PaymentType == ReceiptTypes.eNettCreditCard
				&& Parent.Header.ENettRegistrationNumber.IsEmpty)
			{
				Parent.AV_OHInfo.AddError(AccountingConstants.ENettErrorMessages.OrganisationNotRegisteredForENett);
			}

			if (!Parent.AV_OA_AddressOverride.IsValid)
			{
				Parent.AV_OHInfo.AddError(Res.GetString("3b212ba1-ef06-4787-9894-a54c1c8c27dc", "Enter a valid Payment Address"));
			}

			if (Parent.AV_OH.IsValid && !TransactionCreationRestrictionHelper.Instance.AllowToCreatePaymentApproval(Parent, out ResourceString errorMessage))
			{
				Parent.AV_OHInfo.AddError(errorMessage);
			}

			CheckActiveDealExist(Parent.AV_OHInfo);
		}

		protected override void CheckAV_OA_AddressOverride()
		{
			base.CheckAV_OA_AddressOverride();
			ValidateAV_OH();
		}

		protected override void CheckAV_PaymentType()
		{
			base.CheckAV_PaymentType();

			MandatoryValidation.CheckEntered(Parent.AV_PaymentTypeInfo, Res.GetString("b651a065-5eba-4612-9d90-eea0d0642ef5", "Payment Type"));
			ListValidation.ErrorIfInvalidCode(Parent.AV_PaymentTypeInfo, Parent.Lookups.PaymentMethods);

			if (Parent.IsEditOrViewPaymentBatch && Parent.PaymentBatch.APB_PaymentType != Parent.AV_PaymentType)
			{
				Parent.AV_PaymentTypeInfo.AddError(AccountingConstants.GetIsDiscrepancyWithPaymentBatchErrorMessage(Parent.AV_PaymentTypeInfo.Description));
			}

			if (Parent.AV_PaymentType == ReceiptTypes.eNettDirectDebit)
			{
				if (Parent.Header != null &&
					!(eNettHelper.IsOrganisationeNettRegistered(Parent.Header) ||
						eNettHelper.DoesOrgHaveeNettDDRAccount(Parent.Header)))
				{
					Parent.AV_PaymentTypeInfo.AddError(eNettHelper.NoEnettBankInformationOnOrganisationError);
				}
				if (Parent.BankAccount != null && !eNettHelper.IsBankAccountEnettRegistered(Parent.BankAccount))
				{
					Parent.AV_PaymentTypeInfo.AddError(eNettHelper.BankAccountNotEnettRegistered);
				}
				if (Parent.Header != null &&
					eNettHelper.DoesOrgHaveeNettDDRAccount(Parent.Header) &&
						!eNettHelper.IsOrganisationeNettRegistered(Parent.Header))
				{
					Parent.AV_PaymentTypeInfo.AddWarning(eNettHelper.PayAnyoneWarning);
				}
			}

			if (Parent.AV_PaymentType == ReceiptTypes.eNettCreditCard && !AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value)
			{
				Parent.AV_PaymentTypeInfo.AddError(Res.GetString("eaa0ed2d-c22f-4a27-b1bf-cd9ccc655844", "'Pay via ComPay Credit Card' payment type is not enabled."));
			}

			if (Parent.IsCashAccountType && !Parent.IsCash)
			{
				Parent.AV_PaymentTypeInfo.AddError(TransactionHeaderValidation.GetCashAccountTypeErrorMessage(Parent.AV_PaymentTypeInfo.HumanReadableName));
			}

			if (!Parent.AV_PaymentTypeInfo.HasErrors())
			{
				CheckAV_PaymentTypeSecurity();
			}

			ValidateAV_AK();
			ValidateAV_OH();
		}

		void CheckAV_PaymentTypeSecurity()
		{
			if (!Parent.UserHasPaymentTypeSecurityToPost())
			{
				Parent.AV_PaymentTypeInfo.AddError(Res.GetString("23bb0824-1917-48be-917c-45b36affddae", "You do not have appropriate security rights to select this payment type."));
			}
		}

		protected override void CheckAV_AB()
		{
			base.CheckAV_AB();
			MandatoryValidation.CheckEntered(Parent.AV_ABInfo);

			if (Parent.AV_PaymentType == ReceiptTypes.eNettCreditCard)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AV_ABInfo, Parent.Lookups.CreditCardBankAccounts);
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.AV_ABInfo, Parent.Lookups.BankAccounts);
			}

			if (Parent.IsEditOrViewPaymentBatch && Parent.PaymentBatch.APB_AB != Parent.AV_AB)
			{
				Parent.AV_ABInfo.AddError(AccountingConstants.GetIsDiscrepancyWithPaymentBatchErrorMessage(Parent.AV_ABInfo.Description));
			}

			if (Parent.AV_PaymentType == ReceiptTypes.eNettDirectDebit &&
				!eNettHelper.IsBankAccountEnettRegistered(Parent.BankAccount))
			{
				Parent.AV_ABInfo.AddError(eNettHelper.BankAccountNotEnettRegistered);
			}

			if (Parent.BankAccount != null)
			{
				if (Parent.BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.AV_PaymentType != ReceiptTypes.EPayment)
				{
					Parent.AV_ABInfo.AddError(Res.GetString("26e38ea4-d284-4f33-885c-e473956ab25c", "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment."));
				}
				else if (Parent.BankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.AV_PaymentType == ReceiptTypes.EPayment)
				{
					Parent.AV_ABInfo.AddError(Res.GetString("02794c08-f6b7-4bdf-8bf1-877b495c333e", "Bank Account is not an E-Payment Account."));
				}
			}

			CheckActiveDealExist(Parent.AV_ABInfo);
			ValidateAV_OH();
		}
		
		bool IsMatchingForeignCurrencyENettPayment
		{
			get
			{
				return Parent.AV_PaymentType == ReceiptTypes.eNettDirectDebit
					   && Parent.AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		protected override void CheckAV_Amount()
		{
			base.CheckAV_Amount();
			if (Parent.IsProcessingPaymentDetail && Parent.AV_Amount < 0)
			{
				Parent.AV_AmountInfo.AddError(Res.GetString("9c16794b-fefe-478e-8c60-8e58e9260e70", "Overseas amount must be greater than or equal to zero"));
			}
			else if (!Parent.IsProcessingPaymentDetail && Parent.AV_Amount <= 0)
			{
				Parent.AV_AmountInfo.AddError(Res.GetString("fcd73222-de56-46d5-aa3f-6ed3d61c53f9", "Overseas amount must be greater than zero"));
			}

			if (!Parent.AV_AmountInfo.HasErrors() && IsMatchingForeignCurrencyENettPayment && !Parent.IsPostWithoutMatching
				&& Parent.HasPaymentMatchingBaseObjectBeenCreated && Parent.MatchingBaseObject.MatchedTransactions.Count > 1)
			{
				ZDecimal totalInvoicesOSPartialPaymentAmount = 0m;

				foreach (IMatching transaction in Parent.MatchingBaseObject.MatchedTransactions)
				{
					if (transaction is InvoicingBase)
					{
						InvoicingBase invoice = transaction as InvoicingBase;

						if (invoice.ContainsMatchedLinesInSpecificCurrency(Parent.AV_RX_NKPaymentCurrency))
						{
							totalInvoicesOSPartialPaymentAmount += -invoice.GetOSAmountOfMatchedLinesWithSpecificCurrency(Parent.AV_RX_NKPaymentCurrency);
						}
						else
						{
							totalInvoicesOSPartialPaymentAmount += -invoice.GetAmountOfLinesWithSpecificCurrency(Parent.AV_RX_NKPaymentCurrency);
						}
					}
				}

				if (Parent.AV_Amount > totalInvoicesOSPartialPaymentAmount)
				{
					Parent.AV_AmountInfo.AddError(Res.GetString("7eea1edf-c680-4889-a0e9-b57d3243178a", "When matching a ComPay payment, you can only pay invoices up to a total of the lines that match the payment currency."));
				}
			}

			CheckActiveDealExist(Parent.AV_AmountInfo);
		}

		protected override void CheckAV_AK()
		{
			base.CheckAV_AK();

			if (Parent.IsEditOrViewPaymentBatch && Parent.PaymentBatch.APB_AK != Parent.AV_AK)
			{
				Parent.AV_AKInfo.AddError(AccountingConstants.GetIsDiscrepancyWithPaymentBatchErrorMessage(Parent.AV_AKInfo.Description));
			}

			if (Parent.IsCheque)
			{
				if (!Parent.IsValidToSaveAsDraft)
				{
					MandatoryValidation.CheckEntered(Parent.AV_AKInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.AV_AKInfo);
			}

			if (Parent.ChequeBook != null)
			{
				if (Parent.ChequeBook.AK_AB != Parent.AV_AB)
				{
					Parent.AV_AKInfo.AddError(Res.GetString("3f7de544-a824-4b18-a20b-cff0b16c8877", "This check book does not belong to the bank specified"));
				}
				else
				{
					ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(Parent.ChequeBook, ((IChequeNumberAutoAllocation)Parent).IsAutoAllocationEnabled);
					if (!errorMessage.IsEmpty)
					{
						Parent.AV_AKInfo.AddError(errorMessage);
					}
				}
				Parent.ChequeBook.AddWarningSamePrinter(Parent.AV_AKInfo);

				if (Parent.ChequeBook.AK_AutoPrintCheque)
				{
					if (Parent.ChequeBook.BankAccount.AB_SO_ChequeTemplate.IsEmpty)
					{
						Parent.AV_AKInfo.AddError(Res.GetString("D0DA89C5-673E-4304-91DC-73FC16043ABB", "Auto printing of check is not configured properly."));
					}
					if (!Env.Security.PrintCheque.IsAllowed)
					{
						Parent.AV_AKInfo.AddError(Res.GetString("A2ABE6ED-8946-4569-96D9-1F6C03978520", "You do not have the permission to print Check. Please Contact System Administrator."));
					}
				}
			}
		}

		protected override void CheckAV_Status()
		{
			base.CheckAV_Status();

			if (Parent.IsEditOrViewPaymentBatch && Parent.IsDiscrepancyWithPaymentBatch && Parent.IsCancelledOrIsPosted)
			{
				Parent.AV_StatusInfo.AddError(AccountingConstants.PaymentApprovalIsPostedOrCancelledErrorMessage);
			}
		}

		protected override void CheckAV_ChequeOrReferenceIsWesternEuropean()
		{
			if (Parent.IsValidToSaveAsDraft)
			{
				return;
			}

			base.CheckAV_ChequeOrReferenceIsWesternEuropean();
		}

		protected override void CheckAV_ChequeOrReference()
		{
			if (Parent.IsValidToSaveAsDraft)
			{
				return;
			}

			base.CheckAV_ChequeOrReference();

			if (!((IChequeNumberAutoAllocation)Parent).IsAutoAllocationEnabled)
			{
				if (!Parent.AV_ChequeOrReference.IsEmpty)
				{
					ZString chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(Parent.IsCheque, Parent.AV_ChequeOrReference);
					if (chequeNumberErrorMessage.IsEmpty)
					{
						chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(Parent.ChequeBook, Parent.AV_ChequeOrReference);
					}
					if (!chequeNumberErrorMessage.IsEmpty)
					{
						Parent.AV_ChequeOrReferenceInfo.AddError(chequeNumberErrorMessage);
					}
					else if (Parent.IsCheque)
					{
						if (Parent.ChequeBook != null)
						{
							if (Parent.ImportedHotCheque == null && ChequeNumberHasBeenUsed)
							{
								Parent.AV_ChequeOrReferenceInfo.AddError(ChequeOrReferenceValidationHelper.GetInUseErrorMessage(Parent.AV_ChequeOrReference, Parent.ChequeBook.BankAccount, Parent.Factory));
							}
							else if (Parent.BankAccount != null)
							{
								ValidationHelper.ValidateChequeDigits(Parent.AV_ChequeOrReferenceInfo, Parent.BankAccount.AB_ChequeNumDigits);
							}
						}
					}
				}

				if (!Parent.IsCheque || Parent.PostsOnSave)
				{
					MandatoryValidation.CheckEntered(Parent.AV_ChequeOrReferenceInfo);
				}
			}
		}

		protected override void CheckAV_OC_ContactOverride()
		{
			if (Parent.IsValidToSaveAsDraft)
			{
				return;
			}

			base.CheckAV_OC_ContactOverride();
		}

		protected override void CheckAV_OC_ContactOverrideIsValidZGuid()
		{
			if (Parent.IsValidToSaveAsDraft)
			{
				return;
			}

			base.CheckAV_OC_ContactOverrideIsValidZGuid();
		}

		protected override void CheckAV_PaymentComment()
		{
			base.CheckAV_PaymentComment();

			if (Parent.AV_PaymentComment.IsEmpty)
			{
				Parent.AV_PaymentCommentInfo.AddError(Res.GetString("800cde9f-ef41-48dc-84fd-aace19a2ce46", "You must enter a description"));
			}
		}

		protected override void CheckAV_GB()
		{
			base.CheckAV_GB();
			var department = Parent.Factory.Load<GlbDepartment>(Env.CurrentDepartment.PK);
			GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.AV_GBInfo, Parent.Branch, department);
		}

		bool ChequeNumberHasBeenUsed
		{
			get
			{
				if (Parent.ChequeBook != null && Parent.ChequeBook.BankAccount != null)
				{
					return ChequeNumberHasBeenUsedOnAPaymentApproval ||
							ChequeNumberHasBeenUsedOnAJobCharge ||
							ChequeNumberHasBeenUsedOnAPayment;
				}

				return false;
			}
		}

		bool ChequeNumberHasBeenUsedOnAJobCharge
		{
			get
			{
				PaymentApprovalItemCollection collection = new PaymentApprovalItemCollection(Parent);
				var paymentApprovalItems = Parent.Factory.Load<PaymentApprovalItem>(collection.CompleteFilter);

				var transactionHeaderFilter = new ZQuery();
				transactionHeaderFilter.FetchOnlyFromLocalCache = true;
				transactionHeaderFilter.AddToFilter(AccTransactionHeaderSchema.PK, paymentApprovalItems.Select(x => x.A2_AH));
				var transactionHeaders = Parent.Factory.Load<TransactionHeader>(transactionHeaderFilter);
				var linePKsNotInDB =
					from transaction in transactionHeaders
					let invoice = transaction as APInvoice
					where invoice != null && !invoice.IsInDatabase
					select invoice.Lines.Select(x => x.PK);

				var jobChargeFilter = new ZQuery();
				jobChargeFilter.FetchOnlyFromLocalCache = true;
				jobChargeFilter.AddToFilter(JobChargeSchema.JR_AL_APLine, linePKsNotInDB.SelectMany(x => x));
				JobCharge[] charges = Parent.Factory.Load<JobCharge>(jobChargeFilter);

				var chargePKs = charges.Select(x => x.PK).Distinct();

				string sqlTextTemplate = @"
								SELECT NULL AS PlaceHolderColumn
								WHERE 
									EXISTS
									(
										SELECT
											JR_PK
										FROM
											dbo.JobCharge
										WHERE
											JR_ChequeNo = @CheckNo
											{1}
											AND JR_PaymentType = @PaymentType
											AND JR_AB = @AB_PK
											{0}
										EXCEPT
											SELECT 
												JobCharge.JR_PK
											FROM
												dbo.AccPaymentApprovalItem
												INNER JOIN 
												dbo.AccTransactionHeader ON AccPaymentApprovalItem.A2_AH = AccTransactionHeader.AH_PK AND AccTransactionHeader.AH_Ledger = @LedgerType AND AccTransactionHeader.AH_TransactionType = @TransactionType
												INNER JOIN
												dbo.AccTransactionLines ON AccTransactionLines.AL_AH = AccTransactionHeader.AH_PK
												INNER JOIN
												dbo.JobCharge ON JobCharge.JR_AL_APLine = AccTransactionLines.AL_PK
											WHERE 
												AccPaymentApprovalItem.A2_AV = @A2_AV
									) OPTION (recompile)";

				string sqlText = string.Format(CultureInfo.InvariantCulture, sqlTextTemplate,
					chargePKs.Any() ? string.Format("AND JR_PK NOT IN ({0})", AccountingUtils.GetCommaSeparatedGuidsForInClause(chargePKs)) : "",
					!Parent.AV_ChequeOrReference.IsEmpty ? "AND JR_ChequeNo <> ''" : "");

				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@CheckNo", Parent.AV_ChequeOrReference, JobChargeSchema.JR_ChequeNo);
				parameters.Add("@PaymentType", ReceiptTypes.Cheque, JobChargeSchema.JR_PaymentType);
				parameters.Add("@AB_PK", Parent.AV_AB, JobChargeSchema.JR_AB);
				parameters.Add("@A2_AV", Parent.PK, AccPaymentApprovalItemSchema.A2_AV);
				parameters.Add("@LedgerType", LedgerTypes.AccountsPayable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@TransactionType", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);

				DynamicBusinessObjectCollection query = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
				query.Load(sqlText, parameters);

				return query.Count > 0;
			}
		}

		bool ChequeNumberHasBeenUsedOnAPaymentApproval
		{
			get
			{
				return Parent.ChequeBook.BankAccount.HasChequeNumberBeenUsedOnAPaymentApproval(Parent.AV_ChequeOrReference, Parent.PK, Parent.AV_AH);
			}
		}

		bool ChequeNumberHasBeenUsedOnAPayment
		{
			get
			{
				return Parent.ChequeBook.BankAccount.HasChequeNumberBeenUsedOnAPayment(Parent.AV_ChequeOrReference, Parent.AV_AH);
			}
		}

		protected override void CheckAV_PayExRate()
		{
			base.CheckAV_PayExRate();
			MandatoryValidation.CheckNotNegative(Parent.AV_PayExRateInfo);
			MandatoryValidation.CheckNotZero(Parent.AV_PayExRateInfo);

			CheckActiveDealExist(Parent.AV_PayExRateInfo);
		}

		void CheckActiveDealExist(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.HasChanges && Parent.HasActiveDeal)
			{
				propertyInfo.AddError(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}
		}

		public void ValidateOSPartialPaymentAmount()
		{
			//A.S: 
			//It should not be called in ValidateAll as this field in for Matching form only. 
			//Other IMatching implementations use MatchingValidation implementation and it is also not in ValidateAll. 

			if (Parent is IMatching)
			{
				ValidateCalculatedProperty(((IMatching)Parent).OSPartialPaymentAmountInfo);
			}
		}

		protected virtual void CheckOSPartialPaymentAmount()
		{
			IMatching thisIMatching = Parent;
			if (thisIMatching != null)
			{
				if (Math.Abs(thisIMatching.OSOutstandingAmount) < Math.Abs(thisIMatching.OSPartialPaymentAmount) ||
					Math.Sign(thisIMatching.OSOutstandingAmount) != Math.Sign(thisIMatching.OSPartialPaymentAmount))
				{
					if (thisIMatching.OSOutstandingAmount > 0)
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("eaa34ac2-9f62-4d5c-89c9-d2c85d8d4f80", "Pay Amount must be between 0 and {0}", thisIMatching.OSOutstandingAmount));
					}
					else
					{
						thisIMatching.OSPartialPaymentAmountInfo.AddError(Res.GetString("c3f9a4fc-2f0d-44f9-82ec-6cfc6ca82c26", "Pay Amount must be between {0} and 0", thisIMatching.OSOutstandingAmount));
					}
				}
			}
		}

		public void ValidateCreditCardSecurityCode()
		{
			ValidateCalculatedProperty(Parent.CreditCardSecurityCodeInfo);
		}

		protected virtual void CheckCreditCardSecurityCode()
		{
			if ((Parent.AV_PaymentType == ReceiptTypes.eNettCreditCard) && ((Parent.CreditCardSecurityCode.Length < 3) || (Parent.CreditCardSecurityCode.Length > 4)))
			{
				Parent.CreditCardSecurityCodeInfo.AddError(Res.GetString("ad0ae9e7-84e0-4aa3-a2bb-0118b420fafe", "Card Security Code must be 3 or 4 digits in length."));
			}
		}

		ZBool ShouldCheckMatchStatusAndReasonCode => !Parent.IsPostWithoutMatching && Parent.HasPaymentMatchingBaseObjectBeenCreated;

		protected virtual void CheckMatchStatus()
		{
			if (ShouldCheckMatchStatusAndReasonCode)
			{
				ListValidation.ErrorIfInvalidCode(Parent.MatchStatusInfo);
			}
		}

		protected virtual void CheckMatchStatusReasonCode()
		{
			if (ShouldCheckMatchStatusAndReasonCode)
			{
				var matchStatusReasonCodeInfo = Parent.MatchStatusReasonCodeInfo;
				ListValidation.ErrorIfInvalidCode(matchStatusReasonCodeInfo);

				if (!matchStatusReasonCodeInfo.HasErrors())
				{
					if (!Parent.MatchStatus.IsEmpty)
					{
						MandatoryValidation.CheckEntered(matchStatusReasonCodeInfo);
					}
					else if (!Parent.MatchStatusReasonCode.IsEmpty)
					{
						matchStatusReasonCodeInfo.AddError(AccountingMatchStatusReasonCodeErrorMessage.MatchStatusReasonCodeShouldNotSpecified);
					}
				}
			}
		}

		#region Implementation

		protected new readonly PaymentApprovalBase Parent;

		AccChequeBookAutoAllocationValidation AutoAllocationValidation
		{
			get
			{
				if (fAutoAllocationValidation == null)
				{
					fAutoAllocationValidation = new AccChequeBookAutoAllocationValidation();
				}
				return fAutoAllocationValidation;
			}
		}
		AccChequeBookAutoAllocationValidation fAutoAllocationValidation;

		void ValidateOrganisationDDRDetail()
		{
			if (Parent.IsDirectDebit)
			{
				if (Parent.BankAccount != null && Parent.BankAccount.AB_AllowAutoDDR)
				{
					if (Parent.Header != null)
					{
						GetOrganisationDDRWarning();
					}
				}
			}
		}

		void GetOrganisationDDRWarning()
		{
			var warningMessage = new StringBuilder();
			Parent.ResetAccountDetails();

			if (Parent.AccountDetailsFound)
			{
				if (!Parent.AllowAutoDDR)
				{
					warningMessage.Append("   ").Append(Res.GetString("4b5c5b8d-8e4f-4c3c-9abb-bd9281d4fde2", "- Auto Direct Debit")).Append(System.Environment.NewLine);
				}

				if (Parent.PayeeBankBSB.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("2f945759-4269-4ec3-9241-440ef70c0ec9", "- BSB Number")).Append(System.Environment.NewLine);
				}
				else
				{
					ValidateBSBFormat(warningMessage);
				}

				if (Parent.AccountTitle.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("66b79c7d-98fa-4737-9cdc-c04bb92a55cd", "- Account Name")).Append(System.Environment.NewLine);
				}

				if (Parent.PayeeBankAccountNumber.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("53c1f43b-83a6-4d38-b818-52a328ecc37d", "- Bank Account")).Append(System.Environment.NewLine);
				}
				else
				{
					ValidateBankAccountNumberFormat(warningMessage);
				}

				if (warningMessage.Length > 0)
				{
					warningMessage.Insert(0, System.Environment.NewLine);
					warningMessage.Insert(0, Res.GetString("2b8ea94a-c4c5-4703-8c38-1b7d0472c49d", "Organization {0} has following Banking Details incorrectly setup.", Parent.Header.OH_Code));
					warningMessage.Append(Res.GetString("c671db91-e3ba-4a9b-aaf6-768ce6902e49", "You'll need to correct this before you can generate DDR file."));

					Parent.AV_OHInfo.AddWarning(warningMessage.ToString());
				}
			}
			else
			{
				ZString currencyCode = Parent.PaymentCurrency != null ? Parent.AV_RX_NKPaymentCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				ZString errorMessage = Res.GetString("9c356358-609e-4c40-8756-c41c0392c588", "An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", currencyCode, Parent.Header.OH_Code);
				Parent.AV_OHInfo.AddError(errorMessage);
			}
		}

		void ValidateBSBFormat(StringBuilder warningMessage)
		{
			if (!Parent.BankAccount.IsValidBSBNumber(Parent.PayeeBankBSB))
			{
				if (Parent.BankAccount.AB_AutoDDRFormat != Constants.DDRFileFormat.ASB && Parent.BankAccount.AB_AutoDDRFormat != Constants.DDRFileFormat.BCS && !(Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.NewZealand))
				{
					warningMessage.Append("   ").Append(Res.GetString("0e75bb01-79d4-4c1f-8f0c-c4de430128a3", "- BSB Number is in incorrect format. It should be in XXX-XXX format")).Append(System.Environment.NewLine);
				}
				else
				{
					warningMessage.Append("   ").Append(Res.GetString("12bfd087-c128-4765-bf59-fd9b76fe09f3", "- BSB Number is in incorrect format. It should be in XXXXXX format")).Append(System.Environment.NewLine);
				}
			}
		}

		void ValidateBankAccountNumberFormat(StringBuilder warningMessage)
		{
			if (!Parent.BankAccount.IsValidAccountNumber(Parent.PayeeBankAccountNumber))
			{
				if (Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.BCS)
				{
					warningMessage.Append("   ").Append(Res.GetString("52f7d175-d70b-4381-ba76-a016f0d35c99", "- Bank Account Number is in incorrect format. It should be in XXXXXXXX format")).Append(System.Environment.NewLine);
				}
			}
		}

		public AccValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccValidationHelper();
				}

				return fValidationHelper;
			}
		}
		AccValidationHelper fValidationHelper;

		public PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = new PeriodValidationProvider(Parent.Factory);
				}

				return fPeriodValidation;
			}
		}
		PeriodValidationProvider fPeriodValidation;

		#endregion
	}
}
