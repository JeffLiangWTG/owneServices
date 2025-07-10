namespace Enterprise.Customs.IE.Messaging
{
	public interface INegativeAcknowledgementError
	{
		string LineNumber { get; }
		string Reason { get; }
		string ColumnNumber { get; }
	}
}
