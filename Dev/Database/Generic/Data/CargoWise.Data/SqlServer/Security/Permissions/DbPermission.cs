namespace CargoWise.Data
{
	public sealed class DbPermission
	{
		public DbPermission(DbPermissionState permissionState, DbPermissionType permissionType, DbPermissionClass permissionClass)
		{
			PermissionState = permissionState;
			PermissionType = permissionType;
			PermissionClass = permissionClass;
		}

		public DbPermissionState PermissionState { get; }
		public DbPermissionType PermissionType { get; }
		public DbPermissionClass PermissionClass { get; }

		public void EnsureExists(AdminConnection connection, string dbName, string dbPrincipalName, string permissionClassValue)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				EnsureExists(connection, dbPrincipalName, permissionClassValue);
			}
		}

		public void EnsureExists(AdminConnection connection, string dbPrincipalName, string permissionClassValue)
		{
			if (!SqlSecurityUtils.DbPermission.Exists(connection, dbPrincipalName, permission: this, permissionClassValue))
			{
				SqlSecurityUtils.DbPermission.Create(connection, dbPrincipalName, permission: this, permissionClassValue);
			}
		}

		#region Implementation

		public override string ToString()
		{
			return $"{PermissionState.Description} {PermissionType.Description} ON {PermissionClass.Description}";
		}

		public string ToRevoke()
		{
			return $"REVOKE {PermissionType.Description} ON {PermissionClass.Description}";
		}

		#endregion // Implementation
	}
}
