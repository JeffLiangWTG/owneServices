using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Main.Navigation;

public class SnapshotsService : ISnapshotsService, IDisposable
{
	readonly SnapshotsViewModel viewModel;
	readonly IWinzorControl winzorControl;

	public SnapshotsService(SnapshotsViewModel viewModel, IWinzorControl winzorControl)
	{
		this.winzorControl = winzorControl;
		this.viewModel = viewModel;

		viewModel.PropertyChanged += HandlePropertyChanged;
	}

	public string Title => viewModel.Title;
	public string AddText => viewModel.AddText;
	public string AddToText => viewModel.AddToText;
	public string EditText => viewModel.EditText;
	public string UpdateText => viewModel.UpdateText;
	public string CancelText => viewModel.CancelText;
	public string SaveChangesText => viewModel.SaveChangesText;
	public string ModuleLabel => viewModel.ModuleLabel;
	public string ModulePlaceholder => viewModel.ModulePlaceholder;
	public string ModuleFilterLabel => viewModel.ModuleFilterLabel;
	public string ModuleFilterPlaceholder => viewModel.ModuleFilterPlaceholder;
	public string NoModulesFoundText => viewModel.NoModulesFoundText;
	public string NoLayoutFiltersFoundText => viewModel.NoLayoutFiltersFoundText;
	public string SnapshotsHint => viewModel.SnapshotsHint;
	public string HowToCreateSnapshotText => viewModel.HowToCreateSnapshotText;
	public string HowToCreateSnapshotLinkText => viewModel.HowToCreateSnapshotLinkText;
	public int MaxSnapshots => SnapshotsViewModel.MaxSnapshots;

	public ObservableCollection<Snapshot> Snapshots => viewModel.Snapshots;

	public Task RefreshAsync() => winzorControl.InvokeAsync(() =>
	{
		viewModel.Refresh();
		winzorControl.NotifyStateChanged();
		viewModel.Snapshots.ForEach(FetchSnapshotValue);
	});

	public Task OpenModuleFilterAsync(Snapshot snapshot) => winzorControl.InvokeAsync(() => viewModel.RunOpenModuleFilterCommand.Execute(snapshot));

	public Task OpenModuleAsync(SnapshotModule module) => winzorControl.InvokeAsync(() => viewModel.OpenModuleCommand.Execute(module));

	public async Task<List<SnapshotModuleFilter>> FindModuleFiltersAsync(SnapshotModule module)
	{
		List<SnapshotModuleFilter> filters = [];
		if (module is not null)
		{
			await winzorControl.InvokeAsync(() => filters = viewModel.FindModuleFiltersByModuleId(module.ModuleId).ToList());
		}
		return filters;
	}

	public Task CreateAsync(Snapshot snapshot) => winzorControl.InvokeAsync(() => viewModel.Create(snapshot));
	public Task UpdateAsync(Snapshot snapshot) => winzorControl.InvokeAsync(() => viewModel.Update(snapshot));
	public Task DeleteAsync(Snapshot snapshot) => winzorControl.InvokeAsync(() => viewModel.Delete(snapshot));

	public Task CreateOrUpdateAsync(Snapshot snapshot, int order)
	{
		snapshot.Order = order;
		if (snapshot.Id == Guid.Empty)
		{
			return CreateAsync(snapshot);
		}

		return UpdateAsync(snapshot);
	}

	public async Task<List<SnapshotModule>> FindModulesAsync()
	{
		List<SnapshotModule> modules = [];
		await winzorControl.InvokeAsync(() => modules = viewModel.FindModules().ToList());
		return modules;
	}

	public void Dispose()
	{
		viewModel.PropertyChanged -= HandlePropertyChanged;
	}

	void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SnapshotsViewModel.Snapshots))
		{
			winzorControl.NotifyStateChanged();
		}
	}

	void FetchSnapshotValue(Snapshot snapshot)
	{
		viewModel.RunSnapshotCommand.Execute(snapshot);
		winzorControl.NotifyStateChanged();
	}
}
