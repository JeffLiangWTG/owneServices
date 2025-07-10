using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Common.AuditLogs
{
	public class AuditLogger : IAuditLogger
	{
		const string LogTable = "StmALog";
		readonly IRowRepository rowRepository;
		readonly Table table;

		public AuditLogger(IRowRepository rowRepository)
		{
			Argument.NotNull(rowRepository, nameof(rowRepository));

			this.rowRepository = rowRepository;
			table = Table.Get(LogTable);
		}

		#region IEntityLogger Members

		public void LogAction(IEntity entity, string actionCode)
		{
			Argument.NotNull(entity, nameof(entity));
			Argument.NotNull(entity.Definition, nameof(entity.Definition));
			Argument.NotNullOrEmpty(actionCode, nameof(actionCode));

			var userCode = Env.CurrentUser != null ? new ZString(Env.CurrentUser.Initials) : ZString.Empty;
			var departmentCode = Env.CurrentDepartment != null ? new ZString(Env.CurrentDepartment.Code) : ZString.Empty;
			var branchCode = Env.CurrentBranch != null ? new ZString(Env.CurrentBranch.Code) : ZString.Empty;

			var auditLog = new AuditLog
			{
				Code = actionCode,
				EventTime = ZDateTime.Now,
				PostedTime = ZDateTime.UtcNow,
				UserCode = userCode,
				DepartmentCode  = departmentCode,
				BranchCode = branchCode,
				ParentId = entity.InternalPK,
				TableName = entity.Definition.TableName
			};

			AddLogToDb(entity, auditLog);
		}

		void AddLogToDb(IEntity entity, AuditLog auditLog)
		{
			var row = rowRepository.Create(table);
			row["SL_IsEstimate"] = false;
			row["SL_IsCancelled"] = false;
			row["SL_FireWorkflow"] = true;
			row["SL_Reference"] = entity.Action.Code();
			row["SL_PostedTimeUtc"] = auditLog.PostedTime.ToDateTime();
			row["SL_EventTime"] = auditLog.EventTime.ToDateTime();
			row["SL_GS_NKUser"] = auditLog.UserCode;
			row["SL_GE_NKDepartment"] = auditLog.DepartmentCode;
			row["SL_GB_NKBranch"] = auditLog.BranchCode;
			row["SL_Table"] = auditLog.TableName;
			row["SL_Parent"] = auditLog.ParentId;
			row["SL_SE_NKEvent"] = auditLog.Code;
		}

		#endregion
	}
}
