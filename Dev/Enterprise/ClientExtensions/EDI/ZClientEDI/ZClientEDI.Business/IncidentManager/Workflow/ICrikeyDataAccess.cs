using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface ICrikeyDataAccess
	{
		void AddToSchedule(EDIShelvesetInfo shelf);
		void RemoveFromSchedule(EDIShelvesetInfo shelf);
		bool IsShelfScheduled(EDIShelvesetInfo shelf);
		void UpdateStatus(EDIShelvesetInfo shelf, string status);
		IList<EDIShelvesetInfo> GetScheduledShelvesByStatus(string status, BusinessObjectFactory factory);
		EDIShelvesetInfo LoadShelfByProcessTask(WorkItemProcessTask processTask);
	}
}
