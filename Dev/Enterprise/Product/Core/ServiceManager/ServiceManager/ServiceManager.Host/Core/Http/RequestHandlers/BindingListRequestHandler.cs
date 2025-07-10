using System;
using System.Collections.Generic;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Host
{
	class BindingListRequestHandler : RequestHandler
	{
		public BindingListRequestHandler(IJsonConverter jsonConverter, string hostName)
		{
			this.jsonConverter = jsonConverter;
			Uri = ServiceManagerHelper.GetBindingListUri(hostName);
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			return GetServiceTaskBindings();
		}

		string GetServiceTaskBindings()
		{
			if (string.IsNullOrEmpty(serviceTaskBindings))
			{
				var serviceTaskBindingList = new List<ServiceTaskBindingDTO>();

				var tasks = HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings;
				foreach (var task in tasks)
				{
					var serviceTaskBinding = new ServiceTaskBindingDTO(task.ServiceTaskCode, task.Table, task.Predicates, task.QueueName);
					serviceTaskBindingList.Add(serviceTaskBinding);
				}

				var result = new ServiceTaskBindingListDTO(serviceTaskBindingList);
				serviceTaskBindings = jsonConverter.Serialize(result);
			}

			return serviceTaskBindings;
		}

		string serviceTaskBindings;
		readonly IJsonConverter jsonConverter;
	}
}
