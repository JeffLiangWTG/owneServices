namespace Enterprise.Accounting.Business.EInvoicing.Germany
{
	public static class GermanyEInvoicingFeatureFlags
	{
		/// <summary>
		/// Indicates whether eInvoicing for AR transactions with business debtors (B2B) is enabled.
		/// When disabled B2B AR transactions are ineligible for eInvoicing and will not be batched.
		/// </summary>
		/// <remarks>This does not affect B2G AR transactions which are controlled via the usual compliance date mechanism in the registry.</remarks>
		public static readonly string B2B = "B2B";

		/// <summary>
		/// Indicates whether eInvoicing for AR transactions with government debtors (B2G) is enabled via Direct xT.
		/// B2G AR transactions are controlled via the usual compliance date mechanism in the registry.
		/// When disabled B2G AR transactions are sent via xTrade.
		/// </summary>
		public static readonly string B2GinXT = "B2GinXT";
	}

	public static class GermanyEInvoicingDataItems
	{
		public static readonly string TransactionCategory = "Germany.eInvoicing.TransactionCategory";
		public static readonly string SellerIBAN = "Germany.eInvoicing.SellerIBAN";
		public static readonly string BuyersReference = "Germany.eInvoicing.BuyersReference";
	}
}
