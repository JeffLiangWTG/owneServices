using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
#if WINZOR
using CargoWise.Main.Navigation.ViewModels;
#endif

namespace CargoWise.Main.Startup.Login;

public class NextLoginLocationViewModel : INotifyPropertyChanged
#if WINZOR
	, INextLoginLocationViewModel
#endif
{
	ILoginService LoginService { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	public NextLoginLocationViewModel(ILoginService loginService)
	{
		LoginService = loginService;
	}

	public MultilingualString Title => ResString.GetMultilingualString("CWNext|LoginLocation|001cb29d-a726-4fc4-bc9f-d9a12e616628", "Company, Branch and Department");
	public MultilingualString CompanyLabel => ResString.GetMultilingualString("CWNext|LoginLocation|002cb29d-a726-4fc4-bc9f-d9a12e616628", "Company");
	public MultilingualString BranchLabel => ResString.GetMultilingualString("CWNext|LoginLocation|003cb29d-a726-4fc4-bc9f-d9a12e616628", "Branch");
	public MultilingualString DepartmentLabel => ResString.GetMultilingualString("CWNext|LoginLocation|004cb29d-a726-4fc4-bc9f-d9a12e616628", "Department");
	public MultilingualString LoginButtonCaption => ResString.GetMultilingualString("CWNext|Login|005b7791-85fb-4496-9db6-04b00901c206", "Login");
	public MultilingualString BackButtonCaption => ResString.GetMultilingualString("CWNext|Login|006b7791-85fb-4496-9db6-04b00901c206", "Back");

	public ICompany Company
	{
		get => LoginService.Company;
		set
		{
			if (LoginService.Company != value)
			{
				LoginService.Company = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Branch));
				NotifyPropertyChanged(nameof(Branches));
			}
		}
	}
	public ObservableCollection<ICompany> Companies => LoginService.Companies;

	public IBranch Branch
	{
		get => LoginService.Branch;
		set
		{
			if (LoginService.Branch != value)
			{
				LoginService.Branch = value;
				NotifyPropertyChanged();
			}
		}
	}
	public ObservableCollection<IBranch> Branches => LoginService.Branches;

	public IDepartment Department
	{
		get => LoginService.Department;
		set
		{
			if (LoginService.Department != value)
			{
				LoginService.Department = value;
				NotifyPropertyChanged();
			}
		}
	}
	public ObservableCollection<IDepartment> Departments => LoginService.Departments;

	string errorMessage = string.Empty;

	public string ErrorMessage
	{
		get => errorMessage;
		private set
		{
			errorMessage = value;
			NotifyPropertyChanged();
		}
	}

	public void ShowLoginUserControl() => LoginService.ShowLoginUserControl();

	public void Login()
	{
		ErrorMessage = LoginService.LoginLocationInteractive();
	}

	void NotifyPropertyChanged([CallerMemberName] string property = null)
	{
		if (!string.IsNullOrEmpty(property))
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
