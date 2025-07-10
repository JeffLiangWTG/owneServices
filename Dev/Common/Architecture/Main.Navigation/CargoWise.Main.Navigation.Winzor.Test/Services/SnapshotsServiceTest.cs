using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Services;

public class SnapshotsServiceTest
{
	Mock<IWinzorControl> _mockControl;
	Mock<ISnapshotsRepository> _mockRepo;

	[SetUp]
	public void SetUp()
	{
		var snapshots = new List<Snapshot>
		{
			new Snapshot {
				ModuleFilter = new SnapshotModuleFilter { ModuleFilterName = "Layout 01", ModuleName = "Module 01" },
				Order = 1,
				Value = "123.456",
			},
			new Snapshot {
				ModuleFilter = new SnapshotModuleFilter { ModuleFilterName = "Layout 02", ModuleName = "Module 01" },
				Order = 2,
				Value = "456.123",
			},
		};

		_mockControl = new Mock<IWinzorControl>();
		_mockControl.Setup(c => c.InvokeAsync(It.IsAny<Action>())).Callback<Action>(a => a());

		_mockRepo = new Mock<ISnapshotsRepository>();
		_mockRepo.Setup(r => r.FindByUserId(It.IsAny<Guid>())).Returns(snapshots);
		_mockRepo.Setup(r => r.FindByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(snapshots);
	}

	[Test]
	public void SnapshotsService_Title()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.Title, Is.EqualTo("Snapshots"));
	}

	[Test]
	public void SnapshotsService_Hint()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.SnapshotsHint, Is.EqualTo("Use saved CargoWise filters to get a quick look into data that is relevant to you."));
	}

	[Test]
	public void SnapshotsService_Texts()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.AddText, Is.EqualTo("Add Snapshot"));
		Assert.That(uut.AddToText, Is.EqualTo("Add to Snapshots"));
		Assert.That(uut.EditText, Is.EqualTo("Edit Snapshots"));
		Assert.That(uut.UpdateText, Is.EqualTo("Update Snapshot"));
		Assert.That(uut.CancelText, Is.EqualTo("Cancel"));
		Assert.That(uut.SaveChangesText, Is.EqualTo("Save Changes"));
		Assert.That(uut.NoModulesFoundText, Is.EqualTo("No modules found."));
		Assert.That(uut.NoLayoutFiltersFoundText, Is.EqualTo("No layout filters found."));
	}

	[Test]
	public void SnapshotsService_Labels()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.ModuleLabel, Is.EqualTo("Module"));
		Assert.That(uut.ModuleFilterLabel, Is.EqualTo("Indexed Module Layout"));
	}

	[Test]
	public void SnapshotsService_Placehoders()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.ModulePlaceholder, Is.EqualTo("Search module..."));
		Assert.That(uut.ModuleFilterPlaceholder, Is.EqualTo("Search layout..."));
	}

	[Test]
	public async Task SnapshotsService_RefreshAsync()
	{
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		Assert.That(uut.Snapshots, Has.Count.EqualTo(0));

		await uut.RefreshAsync();

		Assert.That(uut.Snapshots, Has.Count.EqualTo(2));
	}

	[Test]
	public async Task SnapshotsService_DeleteAsync()
	{
		var events = new List<Guid>();
		_mockRepo.Setup(r => r.Delete(It.IsAny<Guid>())).Callback<Guid>(events.Add);

		var snapshot = new Snapshot { Id = Guid.NewGuid() };

		var uut = new SnapshotsService(
			new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		await uut.DeleteAsync(snapshot);

		Assert.That(events, Has.Count.EqualTo(1));
		Assert.That(events[0], Is.EqualTo(snapshot.Id));
	}

	[Test]
	public async Task SnapshotsService_CreateAsync()
	{
		List<Snapshot> events = [];
		_mockRepo.Setup(r => r.Create(It.IsAny<Snapshot>())).Callback<Snapshot>(events.Add);

		var snapshot = new Snapshot();

		var userId = Guid.NewGuid();
		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, userId), _mockControl.Object);

		await uut.CreateAsync(snapshot);

		Assert.That(events, Has.Count.EqualTo(1));
		Assert.That(events[0], Is.EqualTo(snapshot));
		Assert.That(events[0].Owner, Is.EqualTo(userId));
	}

	[Test]
	public async Task SnapshotsService_UpdateAsync()
	{
		List<Snapshot> events = [];
		_mockRepo.Setup(r => r.Update(It.IsAny<Snapshot>())).Callback<Snapshot>(events.Add);

		var snapshot = new Snapshot();

		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		await uut.UpdateAsync(snapshot);

		Assert.That(events, Has.Count.EqualTo(1));
		Assert.That(events[0], Is.EqualTo(snapshot));
	}

	[Test]
	public async Task SnapshotsService_CreateOrUpdateAsync()
	{
		List<Snapshot> createEvents = [];
		List<Snapshot> updateEvents = [];
		_mockRepo.Setup(r => r.Create(It.IsAny<Snapshot>())).Callback<Snapshot>(createEvents.Add);
		_mockRepo.Setup(r => r.Update(It.IsAny<Snapshot>())).Callback<Snapshot>(updateEvents.Add);

		var snapshotToCreate = new Snapshot();
		var snapshotToUpdate = new Snapshot { Id = Guid.NewGuid() };

		var uut = new SnapshotsService(new SnapshotsViewModel(_mockRepo.Object, Guid.NewGuid()), _mockControl.Object);

		await uut.CreateOrUpdateAsync(snapshotToCreate, order: 0);
		await uut.CreateOrUpdateAsync(snapshotToUpdate, order: 1);

		Assert.That(createEvents, Has.Count.EqualTo(1));
		Assert.That(createEvents[0], Is.EqualTo(snapshotToCreate));
		Assert.That(createEvents[0].Order, Is.EqualTo(0));

		Assert.That(updateEvents, Has.Count.EqualTo(1));
		Assert.That(updateEvents[0], Is.EqualTo(snapshotToUpdate));
		Assert.That(updateEvents[0].Order, Is.EqualTo(1));
	}
}
