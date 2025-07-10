using System.Collections.Generic;
using Enterprise.Client.YAS.ServiceTasks.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.YAS.ServiceTasks.ProofOfDeliveryInterface.Testing
{
	[TestedType(typeof(PODDataImportServiceTask))]
	internal class PODDataImportServiceTaskTest : ServiceTaskTestCase<PODDataImportServiceTask>
	{
		public void TestRunTask()
		{
			TestHelper.SetValidRegistryAll();

			PODDataImportServiceTask task = new PODDataImportServiceTask();
			InitialiseAndRunTaskSchedule(task);
			RunTaskSchedule(task);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		protected override void TearDownCore()
		{
			TestHelper.TidyUp();
			base.TearDownCore();
		}

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper()); }
		}
		YASTestHelper testHelper;
	}
}
