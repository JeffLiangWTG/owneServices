namespace Enterprise.Dash.Integration
{
	public interface IParseRequest
	{
		string StatusName { get; }

		string StatusCode { get; }

		bool IsRequestCancelled { get; }

		bool IsRequestCompleted { get; }

		bool IsRequestInProgress { get; }

		bool IsRequestError { get; }

		void UpdateStatus(string status);
	}
}
