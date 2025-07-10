#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Web.Hosting;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure;
using NUnit.Framework;
using WTG.AppDomainWrappers.Net;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	class ZEnterpriseHttpTest
	{
		public class EnterpriseHttpApplicationTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestApplicationInitializationWithException()
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName }
				};

				using (var appDomainWrapper = new AppDomainWrapper("/LM/W3SVC/1234/ROOT-1-130600647879179810"))
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
						ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
						var global = new GlobalWithExceptionForTest();
						AssertNoExceptionThrown(() => global.Application_Start_Exposed());
						AssertEquals("Application should be started.", true, global.Started);
					}, domainData);
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}

			[ExpectNoExceptions]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
			[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
			public void TestExceptionMessageIncludeDatabaseServerNameAndApplicationSite()
			{
				const string tempPath = @"C:\temp\path";
				using (TestHttpContextHelper.DisposableAppDomain(appDomain => { },
				() =>
				{
					var timeStarted = DateTime.Now;
					var global = new GlobalWithExceptionForTest();
					global.Application_Start_Exposed();

					var logs = GetEventLogs(WebInfrastructureConstants.EventSourceName, timeStarted, 2);

					AssertGreaterThanOrEqualTo("We should have at least 1 log or more", logs.Count, 1);

					foreach (var log in logs)
					{
						Assert("We should have the server name in the error message", log.Contains(Db.ServerName));
						Assert("We should have the database name in the error message", log.Contains(Db.DatabaseName));
						Assert("We should have the virtual app path in the error message", log.Contains("/"));
						Assert("We should have the site in the error message", log.Contains(nameof(TestExceptionMessageIncludeDatabaseServerNameAndApplicationSite)));
						Assert("We should have the physical path in the error message", log.Contains(tempPath));
						Assert("We should have exception type in the error message", log.Contains("InvalidOperationException"));
					}
				}, tempPath, hostingEnvironment =>
				{
					if (hostingEnvironment != null)
					{
						typeof(HostingEnvironment)
							.GetField("_siteName", BindingFlags.NonPublic | BindingFlags.Instance)
							?.SetValue(hostingEnvironment, nameof(TestExceptionMessageIncludeDatabaseServerNameAndApplicationSite));

						typeof(HostingEnvironment)
							.GetField("_siteName", BindingFlags.NonPublic | BindingFlags.Instance)
							?.SetValue(hostingEnvironment, nameof(TestExceptionMessageIncludeDatabaseServerNameAndApplicationSite));
					}
				}))
				{ }
			}

			static List<string> GetEventLogs(string source, DateTime sinceTimeCreated, int level = 4 /* info */)
			{
				var eventLogs = new List<string>();
				var eventsQuery = new EventLogQuery(
					"Application",
					PathType.LogName,
					$@"
*[
	System/Level>={level}
	and
	System/Provider/@Name='{source}'
	and
	System/TimeCreated/@SystemTime > '{sinceTimeCreated.ToUniversalTime():o}'
]");
				using (var eventLogReader = new EventLogReader(eventsQuery))
				{
					var eventRecord = eventLogReader.ReadEvent();
					while (eventRecord != null)
					{
						eventLogs.Add(eventRecord.FormatDescription());
						eventRecord = eventLogReader.ReadEvent();
					}
				}

				return eventLogs;
			}

			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestHandlesUpgrade()
			{
				// Arrange
				using (var adminConnection = Db.NewAdminConnection())
				{
					var originalSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(originalSchemaVersion + 1, adminConnection);
					AssertEquals("Precondition: Schema version increased", originalSchemaVersion + 1, DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection));
				}
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName }
				};

				using (var appDomainWrapper = new AppDomainWrapper("/LM/W3SVC/1234/ROOT-1-130600647879179811"))
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
						ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
						var global = new GlobalForTest();
						AssertNoExceptionThrown(() => global.Application_Start_Exposed());

						// Act
						// Assert
						AssertNoExceptionThrown(() => global.CreateWebUpgradeManager_Exposed());
						AssertNoExceptionThrown(() => global.GetSqlConnection_Exposed());
						AssertEquals("Application should be started.", true, global.Started);
					}, domainData);
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}
		}

		public class GlobalWithExceptionForTest : EnterpriseHttpApplication
		{
			public void Application_Start_Exposed()
			{
				Application_Start(null, EventArgs.Empty);
				Started = true;
			}

			public bool Started { get; private set; }

			protected override WebUpgradeManager CreateWebUpgradeManager()
			{
				throw new InvalidOperationException();
			}
		}

		public class GlobalForTest : EnterpriseHttpApplication
		{
			public void Application_Start_Exposed()
			{
				Application_Start(null, EventArgs.Empty);
				Started = true;
			}

			public WebUpgradeManager CreateWebUpgradeManager_Exposed()
			{
				return CreateWebUpgradeManager();
			}

			public System.Data.Common.DbConnection GetSqlConnection_Exposed()
			{
				return GetSqlConnection();
			}

			public bool Started { get; private set; }
		}
	}
}
#endif
