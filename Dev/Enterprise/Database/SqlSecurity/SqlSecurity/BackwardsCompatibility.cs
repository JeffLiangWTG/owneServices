using System.Linq;
using CargoWise.Common;
using CargoWise.SqlSecurity.Database;
using CargoWise.SqlSecurity.Server;

namespace Enterprise.SqlSecurity
{
	static class BackwardsCompatibility
	{
		public static string PrincipalsAndMembershipsAsSql(SqlServerProposedEntities entities)
		{
			var principalsAndMembershipsAsStrings = entities.Principals.SelectMany(principal =>
			{
				var memberName = principal.MemberName.QuoteEscapedName('\'');
				var memberType = principal.MemberType;
				var isExpirationChecked = Helper.DbValueToNullString(principal.IsExpirationChecked);
				var isPolicyChecked = Helper.DbValueToNullString(principal.IsPolicyChecked);
				var defaultDatabaseName = principal.DefaultDatabaseName.QuoteEscapedName('\'');
				var defaultLanguageName = principal.DefaultLanguageName.QuoteEscapedName('\'');
				var sid = principal.SID == null ? "NULL" : Helper.DbValueToNullString(principal.SID?.Bytes);

				string passwordHashForCreate = "NULL";
				string passwordHashForAlter = "NULL";

				if (principal.PasswordMode == ProposedPasswordMode.CreateOrAlter)
				{
					passwordHashForCreate = Helper.DbValueToNullString(principal.PasswordHash);
					passwordHashForAlter = Helper.DbValueToNullString(principal.PasswordHash);
				}
				else if (principal.PasswordMode == ProposedPasswordMode.CreateOnly)
				{
					passwordHashForCreate = Helper.DbValueToNullString(principal.PasswordHash);
				}

				var memberships = entities.RoleMemberships.Where(membership => membership.MemberPrincipalName == principal.MemberName);

				if (!memberships.Any())
				{
					return new[] { $@"(N'{memberName}', '{memberType}', N'', {isExpirationChecked}, {isPolicyChecked}, N'{defaultDatabaseName}', N'{defaultLanguageName}', {passwordHashForCreate}, {passwordHashForAlter}, {sid})" };
				}
				else
				{
					return memberships.Select(membership =>
					{
						var parentRole = membership.RolePrincipalName.QuoteEscapedName('\'');
						return $@"(N'{memberName}', '{memberType}', N'{parentRole}', {isExpirationChecked}, {isPolicyChecked}, N'{defaultDatabaseName}', N'{defaultLanguageName}', {passwordHashForCreate}, {passwordHashForAlter}, {sid})";
					});
				}
			});

			if (principalsAndMembershipsAsStrings.Any())
			{
				return $@"
SELECT *
FROM
	(VALUES
{string.Join($",{System.Environment.NewLine}\t\t", principalsAndMembershipsAsStrings)}
	) AS proposedPrincipalsAndMemberhips(member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";
			}
			else
			{
				return @"
SELECT TOP 0 *
FROM
	(VALUES
		(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
	) AS proposedPrincipalsAndMemberhips(member_name, member_type, parent_role, is_expiration_checked, is_policy_checked, default_database_name, default_language_name, password_hash_create, password_hash_alter, sid)
";
			}
		}

		public static string PermissionsAsSql(SqlServerProposedEntities entities)
		{
			var permissionsAsStrings = entities.Permissions.Select(permission =>
			{
				var granteeName = permission.GranteeName.QuoteEscapedName('\'');
				var grantorName = permission.GrantorName?.QuoteEscapedName('\'');
				var securableType = permission.SecurableType switch
				{
					// Server permissions are an empty string, rest are identical
					"SERVER" => "",
					_ => permission.SecurableType,
				};
				var securable = permission.Securable?.QuoteEscapedName('\'');
				var permissionName = permission.Permission.QuoteEscapedName('\'');
				return $@"('{permission.State}', '{permissionName}', '{securableType}', '{securable}', '{granteeName}', '{grantorName}')";
			});

			if (permissionsAsStrings.Any())
			{
				return $@"
SELECT *
FROM
	(VALUES
{string.Join($",{System.Environment.NewLine}\t\t", permissionsAsStrings)}
	) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
";
			}
			else
			{
				return @"
SELECT TOP 0 *
FROM
	(VALUES
		(NULL, NULL, NULL, NULL, NULL, NULL)
	) AS PER (State, Permission, SecurableType, Securable, Grantee, Grantor)
";
			}
		}

		public static string DatabasePrincipalsAndMembershipsAsSql(SqlDatabaseProposedEntities entities)
		{
			var principalsAndMembershipsAsStrings = entities.Principals.SelectMany(principal =>
			{
				var memberName = $"N'{principal.MemberName.QuoteEscapedName('\'')}'";
				var memberType = $"'{principal.MemberType}'";
				var defaultSchemaName = principal.DefaultSchemaName != null ? $"N'{principal.DefaultSchemaName?.QuoteEscapedName('\'')}'" : "NULL";

				var memberships = entities.RoleMemberships.Where(membership => membership.MemberPrincipalName == principal.MemberName);

				if (!memberships.Any())
				{
					return new[] { $@"({memberName}, {memberType}, N'', {defaultSchemaName})" };
				}
				else
				{
					return memberships.Select(membership =>
					{
						var parentRole = $"N'{membership.RolePrincipalName.QuoteEscapedName('\'')}'";
						return $@"({memberName}, {memberType}, {parentRole}, {defaultSchemaName})";
					});
				}
			});

			if (principalsAndMembershipsAsStrings.Any())
			{
				return $@"
SELECT *
FROM
	(VALUES
{string.Join($",{System.Environment.NewLine}\t\t", principalsAndMembershipsAsStrings)}
	) AS proposedDatabasePrincipalsAndMemberhips(member_name, member_type, parent_role, default_schema_name)
";
			}
			else
			{
				return @"
SELECT TOP 0 *
FROM
	(VALUES
		(NULL, NULL, NULL, NULL)
	) AS proposedDatabasePrincipalsAndMemberhips(member_name, member_type, parent_role, default_schema_name)
";
			}
		}

		public static string DatabasePermissionsAsSql(SqlDatabaseProposedEntities entities)
		{
			var permissionsAsStrings = entities.Permissions.Select(permission =>
			{
				var granteeName = $"N'{permission.GranteeName.QuoteEscapedName('\'')}'";
				var grantorName = permission.GrantorName != null ? $"N'{permission.GrantorName?.QuoteEscapedName('\'')}'" : "N''";
				var securableType = $"N'{permission.SecurableType}'";
				var securableSchema = permission.SecurableSchema != null ? $"N'{permission.SecurableSchema.QuoteEscapedName('\'')}'" : "N''";
				var securable = permission.Securable != null ? $"N'{permission.Securable.QuoteEscapedName('\'')}'" : "N''";
				var securableColumn = permission.SecurableColumn != null ? $"N'{permission.SecurableColumn.QuoteEscapedName('\'')}'" : "N''";
				var permissionName = $"N'{permission.Permission.QuoteEscapedName('\'')}'";
				var permissionState = $"'{permission.State}'";

				return $@"({permissionState}, {permissionName}, {securableType}, {securableSchema}, {securable}, {securableColumn}, {granteeName}, {grantorName})";
			});

			if (permissionsAsStrings.Any())
			{
				return $@"
SELECT *
FROM
	(VALUES
{string.Join($",{System.Environment.NewLine}\t\t", permissionsAsStrings)}
	) AS DbPer (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
";
			}
			else
			{
				return @"
SELECT TOP 0 *
FROM
	(VALUES
		(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
	) AS DbPer (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
";
			}
		}
	}
}
