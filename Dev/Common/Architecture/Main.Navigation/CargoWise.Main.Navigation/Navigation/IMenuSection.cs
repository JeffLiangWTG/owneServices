using System.Collections.ObjectModel;

namespace CargoWise.Main.Navigation;
public interface IMenuSection
{
	string Name { get; set; }
	string DisplayName { get; }
	int Index { get; }
	ObservableCollection<IMenuSection> Subsections { get; }
	bool IsSelected { get; set; }
	string Letter { get; }
}
