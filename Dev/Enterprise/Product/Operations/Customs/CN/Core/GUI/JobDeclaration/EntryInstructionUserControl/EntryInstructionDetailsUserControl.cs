using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class EntryInstructionDetailsUserControl : ZUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();

			BindingSource.SetBindingMember(RemarksTextBox, nameof(CusEntryInstruction.CustomsMessageRemarks));
			OperationMattersUserControl.SetBindingMember(nameof(CusEntryInstruction.OperationMattersAsString), x => (x as CusEntryInstruction)?.OperationMatters);
			OtherPackagesUserControl.SetBindingMember(nameof(CusEntryInstruction.OtherPackagesAsString), x => (x as CusEntryInstruction)?.OtherPackages);
			SpecialBusinessIdentifiersUserControl.SetBindingMember(nameof(CusEntryInstruction.SpecialBusinessIdentifiersAsString), x => (x as CusEntryInstruction)?.SpecialBusinessIdentifiers);
		}
	}
}
