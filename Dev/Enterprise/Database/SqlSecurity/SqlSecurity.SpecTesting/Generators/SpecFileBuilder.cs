using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.SqlSecurity.Common;
using CargoWise.SqlSecurity.Database;
using CargoWise.SqlSecurity.Server;
using Enterprise.MasterFiles.Business;
using Enterprise.SqlSecurity.Test;
using Moq;
using WTG.TestHelpers.SpecTesting;
using TestHelper = Enterprise.SqlSecurity.Test.Helper;

namespace Enterprise.SqlSecurity.SpecTesting
{
	public class DatabasePrincipal : IComparable<DatabasePrincipal>
	{
		[TableColumnName("Member Type")]
		public string MemberType { get; set; }
		[TableColumnName("Member Name")]
		public string MemberName { get; set; }
		[TableColumnName("Default Schema")]
		public string DefaultSchema { get; set; }
		[TableColumnName("Parent Roles")]
		public string ParentRoles { get; set; }

		public int CompareTo(DatabasePrincipal other)
		{
			return (MemberType, MemberName).CompareTo((other.MemberType, other.MemberName));
		}
	}

	public class DatabasePermission : IComparable<DatabasePermission>
	{
		[TableColumnName("State")]
		public string State { get; set; }
		[TableColumnName("Permission")]
		public string Permission { get; set; }
		[TableColumnName("Grantee")]
		public string Grantee { get; set; }
		[TableColumnName("Securable Type")]
		public string SecurableType { get; set; }
		[TableColumnName("Securable Schema")]
		public string SecurableSchema { get; set; }
		[TableColumnName("Securable")]
		public string Securable { get; set; }
		[TableColumnName("Securable Column")]
		public string SecurableColumn { get; set; }
		[TableColumnName("Grantor")]
		public string Grantor { get; set; }

		public int CompareTo(DatabasePermission other)
		{
			return (Grantee, SecurableType, SecurableSchema, Securable, SecurableColumn, Permission).CompareTo((other.Grantee, other.SecurableType, other.SecurableSchema, other.Securable, other.SecurableColumn, other.Permission));
		}
	}

	public class ServerPrincipal : IComparable<ServerPrincipal>
	{
		[TableColumnName("Member Name")]
		public string MemberName { get; set; }
		[TableColumnName("Member Type")]
		public string MemberType { get; set; }
		[TableColumnName("Is Expiration Checked")]
		public string IsExpirationChecked { get; set; }
		[TableColumnName("Is Policy Checked")]
		public string IsPolicyChecked { get; set; }
		[TableColumnName("Default Database")]
		public string DefaultDatabase { get; set; }
		[TableColumnName("Default Language")]
		public string DefaultLanguage { get; set; }
		[TableColumnName("Parent Roles")]
		public string ParentRoles { get; set; }

		public int CompareTo(ServerPrincipal other)
		{
			return (MemberType, MemberName).CompareTo((other.MemberType, other.MemberName));
		}
	}

	public class ServerPermission : IComparable<ServerPermission>
	{
		[TableColumnName("State")]
		public string State { get; set; }
		[TableColumnName("Permission")]
		public string Permission { get; set; }
		[TableColumnName("Grantee")]
		public string Grantee { get; set; }
		[TableColumnName("Securable Type")]
		public string SecurableType { get; set; }
		[TableColumnName("Securable")]
		public string Securable { get; set; }
		[TableColumnName("Grantor")]
		public string Grantor { get; set; }

		public int CompareTo(ServerPermission other)
		{
			return (Grantee, SecurableType, Securable, Permission).CompareTo((other.Grantee, other.SecurableType, other.Securable, other.Permission));
		}
	}

	class DatabaseLevelInfoForSpec
	{
		public DatabasePrincipal[] Principals { get; set; }
		public DatabasePermission[] Permissions { get; set; }
	}

	class ServerLevelInfoForSpec
	{
		public ServerPrincipal[] Principals { get; set; }
		public ServerPermission[] Permissions { get; set; }
	}

	public static class SpecFileBuilder
	{
		static IStaffInfoProvider GetStaffInfoProviderMockFromStaffInfoCollection(IEnumerable<DbUserManager.StaffLoginInfo> staffInfoCollection)
		{
			var staffInfoProviderMock = new Mock<IStaffInfoProvider>();
			staffInfoProviderMock
				.Setup(provider => provider.GetStaffLoginsInfo())
				.Returns(staffInfoCollection);

			return staffInfoProviderMock.Object;
		}

		static string[] GetParentRolesForMember(string memberName, SqlRoleMembership[] memberships)
		{
			return memberships.Where(m => m.MemberPrincipalName == memberName).Select(m => m.RolePrincipalName).ToArray();
		}

		static DatabaseLevelInfoForSpec ProcessDatabaseSecurityLists(SqlDatabaseProposedEntities result)
		{
			var principals = result.Principals
				.Select(p =>
					new DatabasePrincipal()
					{
						MemberName = p.MemberName,
						MemberType = p.MemberType.ToString(),
						DefaultSchema = p.DefaultSchemaName ?? "",
						ParentRoles = string.Join(", ", GetParentRolesForMember(p.MemberName, result.RoleMemberships)),
					}
				)
				.OrderBy((val) => val)
				.ToArray();

			var permissions = result.Permissions
				.Select((p) => new DatabasePermission()
				{
					State = p.State.ToString(),
					Permission = p.Permission,
					SecurableType = p.SecurableType,
					SecurableSchema = p.SecurableSchema ?? "",
					Securable = p.Securable ?? "",
					SecurableColumn = p.SecurableColumn ?? "",
					Grantee = p.GranteeName,
					Grantor = p.GrantorName ?? "",
				})
				.OrderBy((val) => val)
				.ToArray();

			return new DatabaseLevelInfoForSpec()
			{
				Principals = principals.ToArray(),
				Permissions = permissions.ToArray(),
			};
		}

		static ServerLevelInfoForSpec ProcessServerSecurityLists(SqlServerProposedEntities result)
		{
			var principals = result.Principals
				.Select(p =>
					new ServerPrincipal()
					{
						MemberName = p.MemberName,
						MemberType = p.MemberType.ToString(),
						ParentRoles = string.Join(", ", GetParentRolesForMember(p.MemberName, result.RoleMemberships)),
						IsExpirationChecked = p.IsExpirationChecked.HasValue ? (p.IsExpirationChecked.Value ? "Y" : "N") : "",
						IsPolicyChecked = p.IsPolicyChecked.HasValue ? (p.IsPolicyChecked.Value ? "Y" : "N") : "",
						DefaultDatabase = p.DefaultDatabaseName ?? "",
						DefaultLanguage = p.DefaultLanguageName ?? "",
					}
				)
				.OrderBy((val) => val)
				.ToArray();

			var permissions = result.Permissions
				.Select((p) => new ServerPermission()
				{
					State = p.State.ToString(),
					Permission = p.Permission,
					SecurableType = p.SecurableType,
					Securable = p.Securable ?? "",
					Grantee = p.GranteeName,
					Grantor = p.GrantorName ?? "",
				})
				.OrderBy((val) => val)
				.ToArray();

			return new ServerLevelInfoForSpec()
			{
				Principals = principals.ToArray(),
				Permissions = permissions.ToArray(),
			};
		}

		static string SerializeDatabaseTables(DatabaseLevelInfoForSpec groups, string prefix)
		{
			var tables = $"# Principals\r\n{SpecTableGen.ConvertArrayToTable(groups.Principals)}\r\n\r\n# Permissions\r\n{SpecTableGen.ConvertArrayToTable(groups.Permissions)}\n";
			return prefix + tables;
		}

		static string SerializeServerTables(ServerLevelInfoForSpec groups, string prefix)
		{
			var tables = $"# Principals\r\n{SpecTableGen.ConvertArrayToTable(groups.Principals)}\r\n\r\n# Permissions\r\n{SpecTableGen.ConvertArrayToTable(groups.Permissions)}\n";
			return prefix + tables;
		}

		public static string GetSpecTextForDatabasePrincipals(AdminConnection adminConnection, DatabaseType databaseType, DatabaseTestMode databaseTestMode, string databaseName, IEnumerable<DbUserManager.StaffLoginInfo> staffInfoCollection)
		{
			var isHostedInWiseCloud = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer;
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(TestHelper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection));

				var result = securityBuilder.GetDatabaseLevelInfoWithNewBuilder(adminConnection, adminConnection, databaseType);
				var prefix = $"# Database type: {databaseType}\r\n# Database test mode: {databaseTestMode}\r\n\r\n";
				var list = ProcessDatabaseSecurityLists(result);
				return SerializeDatabaseTables(list, prefix);
			}
		}

		public static string GetSpecTextForServerPrincipals(AdminConnection adminConnection, DatabaseTestMode databaseTestMode, IEnumerable<DbUserManager.StaffLoginInfo> staffInfoCollection)
		{
			var isHostedInWiseCloud = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer;
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(TestHelper.GetProductRegistractionMock(databaseTestMode)))
			{
				var securityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, GetStaffInfoProviderMockFromStaffInfoCollection(staffInfoCollection));

				var result = securityBuilder.GetServerLevelInfoWithNewBuilder(adminConnection);
				var prefix = $"# Database test mode: {databaseTestMode}\r\n\r\n";
				var list = ProcessServerSecurityLists(result);
				return SerializeServerTables(list, prefix);
			}
		}
	}
}
