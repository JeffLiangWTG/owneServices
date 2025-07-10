using System;
using Enterprise.MasterFiles.Business.VersionReport;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	class SystemLicenceServiceForTesting : SystemLicenceService
	{
		public SystemLicenceServiceForTesting()
		{
		}

		protected override bool IsDeveloperEnvironment
		{
			get { return false; }
		}

		protected override ILicenceConsumptionLogProcess CreateLicenceUsageProcess()
		{
			NUnit.Framework.Assertion.AssertNull(UsageProcess);
			UsageProcess = new LicenceConsumptionLogProcessForTest();
			return UsageProcess;
		}

		public int CallsToSendCurrentVersionReport;
		public DateTime LastDateFromUtcInclusive;
		public DateTime LastDateToUtcExclusive;
		public Exception ExceptionToThrow;

		protected override void SendCurrentVersionReport(ILicenceConsumptionLogProcess usageProcess, DateTime dateFromUtcInclusive, DateTime dateToUtcExclusive)
		{
			++CallsToSendCurrentVersionReport;
			LastDateFromUtcInclusive = dateFromUtcInclusive;
			LastDateToUtcExclusive = dateToUtcExclusive;

			if (ExceptionToThrow != null)
			{
				throw ExceptionToThrow;
			}

			base.SendCurrentVersionReport(usageProcess, dateFromUtcInclusive, dateToUtcExclusive);
		}

		public LicenceConsumptionLogProcessForTest UsageProcess;

		public IVersionReportSender BaseSender;
		public TestVersionReportSender TestSender;
		protected override IVersionReportSender CreateVersionReportSender()
		{
			BaseSender = base.CreateVersionReportSender();
			TestSender = new TestVersionReportSender();
			return TestSender;
		}
	}
}
