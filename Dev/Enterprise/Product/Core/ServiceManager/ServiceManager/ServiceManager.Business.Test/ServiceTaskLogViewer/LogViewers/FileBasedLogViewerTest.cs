using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(FileBasedLogViewer))]
	class FileBasedLogViewerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTaskTypes()
		{
			//Arrange
			var codes = new[] { 1, 5, 8, 2, 5 };
			codes.ForEach(code =>
			{
				var task = Factory.New<ServiceTaskSchedule>();
				task.S5_ScheduleType = $"C0{code}";
				task.S5_ScheduleDescription = $"Desc {code}";
			});

			var hostCache = new Mock<IServiceHostsCache>();
			hostCache.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(hostCache.Object))
			{
				Factory.Save();

				//Act
				var viewer = new FileBasedLogViewer(new LogViewerDataProviderFactory());

				//Assert
				AssertEquals(5, viewer.TaskTypes.Count);
				AssertContainsExactElementsInExactOrder(new[] { "HOST", "C01", "C02", "C05", "C08" }, viewer.TaskTypes.ToArray().Select(pair => pair.Code));
				AssertContainsExactElementsInExactOrder(new[] { "Service Host", "Desc 1", "Desc 2", "Desc 5", "Desc 8" }, viewer.TaskTypes.ToArray().Select(pair => pair.Description));
			}
		}

		public void TestNewModuleTaskTypes()
		{
			//Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var codes = new[] { "C01", "C05", "C08", "C02" };
				codes.ForEach(code =>
				{
					var task = Factory.New<StmServiceTask>();
					task.SST_ServiceTaskCode = code;
					task.SST_NextRunTime = ZDateTime.Now.AddDays(7).ToOffset();
				});

				var hostCache = new Mock<IServiceHostsCache>();
				hostCache.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var hostedServiceAttributes = new[]
				{
					new HostedServiceAttribute("C01", "Desc 1", "category", typeof(object)),
					new HostedServiceAttribute("C05", "Desc 5", "category", typeof(object)),
					new HostedServiceAttribute("C08", "Desc 8", "category", typeof(object)),
					new HostedServiceAttribute("C02", "Desc 2", "category", typeof(object))
				};

				var provider = new Mock<IClientHostedServiceAttributeProvider>();
				provider.Setup(p => p.GetClientHostedServiceAttributes()).Returns(() => hostedServiceAttributes);
				hostedServiceAttributes.ForEach(attrib => provider.Setup(p => p.GetClientHostedServiceAttribute(attrib.Code)).Returns(attrib));

				using (ObjectFactory.Substitute(hostCache.Object))
				using (ObjectFactory.Substitute(provider.Object))
				{
					Factory.Save();

					//Act
					var viewer = new FileBasedLogViewer(new LogViewerDataProviderFactory());

					//Assert
					AssertEquals(5, viewer.TaskTypes.Count);
					AssertContainsExactElementsInExactOrder(new[] { "HOST", "C01", "C02", "C05", "C08" }, viewer.TaskTypes.ToArray().Select(pair => pair.Code));
					AssertContainsExactElementsInExactOrder(new[] { "Service Host", "Desc 1", "Desc 2", "Desc 5", "Desc 8" }, viewer.TaskTypes.ToArray().Select(pair => pair.Description));
				}
			}
		}

		public void TestTaskTypeViaInterface()
		{
			// Arrange
			var viewer = new FileBasedLogViewer(new LogViewerDataProviderFactory());
			viewer.TaskType = ZString.Empty;
			IServiceTaskLogViewer serviceTaskViewer = viewer;

			// Act
			serviceTaskViewer.TaskType = "~TST";

			// Assert
			AssertEquals("~TST", viewer.TaskType);
		}

		public void TestTaskTypeValidationViaInterface()
		{
			// Arrange
			var viewer = new FileBasedLogViewer(new LogViewerDataProviderFactory());
			viewer.TaskType = ZString.Empty;
			IServiceTaskLogViewer serviceTaskViewer = viewer;

			// Act
			var exception = AssertExceptionThrown<MaxLengthExceededException>(() => serviceTaskViewer.TaskType = "~TST1234");

			// Assert
			AssertContains("The maximum length of 'TaskType' has been exceeded.", exception.Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		void CleanUpStmServiceHost()
		{
			var cleanupTasks = Factory.Load<StmServiceHost>(new ZQuery());
			foreach (var task in cleanupTasks)
			{
				task.Delete();
			}
		}

		public void TestOneHostLogProviderCollection()
		{
			CleanUpStmServiceHost();

			var logViewer = new FileBasedLogViewer();

			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC";
			Factory.Save();

			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.FSL, true);
			SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			AssertEquals("HostABC exists and is found", 1, logViewer.HostLogProviderCollection.Count());
			AssertType<LogViewerDataProviderFactory.RemoteLogViewerDataProvider>("Type", logViewer.HostLogProviderCollection.Last());
		}

		public void TestTwoHostLogProviderCollection()
		{
			CleanUpStmServiceHost();

			var logViewer = new FileBasedLogViewer();

			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.FSL, true);
			SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC";
			var host2 = Factory.New<StmServiceHost>();
			host2.SH_HostName = "DummyRemoteHost";
			Factory.Save();

			AssertEquals("Two remote host should be included in log provider collection", 2, logViewer.HostLogProviderCollection.Count());
			AssertType<LogViewerDataProviderFactory.RemoteLogViewerDataProvider>("Type", logViewer.HostLogProviderCollection.Last());
		}

		public void TestLogFileList()
		{
			CleanUpStmServiceHost();

			var provider = new Mock<ILogViewerDataProvider>(MockBehavior.Strict);
			var providers = new List<ILogViewerDataProvider> { provider.Object };

			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC";
			Factory.Save();

			provider.SetupSequence(p => p.GetFileNames())
					.Returns(new string[] { "file3.txt", "file1.txt", "file2.txt" })
					.Returns(new string[] { "file3.txt", "file4.txt", "file1.txt", "file2.txt" });

			provider.Setup(p => p.Hostname).Returns("HostABC");

			var factory = new Mock<ILogViewerDataProviderFactory>();
			factory.Setup(f => f.GetProviders(It.IsAny<string>())).Returns(providers);

			var viewer = new FileBasedLogViewer(factory.Object);
			viewer.TaskType = "TEST";
			AssertEquals(3, viewer.LogFileList.Count);
			AssertEquals("file3.txt", viewer.LogFileList[0].Name);
			AssertEquals("file1.txt", viewer.LogFileList[1].Name);
			AssertEquals("file2.txt", viewer.LogFileList[2].Name);
			AssertEquals("HostABC", viewer.LogFileList[2].Host);

			viewer.LogFileList.Sort("Name", ListSortDirection.Descending);
			AssertEquals("file3.txt", viewer.LogFileList[0].Name);
			AssertEquals("file2.txt", viewer.LogFileList[1].Name);
			AssertEquals("file1.txt", viewer.LogFileList[2].Name);
			AssertEquals("HostABC", viewer.LogFileList[2].Host);

			provider.Verify(p => p.Hostname, Times.Exactly(3));

			viewer.ReloadLogFileList();
			AssertEquals(4, viewer.LogFileList.Count);
			AssertEquals("file4.txt", viewer.LogFileList[0].Name);
			AssertEquals("file3.txt", viewer.LogFileList[1].Name);
			AssertEquals("file2.txt", viewer.LogFileList[2].Name);
			AssertEquals("file1.txt", viewer.LogFileList[3].Name);
			AssertEquals("HostABC", viewer.LogFileList[3].Host);

			provider.Verify(p => p.Hostname, Times.Exactly(7));
		}

		public void TestLogFileList_DifferentHosts()
		{
			CleanUpStmServiceHost();

			var provider = new Mock<ILogViewerDataProvider>();
			var providers = new List<ILogViewerDataProvider> { provider.Object, provider.Object };

			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC";

			var host2 = Factory.New<StmServiceHost>();
			host2.SH_HostName = "HostBCD";
			Factory.Save();

			provider.SetupSequence(p => p.Hostname).Returns("1").Returns("1").Returns("2").Returns("2");
			provider.SetupSequence(p => p.GetFileNames()).Returns(new[] { "1.txt", "11.txt" }).Returns(new[] { "2.txt", "22.txt" });

			var factory = new Mock<ILogViewerDataProviderFactory>();
			factory.Setup(f => f.GetProviders(It.IsAny<string>())).Returns(providers);

			var viewer = new FileBasedLogViewer(factory.Object);
			viewer.TaskType = "TEST";
			//The logs are loaded and the order of logs is correct
			AssertEquals(4, viewer.LogFileList.Count);
			AssertEquals("1.txt", viewer.LogFileList[0].Name);
			AssertEquals("11.txt", viewer.LogFileList[1].Name);
			AssertEquals("1", viewer.LogFileList[1].Host);
			AssertEquals("2.txt", viewer.LogFileList[2].Name);
			AssertEquals("22.txt", viewer.LogFileList[3].Name);
			AssertEquals("2", viewer.LogFileList[3].Host);

			//Sort the logs, check that the order is correct after sorting
			viewer.LogFileList.Sort("Host", ListSortDirection.Descending);
			AssertEquals(4, viewer.LogFileList.Count);
			AssertEquals("2.txt", viewer.LogFileList[0].Name);
			AssertEquals("22.txt", viewer.LogFileList[1].Name);
			AssertEquals("2", viewer.LogFileList[1].Host);
			AssertEquals("1.txt", viewer.LogFileList[2].Name);
			AssertEquals("11.txt", viewer.LogFileList[3].Name);
			AssertEquals("1", viewer.LogFileList[3].Host);

			provider.Verify(p => p.Hostname, Times.Exactly(4));
			provider.Verify(p => p.GetFileNames(), Times.Exactly(2));

			provider.SetupSequence(p => p.Hostname).Returns("1").Returns("1").Returns("2").Returns("2");
			provider.SetupSequence(p => p.GetFileNames()).Returns(new[] { "1.txt", "11.txt" }).Returns(new[] { "2.txt", "22.txt" });

			//Re-load the logs, check that the order is correct after re-load (the sorting was retained)
			viewer.ReloadLogFileList();
			AssertEquals(4, viewer.LogFileList.Count);
			AssertEquals("2.txt", viewer.LogFileList[0].Name);
			AssertEquals("22.txt", viewer.LogFileList[1].Name);
			AssertEquals("2", viewer.LogFileList[1].Host);
			AssertEquals("1.txt", viewer.LogFileList[2].Name);
			AssertEquals("11.txt", viewer.LogFileList[3].Name);
			AssertEquals("1", viewer.LogFileList[3].Host);

			provider.Verify(p => p.Hostname, Times.Exactly(8));
			provider.Verify(p => p.GetFileNames(), Times.Exactly(4));
		}
	}
}
