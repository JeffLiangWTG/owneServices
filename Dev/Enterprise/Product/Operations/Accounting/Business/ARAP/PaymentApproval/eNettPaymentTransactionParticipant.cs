using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class eNettPaymentTransactionParticipant : SaveInTransactionActionWithMainConnection
	{
		protected IeNettPayment payment;

		public eNettPaymentTransactionParticipant(IeNettPayment payment)
		{
			this.payment = payment;
		}

		#region SaveInTransactionActionWithMainConnection Members

		protected override void OnAllTransactionsBeginning()
		{
		}

		protected override void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			if (payment.PaymentType == ReceiptTypes.eNettCreditCard)
			{
				eNettWebServiceResult result = payment.PayWithCreditCardViaENett();
				if (!result.success)
				{
					throw new ENettProcessCreditCardException("eNett ProcessCreditCard failed!", result.errorCode, result.errorMessage);
				}
			}
			else if (payment.PaymentType == ReceiptTypes.eNettDirectDebit && payment.CurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				payment.PayWithEnettDirectDebitFX();
			}

			return ChangedTableNames.Empty;
		}

		#endregion
	}
}
