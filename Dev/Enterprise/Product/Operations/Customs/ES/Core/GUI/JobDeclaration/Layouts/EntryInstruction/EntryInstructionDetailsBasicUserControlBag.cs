using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class EntryInstructionDetailsBasicUserControlBag : ControlBag
{
	protected override Control CreateTemplate() => new EntryInstructionDetailBasicUserControl();

	public static EntryInstructionDetailsBasicUserControlBag Instance => instance ??= new EntryInstructionDetailsBasicUserControlBag();

	[ThreadStatic]
	static EntryInstructionDetailsBasicUserControlBag instance;

	EntryInstructionDetailsBasicUserControlBag()
	{
		ActivateByOperatorCheckBox = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.ActivateByOperatorCheckBox));
		IncludeRoutingSecurityDataCheckBox = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.IncludeRoutingSecurityDataCheckBox));
		LocationOfGoodsUserControl = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.LocationOfGoodsUserControl));
		BondHolderOrganisationControl = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.BondHolderOrganisationControl));
		NewOwnerOrganisationControl = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.NewOwnerOrganisationControl));
		RemoverOrganisationControl = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.RemoverOrganisationControl));
		RequestLabel = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.RequestLabel));
		RequestTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.RequestTypeDropEdit));
		IndirectTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.IndirectTypeDropEdit));
		NationalCheckBox = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.NationalCheckBox));
		NumberOfDaysCalcEdit = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.NumberOfDaysCalcEdit));
		JustificationTextBox = RegisterControl(nameof(EntryInstructionDetailBasicUserControl.JustificationTextBox));
	}

	public ControlReference ActivateByOperatorCheckBox { get; }

	public ControlReference IncludeRoutingSecurityDataCheckBox { get; }

	public ControlReference LocationOfGoodsUserControl { get; }

	public ControlReference BondHolderOrganisationControl { get; }

	public ControlReference NewOwnerOrganisationControl { get; }

	public ControlReference RemoverOrganisationControl { get; }

	public ControlReference RequestLabel { get; }

	public ControlReference RequestTypeDropEdit { get; }

	public ControlReference IndirectTypeDropEdit { get; }

	public ControlReference NationalCheckBox { get; }

	public ControlReference NumberOfDaysCalcEdit { get; }

	public ControlReference JustificationTextBox { get; }
}
