using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class EntryInstructionGuaranteesUserControl : EU.GUI.EntryInstructionGuaranteesUserControl
{
	public EntryInstructionGuaranteesUserControl()
	{
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new EntryInstructionGuaranteeGridColumnsLayout();
}
