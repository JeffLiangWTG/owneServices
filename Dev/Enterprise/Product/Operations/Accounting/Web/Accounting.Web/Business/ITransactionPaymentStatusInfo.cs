using System;

namespace Enterprise.Accounting.Web.Business
{
	public interface ITransactionPaymentStatusInfo
	{
		string CurrencyCode { get; set; }
		decimal InvoiceTotal { get; set; }
		decimal PaidAmount { get; set; }
		string PaymentStatus { get; set; }
		DateTime FullyPaidDate { get; set; }
		bool FullyPaidDateHasValue { get; set; }
	}

	public static class CoreITransactionPaymentStatusInfoExtensions
	{
		public static void FillFromReader(this ITransactionPaymentStatusInfo info, System.Data.Common.DbDataReader reader)
		{
			info.CurrencyCode = reader["CurrencyCode"].ToString();

			info.InvoiceTotal = (decimal)reader["InvoiceTotal"];
			info.PaidAmount = (decimal)reader["PaidAmount"];

			info.PaymentStatus = reader["PaymentStatus"].ToString();

			if (!reader["FullyPaidDate"].Equals(DBNull.Value))
			{
				info.FullyPaidDate = (DateTime)reader["FullyPaidDate"];
				info.FullyPaidDateHasValue = true;
			}
			else
			{
				info.FullyPaidDateHasValue = false;
			}
		}
	}
}
