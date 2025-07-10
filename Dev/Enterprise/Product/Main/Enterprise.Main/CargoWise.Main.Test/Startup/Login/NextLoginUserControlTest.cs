#if !WINZOR
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CargoWise.Main.Startup.Login;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Login.Testing;

public partial class NextLoginUserControlTest : TestCase
{
	[GuiTest]
	[RequiresSTA]
	public void TestLabels()
	{
		// Arrange
		var viewModel = new NextLoginViewModel(new LoginService());
		var uut = new NextLoginUserControl(viewModel);

		using (new WindowForTest(uut))
		{
			// Act
			var title = (Label)uut.FindName("Title");
			var usernameLabel = (Label)uut.FindName("UsernameLabel");
			var passwordLabel = (Label)uut.FindName("PasswordLabel");
			var verificationCodeLabel = (Label)uut.FindName("VerificationCodeLabel");
			var loginButton = (Button)uut.FindName("LoginButton");

			// Assert
			AssertEquals("Title", viewModel.Title.ToString(), title.Content);
			AssertEquals("Username", viewModel.UsernameLabel, usernameLabel.Content);
			AssertEquals("Password", viewModel.PasswordLabel, passwordLabel.Content);
			AssertEquals("Verification Code", viewModel.VerificationCodeLabel, verificationCodeLabel.Content);
			AssertEquals("Login Button", viewModel.LoginButtonCaption, loginButton.Content);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestUsernameProperty()
	{
		// Arrange
		var name = "John Doe";
		var viewModel = new NextLoginViewModel(new LoginService());
		viewModel.Username = name;

		var uut = new NextLoginUserControl(viewModel);

		using (new WindowForTest(uut))
		{
			// Act
			var input = (TextBox)uut.FindName("UsernameTextBox");

			// Assert
			AssertEquals("User Name", name, input.Text);

			// Act
			name = "Walter Woe";
			input.Text = name;
			var binding = BindingOperations.GetBindingExpression(input, TextBox.TextProperty);
			binding?.UpdateSource();

			// Assert
			AssertEquals("User Name after update", name, viewModel.Username);

			input.Focus();
			input.Dispatcher.BeginInvoke(() => AssertEquals("Select all text after textbox got focus", name, input.SelectedText));
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestPasswordProperty()
	{
		// Arrange
		var password = "#123Password";
		var viewModel = new NextLoginViewModel(new LoginService());
		var uut = new NextLoginUserControl(viewModel);
		var input = (PasswordBox)uut.FindName("PasswordTextBox");

		using (new WindowForTest(uut))
		{
			// Act
			input.Password = password;

			// Assert
			AssertEquals("Password", password, viewModel.Password.ToInsecureString());
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestErrorCallout()
	{
		// Arrange
		var viewModel = new NextLoginViewModel(new LoginService()) { Username = "John Doe", };
		var uut = new NextLoginUserControl(viewModel);

		var button = (Button)uut.FindName("LoginButton");
		var errorControl = (Border)uut.FindName("ErrorCallout");
		var errorTextBlock = (TextBlock)uut.FindName("ErrorTextBlock");

		using (new WindowForTest(uut))
		{
			// Assert
			AssertEquals("Error Callout Visibility", Visibility.Collapsed, errorControl.Visibility);

			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Error Callout Visibility", Visibility.Visible, errorControl.Visibility);
			AssertEquals("Error Callout", "You must enter a correct user name and/or password. Please try again.", errorTextBlock.Text);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestLoginButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.Login()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("LoginButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Login button click", 1, count);
		}
	}

	[GuiTest]
	public async Task TestOIDCLoginButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.OIDCLogin()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("OIDCLoginButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			await Task.Delay(10);

			// Assert
			AssertEquals("OIDC Login button click", 1, count);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestTemporaryPasswordButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.TemporaryLogin()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("TemporaryLoginButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Temporary Login button click", 1, count);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestSupportLoginButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.SupportLogin()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("SupportLoginButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Support Login button click", 1, count);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestCancelButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.CancelOIDCLogin()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("CancelButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Cancel button click", 1, count);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestBackButton()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.GoBack()).Callback(() => count++);

		var uut = new NextLoginUserControl(viewModel.Object);
		var button = (Button)uut.FindName("BackButton");

		using (new WindowForTest(uut))
		{
			// Act
			button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Back button click", 1, count);
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestAutoCloseTimer()
	{
		// Arrange
		var viewModel = new Mock<INextLoginViewModel>();
		viewModel.Setup((vm) => vm.AutoCloseTimerStart(It.IsAny<TimeSpan>()));
		viewModel.Setup((vm) => vm.AutoCloseTimerRestart());
		viewModel.Setup((vm) => vm.AutoCloseTimerStop());
		viewModel.Setup((vm) => vm.Login());

		var uut = new NextLoginUserControl(viewModel.Object);
		AssertNoExceptionThrown(() =>
		{
			using (new WindowForTest(uut))
			{
				// Act
				var input = (TextBox)uut.FindName("UsernameTextBox");
				input.Text = "test";

				// Assert
				viewModel.Verify((vm) => vm.AutoCloseTimerStart(TimeSpan.FromMinutes(10)), Times.Once);
				viewModel.Verify((vm) => vm.AutoCloseTimerRestart(), Times.Once);
				viewModel.Verify((vm) => vm.AutoCloseTimerStop(), Times.Never);

				// Act
				var button = (Button)uut.FindName("LoginButton");
				button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

				// Assert
				viewModel.Verify((vm) => vm.AutoCloseTimerStart(TimeSpan.FromMinutes(10)), Times.Once);
				viewModel.Verify((vm) => vm.AutoCloseTimerRestart(), Times.Exactly(2));
				viewModel.Verify((vm) => vm.AutoCloseTimerStop(), Times.Never);

				// Act
				uut.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

				// Assert
				viewModel.Verify((vm) => vm.AutoCloseTimerStart(TimeSpan.FromMinutes(10)), Times.Once);
				viewModel.Verify((vm) => vm.AutoCloseTimerRestart(), Times.Exactly(2));
				viewModel.Verify((vm) => vm.AutoCloseTimerStop(), Times.Once);
			}
		});
	}
}
#endif
