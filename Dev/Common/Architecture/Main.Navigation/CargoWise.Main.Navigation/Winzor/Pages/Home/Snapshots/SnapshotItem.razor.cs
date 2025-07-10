using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages;

public partial class SnapshotItem : CwnComponentBase
{
	[CascadingParameter]
	public ISnapshotsService? Service { get; set; }

	[Parameter] public Snapshot? Snapshot { get; set; }
	[Parameter] public bool IsEditing { get; set; }
	[Parameter] public bool IsLoading { get; set; }
	[Parameter] public bool IsDraggable { get; set; }

	[Parameter] public EventCallback<Snapshot> OnEditClick { get; set; }
	[Parameter] public EventCallback<Snapshot> OnDeleteClick { get; set; }

	public string Classname => new CssBuilder()
		.AddClass("cwn-snapshot-item")
		.AddClass("cwn-snapshot-item--loading", IsLoading)
		.AddClass("cwn-snapshot-item--editing", IsEditing)
		.AddClass(Class)
		.Build();

	public string ModuleName => Snapshot?.ModuleFilter?.ModuleName ?? string.Empty;
	public string LayoutName => Snapshot?.ModuleFilter?.ModuleFilterName ?? string.Empty;
	public string Value => Snapshot?.Value ?? string.Empty;
	public bool HasError => Snapshot?.HasError ?? false;
	public string ErrorMessage => Snapshot?.ErrorMessage ?? string.Empty;

	public Task HandleOpenModuleFilterClickAsync()
	{
		if (Service is null || Snapshot is null)
		{
			return Task.CompletedTask;
		}

		return Service.OpenModuleFilterAsync(Snapshot);
	}

	protected string Draggable => IsDraggable ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant();

	protected async Task HandleEditClickAsync() => await OnEditClick.InvokeAsync(Snapshot);
	protected async Task HandleDeleteClickAsync() => await OnDeleteClick.InvokeAsync(Snapshot);
}
