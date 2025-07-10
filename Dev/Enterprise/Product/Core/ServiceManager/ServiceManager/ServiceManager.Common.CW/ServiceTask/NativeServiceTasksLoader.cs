using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW;

[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "no use of Business Object and Business Object Factory in Process Controller")]
public class NativeServiceTasksLoader : IServiceTasksLoader
{
	public NativeServiceTasksLoader()
		: this(
			ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
			ObjectFactory.Get<IServiceTaskScheduleStatusProvider>(),
			ObjectFactory.Get<IHostedServiceBusinessObjectBindingsProvider>(),
			new ServiceManagerDateTimeProvider())
	{ }

	public NativeServiceTasksLoader(
		IClientHostedServiceAttributeProvider attributeProvider,
		IServiceTaskScheduleStatusProvider statusProvider,
		IHostedServiceBusinessObjectBindingsProvider bindingsProvider,
		IDateTimeProvider dateTimeProvider)
	{
		this.attributeProvider = attributeProvider ?? throw new ArgumentNullException(nameof(attributeProvider));
		this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
		this.bindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
		this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
	}

	public IServiceTaskCollectionGovernor Load()
	{
		var query = @"
SELECT
ServiceTask.SST_PK,
ServiceTask.SST_ServiceTaskCode,
ServiceTask.SST_Active,
ServiceTask.SST_NextRunTime,
ServiceTask.SST_LastRunTime,
ServiceTask.SST_Configuration,
Branch.GB_PK,
Branch.GB_Code
FROM dbo.StmServiceTask ServiceTask
LEFT JOIN dbo.GlbBranch branch on ServiceTask.SST_GB_Branch = Branch.GB_PK
";

		using var cmd = Db.Connection.Command(query);

		using var reader = cmd.ExecuteReader();
		var dtos = new List<NativeServiceTaskDTO>();

		while (reader.Read())
		{
			dtos.Add(NativeServiceTaskDTO.CreateFromReader(reader));
		}
		return new NativeServiceTaskCollectionGovernor(dtos, attributeProvider, statusProvider, bindingsProvider, dateTimeProvider);
	}

	readonly IClientHostedServiceAttributeProvider attributeProvider;
	readonly IServiceTaskScheduleStatusProvider statusProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider bindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
}

