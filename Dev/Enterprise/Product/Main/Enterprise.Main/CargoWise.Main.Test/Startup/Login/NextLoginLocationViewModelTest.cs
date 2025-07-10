using System.Collections.Generic;
using CargoWise.Main.Startup.Login;
using Enterprise.Startup;
using Enterprise.Startup.Login.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.Login;

class NextLoginLocationViewModelTest : TestCaseWithLoginDirectorAndMainForm
{
	[RequiresSTA]
	public void TestLogin_WhenSuccessful()
	{
		var branchCode = EnvProxy.Instance.CurrentBranch.Code;
		var departmentCode = EnvProxy.Instance.CurrentDepartment.Code;

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(EnvProxy.Instance.CurrentUser);

		var mockLoginDirector = new Mock<LoginDirector>();
		mockLoginDirector.Setup((m) => m.LoginLocationInteractive(
			It.Is<string>(p => p == branchCode),
			It.Is<string>(p => p == departmentCode))).Returns(info).Verifiable();

		using (LoginDirector.UseTestInstance(mockLoginDirector.Object))
		{
			var uut = new NextLoginLocationViewModel(new LoginService());

			uut.Login();

			mockLoginDirector.Verify();
			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		}
	}

	[RequiresSTA]
	public void TestLogin_WhenInvalidCompany()
	{
		var companyMock = new Mock<ICompany>();
		companyMock.SetupGet(m => m.Code).Returns(string.Empty);

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(EnvProxy.Instance.CurrentUser);

		using (LoginDirector.UseTestInstance())
		{
			var uut = new NextLoginLocationViewModel(new LoginService());
			uut.Company = companyMock.Object;

			uut.Login();

			AssertContains("Error Message - Company", "Error - Please enter a Company.", uut.ErrorMessage);
			AssertContains("Error Message - Branch", "Error - Please enter a Branch.", uut.ErrorMessage);
		}
	}

	[RequiresSTA]
	public void TestLogin_WhenInvalidBranch()
	{
		var branchMock = new Mock<IBranch>();
		branchMock.SetupGet(m => m.Code).Returns(string.Empty);

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(EnvProxy.Instance.CurrentUser);

		using (LoginDirector.UseTestInstance())
		{
			var uut = new NextLoginLocationViewModel(new LoginService());
			uut.Branch = branchMock.Object;

			uut.Login();

			AssertEquals("Error Message", "Error - Please enter a Branch.", uut.ErrorMessage);
		}
	}

	[RequiresSTA]
	public void TestLogin_WhenInvalidDepartment()
	{
		var departmentMock = new Mock<IDepartment>();
		departmentMock.SetupGet(m => m.Code).Returns(string.Empty);

		var info = LoginAuthenticationInfo.NewSuccessfulLogin(EnvProxy.Instance.CurrentUser);

		using (LoginDirector.UseTestInstance())
		{
			var uut = new NextLoginLocationViewModel(new LoginService());
			uut.Department = departmentMock.Object;

			uut.Login();

			AssertEquals("Error Message", "Error - Please enter a Department.", uut.ErrorMessage);
		}
	}

	[RequiresSTA]
	public void TestShowLoginUserControl()
	{
		var serviceMock = new Mock<ILoginService>();
		serviceMock.Setup(m => m.ShowLoginUserControl()).Verifiable();

		using (LoginDirector.UseTestInstance())
		{
			var uut = new NextLoginLocationViewModel(serviceMock.Object);

			uut.ShowLoginUserControl();

			serviceMock.Verify();
			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);
		}
	}

	[RequiresSTA]
	public void TestPropertyChanged()
	{
		var companyMock = new Mock<ICompany>();
		companyMock.SetupGet(m => m.Code).Returns("EDI");

		var branchMock = new Mock<IBranch>();
		branchMock.SetupGet(m => m.Code).Returns("BNE");

		var departmentMock = new Mock<IDepartment>();
		departmentMock.SetupGet(m => m.Code).Returns("TLA");

		var properties = new List<string>();
		var uut = new NextLoginLocationViewModel(new LoginService());
		uut.PropertyChanged += (sender, args) => { properties.Add(args.PropertyName); };

		AssertEquals("Initial properties", 0, properties.Count);

		uut.Company = companyMock.Object;
		uut.Branch = branchMock.Object;
		uut.Department = departmentMock.Object;

		AssertEquals("Properties before login attempt", 5, properties.Count);
		AssertEquals("Company", properties[0]);
		AssertEquals("Branch", properties[1]);
		AssertEquals("Branches", properties[2]);
		AssertEquals("Branch", properties[3]);
		AssertEquals("Department", properties[4]);
	}
}
