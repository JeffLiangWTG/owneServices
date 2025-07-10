using System;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public readonly struct DbLoginInfo
	{
		public DbLoginInfo(string loginName, string type, string defaultDatabase, string loginSid, string pwdHash, bool isOdysseyAdminLogin = false)
		{
			if (type != SqlLoginType && type != WindowsUserLoginType && type != WindowsGroupLoginType)
			{
				throw new ArgumentException(
					Invariant($"Invalid login type [{type}]. Valid values are [{SqlLoginType}, {WindowsUserLoginType}, {WindowsGroupLoginType}]."),
					nameof(type));
			}

			if (type == SqlLoginType)
			{
				if (string.IsNullOrWhiteSpace(loginSid))
				{
					throw new ArgumentException("SID is mandatory for SQL logins.", nameof(loginSid));
				}
			}

			LoginName = loginName;
			Type = type == SqlLoginType ? LoginType.SQL : LoginType.WINDOWS;
			DefaultDatabase = defaultDatabase;
			LoginSid = loginSid;
			PwdHash = pwdHash;
			IsOdysseyAdminLogin = isOdysseyAdminLogin;
		}

		public string LoginName { get; }

		public LoginType Type { get; }

		public string DefaultDatabase { get; }

		public string LoginSid { get; }

		public string PwdHash { get; }

		public bool IsOdysseyAdminLogin { get; }

		const string SqlLoginType = "S";
		const string WindowsUserLoginType = "U";
		const string WindowsGroupLoginType = "G";

		public override bool Equals(object obj)
		{
			return obj is DbLoginInfo info && Equals(info);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 991;
				hashCode = (hashCode * 997) + (LoginName == null ? 0 : LoginName.GetHashCode());
				hashCode = (hashCode * 997) + Type.GetHashCode();
				hashCode = (hashCode * 997) + (DefaultDatabase == null ? 0 : DefaultDatabase.GetHashCode());
				hashCode = (hashCode * 997) + (LoginSid == null ? 0 : LoginSid.GetHashCode());
				return hashCode;
			}
		}

		public bool Equals(DbLoginInfo other)
		{
			return
				LoginName == other.LoginName
				&& Type == other.Type
				&& DefaultDatabase == other.DefaultDatabase
				&& LoginSid == other.LoginSid;
		}

		public static bool operator ==(DbLoginInfo login1, DbLoginInfo login2)
		{
			return login1.Equals(login2);
		}

		public static bool operator !=(DbLoginInfo login1, DbLoginInfo login2)
		{
			return !login1.Equals(login2);
		}

		public enum LoginType
		{
			SQL,
			WINDOWS,
		}
	}
}
