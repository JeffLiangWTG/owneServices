using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;
using Google.Apis.Auth.OAuth2;

namespace Enterprise.MailManager.GUI
{
	public class AcquireGmailTokenInteractiveServer : IAcquireGmailTokenInteractiveServer
	{
		public AcquireGmailTokenInteractiveServer(IOAuth2Configuration oAuth2Configuration)
		{
			Argument.NotNull(oAuth2Configuration, nameof(oAuth2Configuration));
			this.oAuth2Configuration = (GmailOAuth2Configuration)oAuth2Configuration;
		}

		readonly GmailOAuth2Configuration oAuth2Configuration;

		#region Constants

		readonly string[] Scopes = new[]
		{
			"https://mail.google.com/",
			"https://www.googleapis.com/auth/gmail.send",
			"https://www.googleapis.com/auth/userinfo.profile",
			"https://www.googleapis.com/auth/userinfo.email"
		};

		#endregion

		#region AcquireByServiceAccountAsync

		public async Task<IGmailAuthenticationResult> AcquireByServiceAccountAsync(CancellationToken token = default)
		{
			GoogleCredential credential;

			var userEmail = oAuth2Configuration.DelegatedMail;
			var serviceAccountKey = oAuth2Configuration.ServiceAccountKey;
			var byteArray = Encoding.UTF8.GetBytes(serviceAccountKey.JsonText);

			using (var stream = new MemoryStream(byteArray))
			{
				credential = await GoogleCredential.FromStreamAsync(stream, token).ConfigureAwait(false);
			}

			var impersonatedCredential = credential.CreateScoped(Scopes).CreateWithUser(userEmail);
			var accessToken = await impersonatedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync(cancellationToken: token);

			return new GmailAuthenticationResult(accessToken, userEmail);
		}

		#endregion
	}
}
