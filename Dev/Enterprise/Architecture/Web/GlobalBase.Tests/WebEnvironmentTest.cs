using System;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	class WebEnvironmentTest : TestCase
	{
		[ExpectNoExceptions]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestWebEnvironment()
		{
			// Arrange
			using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, () =>
			{
				var webConfiguration = new WebDbConfigurationInfo()
				{
					ApplicationPath = WebAppPath.ForCurrentAppDomain(),
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					DatabaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName"),
					ServerName = (string)AppDomain.CurrentDomain.GetData("ServerName")
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
				};
				using (new DisposableAction(() => WebDbConfiguration.DeleteAllConfigurations(webConfiguration.ApplicationPath)))
				{
					WebDbConfiguration.SaveConfiguration(webConfiguration);

					var expectedAppPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
					var webUserMock = new Mock<WebUser>();
					var loggedInUserMock = new Mock<IContactable>();
					var enterpriseGlobalMock = new Mock<ZEnterpriseGlobal> { CallBase = true };

					webUserMock.Protected().Setup<IContactBase>("LoggedInUserCore").Returns(loggedInUserMock.Object);
					enterpriseGlobalMock.Setup(x => x.GetNewSiteUser()).Returns(webUserMock.Object);

					using (var enterpriseGlobal = enterpriseGlobalMock.Object)
					using (enterpriseGlobal.StartApplicationDisposable())
					using (TestHttpContextHelper.DisposableHttpContext(out _, out _, enterpriseGlobal))
					using (TestHttpContextHelper.DisposableSession(out _))
					using (enterpriseGlobal.StartSessionDisposable())
					{
						var webEnvironment = (WebEnvironment)Enterprise.Environment.Env.Instance;

						// Act
						// Assert
						var assemblyLoader = (WebAssemblyLoader)AssemblyLoader.Instance;
						var environmentInstance = Enterprise.Environment.Env.Instance;

						CombineAssertions(() =>
						{
							AssertNotNull(nameof(assemblyLoader), assemblyLoader);
							AssertNotNull(nameof(environmentInstance), environmentInstance);
							AssertType<WebEnvironment>(nameof(environmentInstance), environmentInstance);
							AssertEquals(nameof(environmentInstance.IsWeb), true, environmentInstance.IsWeb);
							AssertType<WebSemaphoreProvider>(nameof(IEnvironment.SemaphoreProvider), ((IEnvironment)environmentInstance).SemaphoreProvider);
							AssertEquals(nameof(webEnvironment.ApplicationStartupPath), expectedAppPath, webEnvironment.ApplicationStartupPath);
							AssertNull("There is no LoginController configured from the base", webEnvironment.LoginController);
							AssertEquals(nameof(webEnvironment.WebUser), loggedInUserMock.Object, webEnvironment.WebUser);
						});
					}
				}
			}))
			{ }
		}
	}
}
