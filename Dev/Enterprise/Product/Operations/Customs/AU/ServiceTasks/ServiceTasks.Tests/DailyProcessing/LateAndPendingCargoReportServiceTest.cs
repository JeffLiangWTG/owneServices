using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(LateAndPendingCargoReportService))]
	sealed class LateAndPendingCargoReportServiceTest : BaseMultiCompanyCustomsMessagingServiceTest<LateAndPendingCargoReportService>
	{
		public void TestMessageProcessorServiceOverride()
		{
			AssertEquals(typeof(CargoReportWorkflow), testServiceTask.ProcessTypeForTesting);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
