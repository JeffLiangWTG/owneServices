using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class GoodsItemDetailsControlBag : ControlBag
{
	public GoodsItemDetailsControlBag()
	{
		CustomsStatusUserControl = RegisterControl(nameof(GoodsItemDetailsUserControl.CustomsStatusUserControl));
	}

	protected override Control CreateTemplate() => new GoodsItemDetailsUserControl();

	public static GoodsItemDetailsControlBag Instance => instance ??= new GoodsItemDetailsControlBag();

	[ThreadStatic]
	static GoodsItemDetailsControlBag instance;

	public ControlReference CustomsStatusUserControl { get; }
}
