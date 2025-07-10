using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Main.Navigation.Pages;

public class LoginLocationData
{
	public string? Title { get; set; }
	public string? BackButtonCaption { get; set; }
	public string? LoginButtonCaption { get; set; }

	public string? CompanyLabel { get; set; }
	public ICompany? Company { get; set; }
	public IEnumerable<ICompany> Companies { get; set; } = [];
	public string? BranchLabel { get; set; }
	public IBranch? Branch { get; set; }
	public IEnumerable<IBranch> Branches { get; set; } = [];
	public string? DepartmentLabel { get; set; }
	public IDepartment? Department { get; set; }
	public IEnumerable<IDepartment> Departments { get; set; } = [];
	public string? ErrorMessage { get; set; }
}
