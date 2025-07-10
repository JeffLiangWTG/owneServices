namespace Enterprise.Integration
{
	public interface IPasswordGrant : IClientSecret, ICommonOAuth2Parameters
	{
		string Username { get; }
		string Password { get; }
	}
}
