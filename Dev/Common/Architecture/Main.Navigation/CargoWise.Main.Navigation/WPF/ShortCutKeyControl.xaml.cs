using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation.WPF
{
	/// <summary>
	/// Interaction logic for ShortCutKeyControl.xaml
	/// </summary>
	public partial class ShortcutKeyControl : UserControl
	{
		readonly ShortcutKeyConverter shortCutKeyConveter = new ShortcutKeyConverter();
		public ShortcutKeyControl()
		{
			InitializeComponent();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (ShortCutKeyStyle is null)
			{
				ShortCutKeyStyle = (Style)FindResource("DefaultShortCutKeyStyle");
			}
			base.OnRender(drawingContext);
		}
		public string ShortcutKey
		{
			get { return (string)GetValue(ShortcutKeyProperty); }
			set { SetValue(ShortcutKeyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ShortCutKey.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShortcutKeyProperty =
			DependencyProperty.Register("ShortCutKey", typeof(string), typeof(ShortcutKeyControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnShortCutKeyChanged)));
		public Brush TextBorderBrush
		{
			get { return (Brush)GetValue(TextBorderBrushProperty); }
			set { SetValue(TextBorderBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TextBorderBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextBorderBrushProperty =
			DependencyProperty.Register("TextBorderBrush", typeof(Brush), typeof(ShortcutKeyControl), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString((NoResString)"#FFBFBEB9"))));

		static void OnShortCutKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ShortcutKeyControl shortCutKeyControl)
			{
				shortCutKeyControl.Keys.Clear();
				if (e.NewValue is string newValue)
				{
					foreach (var key in shortCutKeyControl.SplitKeys(newValue))
					{
						shortCutKeyControl.Keys.Add(key);
					}
				}
			}
		}

		public ObservableCollection<string> Keys { get; set; } = new ObservableCollection<string>();

		public Style ShortCutKeyStyle
		{
			get { return (Style)GetValue(ShortCutKeyStyleProperty); }
			set { SetValue(ShortCutKeyStyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ShortCutKeyStyle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShortCutKeyStyleProperty =
			DependencyProperty.Register("ShortCutKeyStyle", typeof(Style), typeof(ShortcutKeyControl), new PropertyMetadata(null));

		internal IEnumerable<string> SplitKeys(string rowString)
		{
			foreach (var item in shortCutKeyConveter.Convert(rowString))
			{
				yield return ShortcutKeyResources.GetKeyString(item);
			}
		}
	}
}
