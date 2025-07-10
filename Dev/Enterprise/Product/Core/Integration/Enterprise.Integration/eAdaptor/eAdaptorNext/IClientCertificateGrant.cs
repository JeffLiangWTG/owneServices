namespace Enterprise.Integration
{
	public interface IClientCertificateGrant : ICommonOAuth2Parameters
	{
		string PrivateKey { get; }
		string Certificate { get; }
	}
}
