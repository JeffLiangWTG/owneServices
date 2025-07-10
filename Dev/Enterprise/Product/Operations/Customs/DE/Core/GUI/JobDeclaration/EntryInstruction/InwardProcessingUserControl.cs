using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class InwardProcessingUserControl : ZUserControl
	{
		public InwardProcessingUserControl()
		{
			InitializeComponent();
		}

		public void ChangeControlsVisibility(CusEntryInstruction currentEntryInstruction)
		{
			MainAccountingDocAddressControl.Visible = currentEntryInstruction?.IsMainAccountingAddressAvailable ?? false;
		}
	}
}
