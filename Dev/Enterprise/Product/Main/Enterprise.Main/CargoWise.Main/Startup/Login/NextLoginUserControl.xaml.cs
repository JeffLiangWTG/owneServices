#if !WINZOR
using System;
using System.Windows;
using System.Windows.Controls;

namespace CargoWise.Main.Startup.Login;

/// <summary>
/// Interaction logic for NextLoginUserControl.xaml
/// </summary>
public partial class NextLoginUserControl : UserControl
{
	public NextLoginUserControl() : this(new NextLoginViewModel(new LoginService()))
	{ }

	public NextLoginUserControl(INextLoginViewModel viewModel)
	{
		InitializeComponent();
		ViewModel = viewModel;
		DataContext = ViewModel;

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	public INextLoginViewModel ViewModel { get; }

	void LoginButton_Click(object sender, RoutedEventArgs e) => Run(ViewModel.Login);

	void TemporaryLoginButton_Click(object sender, RoutedEventArgs e) => Run(ViewModel.TemporaryLogin);

	void OpenIDConnectLogin_Click(object sender, RoutedEventArgs e) => Run(ViewModel.OIDCLogin);

	void SupportLoginButton_Click(object sender, RoutedEventArgs e) => Run(ViewModel.SupportLogin);

	void UsernameTextBox_TextChanged(object s, TextChangedEventArgs e) => ViewModel.AutoCloseTimerRestart();

	void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e) => Run(() => ViewModel.Password = ((PasswordBox)sender).SecurePassword);

	void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e) => Dispatcher.BeginInvoke(() => UsernameTextBox.SelectAll());

	void PasswordTextBox_GotFocus(object sender, RoutedEventArgs e) => Dispatcher.BeginInvoke(() => PasswordTextBox.SelectAll());

	void CancelButton_Click(object sender, RoutedEventArgs e) => Run(ViewModel.CancelOIDCLogin);

	void BackButton_Click(object sender, RoutedEventArgs e) => Run(ViewModel.GoBack);

	void PasswordTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.Key == System.Windows.Input.Key.Enter)
		{
			LoginButton_Click(sender, e);
		}
	}

	void OnUnloaded(object sender, RoutedEventArgs e)
	{
		ViewModel.AutoCloseTimerStop();
		if (ViewModel.IsOIDCLogin)
		{
			ViewModel.CancelOIDCLogin();
		}
	}

	void OnLoaded(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoaded;
		PasswordTextBox.Focus();

		ViewModel.AutoCloseTimerStart(TimeSpan.FromMinutes(10));
		if (ViewModel.IsOIDCLogin)
		{
			Run(ViewModel.OIDCLogin);
		}
	}

	void Run(Action action)
	{
		ViewModel.AutoCloseTimerRestart();
		action.Invoke();
	}
}
#endif
