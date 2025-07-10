using System;

namespace Enterprise.Integration.Accounting
{
	public interface ITransactionPaymentStatus
	{
		string OrgCode { get; }
		string CompanyCode { get; }
		string PaymentStatus(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber);
		decimal OutstandingAmount(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber);
		DateTime? FullyPaidDate(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber);
	}

	public interface ITransactionPaymentStatusProvider
	{
		ITransactionPaymentStatus GetTransactionPaymentStatus(string orgCode);
	}
}
