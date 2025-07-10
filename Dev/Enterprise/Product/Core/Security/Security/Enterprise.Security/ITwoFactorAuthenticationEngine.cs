using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	public interface ITwoFactorAuthenticationEngine
	{
		string SendTwoFactorAuthenticationCode(IUser user);
	}
}
