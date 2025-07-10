using System.Threading.Tasks;
using CargoWise.Main.Navigation.Pages;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Main.Navigation;

public interface INextLoginLocationService
{
	Task<LoginLocationData> GetLoginLocationData();
	Task Login();
	Task ShowLoginUserControl();
	Task UpdateBranch(IBranch branch);
	Task UpdateCompany(ICompany company);
	Task UpdateDepartment(IDepartment department);
}
