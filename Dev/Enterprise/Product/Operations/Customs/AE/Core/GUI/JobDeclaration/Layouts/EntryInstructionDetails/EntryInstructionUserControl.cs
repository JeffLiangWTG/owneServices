using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI;

public partial class EntryInstructionUserControl : BaseCustomsEntryUserControl
{
	public EntryInstructionUserControl()
	{
		InitializeComponent();
		DetailsUserControl.UserControlType = typeof(LayoutEntryInstructionDetailBasicUserControl);
		DocumentAvailabilityUserControl.UserControlType = typeof(DocumentAvailabilityUserControl);
	}
}
