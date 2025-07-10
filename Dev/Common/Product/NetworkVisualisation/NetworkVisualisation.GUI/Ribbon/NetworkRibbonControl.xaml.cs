using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CargoWise.Application;
using CargoWise.Main.Navigation.WPF;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Interaction logic for NetworkRibbon.xaml
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Baseline")]
	public partial class NetworkRibbonControl : UserControl
	{
		// The parameterless constructor is needed for XAML
		public NetworkRibbonControl()
			: base()
		{
			InitializeComponent();
		}

#if DEBUG
		public NetworkRibbonControl(RibbonViewModel viewModel)
			: this()
		{
			AttachViewModel(viewModel);
		}
#endif

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public NetworkUserControl NetworkUserControl => new Lazy<NetworkUserControl>(() => WpfUtils.FindAncestor<NetworkUserControl>(this)).Value;
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		public RibbonViewModel Model => (RibbonViewModel)DataContext;

		public void AttachViewModel(RibbonViewModel viewModel)
		{
			Model?.DeactivateButtons();
			DataContext = viewModel;

			if (viewModel != null && viewModel.IsSuccessfullyConstructed)
			{
				using (Dispatcher.DisableProcessing())
				{
					PopulateRibbonWithControls();
					RegisterMultilingualTextLabels();
				}
			}

			Visibility = NetworkRibbonControlEnabled ? Visibility.Visible : Visibility.Collapsed;
		}

		public bool NetworkRibbonControlEnabled
		{
			get
			{
				return DesignerProperties.GetIsInDesignMode(this) || Model != null && Model.IsSuccessfullyConstructed;
			}
		}

		#region Tab Selection

		public int SelectedTabIndex
		{
			get
			{
				var selectedTab = Ribbon.Items.Cast<RibbonTab>().SingleOrDefault(t => t.IsSelected);
				return Ribbon.Items.IndexOf(selectedTab);
			}
		}

		public void SelectTab(int index)
		{
			if (index >= 0 && index < Ribbon.Items.Count)
			{
				var tab = Ribbon.Items[index] as RibbonTab;
				tab.IsSelected = true;
			}
		}

		#endregion

		#region Testing

		public Ribbon RibbonExposed_ForTesting => Ribbon;

		#endregion

		#region Implementation

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (NetworkRibbonControlEnabled)
			{
				using (Dispatcher.DisableProcessing())
				{
					HideRibbonTitlePanel();
					RegisterMultilingualTextLabels();
				}
			}
		}

		#region PopulateRibbonWithControls

		void PopulateRibbonWithControls()
		{
			ClearRibbonContent();
			CreateRibbonContent();
		}

		void ClearRibbonContent()
		{
			Ribbon.Items.Clear();
		}

		void CreateRibbonContent()
		{
			if (Model != null)
			{
				foreach (var tab in Model.Tabs)
				{
					Ribbon.Items.Add(CreateTab(tab));
				}
			}
		}

		RibbonTab CreateTab(RibbonTabViewModel model)
		{
			var tab = new RibbonTab()
			{
				DataContext = model
			};

			foreach (var group in model.Groups)
			{
				tab.Items.Add(CreateGroup(group));
			}
			return tab;
		}

		RibbonGroup CreateGroup(RibbonGroupViewModel model)
		{
			var group = new RibbonGroup()
			{
				DataContext = model
			};

			foreach (var item in model.Items)
			{
				if (item is RibbonMenuButtonViewModel menuButtonModel)
				{
					group.Items.Add(CreateMenuButton(menuButtonModel));
				}
				else if (item is RibbonToggleButtonViewModel toggleButtonModel)
				{
					group.Items.Add(CreateToggleButton(toggleButtonModel));
				}
				else if (item is RibbonButtonViewModel buttonModel)
				{
					group.Items.Add(CreateButton(buttonModel));
				}
				else
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Rendering of {0} is not implemented", item.GetType().Name));
				}
			}
			return group;
		}

		RibbonButton CreateButton(RibbonButtonViewModel model)
		{
			var button = new RibbonButton
			{
				DataContext = model
			};
			return button;
		}

		RibbonMenuButton CreateMenuButton(RibbonMenuButtonViewModel model)
		{
			var button = new RibbonMenuButton()
			{
				DataContext = model
			};
			return button;
		}

		RibbonToggleButton CreateToggleButton(RibbonToggleButtonViewModel model)
		{
			var button = new RibbonToggleButton()
			{
				DataContext = model
			};
			return button;
		}

		void RibbonButton_Click(object sender, RoutedEventArgs e)
		{
#if DEBUG
			RibbonButtonClicked_ForTest = true;
#endif
			var button = (Control)sender;

			// Since this handler is used by the RibbonMenuItem style in XAML, it will be shared by two distinct types of menu items:
			// * Custom menu items defined by us and bound to a RibbonButtonViewModel (those we want to handle).
			// * System menu items which appear inside the context menu shown when you right-click the ribbon header (such as "Minimize the Ribbon") - these will be bound to our model root (RibbonViewModel) and we *do not* need to handle them here.
			if (button.DataContext is RibbonButtonViewModel model)
			{
				if (TranslationFeedbackManager.InTranslationFeedbackMode())
				{
					TranslationFeedbackManager.OpenFeedbackForm(model.Label);
				}
				else
				{
					NetworkUserControl?.ExecuteRibbonAction(model.Action);
				}
			}
		}

#if DEBUG
		public bool RibbonButtonClicked_ForTest { get; private set; }
#endif
		#endregion

		#region Multilingual Support

		void RegisterMultilingualTextLabels()
		{
			RegisterMultilingualTextLabelsDelegate(this, new RoutedEventArgs());
		}

		RoutedEventHandler RegisterMultilingualTextLabelsDelegate => XamlTranslator.GetControlLoadedEvent<NetworkRibbonControl>();

		IWpfTranslationFeedbackManager TranslationFeedbackManager => translationFeedbackManager ?? (translationFeedbackManager = ObjectFactory.Get<IWpfTranslationFeedbackManager>());
		IWpfTranslationFeedbackManager translationFeedbackManager;

		#endregion

		#region Appearance

		void ReplaceRibbonToggleButtonContent(RibbonToggleButton toggleButton, string text)
		{
			var grid = (Grid)VisualTreeHelper.GetChild(toggleButton, 0);

			var firstBorder = (Border)grid.Children[0];
			firstBorder.Background = Brushes.Brown;

			// Subdues the aero highlighting to that the text has better contrast
			var middleBorder = (Border)grid.Children[1];
			middleBorder.Opacity = .5;

			// Replaces the images with the label text
			var stackPanel = (StackPanel)grid.Children[2];
			var children = stackPanel.Children;
			children.RemoveRange(0, children.Count);
			var textBlock = new TextBlock(new Run(text))
			{
				Foreground = Brushes.White
			};
			children.Add(textBlock);
		}

		string RibbonApplicationMenuLabel => Res.GetString("DBC5D604-D48E-4452-A244-7B0971809086", "File");

		void HideRibbonTitlePanel()
		{
			var titlePanel = WpfUtils.FindElementWithType<RibbonTitlePanel>(Ribbon);
			var docPanel = (DockPanel)titlePanel?.Parent;
			var grid = (Grid)docPanel?.Parent;
			grid?.Children.Remove(docPanel);
		}

		#endregion

		#region System context menu

		void Ribbon_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			HideRibbonShowQuickAccessToolbarContextMenuItem();
		}

		void HideRibbonShowQuickAccessToolbarContextMenuItem()
		{
			if (Ribbon.ContextMenu.Items.Count > 1)
			{
				Ribbon.ContextMenu.Items.RemoveAt(1); //removing redundant separator
				Ribbon.ContextMenu.Items.RemoveAt(0);
			}
		}

		#endregion

		#region Search

		SearchFinderViewModel SearchFinder;

		void SearchBox_Initialized(object sender, EventArgs e)
		{
			SearchBox.Text = DefaultSearchText;
		}

		void SearchBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (SearchBox.Text == DefaultSearchText)
			{
				SearchBox.Clear();
			}
		}

		void SearchBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(SearchBox.Text))
			{
				SearchBox.Text = DefaultSearchText;
			}
		}

		void SearchBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (SearchFinder == null || SearchFinder.NetworkViewModel != NetworkUserControl.NetworkViewModel)
				{
					SearchFinder = new SearchFinderViewModel(NetworkUserControl.NetworkViewModel);
				}

				if (!SearchFinder.HasPerformedSearch)
				{
					SearchFinder.PerformSearch(SearchBox.Text);
				}
				else
				{
					SearchFinder.ShowNextResult();
				}
			}
		}

		void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			SearchFinder?.ResetSearchState();
		}

		string DefaultSearchText => Res.GetString("df1b702e-b74e-4a6a-9df9-4677d97749fa", "Search") + "...";

		#endregion

		#endregion
	}
}
