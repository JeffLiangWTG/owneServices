using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager
{
	public class GmailOAuth2AuthenticationHelper : IGmailOAuth2AuthenticationHelper
	{
		public GmailOAuth2AuthenticationHelper(GmailOAuth2Configuration oAuth2Configuration)
		{
			Argument.NotNull(oAuth2Configuration, nameof(oAuth2Configuration));
			this.oAuth2Configuration = oAuth2Configuration;
		}

		readonly GmailOAuth2Configuration oAuth2Configuration;

		public async Task<IGmailAuthenticationResult> AcquireTokenSilentlyAsync(CancellationToken token = default)
		{
			IGmailAuthenticationResult result = null;

			var remoteServer = ObjectFactory.Get<ZArchitecture.Core.IAcquireGmailTokenInteractiveServer>("IAcquireGmailTokenInteractiveServer", oAuth2Configuration);
			try
			{
				result = await remoteServer.AcquireByServiceAccountAsync(token);
			}
			catch (OperationCanceledException)
			{
				// User canceled the operation, do nothing
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw;
			}
			return result;
		}
	}
}
