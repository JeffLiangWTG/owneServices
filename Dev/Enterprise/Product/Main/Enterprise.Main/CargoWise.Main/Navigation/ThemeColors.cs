#if !WINZOR
using System.Drawing;
using System.Windows;

namespace CargoWise.Main.Navigation
{
	public class ThemeColors : DependencyObject
	{
		#region DependencyProperties
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty BackgroundColorProperty =
			DependencyProperty.Register(nameof(BackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.Black));

		public Color MenuSelectedColor
		{
			get { return (Color)GetValue(MenuSelectedColorProperty); }
			set { SetValue(MenuSelectedColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MenuSelectedColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MenuSelectedColorProperty =
			DependencyProperty.Register(nameof(MenuSelectedColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.Black));

		public Color GroupBackgroundColor
		{
			get { return (Color)GetValue(GroupBackgroundColorProperty); }
			set { SetValue(GroupBackgroundColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for GroupBackgroundColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GroupBackgroundColorProperty =
			DependencyProperty.Register(nameof(GroupBackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.White));

		public Color GroupHeaderBackgroundColor
		{
			get { return (Color)GetValue(GroupHeaderBackgroundColorProperty); }
			set { SetValue(GroupHeaderBackgroundColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for GroupHeaderBackgroundColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GroupHeaderBackgroundColorProperty =
			DependencyProperty.Register(nameof(GroupHeaderBackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.Black));

		public Color ShortCutIndexTextColor
		{
			get { return (Color)GetValue(ShortCutIndexTextColorProperty); }
			set { SetValue(ShortCutIndexTextColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ShortCutTextColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShortCutIndexTextColorProperty =
			DependencyProperty.Register(nameof(ShortCutIndexTextColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.Blue));

		public Color MenuBackgroundColor
		{
			get { return (Color)GetValue(MenuBackgroundColorProperty); }
			set { SetValue(MenuBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty MenuBackgroundColorProperty =
			DependencyProperty.Register(nameof(MenuBackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.Black));

		public Color MenuTextColor
		{
			get { return (Color)GetValue(MenuTextColorProperty); }
			set { SetValue(MenuTextColorProperty, value); }
		}

		public static readonly DependencyProperty MenuTextColorProperty =
			DependencyProperty.Register(nameof(MenuTextColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.White));

		public Color FooterBackgroundColor
		{
			get { return (Color)GetValue(FooterBackgroundColorProperty); }
			set { SetValue(FooterBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty FooterBackgroundColorProperty =
			DependencyProperty.Register(nameof(FooterBackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.FromArgb(29, 23, 101)));

		public Color RecentPanelBackgroundColor
		{
			get { return (Color)GetValue(RecentPanelBackgroundColorProperty); }
			set { SetValue(RecentPanelBackgroundColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RecentPanelBackgroundColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RecentPanelBackgroundColorProperty =
			DependencyProperty.Register(nameof(RecentPanelBackgroundColor), typeof(Color), typeof(ThemeColors), new PropertyMetadata(Color.White));
		#endregion

		public static ThemeColors Instance { get; } = new ThemeColors();
	}
}
#endif
