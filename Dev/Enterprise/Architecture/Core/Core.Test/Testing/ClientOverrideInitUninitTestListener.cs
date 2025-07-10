using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ClientOverrideInitUninitTestListener : BaseTestListener
	{
		#region Singleton

		public static ClientOverrideInitUninitTestListener Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new ClientOverrideInitUninitTestListener();
				}

				return fInstance;
			}
		}

		static ClientOverrideInitUninitTestListener fInstance;

		protected ClientOverrideInitUninitTestListener()
		{
		}

		#endregion

		public override void StartTest(TestCase test, DateTime startTime)
		{
			var clientLoader = ClientHookLoader.Instance;
			clientLoader.RemoveOverrideClientAssembliesForTest();

			var temporaryEnvironmentRequired = false;
			var testAssembly = test.GetType().Assembly;

			if (Globals.IsTest && IsTestClientAssembly)
			{
				testAssembly = clientLoader.FindAssemblyForTest(nameof(Clients.EDI));
			}

			overriddenClientAssembly = clientLoader.OverrideClientAssemblyForTestIfNeeded(testAssembly);

			if (overriddenClientAssembly != null)
			{
				temporaryEnvironmentRequired = true;
				RowFactory.ResetUniqueColumnInfo();
				(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			}
			else
			{
				overriddenClientAssembly = clientLoader.OverrideClientAssemblyForTest(null);
			}

			if (temporaryEnvironmentRequired && temporaryEnvironment == null)
			{
				temporaryEnvironment = ConfigureTemporaryEnvironment();
			}
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			overriddenClientAssembly?.Dispose();
			temporaryEnvironment?.Dispose();
			if (temporaryEnvironment != null)
			{
				(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			}
			temporaryEnvironment = null;
		}

		IDisposable ConfigureTemporaryEnvironment()
		{
			var environment = EnvProxy.Instance;

			var currentContext = environment.CurrentUserContext;
			var newFactory = new BusinessObjectFactory { NameForDebugging = TemporaryFactoryName };
			var userContextType = ObjectFactory.GetType<IUserContext>();
			var cleanUserContext = Activator.CreateInstance(userContextType, currentContext.User.PK, currentContext.Branch.PK, currentContext.Department.PK, null, true, newFactory) as IUserContext;

			return (environment as IEnvironmentForTest)?.SetTemporaryMasterUserContext(cleanUserContext);
		}

		#region Implementation

		IDisposable overriddenClientAssembly;
		IDisposable temporaryEnvironment;
		const string TemporaryFactoryName = "TemporaryUserContext";

#if DEBUG
		internal bool IsTestClientAssembly;
#endif

		#endregion
	}
}
