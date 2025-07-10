using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.Business
{
	public abstract class WebUser
	{
		public void Login(string username, string password)
		{
			Login("", username, password);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "A simple user context change is not applicable here . It needs a full environment replacement.")]
		public void Login(string affiliationCode, string username, string password, byte[] loginHash = null, bool shouldRecordLoginFailAttempt = true)
		{
			BeginLogin();

			LoggedInUserCore = null;
			SupportStaffCode = ZString.Empty;

			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
			{
				var webFactory = new WebFactory();
				var factory = webFactory.Factory;
				WebUserFactory = webFactory;

				if (username == WebDataRegistry.Instance.WebServiceUsername.Value &&
					password == WebDataRegistry.Instance.WebServicePassword.Value)
				{
					LoggedInUserCore = GetNewContactableForSpecialUser(factory, affiliationCode, WebServicesUserName);
				}
				else if (username == User.WebUserName && password == User.WebTransientPassword)
				{
					LoggedInUserCore = GetNewContactableForSpecialUser(factory, affiliationCode, User.WebUserName);
				}
				else if (username.Equals(User.SupportUserName, StringComparison.OrdinalIgnoreCase) && CWSupportLoginToken.IsValidToken(password, out var tokenValidationResult))
				{
					LoggedInUserCore = GetNewContactableForSpecialUser(factory, affiliationCode, User.SupportUserName);
					SupportStaffCode = tokenValidationResult.UserCode.ToUpperInvariant();
				}
				else
				{
					LoggedInUserCore = LoginCore(factory, affiliationCode, username, password, loginHash, shouldRecordLoginFailAttempt);
				}

				if (LoggedInUserCore != null)
				{
					var pk = GetBranchPKForLogin();
					if (pk.IsValid && Env.CurrentBranchPK != pk)
					{
						Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, pk.ToGuid(), Env.CurrentDepartmentPK)); // A simple user context change is not applicable here . It needs a full environment replacement.
					}
				}
#if DEBUG && NETFRAMEWORK
				if (WebEnv.AppInstance is Testing.DummyHttpApplication)
				{
					((Testing.DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(this);
				}
#endif
			}
		}

		protected string WebServicesUserName => "WebServicesUser"; // Username should not be translated.

		protected IFactoryProvider WebUserFactory { get; private set; }

		protected virtual void BeginLogin() { }
		protected abstract IContactable LoginCore(BusinessObjectFactory factory, string affiliationCode, string username, string password, byte[] loginHash, bool shouldRecordLoginFailAttempt = true);

		protected abstract IContactable GetNewContactableForSpecialUser(BusinessObjectFactory factory, string affiliationCode, string username);

		public abstract bool AreSecurityRightsGranted(WebSecurityRight securityRights);

		public virtual string AffiliationCode { get { return ""; } }
		public virtual string AffiliationName { get { return ""; } }

		public virtual void Logout()
		{
			LoggedInUserCore = null;
			SupportStaffCode = ZString.Empty;
		}

		public bool IsSuperUser => string.Compare(LoggedInUserName, User.SupportUserName, true) == 0;
		public bool IsWebUser => LoggedInUserName.EqualsIgnoringCase(User.WebUserName);
		public bool IsLoggedIn => LoggedInContactable != null;
		public virtual ZString LoggedInUserName => LoggedInContactable?.Name ?? "";

		/// <summary>
		/// Avoid downcasting the return value to an OrgContact.
		/// Recommended method is to cast this as OrgContactWebUser and use LoggedInOrgContact from that class.
		/// </summary>
		public IContactable LoggedInUser => LoggedInUserCore;

		protected virtual IContactable LoggedInUserCore { get; set; }

		/// <summary>
		/// Returns ZGuid.Empty if not logged in
		/// </summary>
		public ZGuid LoggedInUserPK => LoggedInContactable?.PK ?? ZGuid.Empty;

		/// <summary>
		/// Newer, lighter version of LoggedInUser that may not be an OrgContact, even if this is an OrgContactWebUser, and should not be downcast to one.
		/// </summary>
		public virtual IContactable LoggedInContactable => LoggedInUser;

		public ZString SupportStaffCode
		{
			get;
			private set;
		}

		protected virtual ZGuid GetBranchPKForLogin() => ZGuid.Empty;

#if DEBUG
		public void LoginForTest(string affiliationCode, string username, string password)
		{
			Login(affiliationCode, username, password);
		}

		public void LoginSupportForTest(string affiliationCode)
		{
			Login(affiliationCode, User.SupportUserName, CWSupportLoginToken.TokenForTest);
		}

		public void LoginSupportForTest()
		{
			Login("", User.SupportUserName, CWSupportLoginToken.TokenForTest);
		}
#endif
	}
}
