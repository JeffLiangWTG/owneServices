using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation;

public interface ISnapshotsRepository
{
	IEnumerable<Snapshot> FindByUserId(Guid userId);
	Task<IEnumerable<Snapshot>> FindByUserIdAsync(Guid userId);

	IEnumerable<SnapshotModuleFilter> FindModuleFiltersByModuleId(ModuleIdentifier moduleId);
	Task<IEnumerable<SnapshotModuleFilter>> FindModuleFiltersByModuleIdAsync(ModuleIdentifier moduleId);

	IEnumerable<SnapshotModule> FindModules();
	Task<IEnumerable<SnapshotModule>> FindModulesAsync();

	Guid Create(Snapshot snapshot);
	Task<Guid> CreateAsync(Snapshot snapshot);

	int Update(Snapshot snapshot);
	Task<int> UpdateAsync(Snapshot snapshot);

	int Delete(Guid pk);
	Task<int> DeleteAsync(Guid pk);
	
	Task<Snapshot> FindByUserIdAndModuleFilterIdAsync(Guid userId, string moduleFilterId);
	Snapshot FindByUserIdAndModuleFilterId(Guid userId, string moduleFilterId);
}
