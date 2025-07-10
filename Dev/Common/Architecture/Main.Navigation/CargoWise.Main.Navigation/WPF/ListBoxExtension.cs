using System.Windows;
using System.Windows.Controls;
using CargoWise.Common;

namespace CargoWise.Main.Navigation.WPF;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in XAML")]
public static class ListBoxExtension
{
	public static object GetHoveredItem(DependencyObject obj)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		return obj.GetValue<object>(HoveredItemProperty);
	}

	public static void SetHoveredItem(DependencyObject obj, object value)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		obj.SetValue(HoveredItemProperty, value);
	}

	public static readonly DependencyProperty HoveredItemProperty =
		DependencyProperty.RegisterAttached("HoveredItem", typeof(object), typeof(ListBoxExtension), new PropertyMetadata(null));

	public static ListBox GetParentListBox(DependencyObject obj)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		return obj.GetValue<ListBox>(ParentListBoxProperty);
	}

	public static void SetParentListBox(DependencyObject obj, ListBox value)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		obj.SetValue(ParentListBoxProperty, value);
	}

	public static readonly DependencyProperty ParentListBoxProperty =
		DependencyProperty.RegisterAttached("ParentListBox", typeof(ListBox), typeof(ListBoxExtension), new PropertyMetadata(null));

	public static object GetHoveredListItem(DependencyObject obj)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		return obj.GetValue<object>(HoveredListItemProperty);
	}

	public static void SetHoveredListItem(DependencyObject obj, object value)
	{
		Argument.NotNull(obj, nameof(obj)); // Suggested By ReviewBot 
		obj.SetValue(HoveredListItemProperty, value);
	}

	public static readonly DependencyProperty HoveredListItemProperty =
		DependencyProperty.RegisterAttached("HoveredListItem", typeof(object), typeof(ListBoxExtension), new PropertyMetadata(null, new PropertyChangedCallback(OnHoveredListItemChanged)));

	static void OnHoveredListItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d != null)
		{
			var listBox = GetParentListBox(d);
			var listBoxItem = d as ListBoxItem;
			SetHoveredItem(listBox, (listBoxItem == null || e.NewValue == null) ? null : listBoxItem.DataContext);
		}
	}
}
