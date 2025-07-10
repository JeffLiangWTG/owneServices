using AngleSharp.Dom;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Components;

public class SnapshotItemTest : BunitTestContext
{
	Mock<ISnapshotsService> _mockService;

	[SetUp]
	public void SetUp()
	{
		_mockService = new Mock<ISnapshotsService>();
	}

	[Test]
	public void SnapshotItem_ShouldRender()
	{
		var cut = RenderComponent<SnapshotItem>(parameters => parameters.AddCascadingValue(_mockService.Object));

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-snapshot-item"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-snapshot-item--editing"), Has.Count.EqualTo(0));
			Assert.That(cut.FindAll(".cwn-snapshot-item__actions--editing"), Has.Count.EqualTo(0));
		});
	}

	[Test]
	public void SnapshotItem_WhenTitle_WhenSubTitle_WhenValue()
	{
		var snapshot = new Snapshot
		{
			ModuleFilter = new SnapshotModuleFilter { ModuleName = "Module", ModuleFilterName = "Layout" },
			Value = "123",
		};

		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, snapshot));

		var title = cut.Find(".cwn-snapshot-item__title");
		var subTitle = cut.Find(".cwn-snapshot-item__sub-title");
		var value = cut.Find(".cwn-snapshot-item__value");

		Assert.Multiple(() =>
		{
			Assert.That(title.TextContent, Is.EqualTo("Layout"));
			Assert.That(subTitle.TextContent, Is.EqualTo("Module"));
			Assert.That(value.TextContent, Is.EqualTo("123"));
		});
	}

	[Test]
	public async Task SnapshotItem_WhenOpenModuleFilterClickAsync()
	{
		var events = new List<Snapshot>();
		_mockService.Setup(s => s.OpenModuleFilterAsync(It.IsAny<Snapshot>()))
			.Callback<Snapshot>(events.Add)
			.Returns(Task.CompletedTask);

		var snapshot = new Snapshot { Id = Guid.NewGuid() };

		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, snapshot));

		var button = cut.Find("[data-testid='open-module-filter-button']");
		await button.ClickAsync(new());

		Assert.Multiple(() =>
		{
			Assert.That(events, Has.Count.EqualTo(1));
			Assert.That(events[0].Id, Is.EqualTo(snapshot.Id));
		});
	}

	[Test]
	public void SnapshotItem_WhenEditMode()
	{
		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.IsEditing, true));

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-snapshot-item--editing"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-snapshot-item__actions--editing"), Has.Count.EqualTo(1));

			var editBtn = cut.Find("[data-testid='edit-item-btn']");
			var editBtnIcon = editBtn.FindChild<IElement>();
			Assert.That(editBtnIcon, Is.Not.Null);
			Assert.That(editBtnIcon!.GetAttribute("class"), Does.Contain("cwn-snapshot-item__icon-edit"));
			Assert.That(editBtnIcon.GetAttribute("class"), Does.Contain("cwn-icon--settings"));

			var deleteBtn = cut.Find("[data-testid='delete-btn']");
			var deleteBtnIcon = deleteBtn.FindChild<IElement>();
			Assert.That(deleteBtnIcon, Is.Not.Null);
			Assert.That(deleteBtnIcon!.GetAttribute("class"), Does.Contain("cwn-snapshot-item__icon-delete"));
			Assert.That(deleteBtnIcon.GetAttribute("class"), Does.Contain("cwn-icon--delete"));
		});
	}

	[Test]
	public void SnapshotItem_WhenEditMode_WhenDeleteClick()
	{
		var events = new List<Snapshot>();
		var snapshot = new Snapshot { Id = Guid.NewGuid() };

		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, snapshot)
			.Add(p => p.IsEditing, true)
			.Add(p => p.OnDeleteClick, EventCallback.Factory.Create<Snapshot>(this, s => events.Add(s))));

		var deleteBtn = cut.Find("[data-testid='delete-btn']");
		deleteBtn.Click();

		Assert.Multiple(() =>
		{
			Assert.That(events, Has.Count.EqualTo(1));
			Assert.That(events[0].Id, Is.EqualTo(snapshot.Id));
		});
	}

	[Test]
	public void SnapshotItem_WhenEditMode_WhenEditClick()
	{
		var events = new List<Snapshot>();
		var snapshot = new Snapshot { Id = Guid.NewGuid() };

		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, snapshot)
			.Add(p => p.IsEditing, true)
			.Add(p => p.OnEditClick, EventCallback.Factory.Create<Snapshot>(this, s => events.Add(s))));

		var editBtn = cut.Find("[data-testid='edit-item-btn']");
		editBtn.Click();

		Assert.Multiple(() =>
		{
			Assert.That(events, Has.Count.EqualTo(1));
			Assert.That(events[0].Id, Is.EqualTo(snapshot.Id));
		});
	}

	[Test]
	public void SnapshotItem_WhenIsLoading()
	{
		var cut = RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, new())
			.Add(p => p.IsLoading, true));

		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-skeleton-placeholder"), Has.Count.EqualTo(3));
			Assert.That(cut.FindAll(".cwn-snapshot-item__skeleton-title"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-snapshot-item__skeleton-subtitle"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-snapshot-item__skeleton-value"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".wtg-text"), Is.Empty);
		});
	}

	[Test]
	public void SnapshotItem_WhenHasError()
	{
		var snapshot = new Snapshot
		{
			ModuleFilter = new SnapshotModuleFilter
			{
				ModuleName = "The Module",
				ModuleFilterName = "The Layout",
			},
			ErrorMessage = "This is an error!",
		};

		var cut = base.RenderComponent<SnapshotItem>(parameters => parameters
			.AddCascadingValue(_mockService.Object)
			.Add(p => p.Snapshot, snapshot));

		var title = cut.Find(".cwn-snapshot-item__title");
		var subTitle = cut.Find(".cwn-snapshot-item__sub-title");
		var callout = cut.Find(".cwn-callout--error");
		var content = cut.Find(".cwn-snapshot-item__error-content");

		Assert.Multiple(() =>
		{
			Assert.That(title.TextContent, Is.EqualTo("The Layout"));
			Assert.That(subTitle.TextContent, Is.EqualTo("The Module"));
			Assert.That(content.TextContent, Is.EqualTo("This is an error!"));
			Assert.That(callout.GetAttribute("title"), Is.EqualTo("This is an error!"));
			Assert.That(cut.FindAll(".cwn-icon--status-critical"), Has.Count.EqualTo(1));
		});
	}
}
