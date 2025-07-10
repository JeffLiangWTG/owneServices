using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalBulkProcessor : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static ZString GetCheckNumberUpdatedMessage(ZString originalValue, ZString newValue, bool withReprint)
		{
			if (withReprint)
			{
				return ZString.Format((NoResString)"Check number has been updated during reprinting. Was {0} Now {1}", originalValue, newValue);
			}
			else
			{
				return ZString.Format((NoResString)"Check number has been updated and check was not reprinted. Was {0} Now {1}", originalValue, newValue);
			}
		}

		public PaymentApprovalBulkProcessor(BusinessObjectFactory factory)
			: base(factory)
		{
			ExtraTransactionParticipants = new List<ITransactionParticipant>();
		}

		public List<ITransactionParticipant> ExtraTransactionParticipants;

		public void UpdateChequeNumbersOnPaymentApprovals(List<PaymentApprovalBase> approvals, string chequeNumber)
		{
			ProcessPaymentApprovals(approvals, true, false, chequeNumber, true);
		}

		public void PopulateChequeNumbersOnPaymentApprovals(List<PaymentApprovalBase> approvals)
		{
			PopulateChequeNumbersOnPaymentApprovals(approvals, null);
		}

		public void PopulateChequeNumbersOnPaymentApprovals(List<PaymentApprovalBase> approvals, string chequeNumber)
		{
			ProcessPaymentApprovals(approvals, true, false, chequeNumber, false);
		}

		public void PostPaymentApprovals(List<PaymentApprovalBase> approvals)
		{
			ProcessPaymentApprovals(approvals, false, true, null, false);
		}

		public void PopulateChequeNumberAndPostPaymentApprovals(List<PaymentApprovalBase> approvals)
		{
			PopulateChequeNumberAndPostPaymentApprovals(approvals, null);
		}

		public void PopulateChequeNumberAndPostPaymentApprovals(List<PaymentApprovalBase> approvals, string chequeNumber)
		{
			ProcessPaymentApprovals(approvals, true, true, chequeNumber, false);
		}

		void ProcessPaymentApprovals(List<PaymentApprovalBase> approvals, bool populateChequeNumber, bool postApprovals, string chequeNumber, bool updateChequeNumber)
		{
			if (approvals != null && approvals.Count > 0)
			{
				foreach (PaymentApprovalBase paymentApproval in approvals)
				{
					ProcessSinglePaymentApproval(paymentApproval, populateChequeNumber, postApprovals, ref chequeNumber, updateChequeNumber);
				}
			}
		}

		void ProcessSinglePaymentApproval(PaymentApprovalBase paymentApproval, bool populateChequeNumber, bool postApprovals, ref string chequeNumber, bool updateChequeNumber)
		{
			if (postApprovals & updateChequeNumber)
			{
				throw new InvalidOperationException("PostApprovals and UpdateChequeNumber functions cannot be used together.");
			}

			if (paymentApproval.IsBackDatePostingNotAllowedAndPostDateNotToday)
			{
				paymentApproval.AV_PostDate = ZDateTime.Now;
			}

			paymentApproval.RunPrePostingValidation();

			if (!paymentApproval.HasErrors)
			{
				bool shouldPrint = false;

				if (populateChequeNumber && (!paymentApproval.IsPosted && paymentApproval.AV_ChequeOrReference.IsEmpty || updateChequeNumber) && !((IChequeNumberAutoAllocation)paymentApproval).IsAutoAllocationEnabled)
				{
					if (chequeNumber == null)
					{
						paymentApproval.PopulateChequeNumberFromChequeBook();
					}
					else
					{
						paymentApproval.AV_ChequeOrReference = chequeNumber;
					}

					paymentApproval.Validation.ValidateAV_ChequeOrReference();
					if (!paymentApproval.HasErrors)
					{
						shouldPrint = true;
						chequeNumber = paymentApproval.UpdateChequeBookCurrentNo().ToString();
					}
				}

				if (postApprovals && paymentApproval.IsFullyApproved && (!paymentApproval.AV_ChequeOrReference.IsEmpty || ((IChequeNumberAutoAllocation)paymentApproval).IsAutoAllocationEnabled))
				{
					paymentApproval.IsLoadedFromGUI = false;
					paymentApproval.CreateNewPayment();
					shouldPrint = true;
					if (paymentApproval.AV_PaymentType == ReceiptTypes.eNettCreditCard &&
						paymentApproval.BankAccount != null && paymentApproval.BankAccount.IsCreditCardOrLinkedAccount)
					{
						ExtraTransactionParticipants.Add(new eNettPaymentTransactionParticipant(paymentApproval));
					}
					else if (paymentApproval.AV_PaymentType == ReceiptTypes.eNettDirectDebit &&
						paymentApproval.AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						ExtraTransactionParticipants.Add(new eNettPaymentTransactionParticipant(paymentApproval));
					}
				}

				if (shouldPrint && !paymentApproval.HasErrors)
				{
					if (paymentApproval.NewPayment != null)
					{
						if (((IChequeNumberAutoAllocation)paymentApproval).IsAutoAllocationEnabled)
						{
							PaymentsForAutoAllocation.Add(paymentApproval.NewPayment);
						}
						else
						{
							OtherPayments.Add(paymentApproval.NewPayment);
						}
					}
#if DEBUG
					ApprovalsSuccessfullyProcessed_ForTestOnly.Add(paymentApproval);
#endif
				}

				if (updateChequeNumber)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					paymentApproval.Logs.AddNew(Events.EditedARecord, GetCheckNumberUpdatedMessage((ZString)paymentApproval.AV_ChequeOrReferenceInfo.OriginalValue, paymentApproval.AV_ChequeOrReference, shouldPrint));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		public void SaveChanges()
		{
			Factory.Save();
		}

		public TransactionHeaderCollection PaymentsForAutoAllocation
		{
			get
			{
				if (fPaymentsForAutoAllocation == null)
				{
					fPaymentsForAutoAllocation = new TransactionHeaderCollection(Factory);
				}

				return fPaymentsForAutoAllocation;
			}
		}

		TransactionHeaderCollection fPaymentsForAutoAllocation;

		public TransactionHeaderCollection OtherPayments
		{
			get
			{
				if (fOtherPayments == null)
				{
					fOtherPayments = new TransactionHeaderCollection(Factory);
				}

				return fOtherPayments;
			}
		}

		TransactionHeaderCollection fOtherPayments;

		#region Test
#if DEBUG
		public PaymentApprovalCollection ApprovalsSuccessfullyProcessed_ForTestOnly
		{
			get
			{
				if (fApprovalsSuccessfullyProcessed == null)
				{
					fApprovalsSuccessfullyProcessed = new PaymentApprovalCollection(Factory);
				}

				return fApprovalsSuccessfullyProcessed;
			}
		}

		PaymentApprovalCollection fApprovalsSuccessfullyProcessed;
#endif
		#endregion
	}
}
