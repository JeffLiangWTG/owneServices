using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IScheduledReportHelper
	{
		List<string> GetScheduleReportsDescriptionAssignedToUser(string userCode, bool mustBeActive, int maxCount = 0);

		string GetPrintUserSafe(ZBlob scheduleReport);

		byte[] SerializeOrgCodeInScheduledReportsByPK(ZBlob scheduleReport, ZGuid parentID, List<string> filterDisplayNameList);
	}
}
