using CargoWise.Common;

namespace Enterprise.Customs.CH.Business;

public class TokenRefreshRequestBodyDataProvider
{
	public TokenRefreshRequestBodyDataProvider(GlbCompanyTokenCredentials token)
	{
		this.token = Argument.NotNull(token, nameof(token));
	}
	protected readonly GlbCompanyTokenCredentials token;

	const string RequestBody = "client_secret={0}&grant_type=refresh_token&refresh_token={1}&client_id={2}";
	public string GetRequestBody() => string.Format(RequestBody, token.CurrentDecryptedPassword, token.RefreshTokenText, token.GP_UserID);
}
