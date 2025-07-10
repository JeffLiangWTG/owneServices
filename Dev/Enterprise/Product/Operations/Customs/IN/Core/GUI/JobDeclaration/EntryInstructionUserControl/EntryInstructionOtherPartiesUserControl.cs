using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class EntryInstructionOtherPartiesUserControl : ZUserControl
{
	public EntryInstructionOtherPartiesUserControl()
	{
		InitializeComponent();

		NewOwnerOrganisationControl.AllowOverlap(BondHolderOrganisationControl);
	}
}
