using System;

namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceAttribute : IHostedServiceType, IHostedServiceProcess, IHostedServiceControl, IHostedServiceTaskSpecificValidation
	{
		string Code { get; }
		string Description { get; }
		string Category { get; }

		bool IsMandatory { get; }
		bool AllowsMultipleInstances { get; }

		string MinimumPeriod { get; }
		string MaximumPeriod { get; }
		bool IsScheduleReadOnly { get; }
		bool IsReadOnlyForWiseCloudClient { get; }
		string RequiresCompanyInCountry { get; }
		bool CanRunInAnyBranch { get; }
		MutuallyExclusiveServiceTaskGroups MutuallyExclusiveTaskGroup { get; }
		bool IsForClient();
		Type Type { get; }
		bool AlwaysRunAtStartup { get; }
		bool ActiveByDefault { get; }

		IDefaultSchedule DefaultSchedule { get; }
	}
}
