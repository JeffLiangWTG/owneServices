using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWiseNext.Blazor.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Components;

public class SnapshotsTest : BunitTestContext
{
	Mock<ISnapshotsService> _mockService;
	Mock<IDragAndDropJsInterop> _mockDragAndDropJsInterop;

	[SetUp]
	public void SetUp()
	{
		_mockService = new Mock<ISnapshotsService>();
		_mockService.SetupGet(service => service.Title).Returns("Snapshots");
		_mockService.SetupGet(service => service.AddText).Returns("Add Snapshot");
		_mockService.SetupGet(service => service.EditText).Returns("Edit Snapshot");
		_mockService.SetupGet(service => service.CancelText).Returns("Cancel");
		_mockService.SetupGet(service => service.SaveChangesText).Returns("Save Changes");
		_mockService.SetupGet(service => service.SnapshotsHint).Returns("This is snapshots test hint.");
		_mockService.SetupGet(s => s.MaxSnapshots).Returns(5);

		_mockDragAndDropJsInterop = new Mock<IDragAndDropJsInterop>();
		_mockDragAndDropJsInterop.Setup(m => m.InitAsync(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<DotNetObjectReference<IDragAndDropInvokables>>())).Returns(ValueTask.CompletedTask);
		_mockDragAndDropJsInterop.Setup(m => m.TeardownAsync()).Returns(ValueTask.CompletedTask);

		Services.AddSingleton(_mockDragAndDropJsInterop.Object);
	}

	[Test]
	public void Snapshots_ShouldRender()
	{
		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var component = cut.Find(".cwn-snapshots");
		Assert.That(component, Is.Not.Null);
	}

	[Test]
	public void Snapshots_WhenEmpty_WhenNotEditing()
	{
		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));
		Assert.That(cut.Instance.IsEditing, Is.False);
		Assert.That(cut.Instance.IsEmpty, Is.True);

		var buttons = cut.FindAll(".cwn-snapshots__button");
		Assert.That(buttons.Count, Is.EqualTo(2));

		Assert.That(buttons[0].TextContent, Is.EqualTo("Edit Snapshot"));
		Assert.That(buttons[1].TextContent, Is.EqualTo("Add Snapshot"));

		Assert.DoesNotThrow(() => cut.Find(".cwn-snapshots__empty-dialog"));

		var emptyHint = cut.Find(".cwn-snapshots__empty-hint");
		Assert.That(emptyHint.TextContent, Is.EqualTo("This is snapshots test hint."));
	}

	[Test]
	public void Snapshots_WhenEmpty_WhenEditing()
	{
		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.True);
		Assert.That(cut.Instance.IsEmpty, Is.True);

		var buttons = cut.FindAll(".cwn-snapshots__button");
		Assert.That(buttons.Count, Is.EqualTo(3));

		Assert.That(buttons[0].TextContent, Is.EqualTo("Cancel"));
		Assert.That(buttons[1].TextContent, Is.EqualTo("Save Changes"));
		Assert.That(buttons[2].TextContent, Is.EqualTo("Add Snapshot"));

		Assert.DoesNotThrow(() => cut.Find(".cwn-snapshots__empty-dialog"));
	}

	[Test]
	public void Snapshots_WhenEditing_WhenCancelClick()
	{
		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		Assert.That(cut.Instance.IsEditing, Is.False);

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.True);
		_mockDragAndDropJsInterop.Verify(m => m.InitAsync(
			It.Is<string>(m => m == "cwn-snapshot-item--drop-area"),
			It.Is<string>(m => m == "cwn-snapshot-item--drag-over"),
			It.IsAny<DotNetObjectReference<IDragAndDropInvokables>>()), Times.Once);
		_mockDragAndDropJsInterop.Verify(m => m.TeardownAsync(), Times.Never);

		var cancelButton = cut.Find("[data-testid='cancel-btn']");
		cancelButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.False);
		_mockDragAndDropJsInterop.Verify(m => m.InitAsync(
			It.Is<string>(m => m == "cwn-snapshot-item--drop-area"),
			It.Is<string>(m => m == "cwn-snapshot-item--drag-over"),
			It.IsAny<DotNetObjectReference<IDragAndDropInvokables>>()), Times.Once);
		_mockDragAndDropJsInterop.Verify(m => m.TeardownAsync(), Times.Once);
	}

	[Test]
	public void Snapshots_WhenEditing_WhenSaveChangesClick()
	{
		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		Assert.That(cut.Instance.IsEditing, Is.False);

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.True);

		var saveButton = cut.Find("[data-testid='save-btn']");
		saveButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.False);
		_mockDragAndDropJsInterop.Verify(m => m.InitAsync(
			It.Is<string>(m => m == "cwn-snapshot-item--drop-area"),
			It.Is<string>(m => m == "cwn-snapshot-item--drag-over"),
			It.IsAny<DotNetObjectReference<IDragAndDropInvokables>>()), Times.Once);
		_mockDragAndDropJsInterop.Verify(m => m.TeardownAsync(), Times.Once);
	}

	[Test]
	public void Snapshots_WhenEditing_WhenDeleted_WhenSaveChangesClick()
	{
		List<Snapshot> calls = [];

		List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid()];
		_mockService.Setup(s => s.Snapshots)
			.Returns([
				new Snapshot { Id = ids[0] },
				new Snapshot { Id = ids[1] }
			]);

		_mockService.Setup(s => s.DeleteAsync(It.IsAny<Snapshot>()))
			.Callback<Snapshot>(calls.Add)
			.Returns(Task.CompletedTask);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(2));

		var deleteButtons = cut.FindAll("[data-testid='delete-btn']");
		deleteButtons[0].Click();

		Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(1));
		Assert.That(calls, Has.Count.EqualTo(0));

		deleteButtons = cut.FindAll("[data-testid='delete-btn']");
		deleteButtons[0].Click();

		Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(0));
		Assert.That(calls, Has.Count.EqualTo(0));

		var saveButton = cut.Find("[data-testid='save-btn']");
		saveButton.Click();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsEditing, Is.False);
			Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(0));
			Assert.That(calls, Has.Count.EqualTo(2));
			Assert.That(calls[0].Id, Is.EqualTo(ids[0]));
			Assert.That(calls[1].Id, Is.EqualTo(ids[1]));
		});
	}

	[Test]
	public void Snapshots_WhenEditing_WhenAdded_WhenSaveChangesClick()
	{
		List<Snapshot> calls = [];

		_mockService.Setup(s => s.Snapshots).Returns([new()]);

		_mockService.Setup(s => s.CreateOrUpdateAsync(It.IsAny<Snapshot>(), It.IsAny<int>()))
			.Callback<Snapshot, int>((s, i) =>
			{
				s.Order = i;
				calls.Add(s);
			})
			.Returns(Task.CompletedTask);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		cut.Instance.SnapshotsList.Add(new());

		Assert.That(calls, Has.Count.EqualTo(0));

		var saveButton = cut.Find("[data-testid='save-btn']");
		saveButton.Click();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsEditing, Is.False);
			Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(2));
			Assert.That(calls, Has.Count.EqualTo(2));
			Assert.That(calls[0].Order, Is.EqualTo(0));
			Assert.That(calls[1].Order, Is.EqualTo(1));
		});
	}

	[Test]
	public void Snapshots_WhenNotEmpty()
	{
		_mockService.SetupGet(s => s.Snapshots).Returns([new()]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		Assert.That(cut.Instance.IsEditing, Is.False);
		Assert.That(cut.Instance.IsEmpty, Is.False);

		Assert.That(cut.FindAll(".cwn-snapshots__empty-dialog").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll(".cwn-snapshot-item").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll(".cwn-snapshots__add-item").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll("[data-testid='add-btn']").Count, Is.EqualTo(1));
		Assert.That(cut.FindAll("[data-testid='placeholder']").Count, Is.EqualTo(3));
	}

	[Test]
	public void Snapshots_WhenNotEmpty_WhenEditing_WhenDeleteClick()
	{
		_mockService.SetupGet(s => s.Snapshots).Returns([new Snapshot { Id = Guid.NewGuid() }]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.That(cut.Instance.IsEditing, Is.True);
		Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(1));

		var deleteBtn = cut.Find("[data-testid='delete-btn']");
		deleteBtn.Click();

		Assert.That(cut.Instance.IsEditing, Is.True);
		Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(0));
	}

	[Test]
	public void Snapshots_WhenNotEmpty_WhenEditing_WhenEditSnapshotClick()
	{
		var snapshot = new Snapshot
		{
			Id = Guid.NewGuid(),
			ModuleFilter = new SnapshotModuleFilter
			{
				ModuleName = "Module",
				ModuleFilterName = "Module Filter"
			}
		};

		_mockService.SetupGet(s => s.Snapshots).Returns([snapshot]);
		_mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([new SnapshotModule { ModuleName = "Module" }]);
		_mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([snapshot.ModuleFilter]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));
		JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();

		var editButton = cut.Find("[data-testid='edit-btn']");
		editButton.Click();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsEditing, Is.True);
			Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(1));
		});

		var editItemBtn = cut.Find("[data-testid='edit-item-btn']");
		editItemBtn.Click();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsEditing, Is.True);
			Assert.That(cut.Instance.SnapshotsList, Has.Count.EqualTo(1));
			Assert.That(cut.Instance.SnapshotToEdit, Is.EqualTo(snapshot));
		});
	}

	[Test]
	public void Snapshots_WhenReachedMaxCapacity()
	{
		_mockService.SetupGet(s => s.Snapshots).Returns([new(), new(), new(), new(), new()]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		Assert.That(cut.Instance.IsEditing, Is.False);
		Assert.That(cut.Instance.IsEmpty, Is.False);

		Assert.That(cut.FindAll(".cwn-snapshots__empty-dialog").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll(".cwn-snapshot-item").Count, Is.EqualTo(5));
		Assert.That(cut.FindAll(".cwn-snapshots__add-item").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll("[data-testid='add-btn']").Count, Is.EqualTo(0));
		Assert.That(cut.FindAll("[data-testid='placeholder']").Count, Is.EqualTo(0));
	}

	[Test]
	public void Snapshots_WhenAddSnapshotClick_WhenNoModuleAndFilterSelected()
	{
		_mockService.SetupGet(s => s.Snapshots).Returns([]);
		_mockService.Setup(s => s.FindModulesAsync()).ReturnsAsync([]);
		_mockService.Setup(s => s.FindModuleFiltersAsync(It.IsAny<SnapshotModule>())).ReturnsAsync([]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));
		JSInterop.SetupVoid("showModal", _ => true).SetVoidResult();

		var addButton = cut.Find("[data-testid='add-btn']");
		addButton.Click();

		Assert.That(cut.FindAll(".cwn-dialog"), Has.Count.EqualTo(1));
	}

	[Test]
	public async Task Snapshots_OnDrop()
	{
		_mockService.SetupGet(s => s.Snapshots).Returns([
			new Snapshot { Order = 0 },
			new Snapshot { Order = 1 },
			new Snapshot { Order = 2 },
		]);

		var cut = RenderComponent<Snapshots>(parameters => parameters.AddCascadingValue(_mockService.Object));

		var actualOrder = cut.Instance.SnapshotsList.Select(s => s.Order).ToList();
		Assert.That(actualOrder, Is.EqualTo(new List<int> { 0, 1, 2 }));

		await cut.InvokeAsync(() => cut.Instance.OnDrop(dragIndex: "0", dropIndex: "2"));

		actualOrder = cut.Instance.SnapshotsList.Select(s => s.Order).ToList();
		Assert.That(actualOrder, Is.EqualTo(new List<int> { 1, 2, 0 }));

		await cut.InvokeAsync(() => cut.Instance.OnDrop(dragIndex: "1", dropIndex: "0"));

		actualOrder = cut.Instance.SnapshotsList.Select(s => s.Order).ToList();
		Assert.That(actualOrder, Is.EqualTo(new List<int> { 2, 1, 0 }));
	}
}
