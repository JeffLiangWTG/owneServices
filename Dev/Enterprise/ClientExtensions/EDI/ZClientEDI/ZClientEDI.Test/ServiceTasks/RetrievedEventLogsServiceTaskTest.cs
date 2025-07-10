using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	[TestedType(typeof(RetrievedEventLogsServiceTask))]
	public class RetrievedEventLogsServiceTaskTest : ServiceTaskTestCase<RetrievedEventLogsServiceTask>
	{
		[TestDate(2014, 7, 3, 14, 0, 0)]
		[UseSnapshotProtection]
		public void TestEndToEndWithMockRetrieverExpectedAddedToDatabase()
		{
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput.txt")))
			{
				var eventLogRetriever = new Mock<IEventLogRetriever>();
				eventLogRetriever.Setup(m => m.Retrieve(It.IsAny<string>(), It.IsAny<EventLogDataTransmissionHandler>(), It.IsAny<ILogger>()))
					.Returns(XElement.Load(stream));
				var serviceTask = new Mock<RetrievedEventLogsServiceTask>() { CallBase = true };
				serviceTask.Setup(m => m.GetNewEventLogRetriever()).Returns(eventLogRetriever.Object);
				serviceTask.Setup(m => m.MachineNames).Returns(new string[] { System.Environment.MachineName });
				var logger = new LoggerForTest();
				serviceTask.Object.ServiceLogger = logger;
				serviceTask.Object.RunTask();
				var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery()
				{ OrderBy = EdiHelpErrorLog.Schema.HE_LastReported + " desc" });
				AssertContains("The process was terminated due to an unhandled exception.", log.HE_ExceptionMessage);
				AssertContainsExactElementsInAnyOrder(logger.LogEntries, new[] { $"Issue {log.HE_IssueNumber} logged from machine {System.Environment.MachineName}", $"1 issues, 0 invalid events, {0} skipped from machine {System.Environment.MachineName}" });
			}
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput.txt")))
			{
				var eventLogRetriever = new Mock<IEventLogRetriever>();
				eventLogRetriever.Setup(m => m.Retrieve(It.IsAny<string>(), It.IsAny<EventLogDataTransmissionHandler>(), It.IsAny<ILogger>()))
					.Returns(XElement.Load(stream));
				var serviceTask = new Mock<RetrievedEventLogsServiceTask>() { CallBase = true };
				serviceTask.Setup(m => m.GetNewEventLogRetriever()).Returns(eventLogRetriever.Object);
				serviceTask.Setup(m => m.MachineNames).Returns(new string[] { System.Environment.MachineName });
				var logger = new LoggerForTest();
				serviceTask.Object.ServiceLogger = logger;
				AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
				using (ClearUserContext())
				using (Env.Instance.TemporaryServiceTaskContext(serviceTask.Object.GetType().Name, canRunInAnyBranch: true))
				{
					AssertNoExceptionThrown(() => serviceTask.Object.RunTask());
				}
				AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestEndToEnd_SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry()
		{
			EDIDataRegistry.Instance.EvenLogTraceLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Client.EDI.EDIDataRegistry.EvenLogTraceLevelOptions.All);
			RegistryMachinesHandler.Sync(System.Environment.MachineName + "+123");
			var listProvidersAndTheirCapacity = new EventLogsListProvidersAndTheirCapacityCollection();
			listProvidersAndTheirCapacity.AddNewOrGetExisting(".NET Runtime", 2);
			listProvidersAndTheirCapacity.AddNewOrGetExisting("Microsoft-Windows-ApplicationExperienceInfrastructure", 3);
			EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, listProvidersAndTheirCapacity);
			using (var stream = new MemoryStream(new EmbeddedResourceRetriever(GetType().Assembly).GetBytes("ZClientEDI.Test.ServiceTasks.EventLogs.TestFiles.TestEventLogsInput1.txt")))
			{
				var eventLogRetriever = new Mock<IEventLogRetriever>();
				eventLogRetriever.Setup(m => m.Retrieve(It.IsAny<string>(), It.IsAny<EventLogDataTransmissionHandler>(), It.IsAny<ILogger>()))
					.Returns(XElement.Load(stream));
				var serviceTask = new Mock<RetrievedEventLogsServiceTask>() { CallBase = true } ;
				serviceTask.Setup(m => m.GetNewEventLogRetriever()).Returns(eventLogRetriever.Object);
				serviceTask.Setup(m => m.MachineNames).Returns(new string[] { System.Environment.MachineName });
				serviceTask.Object.ServiceLogger = new LoggerForTest();
				serviceTask.Object.RunTask();
			}

			var eventLogHighWaterMarkListRegistry = EDIDataRegistry.Instance.EventLogHighWaterMarkList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var listProvidersAndTheirCapacityRegistry = EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(System.Environment.MachineName + "+3966021", eventLogHighWaterMarkListRegistry);
			AssertEquals(3, listProvidersAndTheirCapacityRegistry.Count);
			AssertNotNull(listProvidersAndTheirCapacityRegistry.GetProvider(".NET Runtime", 2));
			AssertNotNull(listProvidersAndTheirCapacityRegistry.GetProvider("Microsoft-Windows-ApplicationExperienceInfrastructure", 3));
			AssertNotNull(listProvidersAndTheirCapacityRegistry.GetProvider("LogonExpertSvc", 3));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
