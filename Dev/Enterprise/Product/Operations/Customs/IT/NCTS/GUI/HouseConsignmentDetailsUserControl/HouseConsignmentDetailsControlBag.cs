using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class HouseConsignmentDetailsControlBag : ControlBag
{
	public HouseConsignmentDetailsControlBag()
	{
		CustomsStatusUserControl = RegisterControl(nameof(HouseConsignmentDetailsControlBag.CustomsStatusUserControl));
	}

	protected override Control CreateTemplate() => new HouseConsignmentDetailsUserControl();

	public static HouseConsignmentDetailsControlBag Instance => instance ??= new HouseConsignmentDetailsControlBag();

	[ThreadStatic]
	static HouseConsignmentDetailsControlBag instance;

	public ControlReference CustomsStatusUserControl { get; }
}
