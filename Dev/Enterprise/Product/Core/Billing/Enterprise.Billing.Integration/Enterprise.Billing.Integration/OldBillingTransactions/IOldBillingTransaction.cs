namespace Enterprise.Billing.Integration.OldBillingTransactions
{
	interface IOldBillingTransaction
	{
		BillingTransaction ToLatest();
	}
}