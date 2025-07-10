using CargoWise.Application;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using MailKit.Security;

namespace Enterprise.MailManager
{
	public class GmailOAuth2Configuration : IOAuth2Configuration
	{
		public GmailOAuth2Configuration(string delegatedMail, GmailOAuth2JsonFile serviceAccountKey)
		{
			DelegatedMail = delegatedMail;
			ServiceAccountKey = serviceAccountKey;
		}

		#region Service Account

		public string DelegatedMail { get; private set; }
		public GmailOAuth2JsonFile ServiceAccountKey { get; private set; }

		#endregion

		public SaslMechanismOAuth2 GetSaslMechanism()
		{
			var helper = ObjectFactory.Get<IGmailOAuth2AuthenticationHelper>(nameof(IGmailOAuth2AuthenticationHelper), this);
			var auth = helper.AcquireTokenSilentlyAsync().Result;
			return new SaslMechanismOAuth2(auth.Email, auth.AccessToken);
		}
	}
}
