using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace Enterprise.ServiceManager.HostsController
{
	class ServiceHostsProvider : IServiceHostsProvider
	{
		public string EnterpriseCode { get; } = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;

		public string ServerCode { get; } = ObjectFactory.Get<IProductRegistration>().Key.ServerCode;

		public IEnumerable<ServiceHostName> LoadActiveServiceHosts()
		{
			return LoadServiceHosts(true);
		}

		public IEnumerable<ServiceHostName> LoadAllServiceHosts()
		{
			return LoadServiceHosts(false);
		}

		static IEnumerable<ServiceHostName> LoadServiceHosts(bool excludeInactive)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new ReadOnlyBusinessObjectFactory(false)
				{
					NameForDebugging = "ServiceHostProvider Factory",
				};

				return factory.Load<StmServiceHost>(excludeInactive ? new ZQuery(StmServiceHostSchema.SH_IsActive, ZBool.True) : new ZQuery()).Select(host => new ServiceHostName(host.SH_HostName));
			}
		}
	}
}
