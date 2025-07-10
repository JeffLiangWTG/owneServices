using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IOAuth2MailboxSettings : IMailboxSettings
	{
		public string OAuth2Type { get; }

		public bool UseOAuth2 { get; }

		#region	Microsoft 365

		public bool UseGraphApi { get; }

		public string TenantId { get; }

		public string ApplicationId { get; }

		public string AppSecret { get; }

		Ms365OAuth2Token GetMs365UserToken();

		byte[] GetMs365AppToken();

		void SaveMs365OAuth2Token(bool isUserToken, byte[] bytes);

		#endregion

		#region GMail

		public string DelegatedMail { get; }

		public GmailOAuth2JsonFile ServiceAccountKey { get; }

		#endregion

	}
}
