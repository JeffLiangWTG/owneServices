using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Main.Navigation.JsInterop;
using CargoWise.Main.Navigation.ViewModels;
using CargoWiseNext.Blazor.Components;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWise.Main.Navigation.Pages;

public partial class QuickSearch : IDisposable
{
	public QuickSearch()
	{
		_dotNetObjectReference = DotNetObjectReference.Create(this);
		_debounceService = new DebounceService(1000);
	}

	[Inject]
	public IPopoverService? PopoverService { get; set; }

	readonly DebounceService _debounceService;

	[Inject]
	public IQuickSearchJsInterop? QuickSearchJsInterop { get; set; }

	[Parameter]
	public NavigationViewModel? NavigationViewModel { get; set; }

	[Parameter]
	public string? SearchPopoverId { get; set; }

	bool _isInteractionSuppressed;
	Guid quickSearchId;
	protected Guid QuickSearchId
	{
		get
		{
			if (quickSearchId == Guid.Empty)
			{
				quickSearchId = Guid.NewGuid();
			}

			return quickSearchId;
		}
	}

	protected CwnSearch? _searchRef;

	readonly DotNetObjectReference<QuickSearch> _dotNetObjectReference;

	bool shouldShowNoResults;

	string popoverHeight => shouldShowNoResults ? (NoResString)"height: 190px" : (NoResString)"height: 80%";

	EventHandler? _loadResultCompleteHandler;
	protected override async Task OnInitializedAsync()
	{
		if (NavigationViewModel is not null && NavigationViewModel.SearchViewModel is not null)
		{
			_loadResultCompleteHandler = async (sender, e) => await SearchViewModel_LoadResultCompleteEvent(sender, e);
			NavigationViewModel.SearchViewModel.LoadResultCompleteEvent += _loadResultCompleteHandler;
		}

		if (QuickSearchJsInterop is not null)
		{
			await QuickSearchJsInterop.RegisterFocusShortcutAsync(_dotNetObjectReference);
			await QuickSearchJsInterop.RegisterPopoverNavigationAsync(_dotNetObjectReference, SearchPopoverId);
			await QuickSearchJsInterop.RegisterFocusOutHandlerAsync(_dotNetObjectReference, _parentRef, nameof(HandleFocusOutAsync));
		}
		await base.OnInitializedAsync();
	}

	async Task SearchViewModel_LoadResultCompleteEvent(object? sender, EventArgs e)
	{
		shouldShowNoResults = NavigationViewModel?.SearchViewModel.SearchResults.Count == 0;
		await InvokeAsync(StateHasChanged);
		if (QuickSearchJsInterop is not null)
		{
			await QuickSearchJsInterop.UpdateResultsAsync(SearchPopoverId);
		}
	}

	[JSInvokable]
	public async Task FocusAsync()
	{
		if (_searchRef is null)
		{
			return;
		}
		await _searchRef.FocusAsync();
	}

	[JSInvokable]
	public async Task HandleEscapeKeyAsync()
	{
		if (NavigationViewModel!.IsInSearchMode)
		{
			if (!string.IsNullOrEmpty(NavigationViewModel.SearchViewModel.SearchValue))
			{
				NavigationViewModel.SearchViewModel.SearchResults?.Clear();
				NavigationViewModel.SearchViewModel.SearchValue = string.Empty;
			}
			else
			{
				NavigationViewModel?.HideAllMenus();
				await CloseSearchPopupAsync();
			}
			await FocusAsync();
			await InvokeAsync(StateHasChanged);
		}
	}

	async Task HandleSearchInputAsync(string? value)
	{
		if (_isInteractionSuppressed)
		{
			return;
		}
		if (NavigationViewModel?.SearchViewModel is null)
		{
			return;
		}
		NavigationViewModel.SearchViewModel.SearchValue = value;

		if (string.IsNullOrWhiteSpace(value))
		{
			NavigationViewModel.SearchViewModel.SearchResults?.Clear();
			shouldShowNoResults = false;
			await InvokeAsync(StateHasChanged);
			return;
		}
		_debounceService.Debounce(() =>
		{
			if (NavigationViewModel is not null && NavigationViewModel.SearchViewModel is not null)
			{
				NavigationViewModel.IsInSearchMode = true;
			}
		});
		await OpenSearchPopupAsync();
	}

	async Task HandleSearchFocusAsync()
	{
		await OpenSearchPopupAsync();
	}

	async Task OpenSearchPopupAsync()
	{
		if (!_isInteractionSuppressed && NavigationViewModel is not null)
		{
			NavigationViewModel.IsInSearchMode = true;
			if (PopoverService is not null && !string.IsNullOrEmpty(SearchPopoverId))
			{
				await PopoverService.ShowAsync(SearchPopoverId);
			}
		}
	}

	async Task CloseSearchPopupAsync()
	{
		if (_isInteractionSuppressed is false && NavigationViewModel is not null)
		{
			NavigationViewModel.IsInSearchMode = false;
			if (PopoverService is not null && !string.IsNullOrEmpty(SearchPopoverId))
			{
				await PopoverService.HideAsync(SearchPopoverId);
			}
		}
	}

	async Task OnToggleAsync(bool isOpen)
	{
		if (!isOpen)
		{
				ClearSearchValue();
				await CloseSearchPopupAsync();
		}
	}

	void OnMenuClick(MenuItem item)
	{
		_isInteractionSuppressed = true;
		if (NavigationViewModel is not null)
		{
			NavigationViewModel.IsInSearchMode = false;
		}
		_ = RestoreInteractionAsync();
	}

	async Task RestoreInteractionAsync()
	{
		await Task.Delay(300);
		_isInteractionSuppressed = false;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await (PopoverService?.InitAsync(SearchPopoverId) ?? Task.CompletedTask);
		}

		_isInteractionSuppressed = false;
		await base.OnAfterRenderAsync(firstRender);
	}

	public void ClearSearchValue()
	{
		if (!_isInteractionSuppressed)
		{
			_searchRef?.Clear();
			if (NavigationViewModel?.SearchViewModel != null)
			{
				NavigationViewModel.SearchViewModel.SearchValue = string.Empty;
			}
		}
	}

	public void Dispose()
	{
		_dotNetObjectReference?.Dispose();
		_debounceService?.Dispose();
		if (_loadResultCompleteHandler != null && NavigationViewModel != null)
		{
			NavigationViewModel.SearchViewModel.LoadResultCompleteEvent -= _loadResultCompleteHandler;
		}
	}

	readonly List<string>  SearchShortcut =
		[NavigationMultilingualResources.ShortcutCtrl, NavigationMultilingualResources.SearchShortcutKey];

	protected ElementReference? _parentRef;

	[JSInvokable]
	public async Task HandleFocusOutAsync()
	{
		await CloseSearchPopupAsync();
	}
}
