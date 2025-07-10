#if !WINZOR
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CargoWise.Main.Navigation;

/// <summary>
/// Interaction logic for NewsPanel.xaml
/// </summary>
public partial class NewsPanel : UserControl
{
	bool IsDesignMode => System.ComponentModel.DesignerProperties.GetIsInDesignMode(this);

	public NewsPanel()
	{
		InitializeComponent();

		if (IsDesignMode)
		{
			return;
		}
		Loaded += This_Loaded;
	}

	void This_Loaded(object sender, RoutedEventArgs e)
	{
		Loaded -= This_Loaded;
		_ = Dispatcher.BeginInvoke(new Action(() =>
		{
			(DataContext as NewsViewModel).LoadNewsItems();
		}));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used from XAML file")]
	void NoNewsToShow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (IsDesignMode)
		{
			return;
		}

		var newsGrid = FindParentOfType<Grid>(this);
		if (newsGrid == null)
		{
			return;
		}

		var shouldHidden = (sender as UIElement).Visibility == Visibility.Visible;
		var rowHeight = shouldHidden ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
		var rowIndex = Grid.GetRow(this);
		if (newsGrid.RowDefinitions.Count > rowIndex)
		{
			//Hide current news panel
			newsGrid.RowDefinitions[rowIndex].Height = rowHeight;
		}

		if (newsGrid.RowDefinitions.All(rd => rd.Height.Value == 0))
		{
			var parentGrid = FindParentOfType<Grid>(newsGrid);
			var colIndex = Grid.GetColumn(newsGrid);
			if (parentGrid != null && parentGrid.ColumnDefinitions.Count > colIndex)
			{
				//Hide whole news section
				var colDefinition = parentGrid.ColumnDefinitions[Grid.GetColumn(newsGrid)];
				colDefinition.Width = new GridLength(0);
				colDefinition.MinWidth = 8; //8 is the width of the margin between the news section and the rest of the UI
			}
		}
	}

	#region Helper methods, we could move them to a helper class later
	static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject
	{
		DependencyObject parentDepObj = child;
		do
		{
			parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
			T parent = parentDepObj as T;
			if (parent != null)
			{
				return parent;
			}
		}
		while (parentDepObj != null);
		return null;
	}

	static T FindFirstVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		if (parent == null)
		{
			return null;
		}

		//Improve the efficiency by looking for the child in the parent's children collection first
		if (parent is Panel panel)
		{
			foreach (var child in panel.Children)
			{
				if (child is T t)
				{
					return t;
				}
			}
		}

		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child is T t)
			{
				return t;
			}

			T childItem = FindFirstVisualChild<T>(child);
			if (childItem != null)
			{
				return childItem;
			}
		}
		return null;
	}
	#endregion
}
#endif
