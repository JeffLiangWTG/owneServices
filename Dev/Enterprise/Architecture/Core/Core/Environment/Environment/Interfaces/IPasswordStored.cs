using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IPasswordStored : IIdentified
	{
		int PasswordHashIterations { get; }
		ZBlob PasswordSalt { get; }
		ZBlob PasswordHash { get; }
		bool VerifyPassword(IUserSecretsContext userSecretsContext, string password);
	}
}
