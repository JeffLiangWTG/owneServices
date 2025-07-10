using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Client.TNT.ServiceTasks.Testing.TNTTestFileUtils;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	[TestedType(typeof(NADImportServiceTask))]
	class NADImportServiceTaskTest : TNTServiceTaskTest<NADImportServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		protected override void TestRunServiceTaskCore()
		{
			TNTDataRegistry.Instance.NADFileSourceDirectory = ZString.Empty;
			TNTDataRegistry.Instance.NADFileProcessedDirectory = ZString.Empty;
			TNTDataRegistry.Instance.NADFileExtension = ZString.Empty;
			InitialiseTaskSchedule(ServiceTask, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories are not set", true, ServiceTask.ServiceLogger.ToString().Contains(NADImportServiceTask.ErrorMessage));
			TNTDataRegistry.Instance.NADFileExtension = "OK";
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories do not exist", true, ServiceTask.ServiceLogger.ToString().Contains(NADImportServiceTask.ErrorMessage));
			TestHelper.SetupNADDataImportRegistries();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var fileName = "TNTMVS1.20030520051205NADUPDCMS.NADUPD";
				var sourcePath = resourceRetriever.SaveResourceToFile(GetNADInterfaceFilePath(fileName));
				var destPath = Path.Combine(TNTDataRegistry.Instance.NADFileSourceDirectory, fileName);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger shouldn't contain the error message as directories are all set properly", true, !ServiceTask.ServiceLogger.ToString().Contains(NADImportServiceTask.ErrorMessage));
			AssertEquals("Logger should contain this message", true, ServiceTask.ServiceLogger.ToString().Contains("1 NAD file(s) found in directory"));
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var fileName = "TNTMVS1.20030520051205NADUPDCMS.NADUPD";
				var sourcePath = resourceRetriever.SaveResourceToFile(GetNADInterfaceFilePath(fileName));
				var destPath = Path.Combine(TNTDataRegistry.Instance.NADFileSourceDirectory, fileName);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);

				fileName = "TNTMVS1.20030520051205NADUPDCMS.test";
				sourcePath = resourceRetriever.SaveResourceToFile(GetNADInterfaceFilePath(fileName));
				destPath = Path.Combine(TNTDataRegistry.Instance.NADFileSourceDirectory, fileName);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain this message", true, ServiceTask.ServiceLogger.ToString().Contains("2 file(s) found in directory but 1 file(s) determined as NOT NAD files"));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			TestHelper.TidyUp();
		}

		NADImportServiceTask ServiceTask
		{
			get
			{
				return serviceTask ?? (serviceTask = new NADImportServiceTask());
			}
		}

		NADImportServiceTask serviceTask;
		TNTTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TNTTestHelper(Factory));
			}
		}

		TNTTestHelper testHelper;
	}
}
