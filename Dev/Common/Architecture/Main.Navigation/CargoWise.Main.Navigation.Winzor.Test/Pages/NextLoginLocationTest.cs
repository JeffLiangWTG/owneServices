using System.Collections.ObjectModel;
using System.Windows.Forms;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Startup.Login;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Main.Navigation.Winzor.Test
{
	class NextLoginLocationTest
	{
		[Test, WithPlaywrightPage]
		public async Task PressEnterKeyTestAsync()
		{
			await using var ctx = new InMemoryAppServerTestContext();
			var page = await ctx.LoadFormAsync(() =>
			{
				var form = new Form();
				var loginLocationControl = new NextLoginLocationControl()
				{
					Dock = DockStyle.Fill,
					NextLoginLocationViewModel = CreateNextLoginViewModel()
				};

				form.Width = 2000;
				form.Height = 2000;
				form.Controls.Add(loginLocationControl);
				return form;
			});

			await page.Locator(".cwn-login-location").WaitForAsync();
			Assert.That(
				async () => await page.Locator(".cwn-login-location__item").CountAsync(),
				Is.EqualTo(3).After(3000, 300));

			var inputCollection = await page.Locator(".cwn-dropdownlist__selected-key").AllAsync();
			var loginButton = page.Locator(".cwn-login-location__loginbutton");

			Assert.That(
				async () => await inputCollection[0].EvaluateAsync<bool>("node => document.activeElement == node"),
				Is.EqualTo(true).After(3000, 300));

			await page.Keyboard.PressAsync("Enter");
			Assert.That(
				async () => await inputCollection[1].EvaluateAsync<bool>("node => document.activeElement == node"),
				Is.EqualTo(true).After(3000, 300));

			await page.Keyboard.PressAsync("Enter");
			Assert.That(
				async () => await inputCollection[2].EvaluateAsync<bool>("node => document.activeElement == node"),
				Is.EqualTo(true).After(3000, 300));

			await page.Keyboard.PressAsync("Enter");
			Assert.That(
				async () => await loginButton.EvaluateAsync<bool>("node => document.activeElement == node"),
				Is.EqualTo(true).After(3000, 300));
		}

		INextLoginLocationViewModel CreateNextLoginViewModel()
		{
			var mockCompany = new Mock<ICompany>();
			var mockBranch = new Mock<IBranch>();
			var mockDepartment = new Mock<IDepartment>();
			mockCompany.SetupGet(company => company.Code).Returns("AAA");
			mockCompany.SetupGet(company => company.Name).Returns("Test Company");
			mockBranch.SetupGet(branch => branch.Code).Returns("BBB");
			mockBranch.SetupGet(branch => branch.Name).Returns("Test Branch");
			mockDepartment.SetupGet(department => department.Code).Returns("CCC");
			mockDepartment.SetupGet(department => department.Description).Returns("Test Department");

			var mockCompanyCollection = new ObservableCollection<ICompany>();
			var mockBranchCollection = new ObservableCollection<IBranch>();
			var mockDepartmentCollection = new ObservableCollection<IDepartment>();
			mockCompanyCollection.Add(mockCompany.Object);
			mockBranchCollection.Add(mockBranch.Object);
			mockDepartmentCollection.Add(mockDepartment.Object);

			var mockLoginService = new Mock<ILoginService>();
			mockLoginService.SetupGet(service => service.Company).Returns(mockCompany.Object);
			mockLoginService.SetupGet(service => service.Companies).Returns(mockCompanyCollection);
			mockLoginService.SetupGet(service => service.Branch).Returns(mockBranch.Object);
			mockLoginService.SetupGet(service => service.Branches).Returns(mockBranchCollection);
			mockLoginService.SetupGet(service => service.Department).Returns(mockDepartment.Object);
			mockLoginService.SetupGet(service => service.Departments).Returns(mockDepartmentCollection);

			return new NextLoginLocationViewModel(mockLoginService.Object);
		}
	}
}
