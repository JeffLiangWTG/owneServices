namespace Enterprise.Billing.Integration
{
	public enum SubmissionPriority
	{
		Default = 0,
		MandatoryForMilestone = 1,
		NotMandatoryForMilestone = 2
	}

	public sealed class BillingTransactionWrapper
	{
		public BillingTransaction Transaction { get; private set; }
		public SubmissionPriority SubmissionPriority { get; private set; }

		public BillingTransactionWrapper(BillingTransaction transaction, SubmissionPriority submissionPriority)
		{
			Transaction = transaction;
			SubmissionPriority = submissionPriority;
		}
	}
}
