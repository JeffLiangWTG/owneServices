namespace Enterprise.Customs.CA.Business;

public static class TransactionBatchConstants
{
	public static class ARLMessageProcessorConstants
	{
		public const string Dash = "-";
		public const string RM = "RM";
	}

	public static class PostingJournalChargeCodes
	{
		public const string Duty = "DTY";
		public const string SIMA = "SIM";
		public const string ExciseTax = "EXS";
		public const string GST = "GST";
		public const string DSB = "DSB";
		public const string Others = "OTH";
	}

	public static class PostingJournalDescriptions
	{
		public const string TotalPaymentReceived = "TotalPaymentReceived";
		public const string Refund = "Refund";
		public const string PreviousMonthlyStatementTotal = "PreviousMonthlyStatementTotal";
		public const string PaymentReceivedSinceLastMonthlyStatement = "PaymentReceivedSinceLastMonthlyStatement";
		public const string ArrearsInterest = "ArrearsInterest";
		public const string TransactionTotal = "TransactionTotal";
		public const string OtherCharges = "OtherCharges";
		public const string UnpaidBalanceForward = "UnpaidBalanceForward";
		public const string TotalPayableForBrokerOld = "TotalPayableForBroker";
		public const string TotalPayableForBroker = "TotalAmount";
		public const string TotalPayableForImporter = "AmountDue";
		public const string TotalCredits = "TotalCredits";
		public const string InterestAmount = "InterestAmount";
		public const string InstalmentLastAmount = "InstalmentLastAmount";
		public const string InstalmentCurrentAmount = "InstalmentCurrentAmount";
	}

	public static class TransactionAddInfoKeys
	{
		public const string IsGSTDirectPayment = "IsGSTDirectPayment";
		public const string IsImporterDirectPayment = "IsImporterDirectPayment";
		public const string ReleaseOffice = "ReleaseOffice";
		public const string ReleaseDate = "ReleaseDate";
		public const string AccountingDate = "AccountingDate";
		public const string MessageEN = "MessageEN";
		public const string MessageFR = "MessageFR";
		public const string CheckIssueDate = "CheckIssueDate";
		public const string BrokerIndicator = "BrokerIndicator";
		public const string PaymentCategory = "PaymentCategory";
	}

	public static class TransactionCategoryCodes
	{
		public const string Summary = "SUM";
		public const string Normal = "NOR";
		public const string Other = "OTH";
		public const string UnderReview = "OUR";
	}
}
