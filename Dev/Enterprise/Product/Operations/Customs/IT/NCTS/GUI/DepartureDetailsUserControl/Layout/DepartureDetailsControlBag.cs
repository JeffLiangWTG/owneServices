using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class DepartureDetailsControlBag : ControlBag
{
	DepartureDetailsControlBag()
	{
		LocationOfGoodsUserControl = RegisterControl(nameof(DepartureDetailsUserControl.LocationOfGoodsUserControl));
	}

	public static DepartureDetailsControlBag Instance => instance ?? (instance = new DepartureDetailsControlBag());

	public ControlReference LocationOfGoodsUserControl { get; }

	protected override Control CreateTemplate() => new DepartureDetailsUserControl();

	[ThreadStatic]
	static DepartureDetailsControlBag instance;
}
