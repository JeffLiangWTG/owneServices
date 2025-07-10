using System;
using CargoWise.Common;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Test
{
	internal static class ApplicationArgumentsTestHelper
	{
		public static IDisposable TemporaryApplicationArguments(string[] args)
		{
			var originalArgs = CommandLineArguments.UsedToLaunchApplication;
			CommandLineArguments.UsedToLaunchApplication = new ApplicationArguments(args);
			return new DisposableAction(() => CommandLineArguments.UsedToLaunchApplication = originalArgs);
		}
	}
}
