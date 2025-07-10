using System.Collections.Generic;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	public class LeaveModifiedResponse
	{
		public string Status;
		public IList<string> ErrorMessages;
		public IList<EmployeeLeave> Output;
		public string RequestJson;
	}
}
