#if !WINZOR
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CargoWise.Main.Startup.Login
{
	/// <summary>
	/// Interaction logic for NextLoginLocationUserControl.xaml
	/// </summary>
	public partial class NextLoginLocationUserControl : UserControl
	{
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		public static RoutedCommand HandleCompanyComboBoxKeyDownCommand = new RoutedCommand();
		public static RoutedCommand HandleBranchComboBoxKeyDownCommand = new RoutedCommand();
		public static RoutedCommand HandleDepartmentComboBoxKeyDownCommand = new RoutedCommand();
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

		NextLoginLocationViewModel ViewModel { get; }

		public NextLoginLocationUserControl() : this(new NextLoginLocationViewModel(new LoginService())) { }

		public NextLoginLocationUserControl(NextLoginLocationViewModel viewModel)
		{
			InitializeComponent();
			ViewModel = viewModel;
			DataContext = ViewModel;
		}

		void BackButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.ShowLoginUserControl();
		}

		void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Login();
		}

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			CompanyComboBox.Focus();
		}

		void CompanyComboBox_KeyDown(object sender, ExecutedRoutedEventArgs e)
		{
			BranchComboBox.Focus();
		}

		void BranchComboBox_KeyDown(object sender, ExecutedRoutedEventArgs e)
		{
			DepartmentComboBox.Focus();
		}

		void DepartmentComboBox_KeyDown(object sender, ExecutedRoutedEventArgs e)
		{
			LoginButton.Focus();
		}
	}
}
#endif
