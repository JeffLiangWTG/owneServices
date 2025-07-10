using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using WTG.Foundation.Http;

namespace Enterprise.ServiceManager.Business.Testing
{
	class LogViewerDataProviderFactoryTest : TestCase
	{
		class LogViewerDataProviderTest : TestCaseWithFactory
		{
			public void TestInCaseOfMultipleSourcesLocalFileComesFirst()
			{
				// Arrange
				CleanUpStmServiceHost();
				var factory = new LogViewerDataProviderFactory();
				var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = true, Bool2 = true, SystemDefined = true, },
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = true, SystemDefined = true, },
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
				};
				value.SetDefaultCode(LoggingMethods.FSL, true);
				SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

				var host1 = Factory.New<StmServiceHost>();
				host1.SH_HostName = ServiceManagerHelper.GetHostName();
				Factory.Save();

				// Act
				var result = factory.GetProviders(string.Empty);

				// Assert
				AssertType<LogViewerDataProviderFactory.LocalLogViewerDataProvider>(result.First());
			}

			public void TestInCaseOfMultipleSourcesRemoteFileComesSecond()
			{
				// Arrange
				CleanUpStmServiceHost();
				var factory = new LogViewerDataProviderFactory();
				var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = true, Bool2 = true, SystemDefined = true, },
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = true, SystemDefined = true, },
					new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
				};
				value.SetDefaultCode(LoggingMethods.FSL, false);
				SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

				var host1 = Factory.New<StmServiceHost>();
				host1.SH_HostName = "someWeirdHostName";
				Factory.Save();

				// Act
				var result = factory.GetProviders(string.Empty);

				// Assert
				Assert(result.Last() is LogViewerDataProviderFactory.RemoteLogViewerDataProvider);
			}

			void CleanUpStmServiceHost()
			{
				var cleanupTasks = Factory.Load<StmServiceHost>(new ZQuery());
				foreach (var task in cleanupTasks)
				{
					task.Delete();
				}
			}
		}

		sealed class LocalLogViewerDataProviderTest : TestCase
		{
			public void TestGetFileNames()
			{
				var testFilesDirectory = Path.GetDirectoryName(resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.AD_20070720.TXT", "AD_20070720.TXT"));
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.ADC_20070720.TXT", "ADC_20070720.TXT");
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.DBM_20070720.TXT", "DBM_20070720.TXT");
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.LWK_20070720.TXT", "LWK_20070720.TXT");
				var files = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory, null).GetFileNames();
				AssertEquals(4, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("DBM_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("LWK_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory, "lwk").GetFileNames();
				AssertEquals(1, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("LWK_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory, "ad").GetFileNames();
				AssertEquals(1, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("AD_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory, "ts1").GetFileNames();
				AssertEquals(0, files.Length);

				files = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory + "###", "ts1").GetFileNames();
				AssertEquals(0, files.Length);
			}

			public void TestGetStream()
			{
				var testLogsDirectory = Path.GetDirectoryName(resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.LWK_20070720.TXT", "LWK_20070720.TXT"));
				var buffer = new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testLogsDirectory, "LWK").GetBytes("LWK_20070720.TXT");
				var collection = new EventRecordCollection();
				using (var stream = new MemoryStream(buffer))
				{
					collection.Load(stream);
				}

				AssertEquals(19, collection.Count);
			}

			protected override void SetUp()
			{
				base.SetUp();
				resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			}

			protected override void TearDown()
			{
				base.TearDown();
				if (resourceRetriever.IsValueCreated)
				{
					resourceRetriever.Value.Dispose();
				}
			}

			Lazy<EmbeddedResourceRetriever> resourceRetriever;
		}

		sealed class RemoteLogViewerDataProviderTest : TestCase
		{
			public void TestGetFileNames()
			{
				var testFilesDirectory = SetupTestFilesDirectory();
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var serviceHostClient = new Mock<IServiceHostClient>();
				serviceHostClient.SetupGet(sc => sc.HostName).Returns(new ServiceHostName("http://something/logs/"));
				serviceHostsCache.SetupGet(sc => sc.AvailableServiceHosts).Returns(new List<IServiceHostClient>() { serviceHostClient.Object });

				var files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", null, serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(4, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("DBM_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("LWK_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("AD_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("ADC_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", "lwk", serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(1, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("LWK_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", "ad", serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(1, files.Length);
				AssertNotNull(Array.Find(files, fileName => fileName.Equals("AD_20070720.TXT", StringComparison.CurrentCultureIgnoreCase)));

				files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", "ts1", serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(0, files.Length);

				files = new RemoteLogViewerDataProviderForTesting("http://something/logs/" + "###", "ts1", serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(0, files.Length);
			}

			public void TestGetFileNameWithInvalidURI()
			{
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var httpClientFactoryMock = new Mock<IHttpClientFactory>();
				var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				using var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>())).Returns(httpClient);
				using var serviceHostHttpClient = new ServiceHostHttpClient(new Mock<IServiceHostErrorReporter>().Object, TimeSpan.FromSeconds(1), httpClientFactoryMock.Object, new Mock<IJsonDeserializer>().Object);
				var serviceHostClient = new ServiceHostClient(new ServiceHostName("#@!&?"), GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID, serviceHostHttpClient);
				serviceHostsCache.SetupGet(sc => sc.AvailableServiceHosts).Returns(new List<IServiceHostClient>() { serviceHostClient });

				var files = new LogViewerDataProviderFactory.RemoteLogViewerDataProvider("#@!&?", "ts1", serviceHostsCache.Object).GetFileNames();
				AssertEquals(1, files.Length);
				AssertEquals("*Error: Invalid URI: The hostname could not be parsed.", files[0]);
			}

			public void TestGetFileNameWhenNotAvailable()
			{
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache.SetupGet(sc => sc.AvailableServiceHosts).Returns(new List<IServiceHostClient>());
				var testFilesDirectory = SetupTestFilesDirectory();
				var files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", null, serviceHostsCache.Object, testFilesDirectory).GetFileNames();
				AssertEquals(0, files.Length);
			}

			public void TestGetFileNameWithCommunicationException()
			{
				Exception ex = new ServiceHostCommunicationException();
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var serviceHostClient = new Mock<IServiceHostClient>();
				serviceHostClient.SetupGet(sc => sc.HostName).Returns(new ServiceHostName("http://something/logs/"));
				serviceHostsCache.SetupGet(sc => sc.AvailableServiceHosts).Returns(new List<IServiceHostClient>() { serviceHostClient.Object });
				var testFilesDirectory = SetupTestFilesDirectory();
				var files = new RemoteLogViewerDataProviderForTesting("http://something/logs/", null, serviceHostsCache.Object, ex, testFilesDirectory).GetFileNames();
				AssertEquals(0, files.Length);
			}

			public void TestGetStream()
			{
				var testFilesDirectory = SetupTestFilesDirectory();
				var buffer = new RemoteLogViewerDataProviderForTesting("http://something/logs/", "LWK", testFilesDirectory).GetBytes("LWK_20070720.TXT");
				var collection = new EventRecordCollection();
				using (var stream = new MemoryStream(buffer))
				{
					collection.Load(stream);
				}

				AssertEquals(19, collection.Count);
			}

			protected override void SetUp()
			{
				base.SetUp();
				resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			}

			protected override void TearDown()
			{
				base.TearDown();
				if (resourceRetriever.IsValueCreated)
				{
					resourceRetriever.Value.Dispose();
				}
			}

			Lazy<EmbeddedResourceRetriever> resourceRetriever;

			string SetupTestFilesDirectory()
			{
				var testFilesDirectory = Path.GetDirectoryName(resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.AD_20070720.TXT", "AD_20070720.TXT"));
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.ADC_20070720.TXT", "ADC_20070720.TXT");
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.DBM_20070720.TXT", "DBM_20070720.TXT");
				resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.LWK_20070720.TXT", "LWK_20070720.TXT");
				return testFilesDirectory;
			}
		}
	}
}
