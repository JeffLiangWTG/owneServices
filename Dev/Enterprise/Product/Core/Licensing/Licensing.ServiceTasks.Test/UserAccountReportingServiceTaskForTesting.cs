using Enterprise.MasterFiles.Business.UserAccountReport;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	class UserAccountReportingServiceTaskForTesting : UserAccountReportingServiceTask
	{
		public UserAccountReportingServiceTaskForTesting()
		{
		}

		public TestUserAccountReportSender TestSender;
		protected override IUserAccountReportSender CreateUserAccountReportSender()
		{
			TestSender = new TestUserAccountReportSender();
			return TestSender;
		}

		protected override bool IsDeveloperEnvironment => false;
	}
}
