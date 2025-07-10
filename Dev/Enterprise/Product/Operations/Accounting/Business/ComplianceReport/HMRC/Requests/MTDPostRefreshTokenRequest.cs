using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public class MTDPostRefreshTokenRequest : MTDRequestBase
	{
		public MTDPostRefreshTokenRequest(MTDClient client)
			: base(client)
		{
			ClientSecret = MTDClient.AuthorisationPart;
			RefreshToken = client.RefreshToken;
			NeedAccessTokenToExecuteTheRequest = false;
		}

		string ClientSecret { get; set; }

		string RefreshToken { get; set; }

		public override string Path => FormattableString.Invariant($"oauth/token");

		public override HttpMethod Method => HttpMethod.Post;

		protected override void BuildRequestHeader(HttpRequestHeaders requestHeaders)
		{
			//Token Refresh does not require additional headers
		}

		protected override ByteArrayContent GetRequestBody()
		{
			var requestBody = FormattableString.Invariant($"{ClientSecret}&grant_type=refresh_token&refresh_token={RefreshToken}");
			var requestBodyContent = new ByteArrayContent(Encoding.ASCII.GetBytes(requestBody));
			requestBodyContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			return requestBodyContent;
		}
	}

	#endregion
}
