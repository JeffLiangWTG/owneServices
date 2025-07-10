namespace CargoWise.Main.Navigation.ViewModels;

public interface INewsSectionViewModel
{
	string SectionID { get; }

	string SectionName { get; }

	bool IsSelected { get; set; }
}
