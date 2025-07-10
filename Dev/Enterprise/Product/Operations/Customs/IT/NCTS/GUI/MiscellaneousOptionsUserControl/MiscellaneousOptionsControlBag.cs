using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class MiscellaneousOptionsControlBag : ControlBag
{
	public MiscellaneousOptionsControlBag()
	{
		CustomsProfileDropEdit = RegisterControl(nameof(MiscellaneousOptionsUserControl.CustomsProfileDropEdit));
	}

	public static MiscellaneousOptionsControlBag Instance => instance ?? (instance = new MiscellaneousOptionsControlBag());

	[ThreadStatic]
	static MiscellaneousOptionsControlBag instance;

	public ControlReference CustomsProfileDropEdit { get; }

	protected override Control CreateTemplate() => new MiscellaneousOptionsUserControl();
}
