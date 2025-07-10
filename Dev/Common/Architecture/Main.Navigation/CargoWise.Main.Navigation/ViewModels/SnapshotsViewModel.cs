using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CargoWise.Common;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation;

#nullable disable
public class SnapshotsViewModel : INotifyPropertyChanged
{
	public const int MaxSnapshots = 5;

	public event PropertyChangedEventHandler PropertyChanged;

	public SnapshotsViewModel(ISnapshotsRepository repository, Guid userId)
	{
		Repository = repository;
		UserId = userId;
		Snapshots = [];
		DeleteSnapshotCommand = new RelayCommand(DeleteSnapshot);
	}

	public string Title { get; } = ResString.GetMultilingualString("Main.Home.Snapshots", "Snapshots");
	public string AddText { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.AddText", "Add Snapshot");

	public string AddToText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.AddToText", "Add to Snapshots");

	public string EditText { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.EditText", "Edit Snapshots");
	public string UpdateText { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.UpdateText", "Update Snapshot");
	public string CancelText { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.CancelText", "Cancel");

	public string SaveChangesText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.SaveChangesText", "Save Changes");

	public string SnapshotsHint { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.SnapshotsHint",
		"Use saved CargoWise filters to get a quick look into data that is relevant to you.");

	public string ModuleLabel { get; } = ResString.GetMultilingualString("Main.Home.Snapshots.ModuleLabel", "Module");

	public string ModulePlaceholder { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.ModulePlaceholder", "Search module...");

	public string ModuleFilterLabel { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.ModuleFilterLabel", "Indexed Module Layout");

	public string ModuleFilterPlaceholder { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.ModuleFilterPlaceholder", "Search layout...");

	public string NoModulesFoundText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.NoModulesFoundText", "No modules found.");

	public string NoLayoutFiltersFoundText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.NoLayoutFiltersFoundText", "No layout filters found.");

	public string UnableToAddSnapshotReachMaxCount { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.UnableToAddSnapshotReachMaxCount", $"Only {MaxSnapshots} snapshots allowed");

	public string SnapshotExist { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.SnapshotExist", $"Snapshot already exists.");

	public string HowToCreateSnapshotText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.HowToCreateSnapshotText", "To create a snapshot for this module, an Index Search filter needs to be saved first.");

	public string HowToCreateSnapshotLinkText { get; } =
		ResString.GetMultilingualString("Main.Home.Snapshots.HowToCreateSnapshotLinkText", "You can add one in the Module.");

	public Guid UserId { get; }
	public ISnapshotsRepository Repository { get; private set; }

	void DeleteSnapshot(object parameter)
	{
		var snapshot = parameter as Snapshot;
		if (snapshot == null)
		{
			return;
		}

		Snapshots.Remove(snapshot);
	}

	public ICommand DeleteSnapshotCommand { get; set; }

	ObservableCollection<Snapshot> _snapshots;

	public ObservableCollection<Snapshot> Snapshots
	{
		get => _snapshots;
		private set
		{
			if (_snapshots != value)
			{
				_snapshots = value;
				NotifyPropertyChanged();
			}
		}
	}

	public bool CanRunSnapshotCommand(Snapshot snapshot) => RunSnapshotCommand?.CanExecute(snapshot) ?? true;

	public ICommand RunSnapshotCommand { get; init; } = new RelayCommand((_) => { });
	public ICommand RunOpenModuleFilterCommand { get; init; } = new RelayCommand((_) => { });
	public ICommand OpenModuleCommand { get; init; } = new RelayCommand((_) => { });

	public void Refresh()
	{
		var snapshots = Repository.FindByUserId(UserId).ToList();
		Snapshots = new ObservableCollection<Snapshot>(snapshots);
	}

	public void UpdateSnapshots()
	{
		Snapshots.ForEach(snapshot =>
		{
			if (CanRunSnapshotCommand(snapshot))
			{
				RunSnapshotCommand?.Execute(snapshot);
			}
		});
	}

	public void RefreshAndUpdateSnapshots()
	{
		Refresh();
		UpdateSnapshots();
	}

	public IEnumerable<SnapshotModuleFilter> FindModuleFiltersByModuleId(ModuleIdentifier moduleId) => Repository.FindModuleFiltersByModuleId(moduleId);

	public void Create(Snapshot snapshot)
	{
		snapshot.Owner = UserId;
		Repository.Create(snapshot);
	}

	public void Update(Snapshot snapshot) => Repository.Update(snapshot);

	public void Delete(Snapshot snapshot) => Repository.Delete(snapshot.Id);

	public IEnumerable<SnapshotModule> FindModules() => Repository.FindModules();

	Snapshot _selectedSnapshot;
	public Snapshot SelectedSnapshot
	{
		get
		{
			return _selectedSnapshot;
		}
		set
		{
			if (_selectedSnapshot != value)
			{
				_selectedSnapshot = value;
				NotifyPropertyChanged();
			}
		}
	}

	void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void AddToSnapshots(Guid moduleFilterId)
	{
		var snapshots = Repository.FindByUserId(UserId);
		if (snapshots.Count() >= MaxSnapshots)
		{
			throw new InvalidOperationException(UnableToAddSnapshotReachMaxCount);
		}

		var snapshot = Repository.FindByUserIdAndModuleFilterId(UserId, moduleFilterId.ToString());
		if (snapshot != null)
		{
			throw new InvalidOperationException(SnapshotExist);
		}
		snapshot = new Snapshot
		{
			Id = Guid.NewGuid(),
			Owner = UserId,
			Order = (short)(snapshots.Count() + 1),
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = moduleFilterId
			}
		};

		Repository.Create(snapshot);

		Refresh();
	}
}
