
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using eServices.eHubDataModel.eHubTransactions;

namespace eServices.Shared.RoutingRuleEngine
{
	public class ServiceProviderCollection : IEnumerable<ServiceProvider>
	{
		internal ServiceProviderCollection(eHubClient serviceClient)
		{
			this.serviceClient = serviceClient;
			this.serviceProviders = new List<ServiceProvider>();
			foreach (var prov in serviceClient.eHubServiceProviders_Service)
				this.serviceProviders.Add(new ServiceProvider(prov));
		}

		public ServiceProvider Add(ServiceProvider serviceProvider)
		{
			this.serviceProviders.Add(serviceProvider);
			this.serviceClient.eHubServiceProviders_Service.Add(serviceProvider.eHubServiceProvider);
			serviceProvider.SetRuleContexts(this.rule, this.persistingContext);
			return serviceProvider;
		}

		public void Clear()
		{
			foreach (var serviceProvider in this.serviceProviders)
				serviceProvider.Delete();
			this.serviceProviders.Clear();
			this.serviceClient.eHubServiceProviders_Service.Clear();
		}

		public bool Contains(ServiceProvider item)
		{
			return this.serviceProviders.Contains(item);
		}

		public int Count
		{
			get { return this.serviceProviders.Count; }
		}

		public bool Remove(ServiceProvider item)
		{
			bool removed = this.serviceProviders.Remove(item);
			if (removed)
			{
				item.Delete();
				this.serviceClient.eHubServiceProviders_Service.Remove(item.eHubServiceProvider);
			}
			return removed;
		}

		public IEnumerator<ServiceProvider> GetEnumerator()
		{
			return (IEnumerator<ServiceProvider>)this.serviceProviders.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.serviceProviders.GetEnumerator();
		}

		public ServiceProvider this[int index]
		{
			get
			{
				return (ServiceProvider)this.serviceProviders[index];
			}
			set
			{
				this.serviceClient.eHubServiceProviders_Service.Remove(this.serviceProviders[index].eHubServiceProvider);
				this.serviceProviders[index].Delete();
				this.serviceProviders[index] = value;
				this.serviceClient.eHubServiceProviders_Service.Add(value.eHubServiceProvider);
				value.SetRuleContexts(this.rule, this.persistingContext);
			}
		}

		internal void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.serviceProviders.ForEach(p => p.SetRuleContexts(rule, persistingContext));
			this.persistingContext = persistingContext;
			this.rule = rule;
		}

		readonly eHubClient serviceClient;
		readonly List<ServiceProvider> serviceProviders;
		eHubTransactionsContext persistingContext;
		Rule rule;
	}
}
