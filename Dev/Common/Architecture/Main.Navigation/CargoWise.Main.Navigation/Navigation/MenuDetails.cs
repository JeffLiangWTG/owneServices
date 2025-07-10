using System.Collections.ObjectModel;

namespace CargoWise.Main.Navigation;

#nullable disable
public class MenuDetails : IMenuSection
{
	public MenuDetails(int index)
	{
		Index = index;
	}

	public string Name { get; set; }
	public string DisplayName { get; set; }
	public int Index { get; set; }
	public ObservableCollection<IMenuSection> Subsections { get; set; }
	public bool IsSelected { get; set; }
	public string Letter { get; private set; }
}
