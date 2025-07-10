using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Client.TNT.ServiceTasks.Testing.TNTTestFileUtils;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	[TestedType(typeof(QuantumDataImportServiceTask))]
	class QuantumDataImportServiceTaskTest : TNTServiceTaskTest<QuantumDataImportServiceTask>
	{
		protected override void TestRunServiceTaskCore()
		{
			TNTDataRegistry.Instance.QuantumFileProcessedDirectory = ZString.Empty;
			TNTDataRegistry.Instance.OutTurnFileSourceDirectory = ZString.Empty;
			TNTDataRegistry.Instance.TNTReplyDirectory = ZString.Empty;
			InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories are not set", true, ServiceTask.ServiceLogger.ToString().Contains(QuantumDataImportServiceTask.ErrorMessage));
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			TNTDataRegistry.Instance.QuantumFileProcessedDirectory = "Abc";
			TNTDataRegistry.Instance.QuantumFileSourceDirectory = "efg";
			TNTDataRegistry.Instance.TNTReplyDirectory = "ace";
			RunTaskSchedule(ServiceTask);
			AssertEquals("Logger should contain the error message as directories do not exist", true, ServiceTask.ServiceLogger.ToString().Contains(QuantumDataImportServiceTask.ErrorMessage));
			TestHelper.SetupQuantumDataImportRegistries();
			((TestServiceLogger)ServiceTask.ServiceLogger).ClearLog();
			ServiceTask.RunTask();
			AssertEquals("Logger should not contain the error message as all registries are set properly", true, !ServiceTask.ServiceLogger.ToString().Contains(QuantumDataImportServiceTask.ErrorMessage));
			AssertEquals("contain file not found message", true, ServiceTask.ServiceLogger.ToString().Contains("	...no files found for processing at this time"));
		}

		public void TestImportWithErrors()
		{
			ZString fileWithErrors = "BNE.X1.20040601.095320.ok";
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var sourcePath = resourceRetriever.SaveResourceToFile(GetDataImportExportFilePath(fileWithErrors));
				var destPath = Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, fileWithErrors);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			int noOfShipmentsBeforeImport = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			InitialiseAndRunTaskSchedule(ServiceTask);
			ZString notificationStr = ServiceTask.ServiceLogger.ToString();
			AssertEquals("expected message during import process", true, notificationStr.Contains("1 file(s) found in Quantum directory"));
			AssertEquals("expected message during import process", true, notificationStr.Contains("Importing Data from file: " + fileWithErrors));
			AssertEquals("no of shipment is unchanged", noOfShipmentsBeforeImport, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals("notification should contain error message", true, notificationStr.Contains("Error(s) occurred while importing data from file: " + fileWithErrors));
			AssertEquals("source directory doesn't contain the source file", true, !File.Exists(Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, fileWithErrors)));
			AssertEquals("source directory contains the renamed source file", true, File.Exists(Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, "BNE.X1.20040601.095320" + ".err")));
		}

		[TestDate(2009, 12, 23, 4, 56, 12)]
		public void TestValidImport()
		{
			ZString invalidFile = "JJJ.ok";
			ZString validFile = "BNE.x1.20070528.210700.ok";
			int noOfShipmentsBeforeImport = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var sourcePath = resourceRetriever.SaveResourceToFile(GetDataManipulationFilePath(validFile));
				var destPath = Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, validFile);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
				sourcePath = resourceRetriever.SaveResourceToFile(GetDataManipulationFilePath(invalidFile));
				destPath = Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, invalidFile);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			InitialiseAndRunTaskSchedule(ServiceTask);
			int noOfShipmentAfterImport = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			AssertEquals("No of shipment ", noOfShipmentsBeforeImport + 2, noOfShipmentAfterImport);
			ZString notificationStr = ServiceTask.ServiceLogger.ToString();
			AssertEquals("expected message during import process", true, notificationStr.Contains("2 file(s) found in Quantum directory"));
			AssertEquals("expected message during import process", true, notificationStr.Contains("Importing Data from file: " + validFile));
			AssertEquals("source directory doesn't contain the source file", true, !File.Exists(Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, validFile)));
			ZString processedFilePath = Path.Combine(TNTDataRegistry.Instance.QuantumFileProcessedDirectory, "20091223");
			AssertEquals("processed directory contains the source file", true, File.Exists(Path.Combine(processedFilePath, validFile + ".20091223.045612")));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		QuantumDataImportServiceTask ServiceTask
		{
			get
			{
				return serviceTask ?? (serviceTask = new QuantumDataImportServiceTask());
			}
		}

		QuantumDataImportServiceTask serviceTask;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			TestHelper.SetupQuantumDataImportRegistries();
			TestHelper.CreateNotificationGroupForSendingEmails();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			TestHelper.TidyUp();
		}

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
