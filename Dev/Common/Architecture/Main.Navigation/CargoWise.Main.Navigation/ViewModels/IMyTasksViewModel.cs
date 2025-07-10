using System.Collections.ObjectModel;
using System.Windows.Input;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation.ViewModels;

[CodeAlive("Used in CargoWise.Main.Navigation.Winzor project")]
public interface IMyTasksViewModel
{
	ObservableCollection<IMyTasksItemViewModel> MyTasks { get; set; }
	string NoMyTasksText { get; }
	string IdHeaderText { get; }
	string NameHeaderText { get; }
	string TaskDescriptionHeaderText { get; }
	string StatusHeaderText { get; }
	string DisplayName { get; }
	void Refresh();
	public ICommand OpenTaskFormCommand { get; }
}
