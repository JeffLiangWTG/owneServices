using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceHostCollection : BusinessObjectCollection<StmServiceHost>
	{
		public StmServiceHostCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static StmServiceHost[] Load(BusinessObjectFactory factory, ZQuery filters, IServiceHostsCache serviceHostsCache, bool checkStatus)
		{
			var query = new ZQuery { IsNoResultQuery = filters.IsNoResultQuery };
			var compositeQueryParts = filters.GetCompositeParts();
			foreach (var filter in compositeQueryParts)
			{
				query.AddToFilter(filter);
			}

			if (checkStatus)
			{
				return LoadWithStatus(factory, query, serviceHostsCache);
			}
			return LoadWithoutStatus(factory, query);
		}

		static StmServiceHost[] LoadWithoutStatus(BusinessObjectFactory factory, ZQuery filters)
		{
			return factory.Load<StmServiceHost>(filters);
		}

		static StmServiceHost[] LoadWithStatus(BusinessObjectFactory factory, ZQuery filters, IServiceHostsCache serviceHostsCache)
		{
			var configuredHosts = serviceHostsCache.ConfiguredServiceHosts
				.ToDictionary(host => host.HostName.Hostname, host => host.IsAlive());

			var result = factory.Load<StmServiceHost>(filters);
			PopulateData(configuredHosts);
			return result;

			void PopulateData(IDictionary<string, bool> hostStatus)
			{
				if (!hostStatus.Any())
				{
					return;
				}

				foreach (var host in result)
				{
					hostStatus.TryGetValue(host.SH_HostName, out var isAlive);
					host.IsResponding = isAlive ? StmServiceHost.ResponseStatus.Running : StmServiceHost.ResponseStatus.Stopped;
				}
			}
		}
	}
}

