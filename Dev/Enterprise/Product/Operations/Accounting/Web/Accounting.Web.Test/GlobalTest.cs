#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Configuration;
using AppDomainWrappers.Net;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	class GlobalTest : TestCase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00739605 - AppDomains - IISAppDomainNamePattern
		public void TestApplicationStart()
		{
			var appDomainWrapper = new AppDomainWrapper(FormattableString.Invariant($"/LM/W3SVC/1234/ROOT-1-130600647879179810"), true);
			_ = appDomainWrapper.RunActionInAppDomain(() =>
			{
				var currentDomain = AppDomain.CurrentDomain;
				ConfigurationManager.AppSettings["ServerName"] = (string)currentDomain.GetData("ServerName");
				ConfigurationManager.AppSettings["DatabaseName"] = (string)currentDomain.GetData("DatabaseName");

				var globalMock = new Mock<Global> { CallBase = true };
				globalMock.Protected().Setup("InitializeWebUpgradeBootstrapper").Callback(() => { });

				TestingState.SuspendIsRunningTests().Dispose();

				// Act
				using (var global = globalMock.Object)
				using (global.StartApplicationDisposable())
				{
					AssertEquals("IsUserInteractive is false", false, Globals.IsUserInteractive);
					AssertEquals("Env.GetCurrentProvider() is WebEnvironmentProvider", typeof(WebEnvironmentProvider), Env.GetCurrentProvider().GetType());
					AssertEquals("DbEnv.Instance is WebDbEnvironment", typeof(WebDbEnvironment), DbEnv.Instance.GetType());
				}
			},
			new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName },
			});
		}
	}
}
#endif
