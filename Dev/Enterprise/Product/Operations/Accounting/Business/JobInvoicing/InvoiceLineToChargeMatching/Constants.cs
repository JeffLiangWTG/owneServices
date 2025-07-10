namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public static class Constants
	{
		public static class InvoiceLineToChargeMatchingFactor
		{
			public const int OrgTypeFactory = 10000;
			public const int InvoiceLineGroupFactor = 1000;
			public const int ChargeGroupFactor = 100;
			public const int MatchingStrategyFactor = 10;
			public const int ProcessorFactor = 1;
		}

		public static class InvoiceLineToChargeMatchingGroupWeight
		{
			public const int JobAndChargeUsedWeight = 9;
			public const int JobUsedWeight = 7;
		}

		public static class InvoiceLineToChargeMatchingOrgWeight
		{
			public const int OriginalOrgMatchingWeight = 9;
			public const int SettlementGroupOrgMatchingWeight = 7;
			public const int ChildOrgMatchingWeight = 5;
			public const int NoOrgMatchingWeight = 3;
		}

		public static class ProcessorWeight
		{
			public const int Job = 9;
			public const int Consol = 7;
		}
	}

	public enum OrgType
	{
		OriginalOrg,
		SettlementGroupOrg,
		ChildOrg,
		NoOrg
	}

	public enum MatchingOutcome
	{
		MatchingNotPerformed,
		NoMatchFound,
		PartiallyMatched,
		FullyMatched
	}
}
