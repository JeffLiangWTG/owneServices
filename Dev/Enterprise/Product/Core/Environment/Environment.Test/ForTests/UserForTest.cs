#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.Environment
{
	public class UserForTest : IUser
	{
		public bool LoggedInWithMasterPassword { get; set; }
		public bool IsOperational { get; set; }

		#region IUser Members

		public string ActivityTrackingStatus { get; set; }

		public User.IsBatchProcessorOverride BatchProcessorOverride
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string EmailAddress
		{
			get;
			set;
		}

		public string Fax
		{
			get;
			set;
		}

		public string FullName
		{
			get;
			set;
		}

		public string Initials
		{
			get;
			set;
		}

		public string InitialsAndDateTime
		{
			get;
			set;
		}

		public string InitialsAndDateTimeGmt
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public bool IsBatchProcessor
		{
			get;
			set;
		}

		public bool CanLogin
		{
			get;
			set;
		}

		public bool IsController
		{
			get;
			set;
		}

		public bool IsRobot
		{
			get;
			set;
		}

		public bool IsSysAdmin
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IDisposable SetIsControllerOverrideForTesting(bool isController)
		{
			throw new NotImplementedException();
		}

		public bool IsDeveloper
		{
			get;
			set;
		}

		public bool IsDeveloperLogin
		{
			get;
			set;
		}

		public bool IsDeviceOnly
		{
			get;
			set;
		}

		public ZBool IsSupportUser
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsLockedOut
		{
			get;
			set;
		}

		public bool IsResource
		{
			get;
			set;
		}

		public bool IsSystemAccount
		{
			get;
			set;
		}

		public bool IsWebUser
		{
			get;
			set;
		}

		public string LoginName
		{
			get;
			set;
		}

		public Guid PK => Guid.NewGuid();

		public bool VerifyPassword(IUserSecretsContext userSecretsContext, string password)
		{
			throw new NotImplementedException();
		}

		public int PasswordHashIterations { get; }
		public ZBlob PasswordSalt { get; }
		public ZBlob PasswordHash { get; }

		public IDisposable SetUserEmailAddressInTESTINGOnly(string email)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IUser other)
		{
			return this.PK.Equals(other.PK);
		}

		public string Title
		{
			get;
			set;
		}

		public string WorkPhone
		{
			get;
			set;
		}

		public string Language
		{
			get;
			set;
		}

		#endregion

		public ILoginToken LoginToken
		{
			get;
			set;
		}

		public bool IsTwoFactorAuthenticationEnabled
		{
			get
			{
				return false;
			}
		}

		public bool LocalPasswordMustBeReset => false;

		ZGuid IIdentified.Identifier => PK;

		ZDateTime IUser.LastActivityDateTimeUtc => ZDateTime.UtcNow;
	}
}

#endif
