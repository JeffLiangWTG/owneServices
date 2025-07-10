using System.Data;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Common.AuditLogs
{
	public class LogFieldUpdater
	{
		public void Update(DataRow row, Table table, EntityAction action)
		{
			var prefix = table.Prefix;
			var time = ZDateTime.UtcNow;
			var userCode = Env.CurrentUser != null ? new ZString(Env.CurrentUser.Initials) : ZString.Empty;

			if (action == EntityAction.INSERT)
			{
				var createTimePropertyName = prefix + "_" + "SystemCreateTimeUtc";
				if (row.Table.Columns.IndexOf(createTimePropertyName) != -1)
				{
					row[createTimePropertyName] = time;
				}

				var createUserPropertyName = prefix + "_" + "SystemCreateUser";
				if (row.Table.Columns.IndexOf(createUserPropertyName) != -1)
				{
					row[createUserPropertyName] = userCode;
				}

				var createBranchPropertyName = prefix + "_" + "SystemCreateBranch";
				if (row.Table.Columns.IndexOf(createBranchPropertyName) != -1)
				{
					row[createBranchPropertyName] = Env.CurrentBranch != null ? new ZString(Env.CurrentBranch.Code) : ZString.Empty;
				}

				var createDepartmentPropertyName = prefix + "_" + "SystemCreateDepartment";
				if (row.Table.Columns.IndexOf(createDepartmentPropertyName) != -1)
				{
					row[createDepartmentPropertyName] = Env.CurrentDepartment != null ? new ZString(Env.CurrentDepartment.Code) : ZString.Empty;
				}
			}

			if (row.RowState != DataRowState.Unchanged)
			{
				var lastEditTimePropertyName = prefix + "_" + "SystemLastEditTimeUtc";
				if (row.Table.Columns.IndexOf(lastEditTimePropertyName) != -1)
				{
					row[lastEditTimePropertyName] = time;
				}

				var lastEditUserPropertyName = prefix + "_" + "SystemLastEditUser";
				if (row.Table.Columns.IndexOf(lastEditUserPropertyName) != -1)
				{
					row[lastEditUserPropertyName] = userCode;
				}
			}

			return;
		}
	}
}
