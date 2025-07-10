using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Service
{
	class ApplicationTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		[ExpectNoExceptions]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestSetUpUserContext()
		{
			// Arrange
			using (TestHttpContextHelper.DisposableAppDomain(
						appDomain => { },
						() =>
						{
							var enterpriseGlobalMock = new Mock<Global> { CallBase = true };
							using (PopulateWebConfiguration())
							using (var httpApplication = enterpriseGlobalMock.Object)
							using (httpApplication.StartApplicationDisposable())
							using (TestHttpContextHelper.DisposableHttpContext(out _, out _, httpApplication))
							using (TestHttpContextHelper.DisposableSession(out _))
							using (httpApplication.StartSessionDisposable())
							{
								const int numOfThreads = 3;

								var threads = new Thread[numOfThreads];
								var userContexts = new ConcurrentBag<UserContext>();

								// Act
								for (var i = 0; i < numOfThreads; i++)
								{
									threads[i] = new Thread(() =>
									{
										userContexts.Add(httpApplication.ApplicationBeginRequest_ForTest(new object(), EventArgs.Empty));
									});
								}

								threads.ForEach(t => t.Start());
								threads.ForEach(t => t.Join());

								// Assert
								AssertEquals(numOfThreads, userContexts.Count);
								Assert(userContexts.All(x => x != null && x.User.IsWebUser));
							}

							IDisposable PopulateWebConfiguration()
							{
								var webConfiguration = new WebDbConfigurationInfo
								{
									ApplicationPath = WebAppPath.ForCurrentAppDomain(),
									DatabaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName"),
									ServerName = (string)AppDomain.CurrentDomain.GetData("ServerName"),
								};
								WebDbConfiguration.SaveConfiguration(webConfiguration);

								return new DisposableAction(() => WebDbConfiguration.DeleteAllConfigurations(webConfiguration.ApplicationPath));
							}
						}))
			{
			}
		}
	}
}
