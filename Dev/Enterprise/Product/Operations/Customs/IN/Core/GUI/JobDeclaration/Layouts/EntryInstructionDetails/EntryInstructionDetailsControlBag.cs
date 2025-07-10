using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class EntryInstructionDetailsControlBag : ControlBag
{
	public static EntryInstructionDetailsControlBag Instance => instance ?? (instance = new EntryInstructionDetailsControlBag());

	[ThreadStatic]
	static EntryInstructionDetailsControlBag instance;

	EntryInstructionDetailsControlBag()
	{
		RBIWaiverNumberTextBox = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.RBIWaiverNumberTextBox));
		RBIWaiverDateEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.RBIWaiverDateEdit));
		PackagesQtyCalcDropEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.PackagesQtyCalcDropEdit));
		LoosePackagesCalcDropEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.LoosePackagesCalcDropEdit));
		TotalContainerZIntEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.TotalContainerZIntEdit));
		ShippingBillOverrideUserControl = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.ShippingBillOverrideUserControl));
		TotalGrossWeightAndNetWeightGroupBox = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.TotalGrossWeightAndNetWeightGroupBox));
		MessageAndCustomsStatusGroupBox = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.MessageAndCustomsStatusGroupBox));
	}

	protected override Control CreateTemplate() => new EntryInstructionDetailsLayoutUserControl();

	public ControlReference RBIWaiverNumberTextBox { get; }
	public ControlReference RBIWaiverDateEdit { get; }
	public ControlReference PackagesQtyCalcDropEdit { get; }
	public ControlReference LoosePackagesCalcDropEdit { get; }
	public ControlReference TotalContainerZIntEdit { get; }
	public ControlReference ShippingBillOverrideUserControl { get; }
	public ControlReference TotalGrossWeightAndNetWeightGroupBox { get; }
	public ControlReference MessageAndCustomsStatusGroupBox { get; }
}
