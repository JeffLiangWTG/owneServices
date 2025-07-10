namespace CargoWise.Windows.UI.Layout
{
	public interface ISplitterLayoutSaveProvider
	{
		int SplitterPosition { get; set; }
		int ContainerSize { get; }
		bool IsSplitterFixed { get; }
		bool IsLayoutRestored { get; set; }
	}
}
