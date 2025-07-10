using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountLoginRouter : LoginRouter
	{
		public MyAccountLoginRouter(Uri originalUrl, OrgContact contact, ZGlobal appInstance = null) : base(originalUrl, contact, appInstance)
		{
		}

		public MyAccountLoginRouter(Uri originalUrl, string token) : base(originalUrl, token)
		{
		}

		public MyAccountLoginRouter(Uri originalUrl, EdiCustomerUserAccount userAccount, ZGlobal appInstance = null) : base(originalUrl)
		{
			appInstance?.SetupSession(null, EventArgs.Empty, false);
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			InitialiseIdentityManager(token);
		}

		protected override LoginRouterIdentityManager GetNewIdentityManager()
		{
			return new MyAccountLoginRouterIdentityManager(Factory);
		}

		protected new MyAccountLoginRouterIdentityManager IdentityManager => base.IdentityManager as MyAccountLoginRouterIdentityManager;

		protected EdiCustomerUserAccount UserAccount => IdentityManager.UserAccount;

		protected LicenceDatabase Database
		{
			get
			{
				if (UserAccount != null && !UserAccount.EUA_LD.IsEmpty)
				{
					database = Factory.Load<LicenceDatabase>(UserAccount.EUA_LD);
				}

				return database;
			}
		}

		LicenceDatabase database;

		protected override Uri DefaultUrl => new Uri(EDIDataRegistry.Instance.MyAccountIndexPage.Value);

		protected override IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors =>
			new List<ILoginRoutingDescriptor>(base.RoutingDescriptors
				.Where(x => !(x is PasswordRotationRoutingDescriptor)))
				// we need to remove the PasswordRotationRoutingDescriptor here, so as to re-insert it AFTER the SetMasterPasswordRoutingDescriptor
			{
				new AccountReconfigurationRoutingDescriptor(Contact, Database, IdentityManager),
				new UserEmailVerificationRoutingDescriptor(OriginalUrl, Database, UserAccount),
				new DistinctEmailRoutingDescriptor(UserAccount),
				new LoginOptionsExemptionRoutingDescriptor(UserAccount),
				new LoginOptionsRoutingDescriptor(UserAccount, Contact),
				new SetPersonalEmailRoutingDescriptor(Contact),
				new SetMasterPasswordRoutingDescriptor(Contact),
				new PasswordRotationRoutingDescriptor(Contact),
				new SetPasswordRoutingDescriptor(Contact),
				new TermsAndConditionsRoutingDescriptor(Contact),
				new AccountConfirmationRoutingDescriptor(Contact)
			};

		protected override ZGlobal GetNewGlobal()
		{
			return new Global();
		}
	}
}
