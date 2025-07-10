using System.Threading.Tasks;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Main.Navigation;

public class NextLoginLocationService : INextLoginLocationService
{
	readonly INextLoginLocationViewModel viewModel;
	readonly IWinzorControl winzorControl;

	public NextLoginLocationService(INextLoginLocationViewModel viewModel, NextLoginLocationControl winzorControl)
	{
		this.viewModel = viewModel;
		this.winzorControl = winzorControl;
	}

	public async Task<LoginLocationData> GetLoginLocationData()
	{
		var data = new LoginLocationData();
		await winzorControl.InvokeAsync(() =>
		{
			data.CompanyLabel = viewModel.CompanyLabel;
			data.Company = viewModel.Company;
			data.Companies = viewModel.Companies;

			data.BranchLabel = viewModel.BranchLabel;
			data.Branch = viewModel.Branch;
			data.Branches = viewModel.Branches;

			data.DepartmentLabel = viewModel.DepartmentLabel;
			data.Department = viewModel.Department;
			data.Departments = viewModel.Departments;

			data.BackButtonCaption = viewModel.BackButtonCaption;
			data.LoginButtonCaption = viewModel.LoginButtonCaption;

			data.Title = viewModel.Title;
			data.ErrorMessage = viewModel.ErrorMessage;
		});

		return data;
	}

	public Task Login() => winzorControl.InvokeAsync(() => viewModel.Login());

	public Task ShowLoginUserControl() => winzorControl.InvokeAsync(() => viewModel.ShowLoginUserControl());

	public Task UpdateBranch(IBranch branch) => winzorControl.InvokeAsync(() => viewModel.Branch = branch);

	public Task UpdateCompany(ICompany company) => winzorControl.InvokeAsync(() => viewModel.Company = company);

	public Task UpdateDepartment(IDepartment department) => winzorControl.InvokeAsync(() => viewModel.Department = department);
}
