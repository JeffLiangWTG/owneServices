using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowFactory
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Task<Dictionary<ZGuid, IWorkflow>> GetMatchingWorkflows(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
		{
			var strategy = band.GetSqlStrategy(parameters);
			var (sql, zParameters) = strategy.GetMatchingWorkflowsSql();
			return GetMatchingWorkflows((connection) => new ZSqlConnectionInfo(connection, null).GetNewDbCommandForSelect(sql, zParameters));
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static Task<Dictionary<ZGuid, IWorkflow>> GetMatchingWorkflows(string sql)
		{
			return GetMatchingWorkflows((connection) => connection.Command(sql)); // Use db connection command as factory cannot execute this sql statement
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Task<Dictionary<ZGuid, IWorkflow>> GetMatchingWorkflows(Func<DbConnection, DbCommand> commandProvider)
		{
			return AsyncStrategy.Default.GetAsync(() => GetMatchingWorkflowsCore(commandProvider));
		}

		static Dictionary<ZGuid, IWorkflow> GetMatchingWorkflowsCore(Func<DbConnection, DbCommand> commandProvider)
		{
			var results = new Dictionary<ZGuid, IWorkflow>();

			using (var command = commandProvider(Db.Connection))
			using (var reader = command.ExecuteReader()) // Using multicore SQL server instead of business objects
			{
				while (reader.Read())
				{
					var pk = (Guid)reader[ProcessHeaderSchema.Constants.PK];
					var releaseGroupPK = reader[ProcessHeaderSchema.Constants.FH_GG_ReleaseGroup] is DBNull
						? Guid.Empty
						: (Guid)reader[ProcessHeaderSchema.Constants.FH_GG_ReleaseGroup];
					var plannedDurationMinutes = (int)reader[ProcessHeaderSchema.Constants.FH_PlannedDurationInMinutes];

					results.Add(pk, new WorkflowDTO(pk)
					{
						ReleaseGroup = releaseGroupPK,
						PlannedDurationMinutes = plannedDurationMinutes,
					});
				}
			}

			return results;
		}
	}
}
