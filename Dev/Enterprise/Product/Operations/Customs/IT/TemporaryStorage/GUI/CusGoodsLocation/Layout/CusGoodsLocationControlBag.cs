using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class CusGoodsLocationControlBag : ControlBag
{
	public CusGoodsLocationControlBag()
	{
		AdditionalIdentifierDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.AdditionalIdentifierDropEdit));
	}

	public static CusGoodsLocationControlBag Instance => instance ??= new CusGoodsLocationControlBag();

	[ThreadStatic]
	static CusGoodsLocationControlBag instance;

	public ControlReference AdditionalIdentifierDropEdit { get; }

	protected override Control CreateTemplate() => new CusGoodsLocationUserControl();
}
