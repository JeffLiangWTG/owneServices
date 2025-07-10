using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.DbUpgrader.Startup
{
	class ServiceTasksSchedulesCleaner
	{
		public void DeleteSchedulesOfDecommissionedServiceTask(IUpgradeTaskWorkflowLogger logger)
		{
			var allServiceCodes = GetAllCurrentServiceCodes();

			if (allServiceCodes.Count > 0)
			{
				logger.StartTask("Beginning to delete old schedules for decommissioned service tasks.");
				var deletedTasks = DeleteDecommissionedServiceTasks(allServiceCodes);
				logger.StartSubtask(deletedTasks.Count > 0
					? FormattableString.Invariant($"Decommissioned schedule(s): {string.Join(", ", deletedTasks)} are deleted.")
					: "No decommissioned schedules deleted.");
			}
		}

		internal List<string> DeleteDecommissionedServiceTasks(IEnumerable<string> allCodes)
		{
			var allCodesParam = "@allCodes";
			var sql = FormattableString.Invariant(
				$@"DELETE dbo.StmServiceTask OUTPUT deleted.SST_ServiceTaskCode WHERE SST_ServiceTaskCode NOT IN (SELECT value FROM {allCodesParam});
DELETE dbo.StmScheduleTask WHERE S5_ParentTableCode='SH' AND S5_ScheduleType NOT IN (SELECT value FROM {allCodesParam});");  // deletion from old table is kept since we will need to patchback the change to GP1 and GP2
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddTableValuedParameter(allCodesParam, StmServiceTaskSchema.SST_ServiceTaskCode, allCodes);
				return DataUtils.GetListOfValuesFromCommand(cmd).ToList();
			}
		}

		internal List<string> GetAllCurrentServiceCodes()
		{
			var result = new List<string>();

			var metaDataReader = ObjectFactory.Get<IAssemblyMetaDataReader>();
			var hostedServiceAttributes = metaDataReader.GetAttributes<HostedServiceAttribute>(retrieveForAllClients: false);

			foreach (var hostedServiceAttribute in hostedServiceAttributes)
			{
				result.Add(hostedServiceAttribute.Code);
			}

			return result;
		}
	}
}
