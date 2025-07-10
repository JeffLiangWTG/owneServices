using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionDetailBasicUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailBasicUserControl()
		{
			InitializeComponent();
			NewOwnerOrganisationControl.AllowOverlap(BondHolderOrganisationControl);
		}
	}
}
