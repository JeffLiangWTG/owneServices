using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWiseNext.Blazor.Components;
using CargoWiseNext.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace CargoWise.Main.Navigation.Pages
{
	public partial class SnapshotDialog
	{
		[CascadingParameter] public ISnapshotsService? Service { get; set; }
		[CascadingParameter] public KeyboardService? KeyboardService { get; set; }

		[Parameter] public string? ClassContent { get; set; }
		[Parameter] public EventCallback<Snapshot> OnAddSnapshotClick { get; set; }
		[Parameter] public EventCallback OnCancelClick { get; set; }
		[Parameter] public string SelectedModuleName { get; set; } = string.Empty;
		[Parameter] public string SelectedModuleFilterName { get; set; } = string.Empty;
		[Parameter] public string OkButtonText { get; set; } = string.Empty;
		[Parameter] public string CancelButtonText { get; set; } = string.Empty;

		bool ShowModuleFilter => IsModuleFilterDisabled || !ModuleFilters.IsNullOrEmpty();
		bool IsModuleFilterDisabled => string.IsNullOrEmpty(SelectedModuleName);
		bool IsOkButtonDisabled => string.IsNullOrEmpty(SelectedModuleFilterName);

		public List<SnapshotModule> Modules { get; set; } = [];
		public List<SnapshotModuleFilter> ModuleFilters { get; set; } = [];
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			if (KeyboardService is not null)
			{
				KeyboardService.OnKeyDown += HandleKeyDown;
			}
		}

		List<string> ModuleNames => [
			.. Modules
				.Select(m => m.ModuleName)
				.Order()
		];

		List<string> ModuleFilterNames =>
		[
			..ModuleFilters
				.Select(f => f.ModuleFilterName)
				.Order()
		];

		SnapshotModuleFilter? SelectedModuleFilter => ModuleFilters
			.FirstOrDefault(f => f.ModuleFilterName.Equals(SelectedModuleFilterName, StringComparison.OrdinalIgnoreCase));

		SnapshotModule? SelectedModule => Modules
			.FirstOrDefault(m => m.ModuleName.Equals(SelectedModuleName, StringComparison.OrdinalIgnoreCase));

		async Task UpdateModulesAsync()
		{
			Modules = await Service!.FindModulesAsync();
		}

		async Task UpdateModuleFiltersAsync()
		{
			ModuleFilters = [];
			var selectedModule = SelectedModule;
			if (selectedModule is not null)
			{
				ModuleFilters = await Service!.FindModuleFiltersAsync(selectedModule);
			}
		}

		bool isModuleListVisible;
		void HandleModulesVisibilityChanged(bool value)
		{
			isModuleListVisible = value;
		}

		bool isModuleFilterListVisible;
		void HandleModuleFiltersVisibilityChanged(bool value)
		{
			isModuleFilterListVisible = value;
		}

		async Task HandleModuleNameSelectedAsync(string moduleName)
		{
			if (SelectedModuleName != moduleName)
			{
				SelectedModuleName = moduleName;
				SelectedModuleFilterName = string.Empty;

				await UpdateModuleFiltersAsync();
			}
		}

		void HandleModuleFilterNameSelected(string filterName)
		{
			SelectedModuleFilterName = filterName;
		}

		void HandleKeyDown(object? sender, WebKeyboardEventArgs e)
		{
			if (e.Code == "Escape")
			{
				if (!isModuleListVisible && !isModuleFilterListVisible)
				{
					InvokeAsync(() =>
					{
						ClearSelection();
						HandleCancelClickAsync();
					});
				}
			}
		}

		async Task HandleOpenModuleClickAsync()
		{
			var selectedModule = SelectedModule;
			if (selectedModule is not null)
			{
				await Service!.OpenModuleAsync(selectedModule);
				ClearSelection();
			}
		}

		string Classname => new CssBuilder()
			.AddClass(Class)
			.Build();

		string ClassnameContent => new CssBuilder()
			.AddClass(ClassContent)
			.Build();

		protected CwnDialog? dialogRef;
		public async Task ShowModalAsync()
		{
			await UpdateModulesAsync();
			await UpdateModuleFiltersAsync();
			await (dialogRef?.ShowModalAsync() ?? ValueTask.CompletedTask);
		}

		public async Task CloseAsync() => await (dialogRef?.CloseAsync() ?? ValueTask.CompletedTask);
		public async Task<bool> IsOpenAsync() => dialogRef is not null && await dialogRef.IsOpen();

		async Task HandleAddSnapshotClickAsync()
		{
			var moduleFilter = SelectedModuleFilter;
			ClearSelection();

			if (moduleFilter is null)
			{
				return;
			}

			var snapshot = new Snapshot { ModuleFilter = moduleFilter };
			await OnAddSnapshotClick.InvokeAsync(snapshot);
			await CloseAsync();
		}

		void ClearSelection()
		{
			SelectedModuleName = string.Empty;
			SelectedModuleFilterName = string.Empty;
		}

		Task HandleCancelClickAsync() => OnCancelClick.InvokeAsync();
	}
}
