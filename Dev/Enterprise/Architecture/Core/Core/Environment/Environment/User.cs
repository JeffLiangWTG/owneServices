using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IUser : IPasswordStored, IEquatable<IUser>
	{
		string EmailAddress { get; }
		string Fax { get; }
		string FullName { get; }
		string Initials { get; }
		string InitialsAndDateTime { get; }
		string InitialsAndDateTimeGmt { get; }
		bool IsActive { get; }
		bool IsBatchProcessor { get; }
		bool IsController { get; }
		bool IsDeveloper { get; }
		bool IsDeveloperLogin { get; }
		bool IsDeviceOnly { get; }
		bool CanLogin { get; }
		ZBool IsSupportUser { get; }
		bool IsLockedOut { get; }
		bool IsResource { get; }
		bool IsOperational { get; }
		bool IsSystemAccount { get; }
		bool IsWebUser { get; }
		bool IsTwoFactorAuthenticationEnabled { get; }
		bool LoggedInWithMasterPassword { get; }
		string LoginName { get; }
		Guid PK { get; }
		string Title { get; }
		string WorkPhone { get; }
		bool IsSysAdmin { get; }
		string Language { get; }
		ILoginToken LoginToken { get; set; }
		string ActivityTrackingStatus { get; }
		bool LocalPasswordMustBeReset { get; }
		ZDateTime LastActivityDateTimeUtc { get; }
		bool IsRobot { get; }

		#region Test
#if DEBUG
		User.IsBatchProcessorOverride BatchProcessorOverride { get; set; }
		IDisposable SetIsControllerOverrideForTesting(bool isController);
		IDisposable SetUserEmailAddressInTESTINGOnly(string email);
#endif
		#endregion
	}

	public static class ActivityTrackingStatus
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to translate it")]
		public const string Yes = "YES";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to translate it")]
		public const string No = "NO";
		public const string BasedOnCompany = "CMP";
	}

	public interface ILoginToken
	{
		bool IsDeveloper { get; }
		bool LoggedInWithSupportToken { get; }
		bool ForcedRemoteLogoff { get; }
		string SupportTokenUserCode { get; }
		string SupportTokenUserName { get; }
		string DummyTokenOriginalUserName { get; }
		string DummyTokenOriginalUserEmail { get; }
	}

	public static class User
	{
		#region SuppressResourceStringsCheckRegion

		public const string SupportUserCode = "E";
		public const string SupportUserName = "CWSupport";

		public const string ServiceUserCode = "~BP";
		public const string ServiceUserName = "CWService";
		// TODO: this constant should be removed after EDIFaxGatewayStarter no longer using it (WI00541643).
		public const string ServiceUserPwd = "";

		public const string InterchangeUserCode = "~AD";
		public const string InterchangeUserName = "CWAutoDataImport";

		public const string WebUserCode = "ZZ";
		public const string WebUserName = "CWWeb";
		public static string WebTransientPassword = Guid.NewGuid().ToString();

		public const string PostMasterUserName = "CWPostMaster";

		public const string UnKnownUserCode = "~UK";
		public const string UnKnownUserName = "Unknown User";

		#endregion

		#region LoadUserFromStaffPK
		public static IUser LoadUserFromStaffPK(Guid staffPK)
		{
			if (staffPK == EnvProxy.Instance.CurrentUser?.PK)
			{
				return EnvProxy.Instance.CurrentUser;
			}

			var factory = GetOrCreateFactory();
			var query = new ZQuery(GlbStaffSchema.PK, staffPK);
			return factory.LoadTop1(ObjectFactory.GetType("IGlbStaff"), query) as IUser;
		}
		static BusinessObjectFactory factory;

		static BusinessObjectFactory GetOrCreateFactory()
		{
			factory = factory ?? new BusinessObjectFactory();
			return factory;
		}

		public static void DisposeFactory()
		{
			factory = null;
		}
		#endregion

#if DEBUG

		public static string MasterPassword
		{
			get
			{
				return CWSupportLoginToken.TokenForTest;
			}
		}

		public class IsBatchProcessorOverride : Disposable
		{
			public IsBatchProcessorOverride(IUser userToOverride)
			{
				this.userToOverride = userToOverride;
				userToOverride.BatchProcessorOverride = this;
			}

			readonly IUser userToOverride;

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					userToOverride.BatchProcessorOverride = null;
				}
			}
		}

		public static BusinessObjectFactory Factory
		{
			get { return factory; }
			set { factory = value; }
		}

#endif

	}
}
