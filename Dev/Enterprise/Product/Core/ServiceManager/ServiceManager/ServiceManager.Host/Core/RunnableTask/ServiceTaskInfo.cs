using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enterprise.MasterFiles.Integration;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;

namespace Enterprise.ServiceManager.Host
{
	[DebuggerDisplay("{Code}, {Description}")]
	public class ServiceTaskInfo : IServiceTaskInfo
	{
		public ServiceTaskInfo(IHostedServiceAttribute hostedServiceAttribute)
		{
			serviceConfig = new HostedServiceConfiguration(hostedServiceAttribute ?? throw new ArgumentNullException(nameof(hostedServiceAttribute)));
		}

		public string Code => serviceConfig.ServiceAttribute.Code;
		public string Description => serviceConfig.ServiceAttribute.Description;
		public string Category => serviceConfig.ServiceAttribute.Category;

		public IEnumerable<string> BestResultsFromRequirementsCheck { get; private set; }

		public string[] CheckSatisfiesRequirementsForCurrentBranch(IEnumerable<IGlbCompany> activeCompanies)
		{
			var failureDescriptions = serviceConfig.CheckSatisfiesRequirementsForCurrentBranch(activeCompanies);
			if (BestResultsFromRequirementsCheck == null || failureDescriptions.Length < BestResultsFromRequirementsCheck.Count())
			{
				BestResultsFromRequirementsCheck = failureDescriptions;
			}
			return failureDescriptions;
		}

		public IHostedServiceAttribute HostedServiceAttribute => serviceConfig.ServiceAttribute;

		public bool ErrorOnLastRun
		{
			get { return errorOnLastRun; }
			set { errorOnLastRun = value; }
		}

		volatile bool errorOnLastRun;
		readonly HostedServiceConfiguration serviceConfig;
	}
}
