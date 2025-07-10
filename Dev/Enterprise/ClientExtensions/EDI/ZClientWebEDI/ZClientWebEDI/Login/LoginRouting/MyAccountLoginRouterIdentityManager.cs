using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using EdiCustomerUserAccount = Enterprise.Client.EDI.UserManagement.Business.EdiCustomerUserAccount;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountLoginRouterIdentityManager : LoginRouterIdentityManager
	{
		public MyAccountLoginRouterIdentityManager() : base(new BusinessObjectFactory())
		{
		}

		public MyAccountLoginRouterIdentityManager(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static string GenerateToken(EdiCustomerUserAccount userAccount)
		{
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(string.Empty, userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterIdentity, tokenInfo, TimeSpan.FromMinutes(15), 1);
			return token;
		}

		protected override void PopulatePropertiesFromTokenCore(AccessTokenInfo token)
		{
			if (token.ParentTableCode.Equals(EdiCustomerUserAccountSchema.Constants.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				UserAccount = Factory.Load<EdiCustomerUserAccount>(token.ParentId);

				if (UserAccount != null)
				{
					Contact = UserAccount.WebAccessContact;
				}
			}
			else
			{
				base.PopulatePropertiesFromTokenCore(token);
			}
		}

		public override bool IsValidID()
		{
			return !string.IsNullOrEmpty(Token) && (UserAccount != null || Contact != null);
		}

		public EdiCustomerUserAccount UserAccount { get; private set; }
	}
}
