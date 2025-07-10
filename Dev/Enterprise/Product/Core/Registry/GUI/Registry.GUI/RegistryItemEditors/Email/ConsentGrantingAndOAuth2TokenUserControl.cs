using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Identity.Client;

namespace Enterprise.Registry.GUI
{
	enum TokenState
	{
		Granted,
		NotGranted
	}

	public partial class ConsentGrantingAndOAuth2TokenUserControl : ZUserControl
	{
		public ConsentGrantingAndOAuth2TokenUserControl(EmailType emailType, StringRegistryItem ms365OAuth2TenantId, StringRegistryItem ms365ApplicationId, BooleanRegistryItem useGraphApi)
		{
			this.emailType = emailType;
			if (ms365ApplicationId == null)
			{
				ms365ApplicationId = emailType == EmailType.Incoming ? Env.Registry.RawRegistry.Ms365ApplicationIdForIncoming : Env.Registry.RawRegistry.Ms365ApplicationIdForOutgoing;
			}

			if (useGraphApi == null)
			{
				useGraphApi = emailType == EmailType.Incoming ? Env.Registry.RawRegistry.UseGraphApiForIncoming : Env.Registry.RawRegistry.UseGraphApiForOutgoing;
			}

			this.ms365OAuth2TenantId = ms365OAuth2TenantId ?? Env.Registry.RawRegistry.Ms365OAuth2TenantId;
			this.ms365ApplicationId = ms365ApplicationId;
			this.useGraphApi = useGraphApi;

			InitializeComponent();
		}

		readonly EmailType emailType;
		readonly StringRegistryItem ms365OAuth2TenantId;
		readonly StringRegistryItem ms365ApplicationId;
		readonly BooleanRegistryItem useGraphApi;

		public IMs365OAuth2AuthenticationHelper AuthenticationHelper
		{
			get => authenticationHelper ?? (authenticationHelper = ObjectFactory.Get<IMs365OAuth2AuthenticationHelper>(nameof(IMs365OAuth2AuthenticationHelper),
				new Ms365OAuth2Configuration(
					tenantId: ms365OAuth2TenantId?.Value,
					applicationId: ms365ApplicationId?.Value,
					permissionType: useGraphApi != null && useGraphApi.Value ? Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI : Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
					cachedToken: CachedTokenAsBinary,
					tokenSaveAction: SetCachedToken,
					identifier: Token.Identifier,
					shouldAcquireTokenInteractive: true)));
		}

		IMs365OAuth2AuthenticationHelper authenticationHelper;

		public Ms365OAuth2Token Token
		{
			get
			{
				return token ?? (token = new Ms365OAuth2Token());
			}
			set
			{
				token = value;
				SetCachedToken(value?.Token);
			}
		}
		Ms365OAuth2Token token;
		internal byte[] CachedTokenAsBinary { get; set; }

		public void SetCachedToken(byte[] value)
		{
			CachedTokenAsBinary = value;
			Token.Token = value;
			if (value == null)
			{
				Token.User = string.Empty;
				Token.Identifier = string.Empty;
			}
			TokenState = IsTokenEmpty ? TokenState.NotGranted : TokenState.Granted;
		}

		TokenState TokenState
		{
			set
			{
				tokenState = value;
				switch (tokenState)
				{
					case TokenState.Granted:
						lblMessage.Text = string.IsNullOrEmpty(Token.User) ? Res.GetString("B8D8CDCF-93AB-47F0-97C1-4D7E0803D5BF", "Granted") : Res.GetString("E54222A4-41CC-47E6-93C5-874397CDE44D", "Granted, User: {0}", Token.User);
						break;
					case TokenState.NotGranted:
						lblMessage.Text = Res.GetString("AE7F1FB4-CF6A-4CF4-B9E5-89AB70F5F77C", "Not Granted");
						break;

					default:
						lblMessage.Text = "";
						break;
				}
			}
		}
		TokenState tokenState;

		bool IsTokenEmpty => CachedTokenAsBinary == null || CachedTokenAsBinary.Length == 0;

		CancellationTokenSource cts;
		internal async void btnGrant_Click(object sender, System.EventArgs e)
		{
			try
			{
				using (cts = new CancellationTokenSource())
				{
					var authenticationResult = await AuthenticationHelper.AcquireTokenAsync(cts.Token);
					SetAuthenticationResult(authenticationResult);
				}
			}
			catch (MsalException ex) when (ex is MsalClientException || ex is MsalServiceException)
			{
				SetAuthenticationResult(null);
				Globals.Message.ShowError(Res.GetString("FC749793-6129-45C1-AD7B-C3EF0572B027",
					@"Error when granting permissions.
Error Code: {0}
Error Message: {1}", ex.ErrorCode, ex.Message));
			}
			catch (TaskCanceledException)
			{
				// Cancel the token acquiring, do nothing.
			}
			finally
			{
				cts = null;
			}

			void SetAuthenticationResult(AuthenticationResult result)
			{
				token = null;
				if (result == null)
				{
					SetCachedToken(null);
				}
				else
				{
					Token.Identifier = result.Account.HomeAccountId.Identifier;
					Token.User = result.Account.Username;
					SetCachedToken(CachedTokenAsBinary);
				}
			}
		}

		internal void btnClear_Click(object sender, System.EventArgs e)
		{
			Token = null;
		}
	}
}
