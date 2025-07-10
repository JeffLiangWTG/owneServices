#if !WINZOR
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CargoWise.Main.Navigation;

/// <summary>
/// Interaction logic for SnapshotModuleAndLayout.xaml
/// </summary>
public partial class SnapshotModuleAndLayout : UserControl, INotifyPropertyChanged
{
	ObservableCollection<SnapshotModule> _modulesSource;
	ObservableCollection<SnapshotModuleFilter> _filtersSource;

	bool _isArrowKeyPressed;
	bool _loaded;

	List<SnapshotModule> modules;

	List<SnapshotModule> Modules
	{
		get
		{
			if (modules == null)
			{
				if (DataContext is SnapshotsViewModel viewModel)
				{
					modules = viewModel.FindModules().Distinct().OrderBy(t => t.ModuleName).ToList();
				}
			}
			return modules;
		}
	}
	internal SnapshotModuleAndLayout()
	{
		InitializeComponent();

		CbModule.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnComboBoxTextChanged));
		CbModule.SelectionChanged += OnModulesSelectionChanged;
		CbLayout.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnComboBoxTextChanged));
		CbLayout.SelectionChanged += OnModulesSelectionChanged;

		CbModule.AddHandler(TextBox.GotFocusEvent, new RoutedEventHandler(OnGotFocus));
		CbLayout.AddHandler(TextBox.GotFocusEvent, new RoutedEventHandler(OnGotFocus));

		PreviewKeyDown += UserControl_PreviewKeyDown;
	}

	internal IDialog Dialog { get ; set ; }

	void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is not SnapshotsViewModel)
		{
			return;
		}

		var viewModel = (SnapshotsViewModel)DataContext;
		if (!_loaded)
		{
			_modulesSource = new ObservableCollection<SnapshotModule>(Modules);
			FilteredModules = _modulesSource;

			_filtersSource = new ObservableCollection<SnapshotModuleFilter>();
			FilteredLayouts = _filtersSource;
			CbLayout.IsEnabled = false;

			_loaded = true;
			if (SelectedSnapshot != null)
			{
				var selectedModule = viewModel.SelectedSnapshot.ModuleFilter.ModuleId;
				var selectedLayout = viewModel.SelectedSnapshot.ModuleFilter.ModuleFilterId;

				CbModule.SelectedItem = Modules.FirstOrDefault(t => t.ModuleId == selectedModule);
				_filtersSource = new ObservableCollection<SnapshotModuleFilter>(viewModel.FindModuleFiltersByModuleId(selectedModule));
				FilteredLayouts = _filtersSource;
				CbLayout.SelectedItem = _filtersSource.FirstOrDefault(t => t.ModuleFilterId == selectedLayout);

				CbModule.IsDropDownOpen = false;
				CbLayout.IsDropDownOpen = false;
			}
		}
		else
		{
			Clear();
		}
	}

	void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if ((e.Key == Key.Up || e.Key == Key.Down) &&
			(CbModule.IsKeyboardFocusWithin || CbLayout.IsKeyboardFocusWithin))
		{
			_isArrowKeyPressed = true;
		}
	}

	void OnModulesSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (DataContext is SnapshotsViewModel viewModel)
		{
			if (CbModule.SelectedValue is SnapshotModule selectedModule)
			{
				var layouts = new ObservableCollection<SnapshotModuleFilter>(viewModel.FindModuleFiltersByModuleId(selectedModule.ModuleId));
				_filtersSource = new ObservableCollection<SnapshotModuleFilter>(layouts);
				FilteredLayouts = _filtersSource;
			}
			else
			{
				_filtersSource = new ObservableCollection<SnapshotModuleFilter>();
			}

			CbLayout.IsEnabled = CbModule.SelectedItem != null;
		}
	}

	void OnComboBoxTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is ComboBox { IsEditable: true, IsKeyboardFocusWithin: true } comboBox)
		{
			var textBox = comboBox.FindChild<TextBox>("PART_EditableTextBox");
			if (textBox != null)
			{
				textBox.SelectionStart = textBox.Text.Length;
				textBox.SelectionLength = 0;
			}
		}
	}

	void OnGotFocus(object sender, RoutedEventArgs e)
	{
		if (sender is ComboBox comboBox)
		{
			comboBox.IsDropDownOpen = true;
		}
	}

	public string SearchModuleText
	{
		get => (string)GetValue(SearchModuleTextProperty);
		set => SetValue(SearchModuleTextProperty, value);
	}

	public static readonly DependencyProperty SearchModuleTextProperty =
		DependencyProperty.Register(
			nameof(SearchModuleText),
			typeof(string),
			typeof(SnapshotModuleAndLayout),
			new PropertyMetadata(OnSearchModuleTextChanged));

	public string SearchLayoutText
	{
		get => (string)GetValue(SearchLayoutTextProperty);
		set => SetValue(SearchLayoutTextProperty, value);
	}

	public static readonly DependencyProperty SearchLayoutTextProperty =
		DependencyProperty.Register(
			nameof(SearchLayoutText),
			typeof(string),
			typeof(SnapshotModuleAndLayout),
			new PropertyMetadata(OnSearchLayoutChanged));

	static void OnSearchModuleTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (SnapshotModuleAndLayout)d;
		control.CbModule.IsDropDownOpen = true;

		if (control._isArrowKeyPressed)
		{
			control._isArrowKeyPressed = false;
		}
		else
		{
			if (control.CbModule.SelectedItem != null && string.IsNullOrEmpty(control.SearchModuleText))
			{
				control.CbModule.SelectedItem = null;
				control.FilteredModules = control._modulesSource;
				control.FilteredLayouts = new ObservableCollection<SnapshotModuleFilter>();
				control.CbLayout.SelectedItem = null;
				control.CbLayout.IsDropDownOpen = false;
			}
			else
			{
				var filtered = control.Modules
						.Where(item => item.ModuleName.StartsWith(control.SearchModuleText, StringComparison.OrdinalIgnoreCase));

				control.FilteredModules = new ObservableCollection<SnapshotModule>(filtered);
			}
		}
	}

	static void OnSearchLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (SnapshotModuleAndLayout)d;
		control.CbLayout.IsDropDownOpen = true;

		if (control.CbLayout.SelectedItem != null && string.IsNullOrEmpty(control.SearchLayoutText))
		{
			control.FilteredLayouts = control._filtersSource;
			control.CbLayout.SelectedItem = null;
		}
		else
		{
			var filtered = control._filtersSource
					.Where(item => item.ModuleFilterName.StartsWith(control.SearchLayoutText, StringComparison.OrdinalIgnoreCase))
				;
			control.FilteredLayouts = new ObservableCollection<SnapshotModuleFilter>(filtered);
		}
	}

	ObservableCollection<SnapshotModule> _filteredModules = new();
	public ObservableCollection<SnapshotModule> FilteredModules
	{
		get => _filteredModules;
		set
		{
			_filteredModules = value;
			OnPropertyChanged(nameof(FilteredModules));
		}
	}

	ObservableCollection<SnapshotModuleFilter> _filteredLayouts = new();
	public ObservableCollection<SnapshotModuleFilter> FilteredLayouts
	{
		get => _filteredLayouts;
		set
		{
			_filteredLayouts = value;
			OnPropertyChanged(nameof(FilteredLayouts));
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	void AddButton_OnClick(object sender, RoutedEventArgs e)
	{
		HasError = false;
		ErrorMessage = string.Empty;
		if (DataContext is SnapshotsViewModel viewModel)
		{
			try
			{
				if (CbLayout.SelectedItem is SnapshotModuleFilter moduleFilter)
				{
					viewModel.AddToSnapshots(moduleFilter.ModuleFilterId);
					Dialog?.Close();
					HasError = false;
					ErrorMessage = string.Empty;
					CbModule.SelectedItem = null;
					CbLayout.SelectedItem = null;
				}
			}
			catch (Exception ex)
			{
				HasError = true;
				ErrorMessage = ex.Message;
			}
		}
	}

	void Clear()
	{
		CbLayout.SelectedItem = null;
		CbModule.SelectedItem = null;
		CbModule.IsDropDownOpen = false;
		CbLayout.IsDropDownOpen = false;
		CbModule.Text = string.Empty;
		CbLayout.Text = string.Empty;
		HasError = true;
		ErrorMessage = string.Empty;
	}
	void CancelButton_OnClick(object sender, RoutedEventArgs e)
	{
		Dialog?.Close();
		Clear();
	}

	public Snapshot SelectedSnapshot
	{
		get => (Snapshot)GetValue(SelectedSnapshotProperty);
		set => SetValue(SelectedSnapshotProperty, value);
	}

	public static readonly DependencyProperty SelectedSnapshotProperty =
		DependencyProperty.Register(
			nameof(SelectedSnapshot),
			typeof(Snapshot),
			typeof(SnapshotModuleAndLayout),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				propertyChangedCallback: OnSelectedSnapshotChanged
			));

	static void OnSelectedSnapshotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (SnapshotModuleAndLayout)d;
		var newObject = (Snapshot)e.NewValue;
		if (control._loaded)
		{
			if (newObject != null)
			{
				var selectedModule = newObject.ModuleFilter.ModuleId;
				var selectedLayout = newObject.ModuleFilter.ModuleFilterId;

				var viewModel = (SnapshotsViewModel)control.DataContext;

				control.CbModule.SelectedValue = null;
				control.CbModule.SelectedValue = control.FilteredModules.FirstOrDefault(t => t.ModuleId == selectedModule);
				control._filtersSource = new ObservableCollection<SnapshotModuleFilter>(viewModel.FindModuleFiltersByModuleId(selectedModule));
				control.FilteredLayouts = control._filtersSource;

				control.CbLayout.SelectedItem = control._filtersSource.FirstOrDefault(t => t.ModuleFilterId == selectedLayout);

				control.CbModule.IsDropDownOpen = false;
				control.CbLayout.IsDropDownOpen = false;
			}
			else
			{
				control.CbModule.SelectedItem = null;
				control.CbLayout.SelectedItem = null;
			}
		}
	}

	public bool HasError {
		get => (bool)GetValue(HasErrorProperty);
		set => SetValue(HasErrorProperty, value);
	}

	public static readonly DependencyProperty HasErrorProperty =
		DependencyProperty.Register(
			nameof(HasError),
			typeof(bool),
			typeof(SnapshotModuleAndLayout),
			new PropertyMetadata(false));
	public string ErrorMessage
	{
		get => (string)GetValue(ErrorMessageProperty);
		set => SetValue(ErrorMessageProperty, value);
	}

	public static readonly DependencyProperty ErrorMessageProperty =
		DependencyProperty.Register(
			nameof(ErrorMessage),
			typeof(string),
			typeof(SnapshotModuleAndLayout),
			new PropertyMetadata(string.Empty));
}
#endif
