using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class EntryInstructionsDetailsControlBag : ControlBag
{
	public EntryInstructionsDetailsControlBag()
	{
		ReasonDropEdit = RegisterControl(nameof(EntryInstructionDetailPanelUserControl.DeclarationReasonDropEdit));
		PartialDeliveryCheckBox = RegisterControl(nameof(EntryInstructionDetailPanelUserControl.PartialDeliveryCheckBox));
		TransportChargesMethodOfPaymentDropEdit = RegisterControl(nameof(EntryInstructionDetailPanelUserControl.TransportChargesMethodOfPaymentDropEdit));
		ProcedureCodeDropEdit = RegisterControl(nameof(EntryInstructionDetailPanelUserControl.ProcedureCodeDropEdit));
		NextProcedureDropEdit = RegisterControl(nameof(EntryInstructionDetailPanelUserControl.NextProcedureDropEdit));
	}

	public static EntryInstructionsDetailsControlBag Instance => instance ?? (instance = new EntryInstructionsDetailsControlBag());

	[ThreadStatic]
	static EntryInstructionsDetailsControlBag instance;

	protected override Control CreateTemplate() => new EntryInstructionDetailPanelUserControl();

	public ControlReference ReasonDropEdit { get; }

	public ControlReference PartialDeliveryCheckBox { get; }

	public ControlReference TransportChargesMethodOfPaymentDropEdit { get; }

	public ControlReference ProcedureCodeDropEdit { get; }

	public ControlReference NextProcedureDropEdit { get; }
}
