using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW;

[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "no use of Business Object and Business Object Factory in Process Controller")]
public class NativeServiceTaskCollectionGovernor : IServiceTaskCollectionGovernor
{
	internal NativeServiceTaskCollectionGovernor(
		IEnumerable<NativeServiceTaskDTO> dtoCollection,
		IClientHostedServiceAttributeProvider attributeProvider,
		IServiceTaskScheduleStatusProvider statusProvider,
		IHostedServiceBusinessObjectBindingsProvider bindingsProvider,
		IDateTimeProvider dateTimeProvider)
	{
		dtos = dtoCollection ?? throw new ArgumentNullException(nameof(dtoCollection));
		this.attributeProvider = attributeProvider ?? throw new ArgumentNullException(nameof(attributeProvider));
		this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
		this.bindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
		this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
		governedTasks = new Lazy<List<NativeServiceTask>>(ConvertTasks);
	}

	public void SetActive(bool value, IEnumerable<Guid> taskPkFilter)
	{
		GovernedTasks
			.Where(t => taskPkFilter.IsNullOrEmpty() || taskPkFilter.Contains(t.Pk))
			.ForEach(t => t.AssignedGovernor.SetActive(value));
	}

	public void SetBranchPk(Guid pk, IEnumerable<Guid> taskPkFilter)
	{
		GovernedTasks
			.Where(t => taskPkFilter.IsNullOrEmpty() || taskPkFilter.Contains(t.Pk))
			.ForEach(t => t.AssignedGovernor.SetBranchPk(pk));
	}

	public void Reload()
	{
		var existingTaskPks = dtos.Where(IsInDatabase).Select(t => t.Pk).ToArray();

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

		var sb = new StringBuilder(query);
		if (existingTaskPks.Length > 0)
		{
			sb.AppendLine("WHERE SST_PK IN (");
			for (int i = 0; i < existingTaskPks.Length; i++)
			{
				sb.Append($"@TaskPk{i}");
				if (i < existingTaskPks.Length - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append(")");
		}

		using var cmd = Db.Connection.Command(sb.ToString());
		for (int i = 0; i < existingTaskPks.Length; i++)
		{
			cmd.AddParameter($"@TaskPk{i}", SqlDbType.UniqueIdentifier, existingTaskPks[i]);
		}
		using var reader = cmd.ExecuteReader();
		var updatedDtos = new List<NativeServiceTaskDTO>();

		while (reader.Read())
		{
			updatedDtos.Add(NativeServiceTaskDTO.CreateFromReader(reader));
		}

		dtos = dtos.Where(dto => !IsInDatabase(dto)).Concat(updatedDtos);
		governedTasks = new Lazy<List<NativeServiceTask>>(ConvertTasks);

		bool IsInDatabase(NativeServiceTaskDTO dto) => dto.Pk != Guid.Empty;
	}

	public IEnumerable<IServiceTask> GovernedTasks => governedTasks.Value;

	List<NativeServiceTask> ConvertTasks()
	{
		var tasks = new List<NativeServiceTask>();
		foreach (var dto in dtos)
		{
			tasks.Add(new NativeServiceTask(dto, attributeProvider, bindingsProvider, statusProvider, dateTimeProvider));
		}
		return tasks;
	}

	Lazy<List<NativeServiceTask>> governedTasks;
	IEnumerable<NativeServiceTaskDTO> dtos;

	readonly IClientHostedServiceAttributeProvider attributeProvider;
	readonly IServiceTaskScheduleStatusProvider statusProvider;
	readonly IHostedServiceBusinessObjectBindingsProvider bindingsProvider;
	readonly IDateTimeProvider dateTimeProvider;
}
