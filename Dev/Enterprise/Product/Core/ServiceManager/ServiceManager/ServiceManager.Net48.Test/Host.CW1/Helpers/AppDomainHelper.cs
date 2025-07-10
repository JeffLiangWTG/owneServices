using System;
using System.Collections.Generic;
using CargoWise.Data;
using WTG.AppDomainWrappers.Net;

namespace Enterprise.ServiceManager.Host.Testing.Helpers
{
	internal static class AppDomainHelper
	{
		internal static void RunInAnotherAppDomain(string domainFriendlyName, Action action)
		{
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName }
			};

			using (var appDomainWrapper = new AppDomainWrapper(domainFriendlyName, true))
			{
				appDomainWrapper.RunActionInAppDomain(action, domainData);
			}
		}
	}
}
