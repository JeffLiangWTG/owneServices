using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemDetailsControlBag : ControlBag
{
	public GoodsItemDetailsControlBag()
	{
		HarmonisedTariffFindBox = RegisterControl(nameof(GoodsItemDetailsUserControl.HarmonisedTariffFindBox));
	}

	protected override Control CreateTemplate() => new GoodsItemDetailsUserControl();

	public static GoodsItemDetailsControlBag Instance => instance ?? (instance = new GoodsItemDetailsControlBag());

	[ThreadStatic]
	static GoodsItemDetailsControlBag instance;

	public ControlReference HarmonisedTariffFindBox { get; }
}
