using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class DeclarationAgreementUserControl : ZUserControl
	{
		public DeclarationAgreementUserControl()
		{
			InitializeComponent();
			SetupControlProperty();
		}

		void SetupControlProperty()
		{
			AdditionalCommentTextBox.MaxLength = 1000;
			CharacterCountTextBox.Text = "0";
		}

		void AdditionalCommentTextBox_TextChanged(object sender, System.EventArgs e)
		{
			CharacterCountTextBox.Text = AdditionalCommentTextBox.Text.Length.ToString();
		}
	}
}
