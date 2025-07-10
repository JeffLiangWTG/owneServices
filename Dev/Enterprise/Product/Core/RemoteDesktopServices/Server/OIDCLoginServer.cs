using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Shared;
using Enterprise.ZArchitecture.Core;
using WTG.OpenIDConnect.Login;
using WTG.OpenIDConnect.Token;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class OIDCLoginServer : IOIDCLoginServer
	{
		public bool IsSupported => InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.OIDCLogin);

		public OIDCLoginResponseMessage LoginRemote(OIDCLoginRequestMessage loginRequest, CancellationToken cancellationToken)
		{
			return Task.Run(() => LoginRemoteAsync(loginRequest, cancellationToken)).GetAwaiter().GetResult();
		}

		async Task<OIDCLoginResponseMessage> LoginRemoteAsync(OIDCLoginRequestMessage loginRequest, CancellationToken cancellationToken)
		{
			AsyncResult result;

			try
			{
				result =
					EnterpriseChannel.Instance.BeginSendMessage<OIDCLoginRequestMessage, OIDCLoginResponseMessage>(
						EnterpriseChannelMessageTypes.OIDCLogin,
						loginRequest?.RequestID ?? Guid.Empty,
						loginRequest);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return OIDCLoginResponseMessage.CreateFailedResponse(
					OIDCLoginResponseMessage.ErrorType.Exception,
					ex.Message,
					ex.StackTrace);
			}

			try
			{
				await result.AsyncWaitHandle.WaitOneAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Handled below
			}

			if (result.IsCompleted)
			{
				var response = (OIDCLoginResponseMessage)result.AsyncState;
				if (response.Error == OIDCLoginResponseMessage.ErrorType.None)
				{
					return response;
				}

				return OIDCLoginServerResponseCreator.CreateFailedResponse(loginRequest, response);
			}

			if (cancellationToken.IsCancellationRequested)
			{
				var requestId = loginRequest == null ? "" : loginRequest.RequestID.ToString();
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.CancelOIDCLogin, requestId);
				return OIDCLoginServerResponseCreator.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled);
			}

			return OIDCLoginServerResponseCreator.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.LoginFailed);
		}

		public OIDCLoginResponseMessage LoginLocal(
			OIDCLoginRequestMessage loginRequest,
			OIDCWebLauncher webLauncher,
			CancellationToken cancellationToken,
			OIDCLoginFactory oidcLoginFactory = null)
		{
			oidcLoginFactory ??= (OIDCWebLauncher launcher) =>
			{
				var redirectUrl = loginRequest.ServerType == OIDCLoginRequestMessage.OIDCServer.Azure
				? OIDCLoginShared.GetRedirectURIWithRandomPort()
				: OIDCLoginShared.RedirectURI;
				return new OIDCLogin(redirectUrl, OIDCLoginShared.RefreshTokenPath, new NullLogger(), webLauncher);
			};

			var login = oidcLoginFactory.Invoke(webLauncher);
			AddUrlAcl.EnsureCallbackUrlConfigured();
			return login.PerformLogin(loginRequest, cancellationToken);
		}

		/// <summary>
		/// Thread Pool Hack to avoid deadlock https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development#the-thread-pool-hack
		/// </summary>
		public JwtSecurityToken ValidateIdentityToken(
			OIDCLoginRequestMessage request,
			OIDCLoginResponseMessage response,
			CancellationToken cancellationToken)
		{
			return Task.Run(() => ValidateIdentityTokenAsync(request, response, cancellationToken)).GetAwaiter().GetResult();
		}

		public async Task<JwtSecurityToken> ValidateIdentityTokenAsync(
			OIDCLoginRequestMessage request,
			OIDCLoginResponseMessage response,
			CancellationToken cancellationToken)
		{
			return await TokenValidator.ValidateAccessToken(request.Authority, request.ClientID, response.IdentityToken, ConfigurationHelper.ConfigurationManagerCache, new OIDCTokenValidationLogger(), cancellationToken).ConfigureAwait(false);
		}
	}
}
