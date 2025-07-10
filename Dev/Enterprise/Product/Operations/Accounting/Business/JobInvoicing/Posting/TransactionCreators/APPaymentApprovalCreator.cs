using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region AP Payment Approval Creator

	public class APPaymentApprovalCreator : BaseTransactionCreator
	{
		public APPaymentApprovalCreator(Job job)
			: this(job, null, false)
		{
		}

		public APPaymentApprovalCreator(Job job, IJobCostingPlugIn consol, bool consolHasJobOnHold)
			: base(job, consol, consolHasJobOnHold)
		{
		}

		protected override bool IsChargeApplicable(Charge charge)
		{
			bool chargeCostIsAlreadyPosted = charge.IsCostPosted && charge.APLine != null && charge.APLine.IsInDatabase;
			bool isPaymentDetailsEntered = !chargeCostIsAlreadyPosted && charge.CostAccount != null && !charge.JR_PaymentType.IsEmpty && charge.BankAccount != null && (!charge.JR_ChequeNo.IsEmpty || charge.IsChequeNumberAutoAllocated);
			bool apportionIncluded = (IsConsol && charge.JR_IsApportioned) || !charge.JR_IsApportioned;
			return base.IsChargeApplicable(charge) && isPaymentDetailsEntered && apportionIncluded;
		}

		protected override bool CreateTransactionsCore(TransactionCreatorHashtable transactions, bool isMultiJobOperationInProgress)
		{
			int transactionsAdded = 0;

			foreach (Charge paymentCharge in Charges)
			{
				string clientCode = paymentCharge.CostAccount.OH_Code;
				string bankAccountCode = paymentCharge.BankAccount.AB_Code;
				string receiptType = paymentCharge.JR_PaymentType;
				string chequeOrReference = paymentCharge.JR_ChequeNo.IsEmpty ? paymentCharge.ChequeBook.AK_Code : paymentCharge.JR_ChequeNo;
				string jobNumber = paymentCharge.JR_IsApportioned ? ZString.Empty : Job.JH_JobNum;

				var apInvoice = transactions.RetrieveAPInvoice(clientCode, paymentCharge.JR_APInvoiceNum);
				if (apInvoice != null)
				{
					var paymentApproval = transactions.RetrieveAPPaymentApproval(clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);

					if (paymentApproval == null)
					{
						if (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.Count > 0)
						{
							paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
							paymentApproval.IsAllowedToPost = AccountingConfigurationRegistry.Instance.AutoPostPaymentsOnceFullyApprovedWhenPostingCosts.Value;
						}
						else
						{
							paymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
						}

						paymentApproval.IsLoadedFromGUI = false;
						transactions.AddAPPaymentApproval(paymentApproval, clientCode, bankAccountCode, receiptType, chequeOrReference, jobNumber);
						++transactionsAdded;
					}

					paymentApproval.SetValues(apInvoice, Job, paymentCharge, PostingTime);
				}
			}

			return transactionsAdded > 0;
		}
	}

	#endregion

	#region Consol AP Payment Approval Creator

	public class ConsolAPPaymentApprovalCreator : ConsolBaseTransactionCreator
	{
		public ConsolAPPaymentApprovalCreator(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, bool hasJobOnHold)
			: base(fallbackFactory, jobs, hasJobOnHold, consol)
		{
		}

		public override bool CreateTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			foreach (Job job in Jobs)
			{
				if (!job.IsWorkOnHold)
				{
					APPaymentApprovalCreator creator = new APPaymentApprovalCreator(job, Consol, HasJobOnHold);
					result |= creator.CreateTransactions(transactions);
					SetConsolPaymentDescription(transactions, job);
				}
			}

			return result;
		}

		protected void SetConsolPaymentDescription(TransactionCreatorHashtable transactions, Job job)
		{
			foreach (Charge charge in job.Charges)
			{
				PaymentApprovalBase payment = RetrieveConsolPayment(transactions, charge);
				if (charge.JR_IsApportioned && payment != null)
				{
					ZString consolNumber = Consol != null ? Consol.JK_UniqueConsignRef : (ZString)"";
					payment.AV_PaymentComment = string.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Payment, Res.GetString("568d9647-148a-4021-be5e-0c3da7d6515d", "AP Payment")) + " {0}", consolNumber);
				}
			}
		}

		protected PaymentApprovalBase RetrieveConsolPayment(TransactionCreatorHashtable transactions, Charge charge)
		{
			string clientCode = charge.CostAccount != null ? charge.CostAccount.OH_Code : ZString.Empty;
			string bankAccountCode = charge.BankAccount != null ? charge.BankAccount.AB_Code : ZString.Empty;
			string chequeOrReference = (charge.JR_ChequeNo.IsEmpty && charge.ChequeBook != null) ? charge.ChequeBook.AK_Code : charge.JR_ChequeNo;
			return transactions.RetrieveAPPaymentApproval(clientCode, bankAccountCode, charge.JR_PaymentType, chequeOrReference, ZString.Empty);
		}
	}

	#endregion
}
