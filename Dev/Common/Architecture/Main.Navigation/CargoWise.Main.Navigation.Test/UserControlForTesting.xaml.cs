using System.Windows.Controls;

namespace CargoWise.Main.Navigation.WPF.Test
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class UserControlForTesting : UserControl
	{
		public UserControlForTesting()
		{
			InitializeComponent();
			multilingualBindingAttribute.DataContext = new XamlBindingObjectForTest();

			Loaded += XamlTranslator.GetControlLoadedEvent<UserControlForTesting>();
		}
	}
}
