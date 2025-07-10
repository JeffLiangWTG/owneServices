using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class EntryInstructionDetailsControlBag : ControlBag
{
	public static EntryInstructionDetailsControlBag Instance => instance ??= new EntryInstructionDetailsControlBag();

	[ThreadStatic]
	static EntryInstructionDetailsControlBag instance;

	EntryInstructionDetailsControlBag()
	{
		TradeTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.TradeTypeDropEdit));
		DeclarationPurposeDropEdit = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.DeclarationPurposeDropEdit));
		DeclarationPurposeDetailsTextBox = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.DeclarationPurposeDetailsTextBox));
		ToWarehouseLabel = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.ToWarehouseLabel));
		ToWarehouseUserControl = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.ToWarehouseUserControl));
		FromWarehouseLabel = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.FromWarehouseLabel));
		FromWarehouseUserControl = RegisterControl(nameof(EntryInstructionDetailsLayoutUserControl.FromWarehouseUserControl));
	}

	protected override Control CreateTemplate() => new EntryInstructionDetailsLayoutUserControl();

	public ControlReference TradeTypeDropEdit { get; }

	public ControlReference DeclarationPurposeDropEdit { get; }

	public ControlReference DeclarationPurposeDetailsTextBox { get; }

	public ControlReference ToWarehouseLabel { get; }

	public ControlReference ToWarehouseUserControl { get; }

	public ControlReference FromWarehouseLabel { get; }

	public ControlReference FromWarehouseUserControl { get; }
}
