using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWiseNext.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Main.Navigation.Pages;

public partial class NextLoginLocation
{
	const string EnterKeyCode = $"Enter";

	[Parameter]
	public INextLoginLocationService? Service { get; set; }
	LoginLocationData Data { get; set; } = new();
	bool HasErrors => !string.IsNullOrEmpty(Data.ErrorMessage);
	bool IsCompanyDropdownOpen { get; set; }
	bool IsBranchDropdownOpen { get; set; }
	bool IsDepartmentDropdownOpen { get; set; }
	public SortedList<string, string> Companies => new(Data.Companies.ToDictionary(nav => nav.Code, nav => nav.Name));
	public SortedList<string, string> Branches => new(Data.Branches.ToDictionary(nav => nav.Code, nav => nav.Name));
	public SortedList<string, string> Departments => new(Data.Departments.ToDictionary(nav => nav.Code, nav => nav.Description));

	protected CwnDropDownList? companyDropDownList;
	protected CwnDropDownList? branchDropDownList;
	protected CwnDropDownList? departmentDropDownList;
	protected CwnButton? loginButton;

	protected override async Task OnInitializedAsync()
	{
		Data = await Service!.GetLoginLocationData();
		await base.OnInitializedAsync();
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && companyDropDownList != null)
		{
			await (companyDropDownList?.FocusAsync() ?? ValueTask.CompletedTask);
		}

		await base.OnAfterRenderAsync(firstRender);
	}

	void ToggleDropdown(string? dropDownName)
	{
		IsCompanyDropdownOpen = dropDownName == Data.CompanyLabel && !IsCompanyDropdownOpen;
		IsBranchDropdownOpen = dropDownName == Data.BranchLabel && !IsBranchDropdownOpen;
		IsDepartmentDropdownOpen = dropDownName == Data.DepartmentLabel && !IsDepartmentDropdownOpen;
	}

	void HandleFocusIn(string? dropDownName)
	{
		IsCompanyDropdownOpen = dropDownName == Data.CompanyLabel && IsCompanyDropdownOpen;
		IsBranchDropdownOpen = dropDownName == Data.BranchLabel && IsBranchDropdownOpen;
		IsDepartmentDropdownOpen = dropDownName == Data.DepartmentLabel && IsDepartmentDropdownOpen;
	}

	async Task OnBackButtonClickAsync(MouseEventArgs e)
	{
		await Service!.ShowLoginUserControl();
	}

	async Task OnLoginButtonClickAsync(MouseEventArgs e)
	{
		await Service!.Login();
		Data = await Service!.GetLoginLocationData();
	}

	async Task OnCompanyKeyPressAsync(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Code == EnterKeyCode)
		{
			await (branchDropDownList?.FocusAsync() ?? ValueTask.CompletedTask);
		}
	}

	async Task OnBranchKeyPressAsync(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Code == EnterKeyCode)
		{
			await (departmentDropDownList?.FocusAsync() ?? ValueTask.CompletedTask);
		}
	}

	async Task OnDepartmentKeyPressAsync(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Code == EnterKeyCode)
		{
			await (loginButton?.FocusAsync() ?? ValueTask.CompletedTask);
		}
	}

	async Task OnSelectedCompanyChangeAsync(ChangeEventArgs e)
	{
		var company = Data.Companies.FirstOrDefault(c => c.Code == e.Value!.ToString());
		if (company != null)
		{
			Data.Company = company;
			await Service!.UpdateCompany(company);
		}
		Data = await Service!.GetLoginLocationData();
	}

	async Task OnSelectedBranchChangeAsync(ChangeEventArgs e)
	{
		var branch = Data.Branches.FirstOrDefault(c => c.Code == e.Value!.ToString());
		if (branch != null)
		{
			Data.Branch = branch;
			await Service!.UpdateBranch(branch);
		}
		Data = await Service!.GetLoginLocationData();
	}

	async Task OnSelectedDepartmentChangeAsync(ChangeEventArgs e)
	{
		var department = Data.Departments.FirstOrDefault(c => c.Code == e.Value!.ToString());
		if (department != null)
		{
			Data.Department = department;
			await Service!.UpdateDepartment(department);
		}
		Data = await Service!.GetLoginLocationData();
	}

	void HandleComponentClick(WebMouseEventArgs e)
	{
		IsCompanyDropdownOpen = false;
		IsBranchDropdownOpen = false;
		IsDepartmentDropdownOpen = false;

		StateHasChanged();
	}
}
