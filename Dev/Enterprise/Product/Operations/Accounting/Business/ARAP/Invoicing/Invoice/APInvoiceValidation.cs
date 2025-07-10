using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceValidation : InvoiceValidation
	{
		public APInvoiceValidation(APInvoice parent)
			: base(parent)
		{
		}

		protected new APInvoice Parent
		{
			get { return (APInvoice)base.Parent; }
		}

		protected new APInvoice InvoiceTransaction
		{
			get { return (APInvoice)base.InvoiceTransaction; }
		}

		protected override void CheckValidateExpectedInvoiceTotal()
		{
			if (!Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.IsAllowed)
			{
				CheckValidateExpectedInvoiceTotalCore();
			}
		}

		protected override void CheckReceiptPaymentAH_ChequeOrReference()
		{
			base.CheckReceiptPaymentAH_ChequeOrReference();

			if (!InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.HasNotifications() &&
				!(InvoiceTransaction is IChequeNumberAutoAllocation && ((IChequeNumberAutoAllocation)InvoiceTransaction).IsAutoAllocationEnabled))
			{
				AccChequeBook chequeBook = InvoiceTransaction.Factory.Load<AccChequeBook>(InvoiceTransaction.ReceiptPaymentAK_AB);
				if (chequeBook != null)
				{
					string chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(chequeBook, InvoiceTransaction.ReceiptPaymentAH_ChequeOrReference);
					if (!string.IsNullOrEmpty(chequeNumberErrorMessage))
					{
						InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.AddError(chequeNumberErrorMessage);
					}
					else
					{
						if (chequeBook.BankAccount != null && chequeBook.BankAccount.HasChequeNumberBeenUsed(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReference, ZGuid.Empty))
						{
							InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo.AddError(ChequeOrReferenceValidationHelper.GetInUseErrorMessage(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReference, chequeBook.BankAccount, InvoiceTransaction.Factory));
						}
						else if (chequeBook.BankAccount != null)
						{
							ValidationHelper.ValidateChequeDigits(InvoiceTransaction.ReceiptPaymentAH_ChequeOrReferenceInfo, chequeBook.BankAccount.AB_ChequeNumDigits);
						}
					}
				}
			}
		}

		protected override void ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChangesCore()
		{
			base.ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChangesCore();

			foreach (InvoicingLineBase line in Parent.Lines)
			{
				APInvoiceLineValidation invLineValidation = line.Validation as APInvoiceLineValidation;
				if (invLineValidation != null)
				{
					invLineValidation.ValidateAL_OSExTaxAmount();
				}
			}
		}

		protected override void CheckAH_OSTotalAmount_Implementation()
		{
			base.CheckAH_OSTotalAmount_Implementation();

			APInvoice aPInvoice = Parent;
			if (!aPInvoice.AH_OSTotalAmountInfo.HasErrors() && aPInvoice.IsHotChequeImported)
			{
				ZString actualOrMaxIndicator = aPInvoice.ImportedHotCheque.AQ_ActualOrMaxIndicator;
				if (actualOrMaxIndicator == ZArchitecture.Core.ActualOrMaxIndicator.Actual && aPInvoice.AH_OSTotalAmount != aPInvoice.ReceiptPaymentAH_OSTotalAmount)
				{
					aPInvoice.AH_OSTotalAmountInfo.AddError(Res.GetString("73da9174-6f93-493c-91f8-9d005f8def39", "Invoice amount must be the same as the hot check amount"));
				}
				else if (actualOrMaxIndicator == ZArchitecture.Core.ActualOrMaxIndicator.Max && aPInvoice.AH_OSTotalAmount > aPInvoice.ReceiptPaymentAH_OSTotalAmount)
				{
					aPInvoice.AH_OSTotalAmountInfo.AddError(Res.GetString("2a4bcc88-73ca-4267-8023-b621d0eb6e9e", "Invoice amount must be less than or equal to the hot check amount"));
				}
			}

			if (InvoiceTransaction.IsInvoiceReceiptPayment && aPInvoice.AH_OSTotalAmount == 0m)
			{
				aPInvoice.AH_OSTotalAmountInfo.AddError(Res.GetString("31b4216e-fdc1-46e9-831f-055b78294b4b", "Invoice Amount cannot be zero when making a Cash Invoice"));
			}

			if (!aPInvoice.AH_OSTotalAmountInfo.HasErrors())
			{
				if (aPInvoice.TotalCostVarianceAuthorisationRequired == APInvoice.CostVarianceAuthorisationRequiredType.AuthorisationRequired)
				{
					aPInvoice.AH_OSTotalAmountInfo.AddWarning(Res.GetString("72CEBA1A-2574-43F1-A0A7-21C5846EB050", "This Invoice requires approval on posting because it exceeds the registry defined total accrual variance threshold."));
				}
				else if (aPInvoice.TotalCostVarianceAuthorisationRequired == APInvoice.CostVarianceAuthorisationRequiredType.HasAuthorisationRights)
				{
					aPInvoice.AH_OSTotalAmountInfo.AddWarning(Res.GetString("D54A3B93-CE89-4392-B670-FAB27CF65247", "This Invoice will be automatically approved when you post because you already have the necessary authorization security right"));
				}
			}
		}

		protected override void CheckReceiptPaymentAH_ReceiptType()
		{
			base.CheckReceiptPaymentAH_ReceiptType();
			if (Parent.IsInvoiceReceiptPayment && !Parent.IsPosted && !Parent.ReceiptPaymentAH_ReceiptType.IsEmpty && !Parent.MatchedWithTNFJournalNum.IsEmpty)
			{
				Parent.ReceiptPaymentAH_ReceiptTypeInfo.AddError(Res.GetString("59aa3b45-f66a-4717-acb3-65995980fa19", "Payment details cannot be entered as this AP Invoice Number matches an unpaid ‘Carried Forward’ journal’s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", Parent.MatchedWithTNFJournalNum));
			}
		}

		protected override void CheckReceiptPaymentAH_AB()
		{
			base.CheckReceiptPaymentAH_AB();

			if (InvoiceTransaction.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettDirectDebit)
			{
				if (Parent.Header != null &&
					!(eNettHelper.IsOrganisationeNettRegistered(Parent.Header) ||
					 eNettHelper.DoesOrgHaveeNettDDRAccount(Parent.Header)))
				{
					InvoiceTransaction.ReceiptPaymentAH_ABInfo.AddError(eNettHelper.NoEnettBankInformationOnOrganisationError);
				}
				if (InvoiceTransaction.ReceiptPaymentBankAccount != null && !eNettHelper.IsBankAccountEnettRegistered(Parent.ReceiptPaymentBankAccount))
				{
					InvoiceTransaction.ReceiptPaymentAH_ABInfo.AddError(eNettHelper.BankAccountNotEnettRegistered);
				}
				if (Parent.Header != null &&
					eNettHelper.DoesOrgHaveeNettDDRAccount(InvoiceTransaction.Header) &&
					!eNettHelper.IsOrganisationeNettRegistered(InvoiceTransaction.Header))
				{
					InvoiceTransaction.ReceiptPaymentAH_ABInfo.AddWarning(eNettHelper.PayAnyoneWarning);
				}
			}
		}

		protected override void CheckAH_RequisitionDate()
		{
			base.CheckAH_RequisitionDate();

			if (!Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.IsAllowed && Parent.AH_RequisitionDate.Date != Parent.AH_DueDate.Date)
			{
				Parent.AH_RequisitionDateInfo.AddError(Res.GetString("a4211a54-2052-4967-a059-b5ee2c23d12f", "By default the Payment Requested Date is a Transaction’s Due Date. You have not been granted security rights to change this date. Please set this to {0}.", Parent.AH_DueDate.ToShortDateString()));
			}
		}

		protected override void CheckAH_RequisitionStatus()
		{
			base.CheckAH_RequisitionStatus();

			var originalRequisitionStatus = Parent.IsInDatabase ? (ZString)Parent.AH_RequisitionStatusInfo.OriginalValue : Parent.OriginalRequisitionStatusForNewBizo;
			if (Parent.AH_RequisitionStatus != originalRequisitionStatus && !Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.IsAllowed)
			{
				Parent.AH_RequisitionStatusInfo.AddError(Res.GetString("7211888c-af57-4eb7-b074-1f0cd5f41a8e", "You have not been granted security rights to change this transaction’s Payment Requisition Criticality Status. Please set this code to {0}.", originalRequisitionStatus));
			}

			ListValidation.ErrorIfInvalidCode(Parent.AH_RequisitionStatusInfo, Parent.PaymentCriticalityList);
		}

		protected override void CheckReceiptPaymentAK_AB()
		{
			base.CheckReceiptPaymentAK_AB();
			if (Parent.ChequeBook != null)
			{
				if (Parent.ChequeBook.AK_GB != Parent.AH_GB)
				{
					Parent.ReceiptPaymentAK_ABInfo.AddError(Res.GetString("500c6355-6104-4e57-bc94-99599b42d8d3", "You cannot select a check book that is different to the invoice branch ({0})", Parent.Branch.GB_Code));
				}
			}
		}

		public void ValidateReceiptPaymentAddressOverride()
		{
			ValidateCalculatedProperty(Parent.ReceiptPaymentAddressOverrideInfo);
		}

		public void ValidateReceiptPaymentContactOverride()
		{
			ValidateCalculatedProperty(Parent.ReceiptPaymentContactOverrideInfo);
		}

		protected virtual void CheckReceiptPaymentAddressOverride()
		{
			if (Parent.IsInvoiceReceiptPayment)
			{
				ListValidation.ErrorIfInvalidPK(Parent.ReceiptPaymentAddressOverrideInfo, Parent.PaymentOrganisationAddresses);
			}
		}

		protected virtual void CheckReceiptPaymentContactOverride()
		{
			if (Parent.IsInvoiceReceiptPayment)
			{
				ListValidation.ErrorIfInvalidPK(Parent.ReceiptPaymentContactOverrideInfo, Parent.PaymentOrganisationContacts);
			}
		}

		protected override bool GetLevelAuthorizationRequired()
		{
			using (Parent.ClearLineAuthorisationCacheSuspender.GetSuspender())
			{
				return base.GetLevelAuthorizationRequired();
			}
		}

		void ValidateFinalFlags()
		{
			foreach (APInvoiceLine line in Parent.Lines)
			{
				line.Validation.ValidateAL_IsFinalCharge();
			}
		}

		protected override void ValidateAllCore()
		{
			Parent.CostVarianceApprovalHelper.ClearLineAuthorisationCache();
			Parent.ClearLineChargeAmountSumCache();
			ValidateReceiptPaymentAddressOverride();
			ValidateReceiptPaymentContactOverride();
			ValidateFinalFlags();

			base.ValidateAllCore();
		}
	}
}
