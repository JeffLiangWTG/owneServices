#if !WINZOR
using System.Windows;
using System.Windows.Controls;
using static CargoWise.Main.Navigation.SnapshotsControl;

namespace CargoWise.Main.Navigation;

public class SnapshotTemplateSelector : DataTemplateSelector
{
	public DataTemplate SnapshotTemplate { get; set; }
	public DataTemplate AddSnapshotTemplate { get; set; }

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		return item is AddSnapshotPlaceholder
			? AddSnapshotTemplate
			: SnapshotTemplate;
	}
}
#endif
