using System;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface ITokenizedAccessControl
	{
		bool TryConsume(string accessToken, string accessTokenType, out AccessTokenInfo info);
		bool TryPeek(string accessToken, string accessTokenType, out AccessTokenInfo info);
		bool TryCreate(string token, string type, bool isPermanent, DateTime? expiresAtUtc, int useCount, AccessTokenInfo info);
	}
}
