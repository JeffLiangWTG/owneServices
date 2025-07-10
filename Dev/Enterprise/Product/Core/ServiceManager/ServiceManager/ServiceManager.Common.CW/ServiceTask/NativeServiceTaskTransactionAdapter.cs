using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW;

[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "no use of Business Object and Business Object Factory in Process Controller")]
public class NativeServiceTaskTransactionAdapter : ITransactionAdapter
{
	public NativeServiceTaskTransactionAdapter()
		: this(
			ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
			ObjectFactory.Get<IServiceTaskScheduleStatusProvider>(),
			ObjectFactory.Get<IHostedServiceBusinessObjectBindingsProvider>(),
			new ServiceManagerDateTimeProvider())
	{ }

	public NativeServiceTaskTransactionAdapter(IClientHostedServiceAttributeProvider attributeProvider,
		IServiceTaskScheduleStatusProvider statusProvider,
		IHostedServiceBusinessObjectBindingsProvider bindingsProvider,
		IDateTimeProvider dateTimeProvider)
	{
		this.attributeProvider = attributeProvider ?? throw new ArgumentNullException(nameof(attributeProvider));
		this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
		this.bindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
		this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
		tasksLoader = new Lazy<NativeServiceTasksLoader>(() => new NativeServiceTasksLoader(attributeProvider, statusProvider, bindingsProvider, dateTimeProvider));
	}

	public IServiceTaskGovernor? GetServiceTaskGovernor(Guid pk)
	{
		return GetNativeServiceTaskGovernor("SST_PK", "@ServiceTaskPk", SqlDbType.UniqueIdentifier, pk);
	}

	public IServiceTaskGovernor? GetServiceTaskGovernor(string code)
	{
		return GetNativeServiceTaskGovernor("SST_ServiceTaskCode", "@ServiceTaskCode", SqlDbType.VarChar, code);
	}

	NativeServiceTaskGovernor? GetNativeServiceTaskGovernor(string columnName, string filterName, SqlDbType filterType, object filterValue)
	{
		var query = $@"
SELECT TOP 1
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
WHERE {columnName} = {filterName}
";
		using var cmd = Db.Connection.Command(query);
		cmd.AddParameter(filterName, filterType, filterValue);

		using var reader = cmd.ExecuteReader();
		NativeServiceTaskDTO? dto = null;
		while (reader.Read())
		{
			dto = NativeServiceTaskDTO.CreateFromReader(reader);
		}

		if (dto == null)
		{
			return null;
		}
		var governor = new NativeServiceTaskGovernor(dto, attributeProvider, statusProvider, bindingsProvider, dateTimeProvider);
		governors.Add(governor);
		return governor;
	}

	public IServiceTaskGovernor GetNewServiceTaskGovernor(IHostedServiceAttribute newTaskAttribute)
	{
		var branchPk = newTaskAttribute.CanRunInAnyBranch ? Guid.Empty : GetDefaultBranchPk();

		var dto = new NativeServiceTaskDTO(newTaskAttribute, branchPk);

		var governor = new NativeServiceTaskGovernor(dto, attributeProvider, statusProvider, bindingsProvider, dateTimeProvider);
		if (branchPk != null && branchPk != Guid.Empty)
		{
			governor.SetBranchPk((Guid)branchPk);
		}
		governors.Add(governor);
		return governor;
	}

	public IServiceTaskCollectionGovernor GetCollectionGovernorForAllTasks()
	{
		collectionGovernor ??= TasksLoader.Load();
		return collectionGovernor;
	}

	public event EventHandler? Committing;

	void OnCommit()
	{
		Committing?.Invoke(this, EventArgs.Empty);
	}

	public void Commit()
	{
		var allGovernors = collectionGovernor != null
			? governors.Concat(collectionGovernor.GovernedTasks.Select(t => (NativeServiceTaskGovernor)t.AssignedGovernor))
			: governors;

		OnCommit();
		foreach (var governor in allGovernors)
		{
			governor.Save();
		}
	}

	public void Dispose()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		governors = null;
		tasksLoader = null;
		collectionGovernor = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		GC.SuppressFinalize(this);
	}

	NativeServiceTasksLoader TasksLoader => tasksLoader.Value;

	Guid? GetDefaultBranchPk()
	{
		var sqlText = @"
DECLARE @DefaultCompanyPk UNIQUEIDENTIFIER;

SELECT TOP 1 @DefaultCompanyPk = GC_PK FROM dbo.GlbCompany WHERE GC_IsActive = 1 AND GC_Code != 'DEM';

IF @DefaultCompanyPk IS NOT NULL
BEGIN
	SELECT TOP 1 GB_PK
	FROM dbo.GlbBranch
	WHERE GB_IsActive = 1 AND GB_GC = @DefaultCompanyPk
	ORDER BY GB_Code
END
";

		using var cmd = Db.Connection.Command(sqlText);
		return (Guid?)cmd.ExecuteScalar();
	}

	readonly IClientHostedServiceAttributeProvider attributeProvider;
	readonly IServiceTaskScheduleStatusProvider statusProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider bindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
	List<NativeServiceTaskGovernor> governors = new ();
	Lazy<NativeServiceTasksLoader> tasksLoader;
	IServiceTaskCollectionGovernor? collectionGovernor;
}
