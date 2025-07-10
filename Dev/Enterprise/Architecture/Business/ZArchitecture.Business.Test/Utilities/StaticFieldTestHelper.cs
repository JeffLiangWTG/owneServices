using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class StaticFieldTestHelper
	{
		public static DisposableAction CacheAndRestoreStaticCommandLineArguments()
		{
			var cachedArg = CommandLineArguments.UsedToLaunchApplication;
			return new DisposableAction(() => CommandLineArguments.UsedToLaunchApplication = cachedArg);
		}
	}
}
