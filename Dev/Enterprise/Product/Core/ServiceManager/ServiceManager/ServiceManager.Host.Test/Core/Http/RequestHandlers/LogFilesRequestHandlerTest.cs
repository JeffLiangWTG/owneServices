using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(LogFilesRequestHandler))]
	class LogFilesRequestHandlerTest : RequestHandlerBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			handler = new LogFilesRequestHandler(string.Empty, DbServer, DbName);
			httpRequestInfoMock = new Mock<IHttpRequestInfo>();
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/logfiles");
		}

		[ExpectNoExceptions]
		public void TestReturnsFileNames()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(
					$"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/logfiles/HOST",
					new[] { "host.txt", "tes.txt" },
					new[] { "host.txt" });
				Test(
					$"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/logfiles/HOST",
					new[] { "host.txt", "host_.txt", "host_1.txt", "tes.txt" },
					new[] { "host.txt", "host_.txt", "host_1.txt" });
				Test(
					$"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/logfiles/TES",
					new[] { "host.txt", "host_.txt", "host_1.txt", "tes.txt" },
					new[] { "tes.txt" });
			});

			void Test(string uriString, IEnumerable<string> fileNames, IEnumerable<string> expectedResult)
			{
				// Arrange
				httpRequestInfoMock
					.SetupGet(info => info.Uri)
					.Returns(new Uri(uriString));

				using (CreateFilesTemporarily(fileNames))
				{
					// Act
					var result = handler.Handle(httpRequestInfoMock.Object)
						?.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
						?? Enumerable.Empty<string>();

					// Assert
					NUnit.Framework.Assert.That(result, Is.EquivalentTo(expectedResult));
				}
			}

			IDisposable CreateFilesTemporarily(IEnumerable<string> fileNames)
			{
				var filesDirectory = ServiceManagerHelper.GetLogFilesDirectory(DbServer, DbName);
				Directory.CreateDirectory(filesDirectory);
				var fullFileNames = fileNames
					.Select(s => Path.Combine(filesDirectory, s))
					.ToList();
				fullFileNames.ForEach(s => File.Create(s).Dispose());
				return new DisposableAction(() => Directory.Delete(filesDirectory, true));
			}
		}

		const string DbName = "dbName";
		const string DbServer = "dbServer";

		Mock<IHttpRequestInfo> httpRequestInfoMock;
	}
}
