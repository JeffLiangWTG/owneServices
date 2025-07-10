using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	class MTDPostObtainTokenRequest : MTDRequestBase
	{
		public MTDPostObtainTokenRequest(MTDClient client, string authorizationCode)
			: base(client)
		{
			ClientSecret = MTDClient.AuthorisationPart;
			AuthorisationCode = authorizationCode;
			NeedAccessTokenToExecuteTheRequest = false;
		}

		string ClientSecret { get; }

		string AuthorisationCode { get; }

		public override string Path => FormattableString.Invariant($"oauth/token");

		public override HttpMethod Method => HttpMethod.Post;

		protected override void BuildRequestHeader(HttpRequestHeaders requestHeaders)
		{
			//Token Authorisation does not require additional headers
		}

		protected override ByteArrayContent GetRequestBody()
		{
			var requestBody = FormattableString.Invariant($"{ClientSecret}&grant_type=authorization_code&redirect_uri={Uri.EscapeDataString(MTDClient.RedirectUrl)}&code={AuthorisationCode}");
			var requestBodyContent = new ByteArrayContent(Encoding.ASCII.GetBytes(requestBody));
			requestBodyContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			return requestBodyContent;
		}
	}

	#endregion
}
