using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class CusGoodsLocationControlBag : ControlBag
{
	public CusGoodsLocationControlBag()
	{
		AdditionalIdentifierDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.AdditionalIdentifierDropEdit));
		OrganizationAddressControl = RegisterControl(nameof(CusGoodsLocationUserControl.OrganizationAddressControl));
		OverrideCheckBox = RegisterControl(nameof(CusGoodsLocationUserControl.OverrideCheckBox));
	}

	public static CusGoodsLocationControlBag Instance => instance ?? (instance = new CusGoodsLocationControlBag());

	[ThreadStatic]
	static CusGoodsLocationControlBag instance;

	public ControlReference AdditionalIdentifierDropEdit { get; }
	public ControlReference OrganizationAddressControl { get; }
	public ControlReference OverrideCheckBox { get; }

	protected override Control CreateTemplate() => new CusGoodsLocationUserControl();
}
