using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface ISnapshotsService
{
	string Title { get; }
	string AddText { get; }
	string AddToText { get; }
	string UpdateText { get; }
	string EditText { get; }
	string CancelText { get; }
	string SaveChangesText { get; }
	string ModuleLabel { get; }
	string ModulePlaceholder { get; }
	string ModuleFilterLabel { get; }
	string ModuleFilterPlaceholder { get; }
	string NoModulesFoundText { get; }
	string NoLayoutFiltersFoundText { get; }
	string SnapshotsHint { get; }
	string HowToCreateSnapshotText { get; }
	string HowToCreateSnapshotLinkText { get; }
	int MaxSnapshots { get; }
	ObservableCollection<Snapshot> Snapshots { get; }

	Task RefreshAsync();
	Task OpenModuleFilterAsync(Snapshot snapshot);
	Task OpenModuleAsync(SnapshotModule module);
	Task<List<SnapshotModule>> FindModulesAsync();
	Task<List<SnapshotModuleFilter>> FindModuleFiltersAsync(SnapshotModule module);
	Task CreateAsync(Snapshot snapshot);
	Task UpdateAsync(Snapshot snapshot);
	Task DeleteAsync(Snapshot snapshot);
	Task CreateOrUpdateAsync(Snapshot snapshot, int order);
}
