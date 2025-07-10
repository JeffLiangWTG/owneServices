using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWise.Main.Navigation.Pages;

public partial class Snapshots : CwnComponentBase, IDragAndDropInvokables
{
	const string DropAreaClassname = "cwn-snapshot-item--drop-area";
	const string DragOverClassname = "cwn-snapshot-item--drag-over";

	[CascadingParameter] ISnapshotsService? Service { get; set; }

	[Inject] IDragAndDropJsInterop? DragAndDropJsInterop { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		await (Service?.RefreshAsync() ?? Task.CompletedTask);
	}

	protected string Classname => new CssBuilder()
		.AddClass("cwn-snapshots")
		.AddClass(Class)
		.Build();

	protected string ClassnameContent => new CssBuilder()
		.AddClass("cwn-snapshots__content")
		.AddClass("cwn-snapshots__content--empty", IsEmpty)
		.Build();

	protected string ClassnameSnapshotItem => new CssBuilder()
		.AddClass("cwn-snapshots__item")
		.AddClass(DropAreaClassname, IsEditing)
		.Build();

	public bool IsEditing { get; private set; }

	public bool IsEmpty => SnapshotsList.Count == 0;

	public int MaxSnapshots => Service?.MaxSnapshots ?? 5;

	public bool IsLoading(Snapshot snapshot) => !IsEditing && !snapshot.HasError && string.IsNullOrEmpty(snapshot.Value);

	public bool ShowAddSnapshotItem => SnapshotsList.Count < MaxSnapshots;
	public Snapshot? SnapshotToEdit { get; private set; }

	public ObservableCollection<Snapshot> SnapshotsList => Service?.Snapshots ?? [];

	List<Snapshot> DeletedSnapshotsList { get; } = [];

	string? SnapshotDialogOkButtonText => SnapshotToEdit is null ? Service?.AddToText : Service?.UpdateText;
	string? SnapshotDialogCancelButtonText => Service?.CancelText;
	protected SnapshotDialog? SnapshotDialog;

	async Task HandleEditClickAsync() => await EnterEditMode();

	async Task HandleCancelClickAsync() => await ExitEditMode();

	async Task HandleSaveChangesClickAsync()
	{
		if (Service is not null)
		{
			await Task.WhenAll([
				Task.WhenAll(DeletedSnapshotsList.Select(Service.DeleteAsync)),
				Task.WhenAll(SnapshotsList.Select(Service.CreateOrUpdateAsync)),
			]);
		}

		await ExitEditMode();
	}

	async Task EnterEditMode()
	{
		SnapshotToEdit = null;
		IsEditing = true;
		if (DragAndDropJsInterop is not null)
		{
			await DragAndDropJsInterop.InitAsync(DropAreaClassname, DragOverClassname, DotNetObjectReference.Create<IDragAndDropInvokables>(this));
		}
	}

	async Task ExitEditMode()
	{
		if (DragAndDropJsInterop is not null)
		{
			await DragAndDropJsInterop.TeardownAsync();
		}
		DeletedSnapshotsList.Clear();
		SnapshotToEdit = null;
		IsEditing = false;
		await (Service?.RefreshAsync() ?? Task.CompletedTask);
	}

	void HandleSnapshotDialogCancelClick() => SnapshotDialog?.CloseAsync();

	async Task HandleSnapshotDialogAddSnapshotClickAsync(Snapshot snapshot)
	{
		if (!await TryUpdateAsync(snapshot))
		{
			await TryAddAsync(snapshot);
		}

		if (!IsEditing)
		{
			await (Service?.RefreshAsync() ?? Task.CompletedTask);
		}
	}

	async Task<bool> TryAddAsync(Snapshot snapshot)
	{
		try
		{
			if (IsEditing)
			{
				SnapshotsList.Add(snapshot);
				return true;
			}

			snapshot.Order = SnapshotsList.Count;
			await (Service?.CreateAsync(snapshot) ?? Task.CompletedTask);

			return true;
		}
		catch
		{
			return false;
		}
	}

	async Task<bool> TryUpdateAsync(Snapshot snapshot)
	{
		try
		{
			if (SnapshotToEdit is null)
			{
				return false;
			}

			SnapshotToEdit.ModuleFilter = snapshot.ModuleFilter;

			if (IsEditing)
			{
				SnapshotToEdit = null;
			}
			else
			{
				var ss = SnapshotToEdit ?? snapshot;
				var order = SnapshotToEdit?.Order ?? SnapshotsList.Count;

				await (Service?.CreateOrUpdateAsync(ss, order) ?? Task.CompletedTask);
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	void HandleDeleteItemClick(Snapshot snapshot)
	{
		SnapshotsList.Remove(snapshot);
		DeletedSnapshotsList.Add(snapshot);
	}

	Task HandleAddSnapshotClickAsync()
	{
		SnapshotToEdit = null;
		return SnapshotDialog?.ShowModalAsync() ?? Task.CompletedTask;
	}

	Task HandleEditItemClickAsync(Snapshot snapshot)
	{
		SnapshotToEdit = snapshot;
		return SnapshotDialog?.ShowModalAsync() ?? Task.CompletedTask;
	}

	[JSInvokable]
	public void OnDrop(string dragIndex, string dropIndex)
	{
		var dragIndexInt = int.Parse(dragIndex);
		var dropIndexInt = int.Parse(dropIndex);

		SnapshotsList.Move(dragIndexInt, dropIndexInt);
		StateHasChanged();
	}
}
