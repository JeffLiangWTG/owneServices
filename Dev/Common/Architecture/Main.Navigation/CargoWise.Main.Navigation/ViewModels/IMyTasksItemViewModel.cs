namespace CargoWise.Main.Navigation.ViewModels;

public interface IMyTasksItemViewModel
{
	public string ParentId { get; }
	public string ParentSummary { get; }
	public string Status { get; }
	public string Description { get; }
	public string StatusText { get; }

	public ClickCommand LinkAction { get; set; }
}
