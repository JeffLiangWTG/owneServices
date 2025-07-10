using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class LoginRouterIdentityManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LoginRouterIdentityManager() : base(new BusinessObjectFactory()) { }

		public LoginRouterIdentityManager(BusinessObjectFactory factory) : base(factory) { }

		public static string GenerateToken(OrgContact contact)
		{
			if (contact == null)
			{
				return string.Empty;
			}

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(string.Empty, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterIdentity, tokenInfo, TimeSpan.FromMinutes(15), 1);
			return token;
		}

		public void PopulatePropertiesFromToken(string token)
		{
			Token = token;
			if (AccessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out var accessToken))
			{
				PopulatePropertiesFromTokenCore(accessToken);
			}
		}

		protected virtual void PopulatePropertiesFromTokenCore(AccessTokenInfo token)
		{
			if (token.ParentTableCode.Equals(OrgContactSchema.Constants.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				Contact = Factory.Load<OrgContact>(token.ParentId);
			}
		}

		public bool ConsumeToken()
		{
			return AccessControl.TryConsume(Token, AccessTokenTypes.LoginRouterIdentity, out _);
		}

		public virtual bool IsValidID()
		{
			return !string.IsNullOrEmpty(Token) && Contact != null;
		}

		public OrgContact Contact { get; protected set; }
		public string Token { get; private set; }
		protected ITokenizedAccessControl AccessControl { get; } = new TokenizedAccessControl();
	}
}
