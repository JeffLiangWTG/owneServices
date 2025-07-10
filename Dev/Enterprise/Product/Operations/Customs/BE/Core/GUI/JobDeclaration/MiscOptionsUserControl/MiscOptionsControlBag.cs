using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class MiscOptionsControlBag : ControlBag
{
	public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	public MiscOptionsControlBag() : base()
	{
		VATDeferTypeDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATDeferTypeDropEdit));
	}

	public ControlReference VATDeferTypeDropEdit { get; }

	protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
}
