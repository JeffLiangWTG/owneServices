namespace Enterprise.Integration
{
	public interface IClientSecret : ICommonOAuth2Parameters
	{
		string ClientSecret { get; }
	}
}
