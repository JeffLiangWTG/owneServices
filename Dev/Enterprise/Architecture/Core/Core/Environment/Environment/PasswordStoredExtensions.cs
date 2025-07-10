using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.ZArchitecture.Environment
{
	public static class PasswordStoredExtensions
	{
		public static bool VerifyPassword(this IPasswordStored passwordStored, string password)
			=> passwordStored.VerifyPassword(UserSecretsContext.DefaultContext, password);
	}
}
