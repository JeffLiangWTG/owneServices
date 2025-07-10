using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF;

namespace CargoWise.GUI.TileBar
{
	/// <summary>
	/// Interaction logic for RecentItemsControl.xaml
	/// </summary>
	public partial class RecentItemsControl : UserControl
	{
		public RecentItemsControl()
		{
			InitializeComponent();

			Loaded += XamlTranslator.GetControlLoadedEvent<RecentItemsControl>();
		}

		public Color PanelBackgroundColor
		{
			get { return this.GetValue<Color>(PanelBackgroundColorProperty); }
			set { SetValue(PanelBackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty PanelBackgroundColorProperty =
			DependencyProperty.Register("PanelBackgroundColor", typeof(Color), typeof(RecentItemsControl), new PropertyMetadata(Color.FromArgb(255, 255, 255)));

		public Color PanelBorderColor
		{
			get { return this.GetValue<Color>(PanelBorderColorProperty); }
			set { SetValue(PanelBorderColorProperty, value); }
		}

		public static readonly DependencyProperty PanelBorderColorProperty =
			DependencyProperty.Register("PanelBorderColor", typeof(Color), typeof(RecentItemsControl), new PropertyMetadata(Color.FromArgb(0, 0, 0)));
	}
}
