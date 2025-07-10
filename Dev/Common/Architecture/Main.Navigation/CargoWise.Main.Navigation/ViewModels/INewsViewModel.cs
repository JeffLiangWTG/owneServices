using System.Collections.ObjectModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation.ViewModels;

[CodeAlive("Used in CargoWise.GUI.Tilebar.Winzor project")]
public interface INewsViewModel
{
	ObservableCollection<INewsItemViewModel> FilteredNewsItems { get; set; }

	public INewsSectionViewModel SelectedSection { get; set; }

	public ObservableCollection<INewsSectionViewModel> Sections { get; set; }

	void ExecuteShowItemCommand(object parameter);

	string NameAll { get; }
}
