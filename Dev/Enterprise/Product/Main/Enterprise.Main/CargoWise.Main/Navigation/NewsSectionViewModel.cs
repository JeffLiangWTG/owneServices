using Enterprise.Registry.Business;

namespace CargoWise.Main.Navigation;

public class NewsSectionViewModel : ViewModelBase
#if WINZOR
	, ViewModels.INewsSectionViewModel
#endif
{
	public NewsSectionViewModel(NewsSection section)
	{
		SectionID = section.SectionID;
		SectionName = section.SectionName;
	}

	public NewsSectionViewModel(string sectionID, string sectionName)
	{
		this.SectionID = sectionID;
		this.SectionName = sectionName;
	}

	public string SectionID { get; }
	public string SectionName { get; }

	bool isSelected;
	public bool IsSelected
	{
		get => isSelected;
		set
		{
			if (value == isSelected)
			{
				return;
			}
			isSelected = value;
			OnPropertyChanged();
		}
	}
}
