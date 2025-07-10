using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Navigation.WPF;

namespace CargoWise.GUI.TileBar
{
	public partial class TileBarControl : UserControl
	{
		public TileBarControl()
		{
			InitializeComponent();

			Loaded += XamlTranslator.GetControlLoadedEvent<TileBarControl>();
		}

		#region Theme Values

		public Color TileBackgroundColor
		{
			get { return this.GetValue<Color>(TileBackgroundColorProperty); }
			set { SetValue(TileBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty TileBackgroundColorProperty =
			DependencyProperty.Register("TileBackgroundColor", typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.FromArgb(0, 0, 0)));

		public Color TileTextColor
		{
			get { return this.GetValue<Color>(TileTextColorProperty); }
			set { SetValue(TileTextColorProperty, value); }
		}

		public static readonly DependencyProperty TileTextColorProperty =
			DependencyProperty.Register(nameof(TileTextColor), typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.White));

		public Color TileSelectedColor
		{
			get { return this.GetValue<Color>(TileSelectedColorProperty); }
			set { SetValue(TileSelectedColorProperty, value); }
		}

		public static readonly DependencyProperty TileSelectedColorProperty =
			DependencyProperty.Register("TileSelectedColor", typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.FromArgb(255, 0, 0)));

		public Color GroupBackgroundColor
		{
			get { return this.GetValue<Color>(GroupBackgroundColorProperty); }
			set { SetValue(GroupBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty GroupBackgroundColorProperty =
			DependencyProperty.Register("GroupBackgroundColor", typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.FromArgb(240, 240, 240)));

		public Color GroupHeaderBackgroundColor
		{
			get { return this.GetValue<Color>(GroupHeaderBackgroundColorProperty); }
			set { SetValue(GroupHeaderBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty GroupHeaderBackgroundColorProperty =
			DependencyProperty.Register("GroupHeaderBackgroundColor", typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.FromArgb(255, 255, 255)));

		public Color RecentPanelBackgroundColor
		{
			get { return this.GetValue<Color>(RecentPanelBackgroundColorProperty); }
			set { SetValue(RecentPanelBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty RecentPanelBackgroundColorProperty =
			DependencyProperty.Register("RecentPanelBackgroundColor", typeof(Color), typeof(TileBarControl), new PropertyMetadata(Color.FromArgb(255, 255, 255)));

		#endregion

		#region Search
		public void SetCaretPosition(int index)
		{
			QuickSearch.CaretIndex = index;
		}

		public int GetCaretPosition()
		{
			return QuickSearch.CaretIndex;
		}

		void SelectedMode_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var listBox = sender as ListBox;
			if (listBox != null)
			{
				var viewModel = listBox.DataContext as NavigationViewModel;
				if (viewModel != null)
				{
					if (viewModel.SelectedCategory != null && viewModel.SelectedCategory.ShowSearch && viewModel.IsInSearchMode)
					{
						viewModel.ForceClearSearch();
					}
				}
			}
		}

		void Search_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is NavigationViewModel viewModel)
			{
				viewModel.IsInSearchMode = !viewModel.IsInSearchMode;
				if (viewModel.IsInSearchMode && !QuickSearch.IsFocused)
				{
					QuickSearch.Focus();
				}
				else if (!viewModel.IsInSearchMode)
				{
					viewModel.ForceClearSearch();
					viewModel.SelectedCategory = viewModel.Categories[0];
				}
			}
		}
		#endregion
	}
}
