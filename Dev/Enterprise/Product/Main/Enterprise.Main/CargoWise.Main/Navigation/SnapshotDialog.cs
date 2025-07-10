#if !WINZOR
using System.Windows;

namespace CargoWise.Main.Navigation;

internal class SnapshotDialog : IDialog
{
	readonly MaskAdorner _mask;
	readonly SnapshotModuleAndLayout _content;
	readonly SnapshotsViewModel _viewModel;

	public SnapshotDialog(SnapshotsViewModel viewModel, UIElement parent)
	{
		_viewModel = viewModel;
		_content = new SnapshotModuleAndLayout { DataContext = _viewModel };
		_mask = MaskAdorner.Create(parent, _content, 400);
		_content.Dialog = this;
	}

	public void Show()
	{
		_mask.Show();
	}

	public void Close()
	{
		_mask.Close();
	}
}

internal interface IDialog
{
	void Show();
	void Close();
}
#endif
