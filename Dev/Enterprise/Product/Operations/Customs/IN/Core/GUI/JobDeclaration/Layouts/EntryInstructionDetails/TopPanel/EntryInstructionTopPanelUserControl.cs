using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class EntryInstructionTopPanelUserControl : BaseCustomsEntryUserControl
{
	public EntryInstructionTopPanelUserControl()
	{
		InitializeComponent();
		EntryInstructionGrid.ApplyGridColumnLayout(new EntryInstructionGridColumnsLayout());
		MessageAndCustomsStatusWithOverridePanel.UpdateLayout(new MessageAndCustomsStatusWithOverrideLayout());
	}

	protected override void ChangeGridColumnsVisibility()
	{
		var isExport = JobDeclaration?.IsExport ?? false;
		var isImport = JobDeclaration?.IsImport ?? false;
		EntryInstructionGrid.SetAvailability(isExport, [CusEntryInstruction.Schema.CEI_Style, CusEntryInstruction.Schema.CEI_SubStyle, CusEntryInstruction.Schema.ShippingBillNumber, CusEntryInstruction.Schema.ShippingBillDate]);
		EntryInstructionGrid.SetAvailability(isExport || isImport, [CusEntryInstruction.Schema.LocalReferenceNumber, CusEntryInstruction.Schema.LocalReferenceNumberDate]);
	}
}
