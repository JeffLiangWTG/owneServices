using Enterprise.DocumentScanning.Business;
namespace Enterprise.Dash.Business
{
	public interface IDocumentParsingTokenManager
	{
		string GetSystemToSystemTrustToken();

		bool TryDecryptEDocsAuthToken(string encryptedToken, out EDocsAuthTokenDetails result);
	}
}
