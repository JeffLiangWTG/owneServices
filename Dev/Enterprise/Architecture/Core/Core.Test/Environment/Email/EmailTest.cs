using System;
using CargoWise.Data;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public abstract class EmailTest : TransactionedTestCase
	{
		protected int UserCount;

		protected void InsertUser(Guid staffPK, string emailAddress, bool isActive = true)
		{
			UserCount++;
			if (UserCount > 9)
			{
				UserCount = 0;
			}

			string sql = "insert into dbo.GlbStaff (GS_PK, GS_EmailAddress, GS_Code, GS_LoginName, GS_IsActive, GS_IsController, GS_IsDeveloper, GS_IsSystemAccount, GS_FullName, GS_UserAddress1, GS_City, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (";
			sql += "'" + staffPK + "', '" + emailAddress + "', 'T" + UserCount.ToString() + "', 'Test" + UserCount.ToString() + "', " + (isActive ? "1" : "0") + ", 0, 0, 0, 'John Test User', 'Address', 'Gotham', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected void InsertGroupLink(Guid staffPK, Guid groupPK)
		{
			string sql = "insert into dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), ";
			sql += "'" + groupPK + "', '" + staffPK + "')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected int GroupCount;
		protected void InsertGroup(Guid groupPK)
		{
			GroupCount++;
			if (GroupCount > 9)
			{
				GroupCount = 0;
			}

			string sql = "insert into dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES (";
			sql += "'" + groupPK + "', 'T$" + GroupCount.ToString() + "', 'Test', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected void DeleteStmDataNotificationRow()
		{
			((IRegistryItemInternals)EnvProxy.Instance.Registry.RawRegistry.NotificationGroup).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)EnvProxy.Instance.Registry.RawRegistry.NotificationGroup).DeleteValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		protected void InsertStmDataNotificationRow(Guid pK)
		{
			EnvProxy.Instance.Registry.RawRegistry.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pK);
		}

		protected void InsertCompanyStmDataNotificationRow(Guid pK)
		{
			EnvProxy.Instance.Registry.RawRegistry.NotificationGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, pK);
		}
	}
}
