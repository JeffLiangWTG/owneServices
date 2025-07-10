using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class ShelvesetTools
	{
		public static IList<EDIShelvesetInfo> GetScheduledShelvesByStatus(string status, BusinessObjectFactory factory)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				return CrikeyDataAccessFactory.GetInstance(connection).GetScheduledShelvesByStatus(status, factory);
			}
		}

		public static EDIShelvesetInfo LoadShelfByProcessTask(WorkItemProcessTask processTask)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				return CrikeyDataAccessFactory.GetInstance(connection).LoadShelfByProcessTask(processTask);
			}
		}
	}
}
