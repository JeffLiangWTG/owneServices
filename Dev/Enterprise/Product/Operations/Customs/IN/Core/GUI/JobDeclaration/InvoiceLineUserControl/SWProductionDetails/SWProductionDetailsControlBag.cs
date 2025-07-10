using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class SWProductionDetailsControlBag : ControlBag
{
	public static SWProductionDetailsControlBag Instance => instance ??= new SWProductionDetailsControlBag();

	[ThreadStatic]
	static SWProductionDetailsControlBag instance;

	public SWProductionDetailsControlBag()
	{
		BatchIDTextBox = RegisterControl(nameof(BatchIDTextBox));
		BatchQuantityDropEdit = RegisterControl(nameof(BatchQuantityDropEdit));
		ManufacturingDateEdit = RegisterControl(nameof(ManufacturingDateEdit));
		ExpiryDateEdit = RegisterControl(nameof(ExpiryDateEdit));
		BestBeforeDateTimeOffsetEdit = RegisterControl(nameof(BestBeforeDateTimeOffsetEdit));
	}

	protected override Control CreateTemplate() => new SWProductionDetailsUserControl();

	public ControlReference BatchIDTextBox;
	public ControlReference BatchQuantityDropEdit;
	public ControlReference ManufacturingDateEdit;
	public ControlReference ExpiryDateEdit;
	public ControlReference BestBeforeDateTimeOffsetEdit;
}
