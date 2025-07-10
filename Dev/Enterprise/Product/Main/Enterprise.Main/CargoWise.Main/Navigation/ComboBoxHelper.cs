#if !WINZOR
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CargoWise.Main.Navigation
{
	internal class ComboBoxHelper
	{
		public static readonly DependencyProperty EmptyPromptProperty =
			DependencyProperty.RegisterAttached(
				"EmptyPrompt",
				typeof(string),
				typeof(ComboBoxHelper),
				new PropertyMetadata(string.Empty));

		public static string GetEmptyPrompt(ComboBox obj) =>
			(string)obj.GetValue(EmptyPromptProperty);

		public static void SetEmptyPrompt(ComboBox obj, string value) =>
			obj.SetValue(EmptyPromptProperty, value);

		public static readonly DependencyProperty PlaceholderTextProperty =
			DependencyProperty.RegisterAttached(
				"PlaceholderText",
				typeof(string),
				typeof(ComboBoxHelper),
				new PropertyMetadata(string.Empty, OnPlaceholderChanged));

		public static string GetPlaceholderText(DependencyObject obj) =>
			(string)obj.GetValue(PlaceholderTextProperty);

		public static void SetPlaceholderText(DependencyObject obj, string value) =>
			obj.SetValue(PlaceholderTextProperty, value);

		static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ComboBox comboBox)
			{
				comboBox.Loaded += (s, args) =>
				{
					var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
					if (textBox == null)
					{
						return;
					}

					var placeholder = new TextBlock
					{
						Text = e.NewValue?.ToString(),
						Foreground = Brushes.Gray,
						Visibility = string.IsNullOrEmpty(comboBox.Text) ? Visibility.Visible : Visibility.Collapsed,
						IsHitTestVisible = false,
						Margin = new Thickness(10, 0, 0, 0),
						VerticalAlignment = VerticalAlignment.Center
					};

					var grid = new Grid();
					DependencyObject parent = VisualTreeHelper.GetParent(textBox);
					if (parent == null)
					{
						return;
					}

					if (parent is Panel panel)
					{
						int index = panel.Children.IndexOf(textBox);
						if (index >= 0)
						{
							panel.Children.RemoveAt(index);
							panel.Children.Insert(index, grid);
						}
						grid.Children.Add(textBox);
						grid.Children.Add(placeholder);
						textBox.TextChanged += (sender, eventArgs) =>
						{
							placeholder.Visibility = string.IsNullOrEmpty(comboBox.Text)
								? Visibility.Visible
								: Visibility.Collapsed;
						};
					}
				};
			}
		}
	}
}
#endif
