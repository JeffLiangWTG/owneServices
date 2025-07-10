using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class GoodsItemDifferencesDetailsColumnControlBag : ControlBag
{
	GoodsItemDifferencesDetailsColumnControlBag()
	{
		DeclaredCommodityCodeCodeFindBox = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.DeclaredCommodityCodeCodeFindBox));
		UnloadedCommodityCodeCodeFindBox = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.UnloadedCommodityCodeCodeFindBox));
		UnloadingRemarkCodeDropEdit = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.UnloadingRemarkCodeDropEdit));
		UnloadingRemarkTextTextBox = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.UnloadingRemarkTextTextBox));
	}

	public static GoodsItemDifferencesDetailsColumnControlBag Instance => instance ?? (instance = new GoodsItemDifferencesDetailsColumnControlBag());
	[ThreadStatic]
	static GoodsItemDifferencesDetailsColumnControlBag instance;

	public ControlReference DeclaredCommodityCodeCodeFindBox { get; }
	public ControlReference UnloadedCommodityCodeCodeFindBox { get; }
	public ControlReference UnloadingRemarkCodeDropEdit { get; }
	public ControlReference UnloadingRemarkTextTextBox { get; }

	protected override Control CreateTemplate() => new GoodsItemDifferencesDetailsColumnUserControl();
}
