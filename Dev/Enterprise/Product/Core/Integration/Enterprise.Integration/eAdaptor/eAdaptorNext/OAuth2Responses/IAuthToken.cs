namespace Enterprise.Integration
{
	public interface IAuthToken
	{
		string AccessToken { get; set; }
		string TokenType { get; set; }
		int ExpiresIn { get; set; }
		string RefreshToken { get; set; }
	}
}
