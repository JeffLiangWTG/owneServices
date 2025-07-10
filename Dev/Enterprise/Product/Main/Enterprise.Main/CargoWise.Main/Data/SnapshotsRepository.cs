using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Data;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant SD_NAME.")]
[SuppressMessage("dotnet", "IDE0270", Justification = "null check is required")]
[SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]

public class SnapshotsRepository : ISnapshotsRepository
{
	const string SD_NAME = "Snapshot";
	BusinessObjectFactory _factory;
	BusinessObjectFactory Factory => _factory ??= new BusinessObjectFactory();

	ModuleList moduleList;
	ModuleList ModuleList => moduleList ??= new ModuleList();

	readonly SnapshotsModulesProvider ModulesProvider = new();

	public Task<IEnumerable<SnapshotModule>> FindModulesAsync() => Task.Run(FindModules);
	public IEnumerable<SnapshotModule> FindModules() => ModulesProvider.FindModules();

	public Task<IEnumerable<Snapshot>> FindByUserIdAsync(Guid userId) => Task.Run(() => FindByUserId(userId));

	public IEnumerable<Snapshot> FindByUserId(Guid userId)
	{
		var query = new ZQuery(StmDataSchema.SD_Owner, userId);
		query.AddToFilter(JoinCondition.And, StmDataSchema.SD_Name, SQLComparisonOperator.Like, $"{SD_NAME}_%");
		query.OrderBy = $"{StmDataSchema.SD_Name.Name} asc";

		foreach (var stm in Factory.Load<StmData>(query))
		{
			var snapshot = new Snapshot
			{
				Id = stm.PK.ToGuid(),
				Owner = stm.SD_Owner.ToGuid(),
				Order = ParseOrder(stm.SD_Name),
				ModuleFilter = GetModuleFilter(stm.SD_GuidValue.ToGuid()),
			};

			yield return snapshot;
		}
	}

	public SnapshotModuleFilter GetModuleFilter(Guid moduleFilterId)
	{
		if (moduleFilterId == Guid.Empty)
		{
			return null;
		}

		var filter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.PK, moduleFilterId));
		if (filter == null)
		{
			return null;
		}

		var moduleId = ModuleList.GetRegisteredIdentifierByTableName(filter.S9_ModuleID);
		return new SnapshotModuleFilter
		{
			ModuleFilterId = filter.PK.ToGuid(),
			ModuleFilterName = filter.S9_FilterName,
			ModuleId = moduleId,
			ModuleName = moduleId?.ExtendedDescription,
		};
	}

	public Task<Guid> CreateAsync(Snapshot snapshot) => Task.Run(() => Create(snapshot));
	public Guid Create(Snapshot snapshot)
	{
		Validate(snapshot);

		var s = Factory.New<StmData>();
		s.SD_Name = $"{SD_NAME}_{snapshot.Order:D3}";
		s.SD_Owner = snapshot.Owner;
		s.SD_GuidValue = snapshot.ModuleFilter.ModuleFilterId;

		Factory.Save();
		return s.PK.ToGuid();
	}

	public Task<int> UpdateAsync(Snapshot snapshot) => Task.Run(() => Update(snapshot));
	public int Update(Snapshot snapshot)
	{
		var data = FindStmDataById(snapshot.Id);
		if (data == null)
		{
			return 0;
		}

		Validate(snapshot);

		data.SD_Owner = snapshot.Owner;
		data.SD_Name = $"{SD_NAME}_{snapshot.Order:D3}_{snapshot.Id}";
		data.SD_GuidValue = snapshot.ModuleFilter.ModuleFilterId;
		Factory.Save();

		return 1;
	}

	public Task<int> DeleteAsync(Guid pk) => Task.Run(() => Delete(pk));
	public int Delete(Guid pk)
	{
		var data = FindStmDataById(pk);
		if (data == null)
		{
			return 0;
		}

		data.Delete();
		Factory.Save();

		return 1;
	}

	public Task<IEnumerable<SnapshotModuleFilter>> FindModuleFiltersByModuleIdAsync(ModuleIdentifier moduleId) => Task.Run(() => FindModuleFiltersByModuleId(moduleId));
	public IEnumerable<SnapshotModuleFilter> FindModuleFiltersByModuleId(ModuleIdentifier moduleId)
	{
		var indexSearchQuery = new ZQuery()
			.AddToFilter(JoinCondition.And, StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.Equal, moduleId.Name)
			.AddToFilter(JoinCondition.And, StmModuleFilterSchema.S9_IsIndexSearch, SQLComparisonOperator.Equal, true);

		var accessibilityQuery = new ZQuery()
			.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, SQLComparisonOperator.Equal, true)
			.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_RelatedEntityID, SQLComparisonOperator.Equal, Env.CurrentUserPK);

		var query = new ZQuery(indexSearchQuery, JoinCondition.And, accessibilityQuery);

		List<SnapshotModuleFilter> filters = [];
		foreach (var stmModuleFilter in Factory.Load<StmModuleFilter>(query))
		{
			var moduleIdentifier = ModuleList.GetRegisteredIdentifierByTableName(stmModuleFilter.S9_ModuleID);
			if (moduleIdentifier is not null)
			{
				filters.Add(new()
				{
					ModuleFilterId = stmModuleFilter.PK.ToGuid(),
					ModuleFilterName = stmModuleFilter.S9_FilterName,
					ModuleId = moduleIdentifier,
					ModuleName = moduleIdentifier.ExtendedDescription,
				});
			}
		}
		return filters;
	}

	StmData FindStmDataById(Guid id) => Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.PK, id));

	void Validate(Snapshot snapshot)
	{
		if (snapshot.Owner == Guid.Empty)
		{
			throw new ArgumentException(message: ResString.GetMultilingualString("67ab8c87-bac5-4e1c-95ef-4dea27bf5bce", "Owner cannot be empty."), nameof(snapshot.Owner));
		}

		var moduleFilterId = snapshot.ModuleFilter?.ModuleFilterId ?? Guid.Empty;
		if (moduleFilterId == Guid.Empty)
		{
			throw new ArgumentException(message: ResString.GetMultilingualString("166cae2e-3b6d-4d30-8803-a40f6f8dd30b", "Module Filter Id cannot be empty."), nameof(snapshot.ModuleFilter.ModuleFilterId));
		}

		var filter = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.PK, moduleFilterId));
		if (filter == null)
		{
			throw new ArgumentException(message: ResString.GetMultilingualString("b59f1ae9-2f32-4ec3-a8e4-9d3d70db0a63", "Module Filter not found."), nameof(snapshot.ModuleFilter.ModuleFilterId));
		}
	}

	int ParseOrder(string str)
	{
		var parts = str.Split('_');
		return (parts.Length >= 2 && int.TryParse(parts[1], out var order)) ? order : 0;
	}

	public Task<Snapshot> FindByUserIdAndModuleFilterIdAsync(Guid userId, string moduleFilterId) => Task.Run(() => FindByUserIdAndModuleFilterId(userId, moduleFilterId));

	public Snapshot FindByUserIdAndModuleFilterId(Guid userId, string moduleFilterId)
	{
		var query = new ZQuery(StmDataSchema.SD_Owner, userId);
		query.AddToFilter(JoinCondition.And, StmDataSchema.SD_Name, SQLComparisonOperator.Like, $"{SD_NAME}_%");
		query.AddToFilter(JoinCondition.And, StmDataSchema.SD_GuidValue, SQLComparisonOperator.Equal, ZGuid.ParseSafe(moduleFilterId));
		var data = Factory.LoadTop1<StmData>(query);
		return GetSnapshotFromStmData(userId, data);
	}

	Snapshot GetSnapshotFromStmData(ZGuid? owner, StmData data)
	{
		ZQuery query;
		if (data == null)
		{
			return null;
		}

		if (string.IsNullOrEmpty(data.SD_Name) && !data.SD_Name.StartsWith($"{SD_NAME}_"))
		{
			return null;
		}

		if (owner != null && owner != data.SD_Owner)
		{
			return null;
		}

		query = new ZQuery(StmModuleFilterSchema.PK, data.SD_GuidValue);
		var filter = Factory.LoadTop1<StmModuleFilter>(query);
		var snapshot = new Snapshot
		{
			Id = data.PK.ToGuid(),
			Order = ParseOrder(data.SD_Name),
		};

		if (filter != null)
		{
			var moduleFilter = new SnapshotModuleFilter
			{
				ModuleFilterId = filter.PK.ToGuid(),
				ModuleFilterName = filter.S9_FilterName,
			};

			if (filter.S9_ModuleID != ZString.Empty)
			{
				var moduleIdentifier = ModuleList.GetRegisteredIdentifierByTableName(filter.S9_ModuleID);
				if (moduleIdentifier != null)
				{
					moduleFilter.ModuleId = moduleIdentifier;
					moduleFilter.ModuleName = moduleIdentifier.Description;
				}
			}

			snapshot.ModuleFilter = moduleFilter;
		}

		return snapshot;
	}
}
