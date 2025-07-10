using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class DeclarationDetailsControlBag : ControlBag
{
	public DeclarationDetailsControlBag()
	{
		ReleaseCodeTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.ReleaseCodeTextBox));
		ReleaseDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ReleaseDateEdit));
		WriteOffDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.WriteOffDateEdit));
		ControlChannelDropEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ControlChannelDropEdit));
	}

	public static DeclarationDetailsControlBag Instance => instance ??= new DeclarationDetailsControlBag();

	[ThreadStatic]
	static DeclarationDetailsControlBag instance;

	public ControlReference ReleaseCodeTextBox { get; }

	public ControlReference ReleaseDateEdit { get; }

	public ControlReference WriteOffDateEdit { get; }

	public ControlReference ControlChannelDropEdit { get; }

	protected override Control CreateTemplate() => new DeclarationDetailsUserControl();
}
