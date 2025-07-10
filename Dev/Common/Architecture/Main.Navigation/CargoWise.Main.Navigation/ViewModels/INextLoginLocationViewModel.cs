using System.Collections.ObjectModel;
using System.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation.ViewModels;

[CodeAlive("Used in CargoWise.GUI.Tilebar.Winzor project")]
public interface INextLoginLocationViewModel
{
	MultilingualString BackButtonCaption { get; }
	IBranch Branch { get; set; }
	ObservableCollection<IBranch> Branches { get; }
	MultilingualString BranchLabel { get; }
	ObservableCollection<ICompany> Companies { get; }
	ICompany Company { get; set; }
	MultilingualString CompanyLabel { get; }
	IDepartment Department { get; set; }
	MultilingualString DepartmentLabel { get; }
	ObservableCollection<IDepartment> Departments { get; }
	string ErrorMessage { get; }
	MultilingualString LoginButtonCaption { get; }
	MultilingualString Title { get; }

	event PropertyChangedEventHandler PropertyChanged;

	void Login();
	void ShowLoginUserControl();
}
