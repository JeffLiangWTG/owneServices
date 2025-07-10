using System;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	static class IStlTransactionExtensions
	{
		public static string GetCompanyCode(this IStlTransaction transaction)
		{
			if (transaction is UsageTransaction usageTransaction)
			{
				return usageTransaction.CompanyCode;
			}

			if (transaction is BillingTransaction billingTransaction)
			{
				var substrings = billingTransaction.ClientNumber.Split('.');
				return (substrings != null && substrings.Length == 2) ? substrings[1] : null;
			}

			throw new InvalidCastException("Unknown transaction");
		}

		public static string GetBranchCode(this IStlTransaction transaction)
		{
			if (transaction is UsageTransaction usageTransaction)
			{
				return usageTransaction.BranchCode;
			}

			if (transaction is BillingTransaction billingTransaction)
			{
				return billingTransaction.Branch;
			}

			throw new InvalidCastException("Unknown transaction");
		}
	}
}
