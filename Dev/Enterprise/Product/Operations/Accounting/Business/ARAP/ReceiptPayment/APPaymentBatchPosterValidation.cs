using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	internal class APPaymentBatchPosterValidation : AccPaymentBatchValidation
	{
		public APPaymentBatchPosterValidation(APPaymentBatchPoster parent) : base(parent)
		{
			ZValidationInternals = this;
		}

		APPaymentBatchPoster BatchPoster => base.Parent as APPaymentBatchPoster;

		BusinessObjectFactory Factory => BatchPoster.Factory;

		protected override void CheckAPB_AB_FundingBankAccount()
		{
			base.CheckAPB_AB_FundingBankAccount();

			if (BatchPoster.IsInDatabase && BatchPoster.OriginalFundingBankAccountCurrency != BatchPoster.FundingBankAccountCurrency &&
				BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Any(x => x.CurrentDeal != null && EPaymentStatusCodes.Deal.ActiveStatusCodes.Contains(x.CurrentDeal.AED_Status.ToString())))
			{
				BatchPoster.APB_AB_FundingBankAccountInfo.AddError(Res.GetString("B24884DA-0691-4F5F-AAC7-4EF76CE2C84A", "This payment batch has an active E-Payment Deal. Change of Funding Currency is not permitted."));
			}
		}

		protected override void CheckAPB_ChequeOrReference()
		{
			base.CheckAPB_ChequeOrReference();

			if (!BatchPoster.IsSavingPaymentBatchAsDraft && !BatchPoster.IsChequeNumberAutoAllocated)
			{
				MandatoryValidation.CheckEntered(BatchPoster.APB_ChequeOrReferenceInfo);
				if (!BatchPoster.APB_ChequeOrReferenceInfo.HasErrors())
				{
					var isCheque = BatchPoster.APB_PaymentType == ReceiptTypes.Cheque;
					ZString chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(isCheque, BatchPoster.APB_ChequeOrReference);
					if (chequeNumberErrorMessage.IsEmpty)
					{
						chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(BatchPoster.ChequeBook, BatchPoster.APB_ChequeOrReference);
					}
					if (!chequeNumberErrorMessage.IsEmpty)
					{
						BatchPoster.APB_ChequeOrReferenceInfo.AddError(chequeNumberErrorMessage);
					}
					else if (isCheque && BatchPoster.ChequeBook != null)
					{
						var maxChequeNumber = GetChequeBookRemainingNumberOfCheques(BatchPoster.APB_ChequeOrReference);
						ZBool maxChequeNumberOutOfRangeOfChequeBook = !maxChequeNumber.IsInRange(BatchPoster.ChequeBook.AK_StartNo, BatchPoster.ChequeBook.AK_LastNo);

						if ((BatchPoster.PaymentApprovalCollection.Any() || BatchPoster.APB_ChequeOrReferenceInfo.HasChanges) && ChequeNumberHasBeenUsed())
						{
							BatchPoster.APB_ChequeOrReferenceInfo.AddError(ChequeOrReferenceValidationHelper.GetInUseErrorMessage(BatchPoster.APB_ChequeOrReference, BatchPoster.ChequeBook.BankAccount, Factory));
						}
						else if (BatchPoster.BankAccount != null)
						{
							ValidationHelper.ValidateChequeDigits(BatchPoster.APB_ChequeOrReferenceInfo, BatchPoster.BankAccount.AB_ChequeNumDigits);
						}
						if (BatchPoster.PaymentApprovalCollection.Any() && maxChequeNumberOutOfRangeOfChequeBook)
						{
							BatchPoster.APB_ChequeOrReferenceInfo.AddError(Res.GetString("bea5faed-34bd-4fec-b626-78176a837e4b", @"The accessible count of check numbers in the selected check book is less than the payment count.
Please change Start Reference No or select another Check Book."));
						}
					}
				}
			}

			bool ChequeNumberHasBeenUsed()
			{
				if (BatchPoster.ChequeBook != null && BatchPoster.ChequeBook.BankAccount != null)
				{
					return ChequeNumberHasBeenUsedOnAPaymentApproval()
						|| ChequeNumberHasBeenUsedOnAJobCharge()
						|| ChequeNumberHasBeenUsedOnAPayment();
				}

				return false;
			}

			bool ChequeNumberHasBeenUsedOnAJobCharge()
				=> BatchPoster.ChequeBook.BankAccount.HasChequeNumberBeenUsedOnAJobCharge(BatchPoster.APB_ChequeOrReference, ZGuid.Empty);

			bool ChequeNumberHasBeenUsedOnAPaymentApproval()
				=> BatchPoster.ChequeBook.BankAccount.HasChequeNumberBeenUsedOnAPaymentApproval(BatchPoster.APB_ChequeOrReference, BatchPoster.PaymentApprovalCollection.Select(x => x.PK).ToList());

			bool ChequeNumberHasBeenUsedOnAPayment()
			{
				var transactionHeaderPKsToExclude = BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Where(x => x.IsPosted).Select(x => x.AV_AH).ToList();
				return BatchPoster.ChequeBook.BankAccount.HasChequeNumberBeenUsedOnAPayment(BatchPoster.APB_ChequeOrReference, transactionHeaderPKsToExclude);
			}
		}

		protected override void CheckAPB_AK()
		{
			base.CheckAPB_AK();

			if (BatchPoster.APB_PaymentType == ReceiptTypes.Cheque)
			{
				if (!BatchPoster.IsSavingPaymentBatchAsDraft)
				{
					MandatoryValidation.CheckEntered(BatchPoster.APB_AKInfo);
				}

				ListValidation.ErrorIfInvalidPK(BatchPoster.APB_AKInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(BatchPoster.APB_AKInfo);
			}

			if (BatchPoster.ChequeBook != null)
			{
				if (BatchPoster.ChequeBook.AK_AB != BatchPoster.APB_AB)
				{
					BatchPoster.APB_AKInfo.AddError(Res.GetString("3c3015e3-e55c-4b22-8741-d2b6745f0054", "This check book does not belong to the bank specified"));
				}
				else
				{
					ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(BatchPoster.ChequeBook, BatchPoster.IsChequeNumberAutoAllocated);
					if (!errorMessage.IsEmpty)
					{
						BatchPoster.APB_AKInfo.AddError(errorMessage);
					}
					else
					{
						if (GetChequeBookRemainingNumberOfCheques(BatchPoster.ChequeBook.AK_CurrentNo.ToString()) > BatchPoster.ChequeBook.AK_LastNo)
						{
							BatchPoster.APB_AKInfo.AddError(Res.GetString("42646bee-c9ff-4927-b291-021e2839d1b5", @"The accessible count of free check numbers is less than the payment count.
Please change the Current Number of the current Check Book or select another Check Book."));
						}
						BatchPoster.ChequeBook.AddWarningSamePrinter(BatchPoster.APB_AKInfo);
					}
				}
			}
		}

		protected override void CheckAPB_AB()
		{
			base.CheckAPB_AB();

			MandatoryValidation.CheckEntered(BatchPoster.APB_ABInfo);
			if (BatchPoster.APB_PaymentType == ReceiptTypes.eNettCreditCard)
			{
				ListValidation.ErrorIfInvalidPK(BatchPoster.APB_ABInfo, BatchPoster.CreditCardBankAccounts, ResString.GetMultilingualString("8f1b02ef-feca-4bdb-9d9d-80162558826a", "Select a Bank Account that has an Account Type of '{0}' or '{1}'", AccountTypeCodeDescriptionPairList.Descriptions.CCD, AccountTypeCodeDescriptionPairList.Descriptions.LNK));
			}

			if (BatchPoster.BankAccount != null)
			{
				if (BatchPoster.BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA && BatchPoster.APB_PaymentType != ReceiptTypes.EPayment)
				{
					BatchPoster.APB_ABInfo.AddError(Res.GetString("26e38ea4-d284-4f33-885c-e473956ab25c", "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment."));
				}
				else if (BatchPoster.BankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA && BatchPoster.APB_PaymentType == ReceiptTypes.EPayment)
				{
					BatchPoster.APB_ABInfo.AddError(Res.GetString("02794c08-f6b7-4bdf-8bf1-877b495c333e", "Bank Account is not an E-Payment Account."));
				}
			}

			if (BatchPoster.APB_ABInfo.HasChanges && BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Any(p => p.HasActiveDeal))
			{
				BatchPoster.APB_ABInfo.AddError(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails);
			}
		}

		protected override void CheckAPB_PaymentType()
		{
			base.CheckAPB_PaymentType();

			MandatoryValidation.CheckEntered(BatchPoster.APB_PaymentTypeInfo, Res.GetString("d63d0e5b-ffa0-4cc2-8a52-d6b1bc046ea4", "Payment Type"));
			ListValidation.ErrorIfInvalidCode(BatchPoster.APB_PaymentTypeInfo, BatchPoster.Lookups.PaymentTypeList);

			if (!BatchPoster.APB_PaymentTypeInfo.HasErrors())
			{
				CheckPaymentTypeSecurity();
			}

			ValidateAPB_AK();

			if (BatchPoster.APB_PaymentType == ReceiptTypes.eNettCreditCard && !AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value)
			{
				BatchPoster.APB_PaymentTypeInfo.AddError(Res.GetString("c0c7efe1-2004-430e-b2f0-a279458c8f5f", "'Pay via ComPay Credit Card' payment type is not enabled."));
			}

			if (BatchPoster.APB_PaymentType == ReceiptTypes.eNettDirectDebit && !AllAccountPayableInvoice())
			{
				BatchPoster.APB_PaymentTypeInfo.AddError(Res.GetString("f5ff89cb-bfdb-4d11-b08f-1f07b74de3a9", "A ComPay payment can only be matched to AP invoices."));
			}

			bool AllAccountPayableInvoice()
			{
				return BatchPoster.MatchingCollection.OfType<TransactionHeader>()
					.All(transaction => transaction.AH_TransactionType == TransactionTypes.Invoice && transaction.AH_Ledger == LedgerTypes.AccountsPayable);
			}
		}

		protected override void CheckAPB_PostDate()
		{
			base.CheckAPB_PostDate();
			MandatoryValidation.CheckEntered(BatchPoster.APB_PostDateInfo);
			BatchPoster.PeriodValidation.CheckDateFallsIntoValidPeriod(BatchPoster.APB_PostDateInfo);

			if (!BatchPoster.APB_PostDateInfo.HasErrors())
			{
				CheckPostDateNotInPast();
				CheckPostDateNotInFuture();
			}

			void CheckPostDateNotInFuture()
			{
				if (BatchPoster.APB_PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						BatchPoster.APB_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						BatchPoster.APB_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}

			void CheckPostDateNotInPast()
			{
				if (BatchPoster.APB_PostDate.Date < ZDateTime.Today)
				{
					if (!AllowBackPosting())
					{
						BatchPoster.APB_PostDateInfo.AddError(PreviousPostDateError);
					}
					else if (BatchPoster.IsPosted)
					{
						BatchPoster.APB_PostDateInfo.AddWarning(PreviousPostDateWarning);
					}
				}
			}

			bool AllowBackPosting() => AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
		}

		protected override void CheckAPB_PaymentDate()
		{
			base.CheckAPB_PaymentDate();

			MandatoryValidation.CheckEntered(BatchPoster.APB_PaymentDateInfo);
		}

		#region Balance

		public void ValidateBalance()
		{
			ZValidationInternals.Validate(BatchPoster.BalanceInfo, GetBalanceValidationInvoker());
		}

		RunValidationInvoker GetBalanceValidationInvoker()
		{
			return delegate
			{
				CheckBalanceIsWesternEuropean();
				CheckBalance();
			};
		}

		protected virtual void CheckBalanceIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(BatchPoster.BalanceInfo);
		}

		protected virtual void CheckBalance()
		{
			if (BatchPoster.PaymentForBinding != null
				&& !BatchPoster.PaymentForBinding.IsDeleted
				&& !BatchPoster.PaymentForBinding.IsCancelled
				&& BatchPoster.Balance != 0
				&& !BatchPoster.IsSavingPaymentBatchAsDraft)
			{
				BatchPoster.BalanceInfo.AddError(Res.GetString("662c8269-53d2-41a8-b6f4-02d7a850e853", "The balance must equal 0"));
			}
		}

		#endregion

		public void ValidateOSOutstandingamount()
		{
			Factory.ClearQueryCache(AccPaymentApprovalItem.Schema.TableName);

			if (BatchPoster.TransactionCollection != null)
			{
				foreach (TransactionHeader transaction in BatchPoster.TransactionCollection)
				{
					transaction.ExistingPaymentApprovalItems.Load();

					var validation = transaction.Validation as MatchingValidation;
					if (validation != null)
					{
						validation.ValidateOSOutstandingAmount();
					}
				}
			}
		}

		#region CardSecurityCode

		public void ValidateCardSecurityCode()
		{
			ZValidationInternals.Validate(BatchPoster.CardSecurityCodeInfo, GetCardSecurityCodeValidationInvoker());
		}

		RunValidationInvoker GetCardSecurityCodeValidationInvoker()
		{
			return delegate
			{
				CheckCardSecurityCodeIsWesternEuropean();
				CheckCardSecurityCode();
			};
		}

		protected virtual void CheckCardSecurityCodeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(BatchPoster.APB_PaymentTypeInfo);
		}

		protected virtual void CheckCardSecurityCode()
		{
			if ((BatchPoster.APB_PaymentType == ReceiptTypes.eNettCreditCard) && ((BatchPoster.CardSecurityCode.Length < 3) || (BatchPoster.CardSecurityCode.Length > 4)))
			{
				BatchPoster.CardSecurityCodeInfo.AddError(Res.GetString("afa4f6ce-4402-4865-8721-342cd5a8f27a", "Card Security Code must be 3 or 4 digits in length."));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateBalance();
			ValidateOSOutstandingamount();
			ValidateCardSecurityCode();
		}

		static ZString PreviousPostDateError => Res.GetString("98c79877-ad95-4fb7-811d-5027fca51e72", "The post date cannot be in the past");

		static ZString PreviousPostDateWarning
		{
			get
			{
				return Res.GetString("d8e9b8ad-e680-40cb-b37b-dc47cb1feb57", "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckPaymentTypeSecurity()
		{
			bool isAllowed = true;

			switch (BatchPoster.APB_PaymentType)
			{
				case ReceiptTypes.Cheque:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewCheque.IsAllowed : Env.Security.NewPayablesPaymentCheque.IsAllowed;
					break;
				case ReceiptTypes.Cash:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewCash.IsAllowed : Env.Security.NewPayablesPaymentCash.IsAllowed;
					break;
				case ReceiptTypes.CreditCard:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewCreditCard.IsAllowed : Env.Security.NewPayablesPaymentCreditCard.IsAllowed;
					break;
				case ReceiptTypes.DirectDebit:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed : Env.Security.NewPayablesPaymentDirectDebit.IsAllowed;
					break;
				case ReceiptTypes.EFT:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewEFT.IsAllowed : Env.Security.NewPayablesPaymentEFT.IsAllowed;
					break;
				case ReceiptTypes.ScheduledEFT:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewSFT.IsAllowed : Env.Security.NewPayablesPaymentSFT.IsAllowed;
					break;
				case ReceiptTypes.CollectionRequest:
					isAllowed = BatchPoster.PostPaymentsAsPaymentApprovals ? Env.Security.APPaymentProcessingNewCRQ.IsAllowed : Env.Security.NewPayablesPaymentCRQ.IsAllowed;
					break;
			}

			if (!isAllowed)
			{
				BatchPoster.APB_PaymentTypeInfo.AddError(Res.GetString("1df16d05-a8b5-4796-b642-c33db947a9cf", "You do not have appropriate security rights to select this payment type."));
			}
		}

		ZDecimal GetChequeBookRemainingNumberOfCheques(string currentNo) => ZDecimal.Parse(currentNo) + (ZDecimal)BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count() - 1;

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

		readonly IValidationInternals ZValidationInternals;
	}
}
