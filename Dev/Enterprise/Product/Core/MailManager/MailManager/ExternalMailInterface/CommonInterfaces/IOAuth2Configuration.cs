using MailKit.Security;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public interface IOAuth2Configuration
	{
		SaslMechanismOAuth2 GetSaslMechanism();
	}
}
