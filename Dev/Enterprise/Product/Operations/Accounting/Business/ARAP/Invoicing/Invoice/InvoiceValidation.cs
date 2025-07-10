using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceValidation : InvoiceBaseValidation
	{
		public InvoiceValidation(Invoice parent)
			: base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateReceiptPaymentCommon();
			ValidateCashAdvances();

			if (InvoiceTransaction is APInvoice)
			{
				ValidatePayment();
			}
			else
			{
				ValidateReceipt();
			}
		}

		void ValidateReceiptPaymentCommon()
		{
			ValidateReceiptPaymentAH_ReceiptType();
			ValidateReceiptPaymentAH_AB();
			ValidateReceiptPaymentAH_ChequeOrReference();
			ValidateReceiptPaymentAH_InvoiceDate();
			ValidateReceiptPaymentAH_PostDate();
		}

		void ValidateReceipt()
		{
			ValidateReceiptPaymentAH_ChequeDrawer();
			ValidateReceiptPaymentAH_DrawerBank();
			ValidateReceiptPaymentAH_DrawerBranch();
		}

		void ValidatePayment()
		{
			ValidateReceiptPaymentAK_AB();
			ValidateReceiptPaymentAH_OSTotalAmount();
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();
			CheckAH_DueDateMustAfterInvoiceDate();
		}

		protected override void CheckAH_InvoiceTerm()
		{
			base.CheckAH_InvoiceTerm();
			if (InvoiceTransaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				MandatoryValidation.CheckEntered(Parent.AH_InvoiceTermInfo);

				string error = InvoiceTransaction.TermsAndDueDateCalculationProvider.CanInvoiceTermBeSelectedError;
				if (!string.IsNullOrEmpty(error))
				{
					InvoiceTransaction.AH_InvoiceTermInfo.AddError(error);
				}
			}
		}

		protected override void CheckAH_InvoiceTermDays()
		{
			base.CheckAH_InvoiceTermDays();
			if (InvoiceTransaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (InvoiceTransaction.AH_InvoiceTermDays != 0 &&
					(InvoiceTransaction.AH_InvoiceTerm == Core.Constants.InvoiceTerms.CashOnDelivery ||
					InvoiceTransaction.AH_InvoiceTerm == Core.Constants.InvoiceTerms.PaymentInAdvance))
				{
					InvoiceTransaction.AH_InvoiceTermDaysInfo.AddError(Res.GetString("69db2f36-b189-4ac5-9995-7ee355167b33", "An Invoice Term Days should be 0 for this Invoice Term"));
				}
				else if (InvoiceTransaction.AH_InvoiceTerm == Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate && (InvoiceTransaction.AH_InvoiceTermDays < 0 || InvoiceTransaction.AH_InvoiceTermDays > 99))
				{
					InvoiceTransaction.AH_InvoiceTermDaysInfo.AddError(Res.GetString("69db2f36-b189-4ac5-9995-7ee355167c33", "Days must be between 0 and 99."));
				}
			}
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				ValidateReceiptPaymentAH_AB();
			}
		}

		protected override void CheckAH_OSTotal()
		{
			base.CheckAH_OSTotal();
			ValidateExpectedInvoiceTotal();
			ValidateExpectedInvoiceTaxTotal();
			ValidateExpectedInvoiceExclTaxTotal();

			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckNotZero(InvoiceTransaction.AH_OSTotalInfo);
			}
		}

		protected override void CheckAH_InvoiceAmount()
		{
			base.CheckAH_InvoiceAmount();
			ValidateExpectedInvoiceTotal();
			ValidateExpectedInvoiceTaxTotal();
			ValidateExpectedInvoiceExclTaxTotal();
		}

		protected override void CheckAH_Calc_AmendStatusCode()
		{
			base.CheckAH_Calc_AmendStatusCode();

			var amendStatusCodeValidationInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeValidationProvider>;
			var amendStatusCodeValidationProvider = amendStatusCodeValidationInstanceProvider?.Get();
			amendStatusCodeValidationProvider?.ValidateAmendStatusCodeForInvoice(Parent);
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnARDebitNotes;

		#region Calculated Properties Checks

		public void ValidateReceiptPaymentAH_AB()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_ABInfo);
		}

		public void ValidateReceiptPaymentAH_InvoiceDate()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_InvoiceDateInfo);
		}

		protected virtual void CheckReceiptPaymentAH_InvoiceDate()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_InvoiceDateInfo);
				CheckReceiptPaymentAH_InvoiceDateIsValidDate();
			}
		}

		protected void CheckReceiptPaymentAH_InvoiceDateIsValidDate()
		{
			if (!InvoiceTransaction.ReceiptPaymentAH_InvoiceDateInfo.HasErrors())
			{
				if (!InvoiceTransaction.ReceiptPaymentAH_InvoiceDateInfo.Value.IsValid)
				{
					InvoiceTransaction.ReceiptPaymentAH_InvoiceDateInfo.AddError(Res.GetString("c65c4d24-89e6-48f9-8aba-e15498093c4c", "Please enter a valid Receipt Payment Invoice Date."));
				}
			}
		}

		public void ValidateReceiptPaymentAH_PostDate()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_PostDateInfo);
		}

		protected virtual void CheckReceiptPaymentAH_PostDate()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_PostDateInfo);
				PeriodValidation.CheckDateFallsIntoValidPeriod(InvoiceTransaction.ReceiptPaymentAH_PostDateInfo);
				CheckReceiptPaymentAH_PostDateNotInPast();
				CheckReceiptPaymentAH_PostDateNotInFuture();
				CheckReceiptPaymentAH_PostDateIsValidDate();
			}
		}

		protected void CheckReceiptPaymentAH_PostDateNotInFuture()
		{
			if (!InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.HasErrors())
			{
				if (InvoiceTransaction.ReceiptPaymentAH_PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		protected virtual void CheckReceiptPaymentAH_PostDateNotInPast()
		{
			if (!InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.HasErrors())
			{
				if (InvoiceTransaction.ReceiptPaymentAH_PostDate.Date < ZDateTime.Today)
				{
					if (!Parent.AllowBackPosting)
					{
						InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.AddError(PreviousPostDateError);
					}
					else
					{
						InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.AddWarning(PreviousPostDateWarning);
					}
				}
			}
		}

		protected void CheckReceiptPaymentAH_PostDateIsValidDate()
		{
			if (!InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.HasErrors())
			{
				if (!InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.Value.IsValid)
				{
					InvoiceTransaction.ReceiptPaymentAH_PostDateInfo.AddError(Res.GetString("25146d8d-a020-4850-b322-b915d88faa18", "Please enter a valid Receipt Payment Post Date."));
				}
			}
		}

		protected virtual void CheckReceiptPaymentAH_AB()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_ABInfo, Res.GetString("17d64f6e-3ea1-44ff-ae85-d083766038cc", "Bank"));
				MultilingualString errorMessage = ResString.GetMultilingualString("7f2fbf8c-c1c0-4f9c-adee-1e2a96fce15c", "Bank Account must be for the current company.\r\nBank Account currency must be currency of the invoice or local currency.");

				if (InvoiceTransaction.ReceiptPaymentBankAccount?.Branch != null && InvoiceTransaction.ReceiptPaymentBankAccount.AB_GB != Parent.AH_GB)
				{
					InvoiceTransaction.ReceiptPaymentAH_ABInfo.AddError(Res.GetString("39cd04f4-7d9e-4a17-8683-840f631c1a18", "You cannot select a bank account that is different to the invoice branch ({0})", InvoiceTransaction.Branch.GB_Code));
				}

				if (!InvoiceTransaction.ReceiptPaymentAH_ABInfo.HasErrors())
				{
					if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard)
					{
						ListValidation.ErrorIfInvalidPK(InvoiceTransaction.ReceiptPaymentAH_ABInfo, InvoiceTransaction.CreditCardBankAccountLookup, MultilingualString.Join("\r\n", errorMessage, ResString.GetMultilingualString("d58193ca-809b-4484-a8a1-12e8ca08f695", "Select a Bank Account that has an Account Type of '{0}' or '{1}'", AccountTypeCodeDescriptionPairList.Descriptions.CCD, AccountTypeCodeDescriptionPairList.Descriptions.LNK)));
					}
					else
					{
						ListValidation.ErrorIfInvalidPK(InvoiceTransaction.ReceiptPaymentAH_ABInfo, InvoiceTransaction.BankAccountLookup, errorMessage);
					}
				}

				ValidateAH_OA_InvoiceAddressOverride();
			}
		}

		public void ValidateReceiptPaymentAK_AB()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAK_ABInfo);
		}

		protected virtual void CheckReceiptPaymentAK_AB()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAK_ABInfo, Res.GetString("424b9099-7433-473a-abc4-7f8e25e5fd23", "Check Book"));
					ListValidation.ErrorIfInvalidPK(InvoiceTransaction.ReceiptPaymentAK_ABInfo, InvoiceTransaction.ChequeBookLookup);
					if (InvoiceTransaction.ChequeBook != null)
					{
						ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(InvoiceTransaction.ChequeBook, ((IChequeNumberAutoAllocation)InvoiceTransaction).IsAutoAllocationEnabled);
						if (!errorMessage.IsEmpty)
						{
							InvoiceTransaction.ReceiptPaymentAK_ABInfo.AddError(errorMessage);
						}
						InvoiceTransaction.ChequeBook.AddWarningSamePrinter(InvoiceTransaction.ReceiptPaymentAK_ABInfo);
					}
				}
			}
		}

		public void ValidateReceiptPaymentAH_ReceiptType()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo);
		}

		protected virtual void CheckReceiptPaymentAH_ReceiptType()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo);
				if (InvoiceTransaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					ListValidation.ErrorIfInvalidCode(InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo, Parent.PaymentMethods);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo, Parent.ReceiptMethods);
				}
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard && !AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value)
				{
					InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.AddError(Res.GetString("afe50dc6-c074-4574-8651-d854820b3155", "'Pay via ComPay Credit Card' payment type is not enabled."));
				}

				if (!InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors())
				{
					CheckReceiptPaymentAH_ReceiptTypeSecurity();
				}

				ValidateAH_OA_InvoiceAddressOverride();

				if (!InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors() && InvoiceTransaction.ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cash && (InvoiceTransaction.ReceiptPaymentBankAccount?.IsCashAccount ?? false))
				{
					InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.AddError(GetCashAccountTypeErrorMessage(InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.HumanReadableName));
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckReceiptPaymentAH_ReceiptTypeSecurity()
		{
			bool isARTransaction = InvoiceTransaction.AH_Ledger == LedgerTypes.AccountsReceivable;
			Security.SecurityCheckpoint result = null;
			switch (InvoiceTransaction.ReceiptPaymentAH_ReceiptType)
			{
				case ReceiptTypes.Cheque:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentCheque : Env.Security.NewPayablesPaymentCheque;
					break;

				case ReceiptTypes.Cash:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentCash : Env.Security.NewPayablesPaymentCash;
					break;

				case ReceiptTypes.CreditCard:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentCreditCard : Env.Security.NewPayablesPaymentCreditCard;
					break;

				case ReceiptTypes.DirectDebit:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentDirectDebit : Env.Security.NewPayablesPaymentDirectDebit;
					break;

				case ReceiptTypes.EFT:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentEFT : Env.Security.NewPayablesPaymentEFT;
					break;

				case ReceiptTypes.ScheduledEFT:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentSFT : Env.Security.NewPayablesPaymentSFT;
					break;

				case ReceiptTypes.CollectionRequest:
					result = isARTransaction ? Env.Security.NewReceivablesPaymentCRQ : Env.Security.NewPayablesPaymentCRQ;
					break;
			}

			if (result != null && !result.IsAllowed)
			{
				InvoiceTransaction.ReceiptPaymentAH_ReceiptTypeInfo.AddError(Res.GetString("190de071-4016-49d3-b0e4-8914b28172f4", "You do not have appropriate security rights to select this payment type."));
			}
		}

		AccAPAccountDetails AccountDetails
		{
			get
			{
				AccAPAccountDetails result = null;
				if (InvoiceTransaction.Header != null)
				{
					ZString receiptType = InvoiceTransaction.ReceiptPaymentAH_ReceiptType;
					if (receiptType == ReceiptTypes.DirectDebitLine)
					{
						receiptType = ReceiptTypes.DirectDebit;
					}

					ZString currencyCode = !InvoiceTransaction.AH_RX_NKTransactionCurrency.IsEmpty ? InvoiceTransaction.AH_RX_NKTransactionCurrency : InvoiceTransaction.AH_Calc_LocalRXCode;
					result = InvoiceTransaction.Header.CompanyData.AccountDetailsCollection.GetAccountDetails(receiptType, currencyCode, true);
				}
				return result;
			}
		}

		public void ValidateReceiptPaymentAH_Desc()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_DescInfo);
		}

		protected virtual void CheckReceiptPaymentAH_Desc()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_DescInfo);
			}
		}

		public void ValidateReceiptPaymentAH_DrawerBranch()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_DrawerBranchInfo);
		}

		protected virtual void CheckReceiptPaymentAH_DrawerBranch()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_DrawerBranchInfo);
				}
			}
		}

		public void ValidateReceiptPaymentAH_DrawerBank()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_DrawerBankInfo);
		}

		protected virtual void CheckReceiptPaymentAH_DrawerBank()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_DrawerBankInfo);
				}
			}
		}

		public void ValidateReceiptPaymentAH_ChequeDrawer()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_ChequeDrawerInfo);
		}

		protected virtual void CheckReceiptPaymentAH_ChequeDrawer()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_ChequeDrawerInfo);
				}
			}
		}

		public void ValidateReceiptPaymentAH_ChequeOrReference()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		protected virtual void CheckReceiptPaymentAH_ChequeOrReference()
		{
			if (InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				if (!(InvoiceTransaction is IChequeNumberAutoAllocation && ((IChequeNumberAutoAllocation)InvoiceTransaction).IsAutoAllocationEnabled))
				{
					if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo);

						if (!InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.HasErrors())
						{
							string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, InvoiceTransaction.ReceiptPaymentAH_ChequeOrReference);
							if (!string.IsNullOrEmpty(errorMessage))
							{
								InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.AddError(errorMessage);
							}
						}
					}
					else
					{
						ZString errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, InvoiceTransaction.ReceiptPaymentAH_ChequeOrReference);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.AddError(errorMessage);
						}
					}
				}
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					MandatoryValidation.CheckEntered(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo);
				}
			}
		}

		public void ValidateReceiptPaymentAH_OSTotalAmount()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentAH_OSTotalAmountInfo);
		}

		protected virtual void CheckReceiptPaymentAH_OSTotalAmount()
		{
			// sub-classes should override to implement custom validation
		}

		public void ValidateReceiptPaymentCardSecurityCode()
		{
			ValidateCalculatedProperty(InvoiceTransaction.ReceiptPaymentCardSecurityCodeInfo);
		}

		protected virtual void CheckReceiptPaymentCardSecurityCode()
		{
			if ((InvoiceTransaction.IsInvoiceReceiptPayment) && (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard))
			{
				if ((InvoiceTransaction.ReceiptPaymentCardSecurityCode.Length < 3) || (InvoiceTransaction.ReceiptPaymentCardSecurityCode.Length > 4))
				{
					InvoiceTransaction.ReceiptPaymentCardSecurityCodeInfo.AddError(Res.GetString("038e17c3-f63f-4214-8c67-5c2950d18609", "Card Security Code must be 3 or 4 digits in length."));
				}
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			if (Parent.AH_Ledger == LedgerTypes.AccountsPayable &&
				Parent.AH_InvoiceDate.Date > ZDateTime.Today && !AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.Value)
			{
				Parent.AH_InvoiceDateInfo.AddError(
					Res.GetString("A72A4C1B-C1B3-4631-AE4D-E986EFAD0645",
								"Invoice date cannot be in the future.\r\nThis is determined by registry: {0}",
								((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate).Location));
			}
		}

		protected override void CheckAH_OHCore()
		{
			base.CheckAH_OHCore();

			if (InvoiceTransaction.AH_OH.IsValid && InvoiceTransaction.IsInvoiceReceiptPayment)
			{
				ValidateOrganisationDDRDetail();
				if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard && InvoiceTransaction.Header.ENettRegistrationNumber.IsEmpty)
				{
					InvoiceTransaction.AH_OHInfo.AddError(AccountingConstants.ENettErrorMessages.OrganisationNotRegisteredForENett);
				}
			}
		}

		void ValidateOrganisationDDRDetail()
		{
			if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.DirectDebit)
			{
				if (InvoiceTransaction.ReceiptPaymentBankAccount != null && InvoiceTransaction.ReceiptPaymentBankAccount.AB_AllowAutoDDR)
				{
					if (InvoiceTransaction.Header != null && AccountDetails == null)
					{
						ZString currencyCode = InvoiceTransaction.TransactionCurrency != null ? InvoiceTransaction.AH_RX_NKTransactionCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
						ZString errorMessage = Res.GetString("5c6d52ce-ff4d-4d09-8a0a-da7e812d94a8", "An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", currencyCode, InvoiceTransaction.Header.OH_Code);
						InvoiceTransaction.AH_OHInfo.AddError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region Cash advance related

		void ValidateCashAdvances()
		{
			ClearAllCashAdvanceRelatedRowErrors(InvoiceTransaction);
			var errorMessage = RunCashAdvanceRelatedValidation(InvoiceTransaction);
			if (!errorMessage.IsNullOrEmpty())
			{
				InvoiceTransaction.AddRowError(errorMessage);
			}
		}

		internal static string RunCashAdvanceRelatedValidation(Invoice invoiceTransaction)
		{
			var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (invoiceTransaction is IInvoiceAssociatedToCashAdvanceRequest cahUpdater &&
				((invoiceTransaction.AH_Ledger == LedgerTypes.AccountsPayable && cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled) ||
					(invoiceTransaction.AH_Ledger == LedgerTypes.AccountsReceivable && cashAdvanceFunctionalityChecker.IsReceivablesCashAdvanceFunctionalityEnabled)))
			{
				var errorMessages = new Dictionary<string, List<string>>();
				var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
				cahUpdater.Accept(validator);

				if (errorMessages.Any())
				{
					var fullErrorMessageBuilder = new ZStringBuilder();
					foreach (var errorMessage in errorMessages)
					{
						fullErrorMessageBuilder.Append(errorMessage.Key);
						fullErrorMessageBuilder.Append(string.Join(System.Environment.NewLine, errorMessage.Value));
						fullErrorMessageBuilder.Append(string.Empty);
					}
					return fullErrorMessageBuilder.ToStringWithNewLineBetweenAppends();
				}
			}
			return string.Empty;
		}

		internal static void ClearAllCashAdvanceRelatedRowErrors(Invoice invoice)
		{
			foreach (var errMsgHeader in TransactionWithCashAdvanceRequestValidationVisitor.GetMessageHeadersOfAllErrorsCheckedOnInvoicePosting())
			{
				invoice?.ClearRowNotificationsContaining(errMsgHeader);
			}
		}

		#endregion

		#region Implementation

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

		#endregion

		#region Invoice Transaction
		Invoice fInvoiceTransaction;
		public new Invoice InvoiceTransaction
		{
			get
			{
				if (fInvoiceTransaction == null)
				{
					fInvoiceTransaction = (Invoice)Parent;
				}
				return fInvoiceTransaction;
			}
		}

		#endregion
	}
}
