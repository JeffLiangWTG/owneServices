using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager
{
	public class GmailAuthenticationResult : IGmailAuthenticationResult
	{
		public GmailAuthenticationResult(string accessToken, string email)
		{
			AccessToken = accessToken;
			Email = email;
		}

		public string AccessToken { get; }
		public string Email { get; }
	}
}
