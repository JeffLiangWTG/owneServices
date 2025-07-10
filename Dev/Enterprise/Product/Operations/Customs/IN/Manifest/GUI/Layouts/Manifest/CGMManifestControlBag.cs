using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

sealed class CGMManifestControlBag : ControlBag
{
	CGMManifestControlBag()
	{
		ImportGeneralManifestNumberTextBox = RegisterControl(nameof(CGMManifestUserControl.ImportGeneralManifestNumberTextBox));
		ImportGeneralManifestDateEdit = RegisterControl(nameof(CGMManifestUserControl.ImportGeneralManifestDateEdit));
		GrossWeightCalcDropEdit = RegisterControl(nameof(CGMManifestUserControl.GrossWeightCalcDropEdit));
		ManifestQtyCalcDropEdit = RegisterControl(nameof(CGMManifestUserControl.ManifestQtyCalcDropEdit));
		MessageAndCustomsStatusWithOverrideUserControl = RegisterControl(nameof(CGMManifestUserControl.MessageAndCustomsStatusWithOverrideUserControl));
		ActionDropEdit = RegisterControl(nameof(CGMManifestUserControl.ActionDropEdit));
	}

	public static CGMManifestControlBag Instance => instance ??= new CGMManifestControlBag();

	[ThreadStatic]
	static CGMManifestControlBag instance;

	protected override Control CreateTemplate() => new CGMManifestUserControl();

	public ControlReference ImportGeneralManifestNumberTextBox { get; }
	public ControlReference ImportGeneralManifestDateEdit { get; }
	public ControlReference GrossWeightCalcDropEdit { get; }
	public ControlReference ManifestQtyCalcDropEdit { get; }
	public ControlReference MessageAndCustomsStatusWithOverrideUserControl { get; }
	public ControlReference ActionDropEdit { get; }
}
