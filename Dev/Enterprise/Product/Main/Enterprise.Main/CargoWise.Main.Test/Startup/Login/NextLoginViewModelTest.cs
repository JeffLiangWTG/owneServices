using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Main.Startup.Login;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.Login;

public class NextLoginViewModelTest : TransactionedTestCase
{
	public void TestLogin_WhenSuccessful()
	{
		var username = "John Doe";
		var password = "#123pwd";

		var user = new Mock<IUser>();
		user.SetupGet(m => m.LoginName).Returns(username);
		user.SetupGet(m => m.FullName).Returns(username);

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.Is<string>(p => p == username),
			It.Is<string>(p => p == password),
			It.IsAny<string>(),
			It.Is<bool>(p => p))).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService());

			uut.Username = username;
			uut.Password = new NetworkCredential("", password).SecurePassword;

			uut.Login();

			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenEmptyUsername_WhenEmptyPassword()
	{
		using var loginDirector = LoginDirector.UseTestInstance();
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = string.Empty;
		uut.Login();

		AssertEquals("Error Message", "You must enter a correct user name and/or password. Please try again.", uut.ErrorMessage);
	}

	public void TestLogin_WhenEmptyPassword()
	{
		using var loginDirector = LoginDirector.UseTestInstance();
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = "John Doe";
		uut.Login();

		AssertEquals("Error Message", "You must enter a correct user name and/or password. Please try again.", uut.ErrorMessage);
	}

	public void TestLogin_WhenEmptyUsername()
	{
		using var loginDirector = LoginDirector.UseTestInstance();
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = string.Empty;
		uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;

		uut.Login();

		AssertEquals("Error Message", "You must enter a correct user name and/or password. Please try again.", uut.ErrorMessage);
	}

	public void TestLogin_WhenSupport_WhenEmptyPassword()
	{
		using var loginDirector = LoginDirector.UseTestInstance();
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = User.SupportUserName;
		uut.Login();

		AssertEquals("Error Message", "The CWSupport token provided is invalid.", uut.ErrorMessage);
	}

	public void TestLogin_WhenSupport_WhenInvalidPassword()
	{
		using var loginDirector = LoginDirector.UseTestInstance();
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = User.SupportUserName;
		uut.Password = new NetworkCredential("", "#invalid").SecurePassword;
		uut.Login();

		AssertEquals("Error Message", "The CWSupport token provided is invalid.", uut.ErrorMessage);
	}

	public void TestLogin_WhenSupport_WhenValidPassword()
	{
		var username = User.SupportUserName;
		var password = CWSupportLoginToken.TokenForTest;

		var user = new Mock<IUser>();
		user.SetupGet(m => m.LoginName).Returns(username);
		user.SetupGet(m => m.FullName).Returns(username);

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.Is<string>(p => p == username),
			It.Is<string>(p => p == password),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using var loginDirector = LoginDirector.UseTestInstance(mockLoginDirector.Object);
		var uut = new NextLoginViewModel(new LoginService());

		uut.Username = username;
		uut.Password = new NetworkCredential("", password).SecurePassword;
		uut.Login();

		AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
	}

	public void TestLogin_WhenTempPasswordRequired()
	{
		var failureMessage = "Your password must be reset. Click the 'Email Temporary Password' button to send a temporary password to your email address.";

		var user = new Mock<IUser>();
		var info = LoginAuthenticationInfo.NewTempPasswordRequired(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService());

			uut.Username = "John Doe";
			uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;

			uut.Login();

			AssertEquals("Error Message", failureMessage, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenTempPasswordLoginSuccessfully()
	{
		var username = "John Doe";
		var password = "#123pwd";

		var user = new Mock<IUser>();
		var info = LoginAuthenticationInfo.NewTempPasswordLoginSuccessfully(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService());

			uut.Username = username;
			uut.Password = new NetworkCredential("", password).SecurePassword;

			uut.Login();

			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenTwoFactorAuthenticationRequired()
	{
		var failureMessage = "Please enter the validation code that has been sent to you.";

		var user = new Mock<IUser>();
		var info = LoginAuthenticationInfo.NewTwoFactorAuthenticationLogin(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService())
			{
				IsTwoFactorAuthentication = false,
			};

			uut.Username = "John Doe";
			uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;

			uut.Login();

			Assert("Is 2FA", uut.IsTwoFactorAuthentication);
			AssertEquals("Error Message", failureMessage, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenTwoFactorAuthenticationFailed()
	{
		var failureMessage = "The validation code you entered is incorrect. Please try again.";

		var user = new Mock<IUser>();
		var info = LoginAuthenticationInfo.NewTwoFactorAuthenticationFailedLogin(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService())
			{
				IsTwoFactorAuthentication = true,
			};

			uut.Username = "John Doe";
			uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;
			uut.VerificationCode = "12345";

			uut.Login();

			Assert("Is 2FA", uut.IsTwoFactorAuthentication);
			AssertEquals("Verification code", string.Empty, uut.VerificationCode);
			AssertEquals("Error Message", failureMessage, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenTwoFactorAuthenticationFieldMissing()
	{
		var failureMessage = "The validation code could not be sent as your account has no none set up. Please contact your system administrator for assistance.";

		var user = new Mock<IUser>();
		var info = LoginAuthenticationInfo.NewTwoFactorAuthenticationFieldMissing(user.Object);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService())
			{
				IsTwoFactorAuthentication = true,
			};

			uut.Username = "John Doe";
			uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;
			uut.VerificationCode = string.Empty;

			uut.Login();

			Assert("Is 2FA", uut.IsTwoFactorAuthentication);
			AssertEquals("Verification code", string.Empty, uut.VerificationCode);
			AssertEquals("Error Message", failureMessage, uut.ErrorMessage);
		}
	}

	public void TestLogin_WhenLoginNotValidated()
	{
		var failureMessage = "Error: user inactive";

		var info = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserInactive, failureMessage);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginUserInteractive(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool>())).Returns(info);

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginViewModel(new LoginService());

			uut.Username = "John Doe";
			uut.Password = new NetworkCredential("", "#123pwd").SecurePassword;

			uut.Login();

			AssertEquals("Error Message", failureMessage, uut.ErrorMessage);
		}
	}

	public void TestPropertyChanged()
	{
		var properties = new List<string>();
		var uut = new NextLoginViewModel(new LoginService());
		uut.PropertyChanged += (sender, args) => { properties.Add(args.PropertyName); };

		AssertEquals("Initial properties", 0, properties.Count);

		uut.Username = "John Doe";
		uut.Password = new NetworkCredential("", "#123Password").SecurePassword;
		uut.VerificationCode = "1234";

		AssertEquals("Properties before login attempt", 3, properties.Count);
		AssertEquals("Username", properties[0]);
		AssertEquals("Password", properties[1]);
		AssertEquals("VerificationCode", properties[2]);
	}

	public void TestSupportLogin()
	{
		using (LoginDirector.UseTestInstance())
		{
			var uut = new NextLoginViewModel(new LoginService());

			uut.SupportLogin();

			Assert("User/Password login enabled", uut.IsUserPasswordLogin);
			Assert("Kogin button visible", uut.IsLoginButtonVisible);
			AssertEquals("Username", User.SupportUserName, uut.Username);
			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
			AssertEquals("Warning Message", string.Empty, uut.WarningMessage);
			AssertEquals("Info Message", string.Empty, uut.InfoMessage);
		}
	}

	public void TestOIDCLogin_WhenSuccess()
	{
		var username = "MyUserName";
		var userMock = new Mock<IUser>();
		userMock.SetupGet(m => m.LoginName).Returns(username);

		var serviceMock = new Mock<ILoginService>();
		serviceMock.Setup(s => s.LoginOIDC(It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(LoginAuthenticationInfo.NewSuccessfulLogin(userMock.Object)));
		serviceMock.Setup(s => s.SetSession(It.Is<string>(p => p == username)));

		var uut = new NextLoginViewModel(serviceMock.Object);
		uut.OIDCLogin();

		AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		AssertEquals("Warning Message", string.Empty, uut.WarningMessage);
		AssertEquals("Info Message", "Please login via the new tab that has opened in your web browser and return to the application when completed.", uut.InfoMessage);
	}

	public void TestTemporaryLogin()
	{
		var username = "MyUserName";
		var expectedMessage = "A temporary password has been sent to your email address.";
		var serviceMock = new Mock<ILoginService>();
		serviceMock.Setup(s => s.EmailTemporaryPassword(It.Is<string>(p => p == username)))
			.Returns(expectedMessage)
			.Verifiable();

		var uut = new NextLoginViewModel(serviceMock.Object);
		uut.Username = username;
		uut.TemporaryLogin();

		serviceMock.Verify();
		AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		AssertEquals("Warning Message", string.Empty, uut.WarningMessage);
		AssertEquals("Info Message", expectedMessage, uut.InfoMessage);
	}

	public void TestCancelOIDCLogin()
	{
		var expectedMessage = "Authentication operation was canceled, please try again.";
		var serviceMock = new Mock<ILoginService>();
		serviceMock.SetupGet(s => s.IsSupportLoginEnabled).Returns(true);

		var uut = new NextLoginViewModel(serviceMock.Object);
		uut.CancelOIDCLogin();

		Assert("User/Password login disabled", !uut.IsUserPasswordLogin);
		Assert("OIDC Login button visible", uut.IsOIDCButtonVisible);
		Assert("Support login button visible", uut.IsSupportLoginButtonVisible);
		AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		AssertEquals("Warning Message", expectedMessage, uut.WarningMessage);
		AssertEquals("Info Message", string.Empty, uut.InfoMessage);
	}

	public void TestGoBack()
	{
		var expectedMessage = "Authentication operation was canceled, please try again.";
		var serviceMock = new Mock<ILoginService>();
		serviceMock.SetupGet(s => s.IsSupportLoginEnabled).Returns(true);

		var uut = new NextLoginViewModel(serviceMock.Object);
		uut.GoBack();

		Assert("User/Password login disabled", !uut.IsUserPasswordLogin);
		Assert("OIDC Login button visible", uut.IsOIDCButtonVisible);
		Assert("Support login button visible", uut.IsSupportLoginButtonVisible);
		AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		AssertEquals("Warning Message", expectedMessage, uut.WarningMessage);
		AssertEquals("Info Message", string.Empty, uut.InfoMessage);
	}
}
