using System;
using System.Reflection;
using System.Threading;
using CargoWise.EntityFramework;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	public static class ServiceProviderImplExtensionsForTest
	{
		public static void RunTask<T>(this T serviceProvider)
			where T : ServiceProviderImpl
		{
			IDisposable disposable = null;
			if (serviceProvider.GetType().GetCustomAttribute(typeof(NeedsDataRefreshAttribute)) == null)
			{
				disposable = new DataRefreshManager.DisableRefreshForServiceTask();
			}

			using (disposable) // only usable in tests, this just makes sure we don't have refresh enabled.
			{
				serviceProvider.RunTask(CancellationToken.None);
			}
		}
	}
}
