using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

public class SnapshotsViewModelTest : TestCase
{
	readonly Guid userId = Guid.NewGuid();
	readonly Snapshot[] snapshots =
	[
		new Snapshot { ModuleFilter = new SnapshotModuleFilter { ModuleId = ModuleIDs.WorkItem, ModuleName = "Work Items", ModuleFilterName = "Filter A" } },
		new Snapshot { ModuleFilter = new SnapshotModuleFilter { ModuleId = ModuleIDs.WorkItem, ModuleName = "Work Items", ModuleFilterName = "Filter B" } },
		new Snapshot { ModuleFilter = new SnapshotModuleFilter { ModuleId = ModuleIDs.SupplierBooking,  ModuleName = "Incidents", ModuleFilterName = "Filter C" } },
	];

	Mock<ISnapshotsRepository> mockRepository;

	protected override void SetUp()
	{
		base.SetUp();

		mockRepository = new Mock<ISnapshotsRepository>();
		mockRepository.Setup(r => r.FindByUserIdAsync(It.IsAny<Guid>()))
			.Returns(Task.FromResult(snapshots.AsEnumerable()));
		mockRepository.Setup(r => r.FindByUserId(It.IsAny<Guid>()))
			.Returns(snapshots.AsEnumerable());
	}

	public void TestInitialization()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertEquals(userId, uut.UserId);
		AssertNotNull(uut.Snapshots);
		AssertNotNull(uut.Repository);
		AssertNotNull(uut.RunSnapshotCommand);
		AssertNotNull(uut.RunOpenModuleFilterCommand);
	}

	public void TestTexts()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertEquals("Snapshots", uut.Title);
		AssertEquals("Add Snapshot", uut.AddText);
		AssertEquals("Add to Snapshots", uut.AddToText);
		AssertEquals("Edit Snapshots", uut.EditText);
		AssertEquals("Update Snapshot", uut.UpdateText);
		AssertEquals("Cancel", uut.CancelText);
		AssertEquals("Save Changes", uut.SaveChangesText);
		AssertEquals("Use saved CargoWise filters to get a quick look into data that is relevant to you.", uut.SnapshotsHint);
		AssertEquals("Module", uut.ModuleLabel);
		AssertEquals("Search module...", uut.ModulePlaceholder);
		AssertEquals("Indexed Module Layout", uut.ModuleFilterLabel);
		AssertEquals("Search layout...", uut.ModuleFilterPlaceholder);
		AssertEquals("No modules found.", uut.NoModulesFoundText);
		AssertEquals("No layout filters found.", uut.NoLayoutFiltersFoundText);
	}

	public void TestRefresh()
	{
		mockRepository.Setup(r => r.FindByUserId(It.IsAny<Guid>())).Returns(snapshots.AsEnumerable());

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertEquals(0, uut.Snapshots.Count);

		uut.Refresh();

		AssertEquals(3, uut.Snapshots.Count);
		mockRepository.Verify(r => r.FindByUserId(It.Is<Guid>(p => p == userId)), Times.Once);
	}

	public void TestCanRunSnapshotCommand_WhenCommandIsNull()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertEquals("1: CanRunSnapshotCommand", expected: true, uut.CanRunSnapshotCommand(snapshots[0]));
		AssertEquals("2: CanRunSnapshotCommand", expected: true, uut.CanRunSnapshotCommand(snapshots[1]));
	}

	public void TestCanRunSnapshotCommand_WhenCommandNotProvided()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId)
		{
			RunSnapshotCommand = new RelayCommand(s => { }, s => s == snapshots[0]),
		};

		AssertEquals("1: CanRunSnapshotCommand", expected: true, uut.CanRunSnapshotCommand(snapshots[0]));
		AssertEquals("2: CanRunSnapshotCommand", expected: false, uut.CanRunSnapshotCommand(snapshots[1]));
	}

	public void TestCanRunSnapshotCommand_WhenCommandNotProvided_WhenCanExecuteFunctionNotProvided()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId)
		{
			RunSnapshotCommand = new RelayCommand(s => { }),
		};

		AssertEquals("1: CanRunSnapshotCommand", expected: true, uut.CanRunSnapshotCommand(snapshots[0]));
		AssertEquals("2: CanRunSnapshotCommand", expected: true, uut.CanRunSnapshotCommand(snapshots[1]));
	}

	public void TestRunSnapshotCommand_WhenCommandNotProvided()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertNoExceptionThrown(() => uut.RunSnapshotCommand.Execute(snapshots[0]));
	}

	public void TestRunSnapshotCommand_WhenCommandProvided()
	{
		var commandArgs = new List<object>();
		var uut = new SnapshotsViewModel(mockRepository.Object, userId)
		{
			RunSnapshotCommand = new RelayCommand(commandArgs.Add),
		};

		uut.RunSnapshotCommand.Execute(snapshots[0]);

		AssertEquals("1: Command execution count", 1, commandArgs.Count);
		AssertEquals("1: Command execution arg", snapshots[0], commandArgs[0]);

		uut.RunSnapshotCommand.Execute(snapshots[1]);

		AssertEquals("2: Command execution count", 2, commandArgs.Count);
		AssertEquals("2: Command execution arg", snapshots[1], commandArgs[1]);
	}

	public void TestRunSnapshotCommand_WhenCommandProvided_WhenCanRunCommandProvided()
	{
		var commandArgs = new List<object>();
		var uut = new SnapshotsViewModel(mockRepository.Object, userId)
		{
			RunSnapshotCommand = new RelayCommand(commandArgs.Add, (s) => s == snapshots[0]),
		};

		uut.RunSnapshotCommand.Execute(snapshots[0]);

		AssertEquals("1: Command execution count", 1, commandArgs.Count);
		AssertEquals("1: Command execution arg", snapshots[0], commandArgs[0]);

		uut.RunSnapshotCommand.Execute(snapshots[1]);

		AssertEquals("2: Command execution count", 1, commandArgs.Count);
	}

	public void TestRunOpenModuleFilterCommand_WhenCommandNotProvided()
	{
		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertNoExceptionThrown(() => uut.RunOpenModuleFilterCommand.Execute(snapshots[0]));
	}

	public void TestRunOpenModuleFilterCommand_WhenCommandProvided()
	{
		var commandArgs = new List<object>();
		var uut = new SnapshotsViewModel(mockRepository.Object, userId)
		{
			RunOpenModuleFilterCommand = new RelayCommand(commandArgs.Add),
		};

		uut.RunOpenModuleFilterCommand.Execute(snapshots[0]);

		AssertEquals("1: Command execution count", 1, commandArgs.Count);
		AssertEquals("1: Command execution arg", snapshots[0], commandArgs[0]);

		uut.RunOpenModuleFilterCommand.Execute(snapshots[1]);

		AssertEquals("2: Command execution count", 2, commandArgs.Count);
		AssertEquals("2: Command execution arg", snapshots[1], commandArgs[1]);
	}

	public void TestCreate()
	{
		List<Snapshot> calls = [];
		mockRepository.Setup(r => r.Create(It.IsAny<Snapshot>())).Callback<Snapshot>(calls.Add);

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		var snapshot = new Snapshot();
		uut.Create(snapshot);

		AssertEquals(1, calls.Count);
		AssertEquals(snapshot, calls[0]);
		AssertEquals(userId, calls[0].Owner);
	}

	public void TestUpdate()
	{
		List<Snapshot> calls = [];
		mockRepository.Setup(r => r.Update(It.IsAny<Snapshot>())).Callback<Snapshot>(calls.Add);

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		var snapshot = new Snapshot();
		uut.Update(snapshot);

		AssertEquals(1, calls.Count);
		AssertEquals(snapshot, calls[0]);
	}

	public void TestDelete()
	{
		List<Guid> calls = [];
		mockRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Callback<Guid>(calls.Add);

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		var snapshotId = Guid.NewGuid();
		uut.Delete(new Snapshot { Id = snapshotId });

		AssertEquals(1, calls.Count);
		AssertEquals(snapshotId, calls[0]);
	}

	public void TestDeleteSnapshot()
	{
		mockRepository.Setup(r => r.FindByUserId(It.IsAny<Guid>())).Returns(snapshots.AsEnumerable());

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);

		AssertEquals(0, uut.Snapshots.Count);

		uut.Refresh();

		AssertEquals(3, uut.Snapshots.Count);

		uut.DeleteSnapshotCommand.Execute(uut.Snapshots[1]);

		AssertEquals(2, uut.Snapshots.Count);

		uut.Refresh();

		AssertEquals(3, uut.Snapshots.Count);
	}
	
	public void TestAddToSnapshotsMaxCount()
	{
		var snapshotList = new List<Snapshot>();
		for (var i = 0; i < SnapshotsViewModel.MaxSnapshots; i++)
		{
			snapshotList.Add(new Snapshot());
		}
		mockRepository
			.Setup(x => x.FindByUserId(It.IsAny<Guid>()))
			.Returns(snapshotList);

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);
		AssertExceptionThrown<InvalidOperationException>(() => uut.AddToSnapshots(Guid.NewGuid()));
	}

	public void TestAddToSnapshotsExist()
	{
		mockRepository
			.Setup(x => x.FindByUserId(It.IsAny<Guid>()))
			.Returns(new List<Snapshot>
			{
				new Snapshot(),
			});

		mockRepository
			.Setup(x => x.FindByUserIdAndModuleFilterId(It.IsAny<Guid>(), It.IsAny<string>()))
			.Returns(new Snapshot());

		var uut = new SnapshotsViewModel(mockRepository.Object, userId);
		AssertExceptionThrown<InvalidOperationException>(() => uut.AddToSnapshots(Guid.NewGuid()));
	}
}
