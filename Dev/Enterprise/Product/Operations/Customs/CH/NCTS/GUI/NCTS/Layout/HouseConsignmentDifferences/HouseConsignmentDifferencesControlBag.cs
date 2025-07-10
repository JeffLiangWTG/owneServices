using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class HouseConsignmentDifferencesControlBag : ControlBag
{
	HouseConsignmentDifferencesControlBag()
	{
		UnloadingRemarkCodeDropEdit = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.UnloadingRemarkCodeDropEdit));
		UnloadingRemarkTextTextBox = RegisterControl(nameof(GoodsItemDifferencesDetailsColumnUserControl.UnloadingRemarkTextTextBox));
	}

	public static HouseConsignmentDifferencesControlBag Instance => instance ?? (instance = new HouseConsignmentDifferencesControlBag());

	[ThreadStatic]
	static HouseConsignmentDifferencesControlBag instance;

	public ControlReference UnloadingRemarkCodeDropEdit { get; }

	public ControlReference UnloadingRemarkTextTextBox { get; }

	protected override Control CreateTemplate() => new HouseConsignmentDifferencesUserControl();
}
