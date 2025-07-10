namespace Enterprise.Integration.Billing
{
	public interface IBillingTransaction : IStlTransaction
	{
		string Category { get; set; }
		string ClientID { get; set; }
		string ClientNumber { get; set; }
		string ReportingSource { get; set; }
		int Version { get; set; }
		string MessageTrackingID { get; set; }
	}
}
