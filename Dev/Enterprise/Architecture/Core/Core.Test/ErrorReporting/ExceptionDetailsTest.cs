using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ExceptionDetailsTest : ExceptionFullTracerTest
	{
		public void TestGetFullReport_Win32Exception_AccessDenied()
		{
			AssertWriteBackgroundAppDomainWorkerMessage(new Win32Exception(5, "Access denied."));
		}

		public void TestGetFullReport_AppDomainUnloadedException()
		{
			AssertWriteBackgroundAppDomainWorkerMessage(new AppDomainUnloadedException());
		}

		public void TestGetFullReport_CannotUnloadAppDomainException()
		{
			AssertWriteBackgroundAppDomainWorkerMessage(new CannotUnloadAppDomainException());
		}

		void AssertWriteBackgroundAppDomainWorkerMessage(Exception ex)
		{
			var resetEvent1 = new ManualResetEvent(false);
			var resetEvent2 = new ManualResetEvent(false);
			var backgroundAppDomainService = ObjectFactory.Get<IBackgroundAppDomainServiceForTest>();
			using (backgroundAppDomainService.RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTestOnly(SetQueueWorkItemCompleted))
			{
				backgroundAppDomainService.ClearRecentlyCompletedWorkItemForTestOnly();

				var navigator = GetNavigator(ex);
				AssertNull("there is no work item finished.", navigator.SelectSingleNode("//ExceptionDetails/RecentlyCompletedWorkItem"));

				var asyncResult1 = backgroundAppDomainService.QueueWorkItemForTestOnly("TestDesc1", () => { resetEvent1.WaitOne(); });
				var asyncResult2 = backgroundAppDomainService.QueueWorkItemForTestOnly("TestDesc2", () => { resetEvent2.WaitOne(); });

				navigator = GetNavigator(ex);
				var workItemsInProgress = navigator.SelectSingleNode("//ExceptionDetails/WorkItemsInProgress").InnerXml;
				AssertEquals("TestDesc1" + System.Environment.NewLine + "TestDesc2", workItemsInProgress);

				resetEvent1.Set();
				WaitQueueWorkItemCompleted(asyncResult1);
				navigator = GetNavigator(ex);
				var recentlyCompletedWorkItem = navigator.SelectSingleNode("//ExceptionDetails/RecentlyCompletedWorkItem").InnerXml;
				workItemsInProgress = navigator.SelectSingleNode("//ExceptionDetails/WorkItemsInProgress").InnerXml;
				AssertStartsWith("Exception Message Should Start With", "TestDesc1", recentlyCompletedWorkItem);
				AssertEquals("TestDesc2", workItemsInProgress);

				resetEvent2.Set();
				WaitQueueWorkItemCompleted(asyncResult2);
				navigator = GetNavigator(ex);
				recentlyCompletedWorkItem = navigator.SelectSingleNode("//ExceptionDetails/RecentlyCompletedWorkItem").InnerXml;
				AssertStartsWith("Exception Message Should Start With", "TestDesc2", recentlyCompletedWorkItem);
				AssertNull("there should be no work item in progress.", navigator.SelectSingleNode("//ExceptionDetails/WorkItemsInProgress"));
			}

			void SetQueueWorkItemCompleted(IBackgroundAppDomainWorkItem workItem)
			{
				if (workItem.Description == "TestDesc1")
				{
					resetEvent1.WaitOne();
				}

				if (workItem.Description == "TestDesc2")
				{
					resetEvent2.WaitOne();
				}
			}

			void WaitQueueWorkItemCompleted(IAsyncResult result)
			{
				for (int i = 0; i < 100; i++)
				{
					if (result.IsCompleted)
					{
						break;
					}
					Thread.Sleep(50);
				}
			}
		}

		public void TestGetDotNetInfo()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var registryProvider = new ExceptionDetails.WindowsRegistryProvider();
			AssertNotNull(envInfo.GetDotNetRelease(registryProvider));
			AssertNotNull(envInfo.GetDotNetVersion(registryProvider));
		}

		public void TestGetWindowsInstallationType_Simple()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var registry = new ExceptionDetails.WindowsRegistryProvider();
			var installationType = envInfo.GetInstallationType(registry);
			AssertCollectionContains("InstallationType should be a well-known value", installationType, new[] { "Client", "Server", "Server Core" });
		}

		public void TestGetWindowsInstallationType_Registry()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var mockRegistry = new Mock<ExceptionDetails.IRegistryProvider>();
			mockRegistry.Setup(x => x.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion").GetValue("InstallationType", It.IsAny<object>())).Returns("Whatever");
			var installationType = envInfo.GetInstallationType(mockRegistry.Object);
			AssertEquals(installationType, "Whatever");
		}

		public void TestWriteWindowsInstallationType_WindowsWorkstation()
			=> AssertWritesInstallationType("Client");

		public void TestWriteWindowsInstallationType_WindowsServerDesktop()
			=> AssertWritesInstallationType("Server");

		public void TestWriteWindowsInstallationType_WindowsServerCore()
			=> AssertWritesInstallationType("Server Core");

		public void TestWriteWindowsInstallationType_WhateverJunkIsInTheRegistry()
			=> AssertWritesInstallationType("Balalalalala");

		static void AssertWritesInstallationType(string installationType)
		{
			var ex = new InvalidOperationException("wheeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
			var mockEnvInfo = new Mock<ExceptionDetails.IEnvironmentInfoProvider>();
			mockEnvInfo.Setup(x => x.GetOSVersion()).Returns(new ExceptionDetails.OSVersionInfo());
			mockEnvInfo.Setup(x => x.GetInstallationType(It.IsAny<ExceptionDetails.IRegistryProvider>())).Returns(installationType);

			var reportText = new ExceptionDetails(ex, mockEnvInfo.Object).GetFullReport();
			var document = XDocument.Parse(reportText);
			var reportedInstallationType = document.XPathSelectElement("//ExceptionDetails/EnvironmentInfo/OSInfo/InstallationType")?.Value;
			AssertEquals(installationType, reportedInstallationType);
		}

		public void TestGetDotNetInfo_DotNet45FullProfile()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var mockRegistryProvider = new Mock<ExceptionDetails.IRegistryProvider>();
			var mockRegistryValueProvider = new Mock<ExceptionDetails.IRegistryValueProvider>();

			var dotNet45Release = 378758;
			var dotNet45Version = "4.5.52301";
			var dotNet45FullPath = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\";

			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Release", null)).Returns(dotNet45Release);
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Version", "")).Returns(dotNet45Version);
			mockRegistryProvider.Setup(registryProvider => registryProvider.OpenSubKey(dotNet45FullPath)).Returns(mockRegistryValueProvider.Object);

			AssertEquals(dotNet45Version + "", envInfo.GetDotNetVersion(mockRegistryProvider.Object));
			AssertEquals(dotNet45Release + "", envInfo.GetDotNetRelease(mockRegistryProvider.Object));
		}

		public void TestGetDotNetInfo_DotNet45ClientProfile()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var mockRegistryProvider = new Mock<ExceptionDetails.IRegistryProvider>();
			var mockRegistryValueProvider = new Mock<ExceptionDetails.IRegistryValueProvider>();

			var dotNet45Release = 378758;
			var dotNet45Version = "4.5.52301";
			var dotNet45ClientPath = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client\\";
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Release", null)).Returns(dotNet45Release);
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Version", "")).Returns(dotNet45Version);
			mockRegistryProvider.Setup(registryProvider => registryProvider.OpenSubKey(dotNet45ClientPath)).Returns(mockRegistryValueProvider.Object);

			AssertEquals("Should fallback to getting Version from client profile", dotNet45Version + "", envInfo.GetDotNetVersion(mockRegistryProvider.Object));
			AssertEquals("Should fallback to getting Release from client profile", dotNet45Release + "", envInfo.GetDotNetRelease(mockRegistryProvider.Object));
		}

		public void TestGetDotNetInfo_DotNet40()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var mockRegistryProvider = new Mock<ExceptionDetails.IRegistryProvider>();
			var mockRegistryValueProvider = new Mock<ExceptionDetails.IRegistryValueProvider>();

			object dotNet40Release = null;
			var dotNet40Version = "4.0.0.0";
			var dotNet40Path = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4.0";
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Release", null)).Returns(dotNet40Release);
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Version", "")).Returns(dotNet40Version);
			mockRegistryProvider.Setup(registryProvider => registryProvider.OpenSubKey(dotNet40Path)).Returns(mockRegistryValueProvider.Object);
			// edi uses .NET 4.0 as of time of writing
			AssertEquals("Version should be loaded successfully from a different registry key if it's .NET 4.0", dotNet40Version, envInfo.GetDotNetVersion(mockRegistryProvider.Object));
			AssertEquals("Release should be an empty string as it doesn't exist before .NET 4.5", "", envInfo.GetDotNetRelease(mockRegistryProvider.Object));
		}

		public void TestGetDotNetInfo_DotNet35()
		{
			ExceptionDetails.IEnvironmentInfoProvider envInfo = new ExceptionDetails.EnvironmentInfoProvider();
			var mockRegistryProvider = new Mock<ExceptionDetails.IRegistryProvider>();
			var mockRegistryValueProvider = new Mock<ExceptionDetails.IRegistryValueProvider>();

			object dotNet35Release = null;
			var dotNet35Version = "3.5.30729.4926";
			var dotNet35Path = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.5";

			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Release", null)).Returns(dotNet35Release);
			mockRegistryValueProvider.Setup(registryValueProvider => registryValueProvider.GetValue("Version", "")).Returns(dotNet35Version);
			mockRegistryProvider.Setup(registryProvider => registryProvider.OpenSubKey(dotNet35Path)).Returns(mockRegistryValueProvider.Object);

			AssertExceptionThrown<NotImplementedException>(".NET frameworks older than 4.0 should not be supported", () =>
			{
				envInfo.GetDotNetVersion(mockRegistryProvider.Object);
				envInfo.GetDotNetRelease(mockRegistryProvider.Object);
			});
		}

		public void TestGetFullWebReport_RequestTimedOut()
		{
#if NETFRAMEWORK
			XPathNavigator navigator = GetNavigator(new System.Web.HttpException(408, "Request Timeout"));
			AssertEquals("ErrorCode", "-2147467259", navigator.SelectSingleNode("ExceptionDetails/ErrorCode").InnerXml);
			AssertEquals("Message", "<Line>Request Timeout</Line>", navigator.SelectSingleNode("ExceptionDetails/Message").InnerXml);
			AssertEquals("NativeErrorCode", "408", navigator.SelectSingleNode("ExceptionDetails/NativeErrorCode").InnerXml);
#else
			Fail("Test must be migrated to .net core. HttpException equivalent? Is this thrown by http client? If so, what is the new exception?");
#endif
		}

		public void TestGetFullReport()
		{
			XPathNavigator navigator = GetNavigator(new Exception("test message"));
			Assert("Valid Xml", navigator.OuterXml.Length > 0);
			AssertEquals(0, navigator.Select("//FileName").Count);
			AssertEquals(0, navigator.Select("//FusionLog").Count);
			AssertEquals(0, navigator.Select("//Data").Count);

			var xPathIt = navigator.Select("//ExceptionDetails/StackTrace/Call");
			bool hasILOffsetInfo = false;

			if (xPathIt.Count > 0)
			{
				while (xPathIt.MoveNext())
				{
					if (xPathIt.Current.HasAttributes)
					{
						Assert(xPathIt.Current.GetAttribute("Assembly", "").Length > 0);
						Assert(xPathIt.Current.GetAttribute("Type", "").Length > 0);
						Assert(xPathIt.Current.GetAttribute("Method", "").Length > 0);
						Assert(xPathIt.Current.GetAttribute("ILOffset", "").Length > 0);
						Assert(xPathIt.Current.GetAttribute("Parameters", "").Length > 0);
						hasILOffsetInfo = true;
					}
				}
			}

			Assert(hasILOffsetInfo);
		}

		public void TestRethrownByExceptionHandlerException()
		{
			Exception exception = null;
			try
			{
				ThrowsArgumentNullException();
			}
			catch (Exception e)
			{
				exception = e;
			}

			XPathNavigator navigator = GetNavigator(new RethrownByExceptionHandlerException(new RethrownByExceptionHandlerException(exception)));
			AssertContains("ThrowsArgumentNullException", navigator.OuterXml);
			AssertContains("<TargetSite>Enterprise.ZArchitecture.Core.Testing.ExceptionDetailsTest.ThrowsArgumentNullException</TargetSite>", navigator.OuterXml);
		}

		void ThrowsArgumentNullException()
		{
			throw new ArgumentNullException("Value cannot be null.\r\nParameter name: source");
		}

		public void TestGetFullReport_Message()
		{
			var message = "test message" + System.Environment.NewLine +
				"with new line here" + System.Environment.NewLine +
				"and new line over here" + System.Environment.NewLine +
				"\njust before this line is a system newline and a unix newline\r\nand just then was a windows newline";

			var navigator = GetNavigator(new Exception(message));
			var expected =
@"<Line>test message</Line>
<Line>with new line here</Line>
<Line>and new line over here</Line>
<Line />
<Line>just before this line is a system newline and a unix newline</Line>
<Line>and just then was a windows newline</Line>";
			AssertEquals("Message of navigator", expected, navigator.SelectSingleNode("ExceptionDetails/Message").InnerXml);
		}

		public void TestGetFullReport_ExternalException()
		{
			XPathNavigator navigator = GetNavigator(new ExternalException("Oof.", 9876));
			AssertEquals("ErrorCode", "9876", navigator.SelectSingleNode("ExceptionDetails/ErrorCode").InnerXml);
			AssertEquals("Message", "<Line>Oof.</Line>", navigator.SelectSingleNode("ExceptionDetails/Message").InnerXml);
			AssertNull("NativeErrorCode", navigator.SelectSingleNode("ExceptionDetails/NativeErrorCode"));
		}

		public void TestGetFullReport_Win32Exception()
		{
			XPathNavigator navigator = GetNavigator(new Win32Exception(1234, "Ouch."));
			AssertEquals("ErrorCode", "-2147467259", navigator.SelectSingleNode("ExceptionDetails/ErrorCode").InnerXml);
			AssertEquals("Message", "<Line>Ouch.</Line>", navigator.SelectSingleNode("ExceptionDetails/Message").InnerXml);
			AssertEquals("NativeErrorCode", "1234", navigator.SelectSingleNode("ExceptionDetails/NativeErrorCode").InnerXml);
		}

		public void TestGetFullReport_ConcurrencyException()
		{
			string message = "<ConcurrencyTableRows><ConcurrencyTableRow>" +
				"<Column>Weights</Column><Original>Rocks</Original><DB>Dumbbells</DB>" +
				"</ConcurrencyTableRow></ConcurrencyTableRows>";

			XPathNavigator navigator = GetNavigator(new DBConcurrencyException(message));
			string xml = navigator.SelectSingleNode("ExceptionDetails/Message").InnerXml;
			AssertEquals("ConcurrencyTable tags are passed with special characters escaped",
				"<Line>&lt;ConcurrencyTableRows&gt;&lt;ConcurrencyTableRow&gt;&lt;Column&gt;Weights&lt;/Column&gt;&lt;Original&gt;Rocks&lt;/Original&gt;&lt;DB&gt;Dumbbells&lt;/DB&gt;&lt;/ConcurrencyTableRow&gt;&lt;/ConcurrencyTableRows&gt;</Line>",
				xml);
		}

		public void TestGetFullReport_SpecialCharactersEscaped_ConcurrencyException()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "Apples & Bananas";
			ZDataConcurrencyException innerException = new ZDataConcurrencyException(new Exception(), ((INeedRow)dummy).Row, ((IDbConnected)factory).Connection);

			string report = new ExceptionDetails(new ZSaveConcurrencyException(innerException, factory)).GetFullReport();
			Assert("Special character is escaped", report.Contains("Z0_Description = Apples &amp; Bananas"));
			XmlDocument xmlDoc = new XmlDocument();
			AssertNoExceptionThrown("Valid xml", () => xmlDoc.LoadXml(report));
		}

		public void TestGetFullReport_FileNotFoundException()
		{
			FileNotFoundException fnfEx = new FileNotFoundException("test message", "Test File Name");
			AssertFileNameAndFusionLog(fnfEx, fnfEx.FileName, "Test Fusion Log");

			fnfEx = new FileNotFoundException("test message");
			AssertFileNameAndFusionLog(fnfEx, null, null);
		}

		public void TestGetFullReport_BadImageFormatException()
		{
			BadImageFormatException bifEx = new BadImageFormatException("test message", "Test File Name 2");
			AssertFileNameAndFusionLog(bifEx, bifEx.FileName, "Test Fusion Log");

			bifEx = new BadImageFormatException("test message");
			AssertFileNameAndFusionLog(bifEx, null, "Test Fusion Log");
		}

		public void TestGetFullReport_FileLoadException()
		{
			FileLoadException flEx = new FileLoadException("test message", "Test File Name 3");
			AssertFileNameAndFusionLog(flEx, flEx.FileName, "Test Fusion Log");
			AssertFileNameAndFusionLog(flEx, flEx.FileName, null);
		}

		public void TestGetFullReportWithData()
		{
			AssertFullReportWithData(ex => ex);
		}

		public void TestGetFullReportWithData_RethrownByExceptionHandlerException()
		{
			AssertFullReportWithData(ex => new RethrownByExceptionHandlerException(ex));
		}

		public void AssertFullReportWithData(Func<Exception, Exception> manipulateException)
		{
			Exception ex = new Exception("test message");
			ex.Data["TestKey1"] = 31.5M;
			ex.Data[300] = "<TestValue>TestXmlValuesAreEscaped</TestValue>";
			ex.Data["TestKey2"] = new ObjectWithToStringException();
			ex.Data[new ObjectWithToStringException()] = "value";

			string report = new ExceptionDetails(manipulateException(ex)).GetFullReport();
			Assert("Xml Value is escaped", report.Contains("&lt;TestValue&gt;TestXmlValuesAreEscaped&lt;/TestValue&gt;"));

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(report);

			Assert("Valid Xml", xmlDoc.OuterXml.Length > 0);
			AssertEquals(0, xmlDoc.SelectNodes("//FileName").Count);
			AssertEquals(0, xmlDoc.SelectNodes("//FusionLog").Count);
			XmlNodeList dataNodeList = xmlDoc.SelectNodes("//Data");
			AssertEquals(1, dataNodeList.Count);
			AssertEquals(4, dataNodeList[0].ChildNodes.Count);

			for (int i = 0; i < 4; i++)
			{
				AssertEquals("Item", dataNodeList[0].ChildNodes[i].Name);
				AssertEquals(2, dataNodeList[0].ChildNodes[i].ChildNodes.Count);
				AssertEquals("Key", dataNodeList[0].ChildNodes[i].ChildNodes[0].Name);
				AssertEquals("Value", dataNodeList[0].ChildNodes[i].ChildNodes[1].Name);
			}

			string[] exceptionText = { "System.Exception: ToString Bad!",
				"at Enterprise.ZArchitecture.Core.Testing.ExceptionDetailsTest.ObjectWithToStringException.ToString()",
				"at Enterprise.ZArchitecture.Core.ExceptionDetails.WriteExceptionData(Exception exception)"
			};

			AssertEquals("key value", "TestKey1", dataNodeList[0].ChildNodes[0].ChildNodes[0].FirstChild.Value);
			AssertEquals("key value", "300", dataNodeList[0].ChildNodes[1].ChildNodes[0].FirstChild.Value);
			AssertEquals("key value", "TestKey2", dataNodeList[0].ChildNodes[2].ChildNodes[0].FirstChild.Value);
			string[] testKeyLines = dataNodeList[0].ChildNodes[3].ChildNodes[0].FirstChild.Value.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertStartsWith(exceptionText, testKeyLines);

			AssertEquals("value value", "31.5", dataNodeList[0].ChildNodes[0].ChildNodes[1].FirstChild.Value);
			AssertEquals("value value", "<TestValue>TestXmlValuesAreEscaped</TestValue>", dataNodeList[0].ChildNodes[1].ChildNodes[1].FirstChild.Value);
			string[] testValueLines = dataNodeList[0].ChildNodes[2].ChildNodes[1].FirstChild.Value.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertStartsWith(exceptionText, testValueLines);
			AssertEquals("value value", "value", dataNodeList[0].ChildNodes[3].ChildNodes[1].FirstChild.Value);
		}

		public void TestGetFullReportContainSystemResourcesUsage()
		{
			var exception = new Exception("Lorem ipsum dolor sit amet");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>();

			var pdhCounters = new Dictionary<string, object[]>()
			{
					{ ExceptionDetails.SystemResourcesUsageCodes.ProcessesCount, new object[] { @"\Objects(_Total)\Processes", 14.0d } },
					{ ExceptionDetails.SystemResourcesUsageCodes.ThreadsCount, new object[] { @"\Process(_Total)\Thread Count", 36.0d } },
					{ ExceptionDetails.SystemResourcesUsageCodes.HandlesCount, new object[] { @"\Process(_Total)\Handle Count", 88.0d } }
			};

			mock.Setup(m => m.GetCounter(It.IsAny<string>()))
				.Returns((object methodArguments) =>
				{
					var counterName = (string)methodArguments;
					foreach (var pdhCount in pdhCounters)
					{
						if (string.Equals(counterName, pdhCount.Value[0].ToString(), StringComparison.OrdinalIgnoreCase))
						{
							return (double)pdhCount.Value[1];
						}
					}
					return (double)0.0d;
				});

			var report = new ExceptionDetails(exception, mock.Object).GetFullReport();
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(report);

			CombineAssertions(() =>
			{
				Assert("Valid Xml must has content", xmlDoc.OuterXml.Length > 0);
				XmlNodeList systemResourcesUsageNodeList = xmlDoc.SelectNodes("//SystemResourcesUsage");
				AssertEquals("SystemResourcesUsage must contain 1 node", 1, systemResourcesUsageNodeList.Count);
				AssertEquals("SystemResourcesUsage must contain 8 children nodes", 8, systemResourcesUsageNodeList[0].ChildNodes.Count);

				var actualChildrenNode = systemResourcesUsageNodeList[0].ChildNodes.Cast<XmlNode>()
												.Select(node => node.Name)
												.ToList();

				string[] expectedChildrenNode = { "GDIObjectsCount",
																					"USERObjectsCount",
																					"UserWindowHandlesCount",
																					"ManagedHeapUsage",
																					"WorkingSetSize",
																					ExceptionDetails.SystemResourcesUsageCodes.ProcessesCount,
																					ExceptionDetails.SystemResourcesUsageCodes.ThreadsCount,
																					ExceptionDetails.SystemResourcesUsageCodes.HandlesCount };
				AssertContainsExactElementsInAnyOrder(expectedChildrenNode, actualChildrenNode);

				double dataCount = -1;
				foreach (var pdhCounter in pdhCounters)
				{
					double.TryParse(systemResourcesUsageNodeList[0].SelectSingleNode(pdhCounter.Key).FirstChild.Value, out dataCount);
					AssertEquals(string.Format("{0} MUST BE an expectedCount", pdhCounter.Key), pdhCounter.Value[1], dataCount);
				}
			});

			mock.Verify(m => m.GetCounter(It.IsAny<string>()), Times.Exactly(3));
		}

		public void TestGetFullReportContainsDpiScaling()
		{
			var report = new ExceptionDetails(new Exception("Some Exception")).GetFullReport();
			Assert(Regex.IsMatch(report, ("<DpiX>\\d+\\.\\d+</DpiX>")));
			Assert(Regex.IsMatch(report, ("<DpiY>\\d+\\.\\d+</DpiY>")));
		}

		public void TestGetStackTraceAndMessage()
		{
			Exception ex = new Exception("test message");
			string report = new ExceptionDetails(ex).GetStackTraceAndMessage();

			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
		}

		public void TestGetStackTraceAndMessageLong()
		{
			Exception ex = CreateInnerExceptionStructure(new Exception("test message"), 10);
			string report = new ExceptionDetails(ex).GetStackTraceAndMessage();

			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("Tier 10"));
			Assert(report.Contains("test message"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "Testing")]
		public void TestGetVersionEx() // Name of unit test method
		{
			ExceptionDetails.IEnvironmentInfoProvider provider = new ExceptionDetails.EnvironmentInfoProvider();
			ExceptionDetails.OSVersionInfo versionFromPInvoke = provider.GetOSVersion();
			Version versionFromDotNet = System.Environment.OSVersion.Version;

			AssertEquals(versionFromDotNet.Major, versionFromPInvoke.MajorVersion);
			AssertEquals(versionFromDotNet.Minor, versionFromPInvoke.MinorVersion);
			AssertEquals(versionFromDotNet.Build, versionFromPInvoke.BuildNumber);
		}

		public void TestDiskSpace()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			var details = new ExceptionDetails(ex, mock.Object);
			var xmlDoc = new XmlDocument();

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns(EnvProxy.Instance.ApplicationStartupPath);
			mock.Setup(m => m.TemporaryPath)
				.Returns(EnvProxy.Instance.TempPath);
			mock.Setup(m => m.GetDiskFreeSpaceEx(It.IsAny<string>(), ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny))
				.Returns((string)null);
			xmlDoc.LoadXml(details.GetFullReport());
			mock.Verify(m => m.GetDiskFreeSpaceEx(It.IsAny<string>(), ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny), Times.Exactly(2));
			mock.VerifyAll();

			var diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);

			var enterpriseDirectoryElement = diskspaceElement["EnterpriseDirectory"];
			AssertDiskSpaceElement(enterpriseDirectoryElement, TestCase.ExecutableDirectory);

			var tempDirectoryElement = diskspaceElement["TempDirectory"];
			AssertDiskSpaceElement(tempDirectoryElement, EnvProxy.Instance.TempPath);
		}

		public void TestDiskSpace_ErrorFromPInvoke()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			var details = new ExceptionDetails(ex, mock.Object);
			var xmlDoc = new XmlDocument();

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns(EnvProxy.Instance.ApplicationStartupPath);

			mock.Setup(m => m.TemporaryPath)
				.Returns((string)null);

			mock.Setup(m => m.GetDiskFreeSpaceEx(It.IsAny<string>(), ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny, ref It.Ref<long>.IsAny))
				.Throws(new Exception("Exception For DiskSpace"));

			xmlDoc.LoadXml(details.GetFullReport());
			mock.VerifyAll();

			var diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);

			var enterpriseDirectoryElement = diskspaceElement["EnterpriseDirectory"];
			AssertNotNull(enterpriseDirectoryElement);

			var diskSpaceErrorElement = enterpriseDirectoryElement["DiskSpaceError"];
			AssertNotNull(diskSpaceErrorElement);
			Assert(diskSpaceErrorElement.InnerXml.StartsWith("Exception while processing disk space for "));

			var diskSpaceErrorDetailsElement = enterpriseDirectoryElement["DiskSpaceErrorDetails"];
			AssertNotNull(diskSpaceErrorDetailsElement);
			Assert(diskSpaceErrorDetailsElement.InnerXml.StartsWith("System.Exception: Exception For DiskSpace"));
		}

		public void TestDiskSpace_InvalidPath()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			var details = new ExceptionDetails(ex, mock.Object);
			var xmlDoc = new XmlDocument();

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns((string)null);
			xmlDoc.LoadXml(details.GetFullReport());
			mock.VerifyAll();

			var diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);
			Assert(diskspaceElement.InnerText.IndexOf("Failed to get Diskspace information: ") != -1);

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns("");
			mock.Setup(m => m.TemporaryPath)
				.Returns("");
			xmlDoc.LoadXml(details.GetFullReport());
			mock.VerifyAll();

			diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);

			var enterpriseDirectoryElement = diskspaceElement["EnterpriseDirectory"];
			AssertNotNull(enterpriseDirectoryElement);
			AssertEquals("No path found", enterpriseDirectoryElement.InnerXml);

			var tempDirectoryElement = diskspaceElement["TempDirectory"];
			AssertNotNull(tempDirectoryElement);
			AssertEquals("Path Not Found: (empty)", tempDirectoryElement.InnerXml);

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns(Path.Combine(EnvProxy.Instance.TempPath, "TestDirThatShouldNotExist"));
			mock.Setup(m => m.TemporaryPath)
				.Returns((string)null);
			xmlDoc.LoadXml(details.GetFullReport());
			mock.VerifyAll();

			diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);

			enterpriseDirectoryElement = diskspaceElement["EnterpriseDirectory"];
			AssertNotNull(enterpriseDirectoryElement);
			AssertEquals("Path Not Found: ", enterpriseDirectoryElement.InnerXml.Substring(0, 16));
			Assert(enterpriseDirectoryElement.InnerXml.EndsWith("TestDirThatShouldNotExist"));

			tempDirectoryElement = diskspaceElement["TempDirectory"];
			AssertNotNull(tempDirectoryElement);
			AssertEquals("Path Not Found: (null)", tempDirectoryElement.InnerXml);

			mock.Setup(m => m.ApplicationStartupPath)
				.Returns("|InvalidPath|");
			mock.Setup(m => m.TemporaryPath)
				.Returns((string)null);
			xmlDoc.LoadXml(details.GetFullReport());
			mock.VerifyAll();

			diskspaceElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["PCInfo"]["DiskSpace"];
			AssertNotNull(diskspaceElement);

			enterpriseDirectoryElement = diskspaceElement["EnterpriseDirectory"];
			AssertNotNull(enterpriseDirectoryElement);
			AssertEquals("Path Not Found: |InvalidPath|", enterpriseDirectoryElement.InnerXml);
		}

		public void TestGetTraceForTraceableException()
		{
			Exception ex = new TestTraceableException("test message");
			string report = new ExceptionDetails(ex).GetFullReport();

			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert("Includes Test Trace Log", report.IndexOf("Test Trace Log") != -1);
		}

		public void TestLongExceptionTreeFiltering()
		{
			var ex = CreateInnerExceptionStructure(new Exception("Bottom"), 0);
			var report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(!report.Contains("intermediate exceptions skipped"));
			Assert(report.Contains("<ExceptionDetails><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source />"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 1);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(!report.Contains("intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 2);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(!report.Contains("intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 3);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(!report.Contains("intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 3</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 4);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(!report.Contains("intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 3</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 4</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 5);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>1 intermediate exception skipped."));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 4</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 5</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 6);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>2 intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 5</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 6</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 7);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>3 intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 6</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 7</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 10);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>6 intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 9</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 10</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));

			ex = CreateInnerExceptionStructure(new Exception("Bottom"), 100);
			report = new ExceptionDetails(ex).GetFullReport();
			AssertNotNull("Report should not be null", report);
			Assert("Valid Report", report.Length > 0);
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>96 intermediate exceptions skipped"));
			Assert(report.Contains("<Message><Line>Tier 1</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 2</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 99</Line></Message>"));
			Assert(report.Contains("<Message><Line>Tier 100</Line></Message>"));
			Assert(report.Contains("<InnerException><ExceptionType>System.Exception</ExceptionType><Message><Line>Bottom</Line></Message><Source /><StackTrace><Calls>1</Calls></StackTrace></InnerException>"));
		}

		public void TestExceptionReportFormat()
		{
			var ex = CreateInnerExceptionStructure(new Exception("Bottom"), 10);
			var report = new ExceptionDetails(ex).GetFullReport();
			Assert(report.StartsWith("<ExceptionDetails><ExceptionType>System.Exception</ExceptionType><Message><Line>Tier 1</Line></Message><Source /><StackTrace>"));
			Assert(report.EndsWith("</ExceptionDetails>"));
		}

		[ExpectNoExceptions]
		public void TestExceptionDetails_AcceptNull()
		{
			var details = new ExceptionDetails(null);
			details.GetFullReport();
		}

		public void TestXmlContainsFirstInnerExceptionOfAggregateException()
		{
			var aggregate = new AggregateException(new Exception("One"), new Exception("Two"));
			var navigator = GetNavigator(aggregate).Select("//ExceptionDetails/InnerException");
			AssertEquals("There is one InnerException", 1, navigator.Count);
			Assert("Get to the first InnerException", navigator.MoveNext());
			Assert(navigator.Current.MoveToChild("Message", ""));
			AssertEquals("One", navigator.Current.Value);
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetCore)]
#endif
		public void TestGetFullReport_NetCore()
		{
			var fullReport = new ExceptionDetails(new Exception("Fail")).GetFullReport();
			AssertNotContains("Failed to get exception information", fullReport);
		}

		public void TestWriteAuditDBInfo()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			var details = new ExceptionDetails(ex, mock.Object);
			var xmlDoc = new XmlDocument();

			xmlDoc.LoadXml(details.GetFullReport());

			var auditDatabaseInfoElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["AuditDatabaseInfo"];
			AssertNotNull(auditDatabaseInfoElement);

			var sqlServerVersionElement = auditDatabaseInfoElement["SQLServerVersion"];
			var databaseServerName = auditDatabaseInfoElement["DatabaseServerName"];
			var auditDatabaseName = auditDatabaseInfoElement["AuditDatabaseName"];
			var auditDatabaseSchemaVersion = auditDatabaseInfoElement["AuditDatabaseSchemaVersion"];
			var auditVersion = string.Empty;

			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection), Db.SqlMasterDb))
			{
				auditVersion = BiMasterState.GetBiDatabaseExtPty(auditConnection, Db.AuditDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName);
			}

			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				AssertEquals(auditConnection.ExecuteScalar("select @@version").ToString(), sqlServerVersionElement.InnerText);
				AssertEquals(auditConnection.ServerName, databaseServerName.InnerText);
				AssertEquals(Db.AuditDatabaseName, auditDatabaseName.InnerText);
				AssertEquals(auditVersion, auditDatabaseSchemaVersion.InnerText);
			}
		}

		public void TestWriteAuditDBInfoWhenAuditDBNotExists()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			using (BiServers.TemporarilySetAuditServerToNull())
			{
				var details = new ExceptionDetails(ex, mock.Object);
				var xmlDoc = new XmlDocument();

				xmlDoc.LoadXml(details.GetFullReport());

				var auditDatabaseInfoElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["AuditDatabaseInfo"];
				AssertNotNull(auditDatabaseInfoElement);

				var sqlServerVersionElement = auditDatabaseInfoElement["SQLServerVersion"];
				AssertEquals(string.Empty, sqlServerVersionElement.InnerText);
				var databaseServerName = auditDatabaseInfoElement["DatabaseServerName"];
				AssertEquals(string.Empty, databaseServerName.InnerText);
				var auditDatabaseName = auditDatabaseInfoElement["AuditDatabaseName"];
				AssertEquals(string.Empty, auditDatabaseName.InnerText);
				var auditDatabaseSchemaVersion = auditDatabaseInfoElement["AuditDatabaseSchemaVersion"];
				AssertEquals(string.Empty, auditDatabaseSchemaVersion.InnerText);
			}
		}

		public void TestWriteEdwDBInfo()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			var details = new ExceptionDetails(ex, mock.Object);
			var xmlDoc = new XmlDocument();

			xmlDoc.LoadXml(details.GetFullReport());

			var edwDatabaseInfoElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["EdwDatabaseInfo"];
			AssertNotNull(edwDatabaseInfoElement);

			var sqlServerVersionElement = edwDatabaseInfoElement["SQLServerVersion"];
			var databaseServerName = edwDatabaseInfoElement["DatabaseServerName"];
			var edwDatabaseName = edwDatabaseInfoElement["EdwDatabaseName"];
			var edwDatabaseSchemaVersion = edwDatabaseInfoElement["EdwDatabaseSchemaVersion"];
			var edwVersion = string.Empty;

			using (var edwConnection = Db.NewExtraConnectionWithMainDbCredentials(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection), Db.SqlMasterDb))
			{
				edwVersion = BiMasterState.GetBiDatabaseExtPty(edwConnection, Db.EdwDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName);
			}

			using (var edwConnection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				AssertEquals(edwConnection.ExecuteScalar("select @@version").ToString(), sqlServerVersionElement.InnerText);
				AssertEquals(edwConnection.ServerName, databaseServerName.InnerText);
				AssertEquals(Db.EdwDatabaseName, edwDatabaseName.InnerText);
				AssertEquals(edwVersion, edwDatabaseSchemaVersion.InnerText);
			}
		}

		public void TestWriteEdwDBInfoWhenEdwDBNotExists()
		{
			var ex = new Exception("test message");
			var mock = new Mock<ExceptionDetails.IEnvironmentInfoProvider>(MockBehavior.Strict);

			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				var details = new ExceptionDetails(ex, mock.Object);
				var xmlDoc = new XmlDocument();

				xmlDoc.LoadXml(details.GetFullReport());

				var edwDatabaseInfoElement = xmlDoc["ExceptionDetails"]["EnvironmentInfo"]["EdwDatabaseInfo"];
				AssertNotNull(edwDatabaseInfoElement);

				var sqlServerVersionElement = edwDatabaseInfoElement["SQLServerVersion"];
				AssertEquals(string.Empty, sqlServerVersionElement.InnerText);
				var databaseServerName = edwDatabaseInfoElement["DatabaseServerName"];
				AssertEquals(string.Empty, databaseServerName.InnerText);
				var edwDatabaseName = edwDatabaseInfoElement["EdwDatabaseName"];
				AssertEquals(string.Empty, edwDatabaseName.InnerText);
				var edwDatabaseSchemaVersion = edwDatabaseInfoElement["EdwDatabaseSchemaVersion"];
				AssertEquals(string.Empty, edwDatabaseSchemaVersion.InnerText);
			}
		}

		#region Implementation

		static Exception CreateInnerExceptionStructure(Exception exc, int depth)
		{
			if (depth > 0)
			{
				exc = CreateInnerExceptionStructure(new Exception(string.Format("Tier {0}", depth), exc), depth - 1);
			}
			return exc;
		}

		void AssertDiskSpaceElement(XmlElement directoryElement, string expectedPath)
		{
			AssertNotNull(directoryElement);

			XmlElement pathElement = directoryElement["Path"];
			AssertNotNull(pathElement);
			AssertEquals(expectedPath.ToLower(), pathElement.InnerXml.ToLower());

			XmlElement freeForUserElement = directoryElement["FreeForUser"];
			AssertNotNull(freeForUserElement);
			AssertEquals("0 MB", freeForUserElement.InnerText);

			XmlElement availableToUserElement = directoryElement["AvailableToUser"];
			AssertNotNull(availableToUserElement);
			AssertEquals("0 MB", availableToUserElement.InnerText);

			XmlElement freeForSystemElement = directoryElement["FreeForSystem"];
			AssertNotNull(freeForSystemElement);
			AssertEquals("0 MB", freeForSystemElement.InnerText);
		}

		void AssertFileNameAndFusionLog(Exception ex, string fileName, string fusionLog)
		{
			SetFusionLog(ex, fusionLog);

			string report = new ExceptionDetails(ex).GetFullReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(report);

			Assert("Valid Xml", xmlDoc.OuterXml.Length > 0);
			XmlNodeList fileNameNodes = xmlDoc.SelectNodes("//FileName");
			XmlNodeList fusionLogNodes = xmlDoc.SelectNodes("//FusionLog");

			if (fileName != null)
			{
				AssertEquals(1, fileNameNodes.Count);
				AssertEquals(fileName, fileNameNodes[0].FirstChild.Value);
			}
			else
			{
				AssertEquals(0, fileNameNodes.Count);
			}

			if (fusionLog != null)
			{
				AssertEquals(1, fusionLogNodes.Count);
				AssertEquals(fusionLog, fusionLogNodes[0].FirstChild.Value);
			}
			else
			{
				AssertEquals(0, fusionLogNodes.Count);
			}

			AssertEquals(0, xmlDoc.SelectNodes("//Data").Count);
		}

		void AssertStartsWith(string[] expected, string[] actual)
		{
			AssertEquals("String[] length", expected.Length, actual.Length);
			for (int i = 0; i < expected.Length; i++)
			{
				Assert(actual[i] + "; should start with " + expected[i], actual[i].Trim().StartsWith(expected[i]));
			}
		}

		XPathNavigator GetNavigator(Exception ex)
		{
			string report = new ExceptionDetails(ex).GetFullReport();
			using (StringReader reader = new StringReader(report))
			{
				XPathDocument document = new XPathDocument(reader);
				return document.CreateNavigator();
			}
		}

		void SetFusionLog(Exception ex, string fusionLog)
		{
			ex.GetType().GetField("_fusionLog", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ex, fusionLog);
		}

		#region ObjectWithToStringException

		[Serializable]
		class ObjectWithToStringException
		{
			public ObjectWithToStringException()
			{
			}

			public override string ToString()
			{
				throw new Exception("ToString Bad!");
			}
		}

		#endregion

		#region TestTraceableException

		[Serializable]
		class TestTraceableException : Exception, ITraceableException
		{
			public TestTraceableException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected TestTraceableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public string GetTraceLog()
			{
				return "Test Trace Log";
			}
		}

		#endregion

		#endregion
	}
}
