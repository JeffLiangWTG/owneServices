using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class EntryInstructionTopDetailsUserControl : ZUserControl
	{
		public EntryInstructionTopDetailsUserControl()
		{
			InitializeComponent();
			SetAdditionalInfoTextBoxBinding();
		}

		void SetAdditionalInfoTextBoxBinding()
		{
			BindingSource.SetBindingMember(AdditionalInfoTextBox, "AdditionalInformation");
		}
	}
}
