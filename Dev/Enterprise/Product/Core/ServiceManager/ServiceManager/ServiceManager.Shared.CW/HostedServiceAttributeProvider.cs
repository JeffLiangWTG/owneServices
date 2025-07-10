using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	public class HostedServiceAttributeProvider : IHostedServiceAttributeProvider, IClientHostedServiceAttributeProvider, IHostedServiceCodeDescriptionProvider
	{
		public HostedServiceAttributeProvider()
		{
			allConfiguredServices = new Lazy<IHostedServiceAttribute[]>(() =>
				AssemblyMetaDataReader
					.GetAttributes<HostedServiceAttribute>(true)
					.Cast<IHostedServiceAttribute>()
					.ToArray()
			);

			clientFilteredConfiguredServices = new Lazy<IDictionary<string, IHostedServiceAttribute>>(() =>
			{
				var attributes = new Dictionary<string, IHostedServiceAttribute>();
				allConfiguredServices.Value
					.Where(a => a.IsForClient())
					.ForEach(a =>
					{
						if (attributes.TryGetValue(a.Code, out var existingAttribute))
						{
							throw new ArgumentException($"'{a.Code}' is already registered with type '{existingAttribute.Type.FullName}'. Attempted to register type '{a.TypeName}'");
						}

						attributes.Add(a.Code, a);
					});
				return attributes;
			});
		}

		public IEnumerable<IHostedServiceAttribute> GetHostedServiceAttributes()
		{
			return allConfiguredServices.Value;
		}

		public IHostedServiceAttribute GetHostedServiceAttribute(string assemblyName, string code)
		{
			return allConfiguredServices.Value.FirstOrDefault(t => t.TypeAssemblyName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)
				&& t.Code.Equals(code));
		}

		public IEnumerable<IHostedServiceAttribute> GetClientHostedServiceAttributes()
		{
			return clientFilteredConfiguredServices.Value.Values;
		}

		public IHostedServiceAttribute GetClientHostedServiceAttribute(string code)
		{
			return clientFilteredConfiguredServices.Value.TryGetValue(code, out var config) ? config : null;
		}

		public IEnumerable<HostedServiceCodeDescription> GetHostedServices()
		{
			return clientFilteredConfiguredServices.Value
				.Select(pair => new HostedServiceCodeDescription(pair.Value.Code, pair.Value.Description));
		}

		readonly Lazy<IDictionary<string, IHostedServiceAttribute>> clientFilteredConfiguredServices;
		readonly Lazy<IHostedServiceAttribute[]> allConfiguredServices;
	}
}

