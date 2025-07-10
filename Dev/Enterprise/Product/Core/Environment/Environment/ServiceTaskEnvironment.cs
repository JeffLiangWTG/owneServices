using System.IO;
using System.Reflection;
using CargoWise.Application;
using Enterprise.Environment.Semaphore;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Environment
{
	public class ServiceTaskEnvironment : BaseEnvironment
	{
		internal ServiceTaskEnvironment() : base(new MultiThreadUserContextManager())
		{
		}

		#region Enterprise Semaphore Provider

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider != null)
				{
					return Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider;
				}
#endif

				return semaphoreProvider;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public override string ApplicationStartupPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

		public override IUserLoginController LoginController => loginController ?? (loginController = ObjectFactory.Get<IUserLoginController>());
		IUserLoginController loginController;

		public override void ExitApplication() => System.Environment.Exit(0);

		[ThreadSafe]
#if DEBUG
		public
#endif
		static readonly ISemaphoreProvider semaphoreProvider = new ServiceTaskSemaphoreProvider();

		#endregion
	}
}
