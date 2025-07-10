using System.Linq;
using CargoWise.Application;
using CargoWise.Main.Startup.Tasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	static class PostLoginTasks
	{
		public static IPostLoginTask[] GetPostLoginTasks()
		{
			var postLoginTasks = new IPostLoginTask[] {
				new StartupInitDisableMemoryManager(),
#if !WINZOR
				new LaunchDefaultDotNetVersionTask(),
				new ForceCW1HomeScreenTask(),
				new PostLoginInitEnterpriseUrlHandlerService(),
				new RemoteDesktopServicesPostLoginTask(),
				new EnableHybridModeStartupTask(),
				new EnforceWebVersionTask(),
#endif
#if WINZOR
				new MainFormTask<RegisterWinzorUrlHandlerService>(),
#endif
				new MainFormTask<ConfigurationItemChecker>(),
				new StartupEnableActivityLogger(),
				new MainFormTask<StartupShowStaffForm>(),
#if !WINZOR
				new MainFormTask<StartupADPasswordExpiryChecker>(),
#endif
				new MainFormTask<WindowPersisterTask>(),
				new MainFormTask<StartupCheckForthcomingSystemUpgrade>(),
				new MacroCacheInitializerTask(),
				new MainFormTask<StartupDbHealthRestoreWarningChecker>(),
				new DbUpgradeCaptionCacheTask()
			};

			var countrySpecificTasks = ObjectFactory.GetCountrySpecificOrDefault<PostLoginTasksProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).GetPostLoginTasks();
			return countrySpecificTasks.Count > 0
				? postLoginTasks.Concat(countrySpecificTasks).ToArray()
				: postLoginTasks;
		}
	}
}
