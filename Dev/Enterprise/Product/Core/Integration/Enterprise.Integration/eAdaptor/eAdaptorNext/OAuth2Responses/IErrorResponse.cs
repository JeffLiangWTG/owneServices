namespace Enterprise.Integration
{
	public interface IErrorResponse
	{
		string Error { get; set; }
		string ErrorDescription { get; set; }
		string ErrorURI { get; set; }
	}
}
