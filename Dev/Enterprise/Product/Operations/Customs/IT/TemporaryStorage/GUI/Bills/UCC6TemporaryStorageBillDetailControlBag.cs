using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStorageBillDetailControlBag : ControlBag
{
	public UCC6TemporaryStorageBillDetailControlBag()
	{
		GoodsDescTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.GoodsDescTextBox));
		LrnTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.LrnTextBox));
		MrnTextBox = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.MrnTextBox));
		GrossWeightCalcEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.GrossWeightCalcEdit));
		NetWeightCalcEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.NetWeightCalcEdit));
		SuppQtyCalcEdit = RegisterControl(nameof(UCC6TemporaryStorageBillDetailControl.SuppQtyCalcEdit));
	}

	public ControlReference LrnTextBox { get; }

	public ControlReference MrnTextBox { get; }

	public ControlReference GrossWeightCalcEdit { get; }

	public ControlReference NetWeightCalcEdit { get; }

	public ControlReference SuppQtyCalcEdit { get; }

	public ControlReference GoodsDescTextBox { get; }

	public static UCC6TemporaryStorageBillDetailControlBag Instance => uCC6TemporaryStorageBillDetailControlBag.Value;

	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	static readonly Lazy<UCC6TemporaryStorageBillDetailControlBag> uCC6TemporaryStorageBillDetailControlBag = new Lazy<UCC6TemporaryStorageBillDetailControlBag>(() => new UCC6TemporaryStorageBillDetailControlBag());

	protected override Control CreateTemplate() => new UCC6TemporaryStorageBillDetailControl();
}
