using System;
using Enterprise.MasterFiles.Business.UserAccountReport;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	sealed class TestUserAccountReportSender : IUserAccountReportSender
	{
		public int SendUserAccountCalls;
		public UserAccountReport SendReport(DateTime lastRunTime)
		{
			SendUserAccountCalls++;
			return null;
		}
	}
}
