using System;
using System.Diagnostics;
using System.Reflection;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Loader.Testing.Installers
{
	[TestRequiresAdministrativePrivileges("Create event log source")]
	class EventSourceCreatorTest : TestCase
	{
		MockEnterpriseConfiguration configuration;
		MockEventSourceCreator mockInstaller;
		string testSourceName;

		protected override void SetUp()
		{
			testSourceName = "TestEventSourceCreator";

			configuration = new MockEnterpriseConfiguration();
			configuration.ServerName = "myserver";
			configuration.DatabaseName = "mydb";

			mockInstaller = new MockEventSourceCreator(new ClientInstallation(configuration), testSourceName);
			configuration.SetAppManagerClient(new AppManagerForTesting(mockInstaller));
			if (EventLog.SourceExists(testSourceName))
			{
				EventLog.DeleteEventSource(testSourceName);
			}
		}

		public void TestEventSourceCreator()
		{
			Assert(!EventLog.SourceExists(testSourceName));
			Assert(mockInstaller.InstallExcludingDependencies().IsOK);
			Assert(EventLog.SourceExists(testSourceName));
			EventLog.DeleteEventSource(testSourceName);
		}

		public void TestHasParameterlessConstructor()
		{
			var type = typeof(EventSourceCreator);
			AssertNotNull(type.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null, Type.EmptyTypes, null));
		}

		class AppManagerForTesting : MockAppManager
		{
			readonly MockEventSourceCreator host;

			public AppManagerForTesting(MockEventSourceCreator host)
			{
				this.host = host;
			}

			public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
			{
				return ((IAppManagerInvocable)host).Invoke(false, state);
			}
		}

		class MockEventSourceCreator : EventSourceCreator
		{
			public MockEventSourceCreator(Installation installation, string eventSourceName) : base(installation)
			{
				this.eventSourceName = eventSourceName;
			}
			readonly string eventSourceName;

			protected override string GetEventSourceName()
			{
				return eventSourceName;
			}

			public new InstallationResult InstallExcludingDependencies() => base.InstallExcludingDependencies();
		}
	}
}
