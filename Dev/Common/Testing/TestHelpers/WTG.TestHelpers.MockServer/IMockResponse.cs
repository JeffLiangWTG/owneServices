namespace WTG.TestHelpers.MockServer
{
	public interface IMockResponse
	{
		string Content { get; set; }
		string ContentType { get; set; }
		int StatusCode { get; set; }
	}
}